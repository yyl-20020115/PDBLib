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
            => from b in bytes
               select string.Format("{0:X2}", b);

        public static PDBType ToPDBType(uint type, Dictionary<uint, TypeInfo> type_info)
        {
            var ret = new PDBType() { TypeName = "T_NONE" };
            if (type == 0)
                return ret;
            if (type_info.TryGetValue(type, out var ti))
            {
                TYPE_ENUM type_enum = (TYPE_ENUM)(type & 0xff);
                ret.TypeName = type_enum.ToString();
                ret.IsPointer = ((type & (0x0600 | 0x0400)) != 0);
                ret.TypeLeaf = ti.Type.ToString();
                var data = ti.Data;
                switch (ti.Type)
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
                            {
                                ret.SubTypes.Add("arg_"+i,ToPDBType(
                                    BitConverter.ToUInt32(data, 
                                        sizeof(uint) + i * sizeof(uint)), type_info));
                            }
                        }
                        break;
                    // A pointer, with an underlying type
                    case LEAF.LF_POINTER:
                        {
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
                            if (la.idxtype < 0x8000)
                            {
                                ret.Values.Add("value",String.Format("{0:X4}", BitConverter.ToUInt16(data, Marshal.SizeOf<LeafArray>())));
                            }
                            else
                            {
                                ret.SubTypes.Add("idxtype",ToPDBType(la.idxtype, type_info));
                            }
                        }
                        break;
                    case LEAF.LF_MFUNCTION:
                        {
                            LeafMFunc lmf = From<LeafMFunc>(data);
                            ret.SubTypes.Add("rvtype",ToPDBType(lmf.rvtype, type_info));
                            ret.SubTypes.Add("clstype",ToPDBType(lmf.classtype, type_info));
                            ret.SubTypes.Add("arglist",ToPDBType(lmf.arglist, type_info));
                        }
                        break;
                    case LEAF.LF_PROCEDURE:
                        {
                            LeafProc proc = From<LeafProc>(data);
                            ret.SubTypes.Add("rvtype",ToPDBType(proc.rvtype, type_info));
                            ret.SubTypes.Add("arglist",ToPDBType(proc.arglist, type_info));
                        }
                        break;
                    case LEAF.LF_INDEX:
                        {
                            LeafIndex li = From<LeafIndex>(data);
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
                            ret.Values.Add("value",string.Join(',', ToHex(ti.Data)));
                        }
                        break;
                    case LEAF.LF_CHAR:
                        {
                            LeafChar ch = From<LeafChar>(data);
                            ret.Values.Add("value",((char)ch.val).ToString());
                        }
                        break;
                    case LEAF.LF_SHORT:
                        {
                            LeafShort ll = From<LeafShort>(data);
                            ret.Values.Add("value", string.Format("{0:X4}", ll.val));
                        }
                        break;
                    case LEAF.LF_USHORT:
                        {
                            LeafUShort ll = From<LeafUShort>(data);
                            ret.Values.Add("value", string.Format("{0:X4}", ll.val));
                        }
                        break;
                    case LEAF.LF_LONG:
                        {
                            LeafLong ll = From<LeafLong>(data);
                            ret.Values.Add("value", string.Format("{0:X8}", ll.val));
                        }
                        break;
                    case LEAF.LF_ULONG:
                        {
                            LeafULong ll = From<LeafULong>(data);
                            ret.Values.Add("value", string.Format("{0:X8}", ll.val));
                        }
                        break;
                    case LEAF.LF_REAL32:
                        {
                            LeafReal32 ll = From<LeafReal32>(data);
                            ret.Values.Add("value", string.Format("{0}", ll.val));
                        }
                        break;
                    case LEAF.LF_REAL64:
                        {
                            LeafReal64 ll = From<LeafReal64>(data);
                            ret.Values.Add("value", string.Format("{0}", ll.val));
                        }
                        break;
                    case LEAF.LF_REAL80:
                        {
                            ret.Values.Add("value", string.Join(',', ToHex(ti.Data)));
                        }
                        break;
                    case LEAF.LF_REAL128:
                        {
                            ret.Values.Add("value", string.Join(',', ToHex(ti.Data)));
                        }
                        break;
                    case LEAF.LF_REAL256:
                        {
                            ret.Values.Add("value", string.Join(',', ToHex(ti.Data)));
                        }
                        break;
                    case LEAF.LF_REAL512:
                        {
                            ret.Values.Add("value", string.Join(',', ToHex(ti.Data)));
                        }
                        break;
                    case LEAF.LF_QUADWORD:
                        {
                            LeafQuad ll = From<LeafQuad>(data);
                            ret.Values.Add("value", string.Format("{0:X16}",ll.val));
                        }
                        break;
                    case LEAF.LF_UQUADWORD:
                        {
                            LeafUQuad ll = From<LeafUQuad>(data);
                            ret.Values.Add("value", string.Format("{0:X16}", ll.val));
                        }
                        break;
                    default:
                        {
                            ret.Values.Add("value", string.Join(',', ToHex(ti.Data)));
                        }
                        break;
                }
            }

            return ret;
        }

        public static bool StringizeType(uint type, StringBuilder output, Dictionary<uint, TypeInfo> tm, uint flags)
        {
            if (type == 0)
            {
                output.Append("...");
                return false;
            }

            if (tm.TryGetValue(type, out var ti))
            {
                switch ((TYPE_ENUM)(type & 0xff))
                {
                    case TYPE_ENUM.T_VOID:
                        output.Append("void");
                        break;
                    // These ones don't follow the pattern
                    case TYPE_ENUM.T_PVOID:
                    case TYPE_ENUM.T_PFVOID:
                    case TYPE_ENUM.T_PHVOID:
                        output.Append("void *");
                        break;
                    case TYPE_ENUM.T_HRESULT: // Thanks Microsoft!
                        output.Append("long");
                        break;
                    case TYPE_ENUM.T_INT1:
                    case TYPE_ENUM.T_CHAR:
                        output.Append("signed char");
                        break;
                    case TYPE_ENUM.T_RCHAR: // I have no idea what a "really char" is
                        output.Append("char");
                        break;
                    case TYPE_ENUM.T_UINT1:
                    case TYPE_ENUM.T_UCHAR:
                        output.Append("unsigned char");
                        break;
                    case TYPE_ENUM.T_WCHAR:
                        output.Append("wchar_t");
                        break;
                    case TYPE_ENUM.T_SHORT:
                    case TYPE_ENUM.T_INT2:
                        output.Append("short");
                        break;
                    case TYPE_ENUM.T_USHORT:
                    case TYPE_ENUM.T_UINT2:
                        output.Append("unsigned short");
                        break;
                    case TYPE_ENUM.T_LONG:
                        output.Append("long");
                        break;
                    case TYPE_ENUM.T_INT4:
                        output.Append("int");
                        break;
                    case TYPE_ENUM.T_ULONG:
                        output.Append("unsigned long");
                        break;
                    case TYPE_ENUM.T_UINT4:
                        output.Append("unsigned int");
                        break;
                    case TYPE_ENUM.T_QUAD:
                    case TYPE_ENUM.T_INT8:
                        output.Append("__int64");
                        break;
                    case TYPE_ENUM.T_UQUAD:
                    case TYPE_ENUM.T_UINT8:
                        output.Append("unsigned __int64");
                        break;
                    case TYPE_ENUM.T_OCT:
                    case TYPE_ENUM.T_INT16:
                        output.Append("s128");
                        break;
                    case TYPE_ENUM.T_UOCT:
                    case TYPE_ENUM.T_UINT16:
                        output.Append("u128");
                        break;
                    case TYPE_ENUM.T_REAL32:
                        output.Append("float");
                        break;
                    case TYPE_ENUM.T_REAL64:
                        output.Append("double");
                        break;
                    case TYPE_ENUM.T_REAL80:
                        // I don't think this actually exists anymore.
                        output.Append("long double");
                        break;
                    case TYPE_ENUM.T_REAL128:
                        output.Append("f128");
                        break;
                    case TYPE_ENUM.T_BOOL08:
                    case TYPE_ENUM.T_BOOL16:
                    case TYPE_ENUM.T_BOOL32:
                    case TYPE_ENUM.T_BOOL64:
                        output.Append("bool");
                        break;
                    default:
                        output.Append("!Unknown!");
                        return false;
                }

                // Check to see if it is a pointer type, thankfully the enum values are consistent
                if ((type & (0x0600 | 0x0400)) != 0)
                    output.Append(" *");

                var data = ti.Data;

                switch (ti.Type)
                {
                    // const/volatile/unaligned
                    case LEAF.LF_MODIFIER:
                        {
                            LeafModifier lm = From<LeafModifier>(data);
                            StringizeType(lm.type, output, tm, 0);

                            if (0 != (flags & ((uint)StringizeFlags.IsUnderlying | ~(uint)StringizeFlags.IsTopLevel)))
                                return false;

                            switch ((CV_modifier)lm.attr)
                            {
                                case CV_modifier.MOD_const:
                                    output.Append(" const");
                                    break;
                                case CV_modifier.MOD_volatile:
                                    output.Append(" volatile");
                                    break;
                                case CV_modifier.MOD_unaligned:
                                    output.Append(" unaligned");
                                    break;
                            }
                        }
                        break;
                    // The argument list for a function definition
                    case LEAF.LF_ARGLIST:
                        {
                            LeafArgList lal = From<LeafArgList>(data);
                            output.Append("(");

                            if (lal.count == 0 && 0 != (flags & ~(uint)StringizeFlags.IsTopLevel))
                            {
                                output.Append(")");
                                return false;
                            }

                            for (int i = 0; i < lal.count; ++i)
                            {
                                StringizeType(
                                    BitConverter.ToUInt32(data, sizeof(uint) + i * sizeof(uint)), output, tm, flags);

                                if (i != lal.count - 1)
                                    output.Append(",");
                            }

                            output.Append(")");
                        }
                        break;
                    // A pointer, with an underlying type
                    case LEAF.LF_POINTER:
                        {
                            LeafPointer lp = From<LeafPointer>(data);

                            if (!StringizeType(lp.utype, output, tm, (uint)StringizeFlags.IsUnderlying & flags))
                            {
                                switch ((CV_ptrmode)((lp.attr & (uint)LeafPointerAttr.ptrmode) >> 5))
                                {
                                    case CV_ptrmode.CV_PTR_MODE_REF:
                                        output.Append(" &");
                                        break;
                                    case CV_ptrmode.CV_PTR_MODE_PTR:
                                        output.Append(" *");
                                        break;
                                    case CV_ptrmode.CV_PTR_MODE_PMEM:
                                        output.Append(".*");
                                        break;
                                    case CV_ptrmode.CV_PTR_MODE_PMFUNC:
                                        output.Append(".");
                                        break;
                                    case CV_ptrmode.CV_PTR_MODE_RESERVED: // This is now being used for r-value references
                                        output.Append("&&");
                                        break;
                                    default:
                                        break;
                                }
                            }

                            if (0 != (lp.attr & (uint)LeafPointerAttr.isconst))
                                output.Append(" const");

                            if (0 != (lp.attr & (uint)LeafPointerAttr.isvolatile))
                                output.Append(" volatile");

                            // restrict could be added, but not necessarily interesting?
                        }
                        break;
                    case LEAF.LF_ARRAY:
                        {
                            LeafArray la = From<LeafArray>(data);

                            StringizeType(la.elemtype, output, tm, 0);

                            output.Append("[");
                            // According to the comments, if this value is less than 0x8000 then the next 2 bytes are the actual value
                            if (la.idxtype < 0x8000)
                                output.Append(BitConverter.ToUInt16(data, Marshal.SizeOf<LeafArray>()));
                            else
                                StringizeType(la.idxtype, output, tm, 0);
                            output.Append("]");
                        }
                        break;
                    case LEAF.LF_MFUNCTION:
                        {
                            LeafMFunc lmf = From<LeafMFunc>(data);

                            if (0 != (flags & (uint)StringizeFlags.IsUnderlying))
                            {
                                StringizeType(lmf.rvtype, output, tm, 0);
                                output.Append(" (");
                                StringizeType(lmf.classtype, output, tm, 0);
                                output.Append(".*)");
                            }

                            StringizeType(lmf.arglist, output, tm, 0);
                        }
                        return true;
                    case LEAF.LF_PROCEDURE:
                        {
                            LeafProc proc = From<LeafProc>(data);

                            if (0 != (flags & (uint)StringizeFlags.IsUnderlying))
                            {
                                StringizeType(proc.rvtype, output, tm, 0);
                                output.Append(" (*)");
                            }

                            StringizeType(proc.arglist, output, tm, 0);
                        }
                        return true;
                    case LEAF.LF_INDEX:
                        {
                            LeafIndex li = From<LeafIndex>(data);
                            StringizeType(li.index, output, tm, flags);
                        }
                        break;
                    // All types past this point are leaf types that terminate recursion
                    case LEAF.LF_ENUM:
                    case LEAF.LF_ALIAS:
                    case LEAF.LF_UNION:
                    case LEAF.LF_CLASS:
                    case LEAF.LF_STRUCTURE:
                        {
                            output.Append(String.Join(',', ToHex(ti.Data)));
                        }
                        break;
                    case LEAF.LF_CHAR:
                        {
                            LeafChar ch = From<LeafChar>(data);
                            output.Append((char)ch.val);
                        }
                        break;
                    case LEAF.LF_SHORT:
                        {
                            LeafShort sh = From<LeafShort>(data);
                            output.Append(sh.val);
                        }
                        break;
                    case LEAF.LF_USHORT:
                        {
                            LeafUShort sh = From<LeafUShort>(data);
                            output.Append(sh.val);
                        }
                        break;
                    case LEAF.LF_LONG:
                        {
                            LeafLong ll = From<LeafLong>(data);
                            output.Append(ll.val);
                        }
                        break;
                    case LEAF.LF_ULONG:
                        {
                            LeafULong ll = From<LeafULong>(data);
                            output.Append(ll.val);
                        }
                        break;
                    case LEAF.LF_REAL32:
                        {
                            LeafReal32 ll = From<LeafReal32>(data);
                            output.Append(ll.val);
                        }
                        break;
                    case LEAF.LF_REAL64:
                        {
                            LeafReal64 ll = From<LeafReal64>(data);
                            output.Append(ll.val);
                        }
                        break;
                    case LEAF.LF_REAL80:
                        output.Append("f80");
                        break;
                    case LEAF.LF_REAL128:
                        output.Append("f128");
                        break;
                    case LEAF.LF_QUADWORD:
                        {
                            LeafQuad ll = From<LeafQuad>(data);
                            output.Append(ll.val);
                        }
                        break;
                    case LEAF.LF_UQUADWORD:
                        {
                            LeafUQuad ll = From<LeafUQuad>(data);
                            output.Append(ll.val);
                        }
                        break;
                    default:
                        // Unhandled...these are the only records I encountered, would need to be extended for managed code
                        break;

                }
                return true;
            }
            return false;
        }
    }
}
