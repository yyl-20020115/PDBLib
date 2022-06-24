using System.Runtime.InteropServices;
using System.Text;

namespace PDBLib
{
    public static class Utils
    {
        public static uint GetNumPages(uint length, uint pageSize)
            => length % pageSize != 0 ? (length / pageSize) + 1 : (length / pageSize);
        public static uint GetAlignedLength(uint length, uint pageSize)
            => GetNumPages(length, pageSize) * pageSize;
        public static int GetNumPages(int length, int pageSize)
            => length % pageSize != 0 ? (length / pageSize) + 1 : (length / pageSize);
        public static int GetAlignedLength(int length, int pageSize)
            => GetNumPages(length, pageSize) * pageSize;

        public static string ReadString(byte[] bytes, int start)
        {
            var ret = new List<byte>();
            byte b = 0;
            while (start<bytes.Length && (b = bytes[start]) != 0)
            {
                ret.Add(b);
                start++;
            }
            return Encoding.ASCII.GetString(ret.ToArray());
        }
        public static byte[] ReadFile(string path, int length, byte[]? data = null)
        {
            data ??= new byte[length];
            if (data.Length < length)
                length = data.Length;
            using var fileStream = File.OpenRead(path);

            var toread = (int)Math.Min(fileStream.Length, length);
            fileStream.Read(data, 0, toread);
            return data;
        }
        public static byte[] From<T>(T t) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            var data = new byte[size];
            var structPtr = Marshal.AllocHGlobal(size);
            if (structPtr != IntPtr.Zero)
            {
                Marshal.StructureToPtr(t, structPtr, true);
                Marshal.Copy(structPtr, data, 0, size);
                Marshal.FreeHGlobal(structPtr);
            }

            return data;
        }

        public static T From<T>(byte[] bytes) where T : struct
        {
            var ret = new T();
            var size = Marshal.SizeOf<T>();

            if (size <= bytes.Length)
            {
                var structPtr = Marshal.AllocHGlobal(size);
                if (structPtr != IntPtr.Zero)
                {
                    Marshal.Copy(bytes, 0, structPtr, size);
                    if (Marshal.PtrToStructure(structPtr, typeof(T)) is T t)
                    {
                        ret = (T)t;
                    }
                    Marshal.FreeHGlobal(structPtr);
                }
            }

            return ret;
        }
        public static IEnumerable<string> ToHex(IEnumerable<byte> bytes) 
            => bytes!=null ? from b in bytes
               select string.Format("{0:X2}", b):Array.Empty<string>();
        public static string ToTypeName(uint type, Dictionary<uint, TypeInfo> type_info)
            => type_info.TryGetValue(type, out var ti) ? ti.Name : (type==2?"T_FUNCTION":string.Format("T_TYPE_{0:X8}", type));

        public static PDBType ToPDBType(uint type, Dictionary<uint, TypeInfo> type_info)
        {
            var ret = new PDBType() { TypeName = type.ToString() };
            if (type == 0)
                return ret;
            if (!type_info.TryGetValue(type, out var ti))
            {
                TYPE_ENUM type_enum = (TYPE_ENUM)(type & 0xff);
                ret.TypeName = type_enum.ToString();
                ret.IsPointer = ((type & (0x0600 | 0x0400)) != 0);
            }
            else
            {
                var leafType = LEAF.LF_NONE;
                var data = Array.Empty<byte>();

                ret.TypeName = ti.Type.ToString();
                leafType = ti.Type;
                data = ti.Data;

                switch (leafType)
                {
                    // const/volatile/unaligned
                    case LEAF.LF_MODIFIER:
                        {
                            var lm = From<LeafModifier>(data);
                            var mf = ToPDBType(lm.type, type_info);
                            ret.SubTypes.Add("modifier", mf);
                            ret.Values.Add("modifier",((CV_modifier)lm.attr).ToString());
                        }
                        break;
                    // The argument list for a function definition
                    case LEAF.LF_ARGLIST:
                        {
                            var lal = From<LeafArgList>(data);
                            for (int i = 0; i < lal.count; ++i)
                                ret.SubTypes.Add("arg_"+i,ToPDBType(
                                    BitConverter.ToUInt32(data, 
                                        sizeof(uint) + i * sizeof(uint)), type_info));
                        }
                        break;
                    // A pointer, with an underlying type
                    case LEAF.LF_POINTER:
                        {
                            ret.IsPointer = true;
                            var lp = From<LeafPointer>(data);
                            var ptr = ToPDBType(lp.utype, type_info);
                            ret.SubTypes.Add("basetype",ptr);
                            var mf = (CV_ptrmode)((lp.attr & (uint)LeafPointerAttr.ptrmode) >> 5);
                            ret.Values.Add("mode",mf.ToString());
                            if (0 != (lp.attr & (uint)LeafPointerAttr.isconst))
                                ret.Values.Add("const","true"); 
                            if (0 != (lp.attr & (uint)LeafPointerAttr.isvolatile))
                                ret.Values.Add("volatile","true");
                            // restrict could be added, but not necessarily interesting?
                        }
                        break;
                    case LEAF.LF_ARRAY:
                        {
                            var la = From<LeafArray>(data);
                            var tp = ToPDBType(la.elemtype, type_info);
                            ret.SubTypes.Add("elemtype",tp);
                            // According to the comments, if this value is less than 0x8000 then
                            // the next 2 bytes are the actual value
                            var header_size = Marshal.SizeOf<LeafArray>();
                            if (la.idxtype < 0x8000 && data.Length>= header_size)
                            {
                                ret.Values.Add("value", string.Join(',', ToHex(data[header_size..])));
                            }
                            else
                            {
                                ret.SubTypes.Add("idxtype",ToPDBType(la.idxtype, type_info));
                            }
                        }
                        break;
                    case LEAF.LF_MFUNCTION:
                        {
                            var lmf = From<LeafMFunc>(data);
                            ret.SubTypes.Add("rvtype",ToPDBType(lmf.rvtype, type_info));
                            ret.SubTypes.Add("clstype",ToPDBType(lmf.classtype, type_info));
                            ret.SubTypes.Add("arglist",ToPDBType(lmf.arglist, type_info));
                        }
                        break;
                    case LEAF.LF_PROCEDURE:
                        {
                            var proc = From<LeafProc>(data);
                            ret.SubTypes.Add("rvtype",ToPDBType(proc.rvtype, type_info));
                            ret.SubTypes.Add("arglist",ToPDBType(proc.arglist, type_info));
                        }
                        break;
                    case LEAF.LF_INDEX:
                        {
                            var li = From<LeafIndex>(data);
                            ret.SubTypes.Add("index",ToPDBType(li.index, type_info));
                        }
                        break;
                    // All types past this point are leaf types that terminate recursion
                    case LEAF.LF_ENUM:
                    case LEAF.LF_ALIAS:
                    case LEAF.LF_UNION:
                    case LEAF.LF_CLASS:
                    case LEAF.LF_STRUCTURE:
                        {
                            ret.Values.Add("value",string.Join(',', ToHex(data)));
                        }
                        break;
                    case LEAF.LF_CHAR:
                        {
                            var ch = From<LeafChar>(data);
                            ret.Values.Add("value",((char)ch.val).ToString());
                        }
                        break;
                    case LEAF.LF_SHORT:
                        {
                            var ll = From<LeafShort>(data);
                            ret.Values.Add("value", string.Format("{0:X4}", ll.val));
                        }
                        break;
                    case LEAF.LF_USHORT:
                        {
                            var ll = From<LeafUShort>(data);
                            ret.Values.Add("value", string.Format("{0:X4}", ll.val));
                        }
                        break;
                    case LEAF.LF_LONG:
                        {
                            var ll = From<LeafLong>(data);
                            ret.Values.Add("value", string.Format("{0:X8}", ll.val));
                        }
                        break;
                    case LEAF.LF_ULONG:
                        {
                            var ll = From<LeafULong>(data);
                            ret.Values.Add("value", string.Format("{0:X8}", ll.val));
                        }
                        break;
                    case LEAF.LF_REAL32:
                        {
                            var ll = From<LeafReal32>(data);
                            ret.Values.Add("value", string.Format("{0}", ll.val));
                        }
                        break;
                    case LEAF.LF_REAL64:
                        {
                            var ll = From<LeafReal64>(data);
                            ret.Values.Add("value", string.Format("{0}", ll.val));
                        }
                        break;
                    case LEAF.LF_REAL80:
                        {
                            ret.Values.Add("value", string.Join(',', ToHex(data)));
                        }
                        break;
                    case LEAF.LF_REAL128:
                        {
                            ret.Values.Add("value", string.Join(',', ToHex(data)));
                        }
                        break;
                    case LEAF.LF_REAL256:
                        {
                            ret.Values.Add("value", string.Join(',', ToHex(data)));
                        }
                        break;
                    case LEAF.LF_REAL512:
                        {
                            ret.Values.Add("value", string.Join(',', ToHex(data)));
                        }
                        break;
                    case LEAF.LF_QUADWORD:
                        {
                            var ll = From<LeafQuad>(data);
                            ret.Values.Add("value", string.Format("{0:X16}",ll.val));
                        }
                        break;
                    case LEAF.LF_UQUADWORD:
                        {
                            var ll = From<LeafUQuad>(data);
                            ret.Values.Add("value", string.Format("{0:X16}", ll.val));
                        }
                        break;
                    default:
                        {
                            ret.Values.Add("value", string.Join(',', ToHex(data)));
                        }
                        break;
                }
            }

            return ret;
        }
    }
}
