using System.Text;
using System.Runtime.InteropServices;

namespace PDBLib;
public enum PdbRaw_ImplVer : uint
{
    PdbImplVC2 = 19941610,
    PdbImplVC4 = 19950623,
    PdbImplVC41 = 19950814,
    PdbImplVC50 = 19960307,
    PdbImplVC98 = 19970604,
    PdbImplVC70Dep = 19990604, // deprecated
    PdbImplVC70 = 20000404,
    PdbImplVC80 = 20030901,
    PdbImplVC110 = 20091201,
    PdbImplVC140 = 20140508,
}

public enum PdbRaw_SrcHeaderBlockVer : uint
{
    SrcVerOne = 19980827
}


public enum PdbRaw_FeatureSig : uint
{
    VC110 = PdbRaw_ImplVer.PdbImplVC110,
    VC140 = PdbRaw_ImplVer.PdbImplVC140,
    NoTypeMerge = 0x4D544F4E,
    MinimalDebugInfo = 0x494E494D,
}

public enum PdbRaw_Features : uint
{
    PdbFeatureNone = 0x0,
    PdbFeatureContainsIdStream = 0x1,
    PdbFeatureMinimalDebugInfo = 0x2,
    PdbFeatureNoTypeMerging = 0x4,
    //LLVM_MARK_AS_BITMASK_ENUM(/* LargestValue = */ PdbFeatureNoTypeMerging)
}

public enum PdbRaw_DbiVer : uint
{
    PdbDbiVC41 = 930803,
    PdbDbiV50 = 19960307,
    PdbDbiV60 = 19970606,
    PdbDbiV70 = 19990903,
    PdbDbiV110 = 20091201
};

public enum PdbRaw_TpiVer : uint
{
    PdbTpiV40 = 19950410,
    PdbTpiV41 = 19951122,
    PdbTpiV50 = 19961031,
    PdbTpiV70 = 19990903,
    PdbTpiV80 = 20040203,
}

public enum PdbRaw_DbiSecContribVer : uint
{
    DbiSecContribVer60 = 0xeffe0000 + 19970605,
    DbiSecContribV2 = 0xeffe0000 + 20140516
}

public enum SpecialStream : uint
{
    // Stream 0 contains the copy of previous version of the MSF directory.
    // We are not currently using it, but technically if we find the main
    // MSF is corrupted, we could fallback to it.
    OldMSFDirectory = 0,

    StreamPDB = 1,
    StreamTPI = 2,
    StreamDBI = 3,
    StreamIPI = 4,

    kSpecialStreamCount
}

public enum DbgHeaderType : uint
{
    FPO,
    Exception,
    Fixup,
    OmapToSrc,
    OmapFromSrc,
    SectionHdr,
    TokenRidMap,
    Xdata,
    Pdata,
    NewFPO,
    SectionHdrOrig,
    Max
}

public enum OMFSegDescFlags : ushort
{
    None = 0,
    Read = 1 << 0,              // Segment is readable.
    Write = 1 << 1,             // Segment is writable.
    Execute = 1 << 2,           // Segment is executable.
    AddressIs32Bit = 1 << 3,    // Descriptor describes a 32-bit linear address.
    IsSelector = 1 << 8,        // Frame represents a selector.
    IsAbsoluteAddress = 1 << 9, // Frame represents an absolute address.
    IsGroup = 1 << 10,          // If set, descriptor represents a group.
                                //LLVM_MARK_AS_BITMASK_ENUM(/* LargestValue = */ IsGroup)
}


public static class PDBConsts
{
    public const int DefaultPageAlignmentSize = 4096;
    public static Encoding DefaultEncoding = Encoding.Latin1;
    public static string Signature = "Microsoft C/C++ MSF 7.00\r\n" + ((char)26) + "DS\0\0\0";
    public static byte[] SignatureBytes = DefaultEncoding.GetBytes(Signature);
    public const string NameStreamName = "/NAMES";
    public const uint NameStreamSignature = 0xeffeeffe;
    public const int NameStreamVersion = 1;
    public const ushort kInvalidStreamIndex = 0xFFFF;

}
public struct TypeInfo
{
    public byte[] Data;
    public string Name;
    public LEAF Type;
}
public enum StringizeFlags : uint
{
    IsUnderlying = 0x1,
    IsTopLevel = 0x2
}

public struct SymbolSource
{
    public ushort NumModules;
    public ushort NumModuleSources;
}

public struct NameStreamHeader
{
    public uint Sig;
    public int Version;
    public int OffsetsOffset;
}

public struct StringStreamIds
{
    public uint StringId;
    public uint StreamId;
};

public class UniqueSrc
{
    public uint Id = 0;
    public uint Visited = 0;
}
public class NameStream
{
    public Dictionary<uint, string> Dict = new();
    public byte[] Buffer = Array.Empty<byte>();
}

public enum KnownStream : uint
{
    RootStream = 0,
    StreamsStream = 1,
    TypeInfoStream = 2,
    DebugInfoStream = 3,
}
public class StreamPair
{
    public uint Size = 0;
    public List<uint> PageIndices = new();

    public StreamPair(uint size = 0)
    {
        this.Size = size;
    }
    public override string ToString() => $"Size = {this.Size}, Count = {this.PageIndices.Count}";
}

public class FunctionRecord : IComparable<FunctionRecord>
{
    public string Name = "";
    public string Source = "";
    public List<CV_Line> Lines = new();
    public uint LineCount = 0;
    public uint Segment = 0;
    public uint Offset = 0;
    public uint Length = 0;
    public uint LineOffset = 0;
    public uint TypeIndex = 0; // If this is non-zero, the function is a procedure, not a thunk (I don't know how to read thunk type info...)
    public uint ParamSize = 0;
    public FunctionRecord(string name)
    {
        this.Name = name;
    }
    public FunctionRecord(uint offset, uint segment)
    {
        this.Segment = segment;
        this.Offset = offset;
    }
    public override bool Equals(object? obj) => (obj is FunctionRecord f) && (this == f);
    public override int GetHashCode() => base.GetHashCode();
    public static bool operator <(FunctionRecord self, FunctionRecord other)
    {
        if (self.Segment != other.Segment)
            return self.Segment < other.Segment;
        else if (self.Offset != other.Offset)
            return self.Offset < other.Offset;
        else
        {
            // This is merely to make the sort algorithm not throw a shit fit
            // at this point we have no way to differentiate between two functions
            // so we will have to let one of them 'win' after the sort is finished
            return self.TypeIndex < other.TypeIndex;
        }
    }
    public static bool operator >(FunctionRecord self, FunctionRecord other) => self != other && !(self < other);
    public static bool operator ==(FunctionRecord self, FunctionRecord other) => self.Segment == other.Segment && self.Offset == other.Offset;
    public static bool operator !=(FunctionRecord self, FunctionRecord other) => !(self == other);

    public int CompareTo(FunctionRecord? other)
    {
        if (other is FunctionRecord f)
        {
            if (this == f) return 0;
            else if (this > f) return 1;
            else return -1;
        }
        else
        {
            return 1;
        }
    }
}

public struct SymbolHeader
{
    public ushort Size;
    public ushort Type;
}
public struct SubsectionHeader
{
    public int Sig;
    public int Size;
}
public class Module
{
    public DBIModuleInfo Info = new();
    public List<string> Sources = new();
    public List<FunctionRecord> Functions = new();
    public Dictionary<uint, uint> SrcIndex = new();
    public string ModuleName = "";
    public string ObjectName = "";
}

public static class Consts
{
    public const int IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR = 14;
    public const int IMAGE_NUMBEROF_DIRECTORY_ENTRIES = 16;
    public const ushort IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10b;
    public const ushort IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20b;
    public const int IMAGE_SIZEOF_SHORT_NAME = 8;
    public const ushort IMAGE_FILE_MACHINE_I386 = 0x014c;
    public const ushort IMAGE_FILE_MACHINE_AMD64 = 0x8664;
    public const ushort IMAGE_FILE_MACHINE_ARM = 0x01c4;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IMAGE_DOS_HEADER
{
    public ushort e_magic;
    public ushort e_cblp;
    public ushort e_cp;
    public ushort e_crlc;
    public ushort e_cparhdr;
    public ushort e_minalloc;
    public ushort e_maxalloc;
    public ushort e_ss;
    public ushort e_sp;
    public ushort e_csum;
    public ushort e_ip;
    public ushort e_cs;
    public ushort e_lfarlc;
    public ushort e_ovno;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public ushort[] e_res;
    public ushort e_oemid;
    public ushort e_oeminfo;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public ushort[] e_res2;
    public uint e_lfanew;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IMAGE_FILE_HEADER
{
    public ushort Machine;
    public ushort NumberOfSections;
    public uint TimeDateStamp;
    public uint PointerToSymbolTable;
    public uint NumberOfSymbols;
    public ushort SizeOfOptionalHeader;
    public ushort Characteristics;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IMAGE_DATA_DIRECTORY
{
    public uint VirtualAddress;
    public uint Size;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IMAGE_OPTIONAL_HEADER
{
    public ushort Magic;
    public byte MajorLinkerVersion;
    public byte MinorLinkerVersion;
    public uint SizeOfCode;
    public uint SizeOfInitializedData;
    public uint SizeOfUninitializedData;
    public uint AddressOfEntryPoint;
    public uint BaseOfCode;
    public uint BaseOfData;
    public uint ImageBase;
    public uint SectionAlignment;
    public uint FileAlignment;
    public ushort MajorOperatingSystemVersion;
    public ushort MinorOperatingSystemVersion;
    public ushort MajorImageVersion;
    public ushort MinorImageVersion;
    public ushort MajorSubsystemVersion;
    public ushort MinorSubsystemVersion;
    public uint Win32VersionValue;
    public uint SizeOfImage;
    public uint SizeOfHeaders;
    public uint CheckSum;
    public ushort Subsystem;
    public ushort DllCharacteristics;
    public uint SizeOfStackReserve;
    public uint SizeOfStackCommit;
    public uint SizeOfHeapReserve;
    public uint SizeOfHeapCommit;
    public uint LoaderFlags;
    public uint NumberOfRvaAndSizes;
    [MarshalAs(UnmanagedType.ByValArray,
        SizeConst = Consts.IMAGE_NUMBEROF_DIRECTORY_ENTRIES)]
    IMAGE_DATA_DIRECTORY[] DataDirectory;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IMAGE_OPTIONAL_HEADER64
{
    public ushort Magic;
    public byte MajorLinkerVersion;
    public byte MinorLinkerVersion;
    public uint SizeOfCode;
    public uint SizeOfInitializedData;
    public uint SizeOfUninitializedData;
    public uint AddressOfEntryPoint;
    public uint BaseOfCode;
    public ulong ImageBase;
    public uint SectionAlignment;
    public uint FileAlignment;
    public ushort MajorOperatingSystemVersion;
    public ushort MinorOperatingSystemVersion;
    public ushort MajorImageVersion;
    public ushort MinorImageVersion;
    public ushort MajorSubsystemVersion;
    public ushort MinorSubsystemVersion;
    public uint Win32VersionValue;
    public uint SizeOfImage;
    public uint SizeOfHeaders;
    public uint CheckSum;
    public ushort Subsystem;
    public ushort DllCharacteristics;
    public ulong SizeOfStackReserve;
    public ulong SizeOfStackCommit;
    public ulong SizeOfHeapReserve;
    public ulong SizeOfHeapCommit;
    public uint LoaderFlags;
    public uint NumberOfRvaAndSizes;

    [MarshalAs(UnmanagedType.ByValArray,
        SizeConst = Consts.IMAGE_NUMBEROF_DIRECTORY_ENTRIES)]
    IMAGE_DATA_DIRECTORY[] DataDirectory;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IMAGE_NT_HEADERS64
{
    public uint Signature;
    public IMAGE_FILE_HEADER FileHeader;
    public IMAGE_OPTIONAL_HEADER64 OptionalHeader;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IMAGE_NT_HEADERS32
{
    public uint Signature;
    public IMAGE_FILE_HEADER FileHeader;
    public IMAGE_OPTIONAL_HEADER OptionalHeader;
}

[StructLayout(LayoutKind.Explicit, Size = sizeof(uint))]
public struct IMAGE_SECTION_HEADER_PADDRESS_OR_SIZE
{
    [FieldOffset(0)]
    public uint PhysicalAddress;
    [FieldOffset(0)]
    public uint VirtualSize;
}
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IMAGE_SECTION_HEADER
{
    [MarshalAs(UnmanagedType.ByValArray,
        SizeConst = Consts.IMAGE_SIZEOF_SHORT_NAME)]
    public byte[] Name;

    public IMAGE_SECTION_HEADER_PADDRESS_OR_SIZE PAddressOrSize;
    public uint VirtualAddress;
    public uint SizeOfRawData;
    public uint PointerToRawData;
    public uint PointerToRelocations;
    public uint PointerToLinenumbers;
    public ushort NumberOfRelocations;
    public ushort NumberOfLinenumbers;
    public uint Characteristics;
}

