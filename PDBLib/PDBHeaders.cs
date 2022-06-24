using System.Runtime.InteropServices;

namespace PDBLib
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct PDBHeader
	{
		[MarshalAs(UnmanagedType.ByValArray,SizeConst = 32)]
		public byte[] Signature;
		public int PageSize;
		public int FreePageMapIndex;
		public int PagesUsed;
		public int DirectorySize;
		public uint Reserved;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct NameIndexHeader
	{
		public uint version;
		public uint timeDateStamp;
		public uint age;
		public Guid guid;
		public uint names;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct DBIHeader
	{
		public uint signature;             // 0xFFFFFFFF
		public uint version;
		public uint age;
		public short gssymStream;
		public ushort vers;
		public short pssymStream;
		public ushort pdbVersion;
		public short symRecordStream;    // Stream index containing global symbols
		public ushort pdbVersion2;
		public uint moduleSize;
		public uint secConSize;
		public uint secMapSize;
		public uint fileInfoSize;
		public uint srcModuleSize;
		public uint mfcIndex;
		public uint dbgHeaderSize;
		public uint ecInfoSize;
		public ushort flags;
		public ushort machine;           // ImageFileMachine
		public uint reserved;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct DBIDebugHeader
	{
		public ushort FPO;
		public ushort exception;
		public ushort fixup;
		public ushort omapToSource;
		public ushort omapFromSource;
		public ushort sectionHdr;
		public ushort tokenRidMap;
		public ushort XData;
		public ushort PData;
		public ushort newFPO;
		public ushort sectionHdrOriginal;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct DBISecCon
	{
		public short section;
		public ushort padding1;
		public int offset;
		public uint size;
		public uint flags;
		public short module;
		public ushort pad2;
		public uint dataCrc;
		public uint relocCrc;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct DBIModuleInfo
	{
		public int opened;
		public DBISecCon sector;
		public ushort lags;
		public short stream;
		public int cbSyms;
		public int cbOldLines;
		public int cbLines;
		public short files;
		public short padding;
		public uint offsets;
		public int niSource;
		public int niCompiler;

		// const char* moduleName;
		// const char* objectName;
	}
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct OffCb
	{
		public int off;
		public int cb;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct TypeInfoHeader
	{
		public uint version;
		public int headerSize;
		public uint min;
		public uint max;
		public uint followSize;

		public ushort sn;
		public ushort padding;
		public int hashKey;
		public int buckets;
		public OffCb hashVals;
		public OffCb tiOff;
		public OffCb hashAdjust;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct float10
	{
        [MarshalAs(UnmanagedType.ByValArray,SizeConst =10)]
		public byte[] val;
	}

	public enum CV_SIGNATURE :uint
	{
		C6 = 0,    // Actual signature is >64K
		C7 = 1,    // First explicit signature
		C11 = 2,    // C11 (vc5.x) 32-bit types
		C13 = 4,    // C13 (vc7.x) zero terminated names
		RESERVERD = 5,    // All signatures from 5 to 64K are reserved
	}
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_SIGNATURE_
	{
		public CV_SIGNATURE signature;
	}


	//  CodeView Symbol and Type OMF type information is broken up into two
	//  ranges.  Type indices less than 0x1000 describe type information
	//  that is frequently used.  Type indices above 0x1000 are used to
	//  describe more complex features such as functions, arrays and
	//  structures.
	//

	//  Primitive types have predefined meaning that is encoded in the
	//  values of the various bit fields in the value.
	//
	//  A CodeView primitive type is defined as:
	//
	//  1 1
	//  1 089  7654  3  210
	//  r mode type  r  sub
	//
	//  Where
	//      mode is the pointer mode
	//      type is a type indicator
	//      sub  is a subtype enumeration
	//      r    is a reserved field
	//
	//  See Microsoft Symbol and Type OMF (Version 4.0) for more
	//  information.
	//

	//  pointer mode enumeration values

	public enum CV_promode : uint
	{
		CV_TM_DIRECT = 0,        // mode is not a pointer
		CV_TM_NPTR32 = 4,        // mode is a 32 bit near pointer
		CV_TM_NPTR64 = 6,        // mode is a 64 bit near pointer
		CV_TM_NPTR128 = 7,        // mode is a 128 bit near pointer
	}
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CV_prmode_
	{
		public CV_promode promode;
	}

	//  type enumeration values
	public enum CV_type : uint
	{
		CV_SPECIAL = 0x00,     // special type size values
		CV_SIGNED = 0x01,     // signed integral size values
		CV_UNSIGNED = 0x02,     // unsigned integral size values
		CV_BOOLEAN = 0x03,     // Boolean size values
		CV_REAL = 0x04,     // real number size values
		CV_COMPLEX = 0x05,     // complex number size values
		CV_SPECIAL2 = 0x06,     // second set of special types
		CV_INT = 0x07,     // integral (int) values
		CV_CVRESERVED = 0x0f,
	}
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CV_type_
	{
		public CV_type type;	
	}

	//  subtype enumeration values for CV_SPECIAL

	public enum CV_special : uint
	{
		CV_SP_NOTYPE = 0x00,
		CV_SP_ABS = 0x01,
		CV_SP_SEGMENT = 0x02,
		CV_SP_VOID = 0x03,
		CV_SP_CURRENCY = 0x04,
		CV_SP_NBASICSTR = 0x05,
		CV_SP_FBASICSTR = 0x06,
		CV_SP_NOTTRANS = 0x07,
		CV_SP_HRESULT = 0x08,
	}
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_special_
	{
		public CV_special special;	
	}

	//  subtype enumeration values for CV_SPECIAL2
	public enum CV_special2 : uint
	{
		CV_S2_BIT = 0x00,
		CV_S2_PASCHAR = 0x01,     // Pascal CHAR
	}
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_special2_
	{
		public CV_special2 special2;	
	}
	//  subtype enumeration values for CV_SIGNED, CV_UNSIGNED and CV_BOOLEAN
	public enum CV_integral : uint
	{
		CV_IN_1BYTE = 0x00,
		CV_IN_2BYTE = 0x01,
		CV_IN_4BYTE = 0x02,
		CV_IN_8BYTE = 0x03,
		CV_IN_16BYTE = 0x04,
	}
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_integral_
	{
		public CV_integral integral;
	}
	//  subtype enumeration values for CV_REAL and CV_COMPLEX

	public enum CV_real : uint
	{
		CV_RC_REAL32 = 0x00,
		CV_RC_REAL64 = 0x01,
		CV_RC_REAL80 = 0x02,
		CV_RC_REAL128 = 0x03,
	}
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_real_
	{
        public CV_real real;
	}

	//  subtype enumeration values for CV_INT (really int)

	public enum CV_int : uint
	{
		CV_RI_CHAR = 0x00,
		CV_RI_INT1 = 0x00,
		CV_RI_WCHAR = 0x01,
		CV_RI_UINT1 = 0x01,
		CV_RI_INT2 = 0x02,
		CV_RI_UINT2 = 0x03,
		CV_RI_INT4 = 0x04,
		CV_RI_UINT4 = 0x05,
		CV_RI_INT8 = 0x06,
		CV_RI_UINT8 = 0x07,
		CV_RI_INT16 = 0x08,
		CV_RI_UINT16 = 0x09,
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_int_
	{
		public CV_int cv_int;
	}

	public static class CV_PRIMITIVE_TYPE
	{
		public const uint CV_MMASK = 0x700;       // mode mask
		public const uint CV_TMASK = 0x0f0;       // type mask
		public const uint CV_SMASK = 0x00f;       // subtype mask

		public const int CV_MSHIFT = 8;           // primitive mode right shift count
		public const int CV_TSHIFT = 4;           // primitive type right shift count
		public const int CV_SSHIFT = 0;           // primitive subtype right shift count
		public const uint CV_FIRST_NONPRIM = 0x1000;
	}

	// selected values for type_index - for a more complete definition, see
	// Microsoft Symbol and Type OMF document

	//  Special Types
	public enum TYPE_ENUM : uint
	{
		T_NOTYPE = 0x0000,   // uncharacterized type (no type)
		T_ABS = 0x0001,   // absolute symbol
		T_SEGMENT = 0x0002,   // segment type
		T_VOID = 0x0003,   // void
		T_HRESULT = 0x0008,   // OLE/COM HRESULT
		T_32PHRESULT = 0x0408,   // OLE/COM HRESULT __ptr32//
		T_64PHRESULT = 0x0608,   // OLE/COM HRESULT __ptr64//
		T_PVOID = 0x0103,   // near pointer to void
		T_PFVOID = 0x0203,   // far pointer to void
		T_PHVOID = 0x0303,   // huge pointer to void
		T_32PVOID = 0x0403,   // 32 bit pointer to void
		T_64PVOID = 0x0603,   // 64 bit pointer to void
		T_CURRENCY = 0x0004,   // BASIC 8 byte currency value
		T_NOTTRANS = 0x0007,   // type not translated by cvpack
		T_BIT = 0x0060,   // bit
		T_PASCHAR = 0x0061,   // Pascal CHAR

		//  Character types

		T_CHAR = 0x0010,   // 8 bit signed
		T_32PCHAR = 0x0410,   // 32 bit pointer to 8 bit signed
		T_64PCHAR = 0x0610,   // 64 bit pointer to 8 bit signed

		T_UCHAR = 0x0020,   // 8 bit unsigned
		T_32PUCHAR = 0x0420,   // 32 bit pointer to 8 bit unsigned
		T_64PUCHAR = 0x0620,   // 64 bit pointer to 8 bit unsigned

		//  really a character types

		T_RCHAR = 0x0070,   // really a char
		T_32PRCHAR = 0x0470,   // 32 bit pointer to a real char
		T_64PRCHAR = 0x0670,   // 64 bit pointer to a real char

		//  really a wide character types

		T_WCHAR = 0x0071,   // wide char
		T_32PWCHAR = 0x0471,   // 32 bit pointer to a wide char
		T_64PWCHAR = 0x0671,   // 64 bit pointer to a wide char

		//  8 bit int types

		T_INT1 = 0x0068,   // 8 bit signed int
		T_32PINT1 = 0x0468,   // 32 bit pointer to 8 bit signed int
		T_64PINT1 = 0x0668,   // 64 bit pointer to 8 bit signed int

		T_UINT1 = 0x0069,   // 8 bit unsigned int
		T_32PUINT1 = 0x0469,   // 32 bit pointer to 8 bit unsigned int
		T_64PUINT1 = 0x0669,   // 64 bit pointer to 8 bit unsigned int

		//  16 bit short types

		T_SHORT = 0x0011,   // 16 bit signed
		T_32PSHORT = 0x0411,   // 32 bit pointer to 16 bit signed
		T_64PSHORT = 0x0611,   // 64 bit pointer to 16 bit signed

		T_USHORT = 0x0021,   // 16 bit unsigned
		T_32PUSHORT = 0x0421,   // 32 bit pointer to 16 bit unsigned
		T_64PUSHORT = 0x0621,   // 64 bit pointer to 16 bit unsigned

		//  16 bit int types

		T_INT2 = 0x0072,   // 16 bit signed int
		T_32PINT2 = 0x0472,   // 32 bit pointer to 16 bit signed int
		T_64PINT2 = 0x0672,   // 64 bit pointer to 16 bit signed int

		T_UINT2 = 0x0073,   // 16 bit unsigned int
		T_32PUINT2 = 0x0473,   // 32 bit pointer to 16 bit unsigned int
		T_64PUINT2 = 0x0673,   // 64 bit pointer to 16 bit unsigned int

		//  32 bit long types

		T_LONG = 0x0012,   // 32 bit signed
		T_ULONG = 0x0022,   // 32 bit unsigned
		T_32PLONG = 0x0412,   // 32 bit pointer to 32 bit signed
		T_32PULONG = 0x0422,   // 32 bit pointer to 32 bit unsigned
		T_64PLONG = 0x0612,   // 64 bit pointer to 32 bit signed
		T_64PULONG = 0x0622,   // 64 bit pointer to 32 bit unsigned

		//  32 bit int types

		T_INT4 = 0x0074,   // 32 bit signed int
		T_32PINT4 = 0x0474,   // 32 bit pointer to 32 bit signed int
		T_64PINT4 = 0x0674,   // 64 bit pointer to 32 bit signed int

		T_UINT4 = 0x0075,   // 32 bit unsigned int
		T_32PUINT4 = 0x0475,   // 32 bit pointer to 32 bit unsigned int
		T_64PUINT4 = 0x0675,   // 64 bit pointer to 32 bit unsigned int

		//  64 bit quad types

		T_QUAD = 0x0013,   // 64 bit signed
		T_32PQUAD = 0x0413,   // 32 bit pointer to 64 bit signed
		T_64PQUAD = 0x0613,   // 64 bit pointer to 64 bit signed

		T_UQUAD = 0x0023,   // 64 bit unsigned
		T_32PUQUAD = 0x0423,   // 32 bit pointer to 64 bit unsigned
		T_64PUQUAD = 0x0623,   // 64 bit pointer to 64 bit unsigned

		//  64 bit int types

		T_INT8 = 0x0076,   // 64 bit signed int
		T_32PINT8 = 0x0476,   // 32 bit pointer to 64 bit signed int
		T_64PINT8 = 0x0676,   // 64 bit pointer to 64 bit signed int

		T_UINT8 = 0x0077,   // 64 bit unsigned int
		T_32PUINT8 = 0x0477,   // 32 bit pointer to 64 bit unsigned int
		T_64PUINT8 = 0x0677,   // 64 bit pointer to 64 bit unsigned int

		//  128 bit octet types

		T_OCT = 0x0014,   // 128 bit signed
		T_32POCT = 0x0414,   // 32 bit pointer to 128 bit signed
		T_64POCT = 0x0614,   // 64 bit pointer to 128 bit signed

		T_UOCT = 0x0024,   // 128 bit unsigned
		T_32PUOCT = 0x0424,   // 32 bit pointer to 128 bit unsigned
		T_64PUOCT = 0x0624,   // 64 bit pointer to 128 bit unsigned

		//  128 bit int types

		T_INT16 = 0x0078,   // 128 bit signed int
		T_32PINT16 = 0x0478,   // 32 bit pointer to 128 bit signed int
		T_64PINT16 = 0x0678,   // 64 bit pointer to 128 bit signed int

		T_UINT16 = 0x0079,   // 128 bit unsigned int
		T_32PUINT16 = 0x0479,   // 32 bit pointer to 128 bit unsigned int
		T_64PUINT16 = 0x0679,   // 64 bit pointer to 128 bit unsigned int

		//  32 bit real types

		T_REAL32 = 0x0040,   // 32 bit real
		T_32PREAL32 = 0x0440,   // 32 bit pointer to 32 bit real
		T_64PREAL32 = 0x0640,   // 64 bit pointer to 32 bit real

		//  64 bit real types

		T_REAL64 = 0x0041,   // 64 bit real
		T_32PREAL64 = 0x0441,   // 32 bit pointer to 64 bit real
		T_64PREAL64 = 0x0641,   // 64 bit pointer to 64 bit real

		//  80 bit real types

		T_REAL80 = 0x0042,   // 80 bit real
		T_32PREAL80 = 0x0442,   // 32 bit pointer to 80 bit real
		T_64PREAL80 = 0x0642,   // 64 bit pointer to 80 bit real

		//  128 bit real types
		//NOTICE: added by YILIN
		T_REAL128 = 0x0043,   // 128 bit real
		T_32PREAL128 = 0x0443,   // 32 bit pointer to 128 bit real
		T_64PREAL128 = 0x0643,   // 64 bit pointer to 128 bit real

		//NOTICE: added by YILIN
		T_REAL256 = 0x0044,   // 256 bit real
		T_32PREAL256 = 0x0444,   // 32 bit pointer to 256 bit real
		T_64PREAL256 = 0x0644,   // 64 bit pointer to 256 bit real

		//NOTICE: added by YILIN
		T_REAL512 = 0x0045,   // 512 bit real
		T_32PREAL512 = 0x0445,   // 32 bit pointer to 512 bit real
		T_64PREAL512 = 0x0645,   // 64 bit pointer to 512 bit real



		//  32 bit complex types

		T_CPLX32 = 0x0050,   // 32 bit complex
		T_32PCPLX32 = 0x0450,   // 32 bit pointer to 32 bit complex
		T_64PCPLX32 = 0x0650,   // 64 bit pointer to 32 bit complex

		//  64 bit complex types

		T_CPLX64 = 0x0051,   // 64 bit complex
		T_32PCPLX64 = 0x0451,   // 32 bit pointer to 64 bit complex
		T_64PCPLX64 = 0x0651,   // 64 bit pointer to 64 bit complex

		//  80 bit complex types

		T_CPLX80 = 0x0052,   // 80 bit complex
		T_32PCPLX80 = 0x0452,   // 32 bit pointer to 80 bit complex
		T_64PCPLX80 = 0x0652,   // 64 bit pointer to 80 bit complex

		//  128 bit complex types

		T_CPLX128 = 0x0053,   // 128 bit complex
		T_32PCPLX128 = 0x0453,   // 32 bit pointer to 128 bit complex
		T_64PCPLX128 = 0x0653,   // 64 bit pointer to 128 bit complex

		//  boolean types

		T_BOOL08 = 0x0030,   // 8 bit boolean
		T_32PBOOL08 = 0x0430,   // 32 bit pointer to 8 bit boolean
		T_64PBOOL08 = 0x0630,   // 64 bit pointer to 8 bit boolean

		T_BOOL16 = 0x0031,   // 16 bit boolean
		T_32PBOOL16 = 0x0431,   // 32 bit pointer to 18 bit boolean
		T_64PBOOL16 = 0x0631,   // 64 bit pointer to 18 bit boolean

		T_BOOL32 = 0x0032,   // 32 bit boolean
		T_32PBOOL32 = 0x0432,   // 32 bit pointer to 32 bit boolean
		T_64PBOOL32 = 0x0632,   // 64 bit pointer to 32 bit boolean

		T_BOOL64 = 0x0033,   // 64 bit boolean
		T_32PBOOL64 = 0x0433,   // 32 bit pointer to 64 bit boolean
		T_64PBOOL64 = 0x0633,   // 64 bit pointer to 64 bit boolean
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct TYPE_ENUM_
	{
		public TYPE_ENUM type;
	}

	//  No leaf index can have a value of 0x0000.  The leaf indices are
	//  separated into ranges depending upon the use of the type record.
	//  The second range is for the type records that are directly referenced
	//  in symbols. The first range is for type records that are not
	//  referenced by symbols but instead are referenced by other type
	//  records.  All type records must have a starting leaf index in these
	//  first two ranges.  The third range of leaf indices are used to build
	//  up complex lists such as the field list of a class type record.  No
	//  type record can begin with one of the leaf indices. The fourth ranges
	//  of type indices are used to represent numeric data in a symbol or
	//  type record. These leaf indices are greater than 0x8000.  At the
	//  point that type or symbol processor is expecting a numeric field, the
	//  next two bytes in the type record are examined.  If the value is less
	//  than 0x8000, then the two bytes contain the numeric value.  If the
	//  value is greater than 0x8000, then the data follows the leaf index in
	//  a format specified by the leaf index. The final range of leaf indices
	//  are used to force alignment of subfields within a complex type record..
	//
	public enum LEAF : uint
	{
		LF_NONE = 0,
		// leaf indices starting records but referenced from symbol records
		LF_VTSHAPE = 0x000a,
		LF_COBOL1 = 0x000c,
		LF_LABEL = 0x000e,
		LF_NULL = 0x000f,
		LF_NOTTRAN = 0x0010,
		LF_ENDPRECOMP = 0x0014,       // not referenced from symbol
		LF_TYPESERVER_ST = 0x0016,       // not referenced from symbol

		// leaf indices starting records but referenced only from type records

		LF_LIST = 0x0203,
		LF_REFSYM = 0x020c,

		LF_ENUMERATE_ST = 0x0403,

		// 32-bit type index versions of leaves, all have the 0x1000 bit set
		//
		LF_TI16_MAX = 0x1000,

		LF_MODIFIER = 0x1001,
		LF_POINTER = 0x1002,
		LF_ARRAY_ST = 0x1003,
		LF_CLASS_ST = 0x1004,
		LF_STRUCTURE_ST = 0x1005,
		LF_UNION_ST = 0x1006,
		LF_ENUM_ST = 0x1007,
		LF_PROCEDURE = 0x1008,
		LF_MFUNCTION = 0x1009,
		LF_COBOL0 = 0x100a,
		LF_BARRAY = 0x100b,
		LF_DIMARRAY_ST = 0x100c,
		LF_VFTPATH = 0x100d,
		LF_PRECOMP_ST = 0x100e,       // not referenced from symbol
		LF_OEM = 0x100f,       // oem definable type string
		LF_ALIAS_ST = 0x1010,       // alias (typedef) type
		LF_OEM2 = 0x1011,       // oem definable type string

		// leaf indices starting records but referenced only from type records

		LF_SKIP = 0x1200,
		LF_ARGLIST = 0x1201,
		LF_DEFARG_ST = 0x1202,
		LF_FIELDLIST = 0x1203,
		LF_DERIVED = 0x1204,
		LF_BITFIELD = 0x1205,
		LF_METHODLIST = 0x1206,
		LF_DIMCONU = 0x1207,
		LF_DIMCONLU = 0x1208,
		LF_DIMVARU = 0x1209,
		LF_DIMVARLU = 0x120a,

		LF_BCLASS = 0x1400,
		LF_VBCLASS = 0x1401,
		LF_IVBCLASS = 0x1402,
		LF_FRIENDFCN_ST = 0x1403,
		LF_INDEX = 0x1404,
		LF_MEMBER_ST = 0x1405,
		LF_STMEMBER_ST = 0x1406,
		LF_METHOD_ST = 0x1407,
		LF_NESTTYPE_ST = 0x1408,
		LF_VFUNCTAB = 0x1409,
		LF_FRIENDCLS = 0x140a,
		LF_ONEMETHOD_ST = 0x140b,
		LF_VFUNCOFF = 0x140c,
		LF_NESTTYPEEX_ST = 0x140d,
		LF_MEMBERMODIFY_ST = 0x140e,
		LF_MANAGED_ST = 0x140f,

		// Types w/ SZ names

		LF_ST_MAX = 0x1500,

		LF_TYPESERVER = 0x1501,       // not referenced from symbol
		LF_ENUMERATE = 0x1502,
		LF_ARRAY = 0x1503,
		LF_CLASS = 0x1504,
		LF_STRUCTURE = 0x1505,
		LF_UNION = 0x1506,
		LF_ENUM = 0x1507,
		LF_DIMARRAY = 0x1508,
		LF_PRECOMP = 0x1509,       // not referenced from symbol
		LF_ALIAS = 0x150a,       // alias (typedef) type
		LF_DEFARG = 0x150b,
		LF_FRIENDFCN = 0x150c,
		LF_MEMBER = 0x150d,
		LF_STMEMBER = 0x150e,
		LF_METHOD = 0x150f,
		LF_NESTTYPE = 0x1510,
		LF_ONEMETHOD = 0x1511,
		LF_NESTTYPEEX = 0x1512,
		LF_MEMBERMODIFY = 0x1513,
		LF_MANAGED = 0x1514,
		LF_TYPESERVER2 = 0x1515,

		LF_NUMERIC = 0x8000,
		LF_CHAR = 0x8000,
		LF_SHORT = 0x8001,
		LF_USHORT = 0x8002,
		LF_LONG = 0x8003,
		LF_ULONG = 0x8004,
		LF_REAL32 = 0x8005,
		LF_REAL64 = 0x8006,
		LF_REAL80 = 0x8007,
		LF_REAL128 = 0x8008,
		LF_QUADWORD = 0x8009,
		LF_UQUADWORD = 0x800a,
		LF_COMPLEX32 = 0x800c,
		LF_COMPLEX64 = 0x800d,
		LF_COMPLEX80 = 0x800e,
		LF_COMPLEX128 = 0x800f,
		LF_VARSTRING = 0x8010,

		//NOTICE: added by Yilin
		LF_REAL256 = 0x8011,
		LF_REAL512 = 0x8012,

		LF_OCTWORD = 0x8017,
		LF_UOCTWORD = 0x8018,

		LF_DECIMAL = 0x8019,
		LF_DATE = 0x801a,
		LF_UTF8STRING = 0x801b,

		LF_PAD0 = 0xf0,
		LF_PAD1 = 0xf1,
		LF_PAD2 = 0xf2,
		LF_PAD3 = 0xf3,
		LF_PAD4 = 0xf4,
		LF_PAD5 = 0xf5,
		LF_PAD6 = 0xf6,
		LF_PAD7 = 0xf7,
		LF_PAD8 = 0xf8,
		LF_PAD9 = 0xf9,
		LF_PAD10 = 0xfa,
		LF_PAD11 = 0xfb,
		LF_PAD12 = 0xfc,
		LF_PAD13 = 0xfd,
		LF_PAD14 = 0xfe,
		LF_PAD15 = 0xff,
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LEAF_
	{
		public LEAF leaf;
	}

	// end of leaf indices

	//  Type enum for pointer records
	//  Pointers can be one of the following types

	public enum CV_ptrtype : uint
	{
		CV_PTR_BASE_SEG = 0x03, // based on segment
		CV_PTR_BASE_VAL = 0x04, // based on value of base
		CV_PTR_BASE_SEGVAL = 0x05, // based on segment value of base
		CV_PTR_BASE_ADDR = 0x06, // based on address of base
		CV_PTR_BASE_SEGADDR = 0x07, // based on segment address of base
		CV_PTR_BASE_TYPE = 0x08, // based on type
		CV_PTR_BASE_SELF = 0x09, // based on self
		CV_PTR_NEAR32 = 0x0a, // 32 bit pointer
		CV_PTR_64 = 0x0c, // 64 bit pointer
		CV_PTR_UNUSEDPTR = 0x0d  // first unused pointer type
	}
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_ptrtype_
	{
		public CV_ptrtype CV_ptrtype;
	}

	//  Mode enum for pointers
	//  Pointers can have one of the following modes

	public enum CV_ptrmode : uint
	{
		CV_PTR_MODE_PTR = 0x00, // "normal" pointer
		CV_PTR_MODE_REF = 0x01, // reference
		CV_PTR_MODE_PMEM = 0x02, // pointer to data member
		CV_PTR_MODE_PMFUNC = 0x03, // pointer to member function
		CV_PTR_MODE_RESERVED = 0x04  // first unused pointer mode
	}
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_ptrmode_
	{
		public CV_ptrmode cv_ptrmode;	
	}

	//  enumeration for pointer-to-member types
	public enum CV_pmtype : uint
	{
		CV_PMTYPE_Undef = 0x00, // not specified (pre VC8)
		CV_PMTYPE_D_Single = 0x01, // member data, single inheritance
		CV_PMTYPE_D_Multiple = 0x02, // member data, multiple inheritance
		CV_PMTYPE_D_Virtual = 0x03, // member data, virtual inheritance
		CV_PMTYPE_D_General = 0x04, // member data, most general
		CV_PMTYPE_F_Single = 0x05, // member function, single inheritance
		CV_PMTYPE_F_Multiple = 0x06, // member function, multiple inheritance
		CV_PMTYPE_F_Virtual = 0x07, // member function, virtual inheritance
		CV_PMTYPE_F_General = 0x08, // member function, most general
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_pmtype_
	{
		public CV_pmtype cv_pmtype;
	}

	//  enumeration for method properties

	public enum CV_methodprop : uint
	{
		CV_MTvanilla = 0x00,
		CV_MTvirtual = 0x01,
		CV_MTstatic = 0x02,
		CV_MTfriend = 0x03,
		CV_MTintro = 0x04,
		CV_MTpurevirt = 0x05,
		CV_MTpureintro = 0x06
	}
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_methodprop_
	{
		public CV_methodprop cv_methodprop;
	}

	//  enumeration for virtual shape table entries

	public enum CV_VTS_desc : uint
	{
		CV_VTS_near = 0x00,
		CV_VTS_far = 0x01,
		CV_VTS_thin = 0x02,
		CV_VTS_outer = 0x03,
		CV_VTS_meta = 0x04,
		CV_VTS_near32 = 0x05,
		CV_VTS_far32 = 0x06,
		CV_VTS_unused = 0x07
	}
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_VTS_desc_
	{
		public CV_VTS_desc cv_vts_desc;
	}

	//  enumeration for LF_LABEL address modes

	public enum CV_LABEL_TYPE : uint
	{
		CV_LABEL_NEAR = 0,       // near return
		CV_LABEL_FAR = 4        // far return
	}
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_LABEL_TYPE_
	{
		public CV_LABEL_TYPE cv_label_type;
	}

	//  enumeration for LF_MODIFIER values

	public enum CV_modifier : uint
	{
		MOD_const = 0x0001,
		MOD_volatile = 0x0002,
		MOD_unaligned = 0x0004,
	}
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_modifier_
	{
		public CV_modifier cv_modifier;
	}

	//  bit field structure describing class/struct/union/enum properties

	public enum CV_prop : uint
	{
		packed = 0x0001,   // true if structure is packed
		ctor = 0x0002,   // true if constructors or destructors present
		ovlops = 0x0004,   // true if overloaded operators present
		isnested = 0x0008,   // true if this is a nested class
		cnested = 0x0010,   // true if this class contains nested types
		opassign = 0x0020,   // true if overloaded assignment (=)
		opcast = 0x0040,   // true if casting methods
		fwdref = 0x0080,   // true if forward reference (incomplete defn)
		scoped = 0x0100,   // scoped definition
	}
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_prop_
	{
		public CV_prop cv_prop;
	}

	//  class field attribute

	public enum CV_fldattr : uint
	{
		access = 0x0003,   // access protection CV_access_t
		mprop = 0x001c,   // method properties CV_methodprop_t
		pseudo = 0x0020,   // compiler generated fcn and does not exist
		noinherit = 0x0040,   // true if class cannot be inherited
		noconstruct = 0x0080,   // true if class cannot be constructed
		compgenx = 0x0100,   // compiler generated fcn and does exist
	}
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_fldattr_
	{
		public CV_fldattr cv_fldattr;
	}

	//  Structures to access to the type records

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct TYPTYPE
	{
		public ushort len;
		public ushort leaf;
		// byte data[];

		//  char *NextType (char * pType) {
		//  return (pType + ((TYPTYPE *)pType)->len + sizeof(ushort));
		//  }
	}          // general types record

	//  memory representation of pointer to member.  These representations are
	//  indexed by the enumeration above in the LF_POINTER record

	//  representation of a 32 bit pointer to data for a class with
	//  or without virtual functions and no virtual bases

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_PDMR32_NVVFCN
	{
		public int mdisp;      // displacement to data (NULL = 0x80000000)
	}

	//  representation of a 32 bit pointer to data for a class
	//  with virtual bases

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_PDMR32_VBASE
	{
		public int mdisp;      // displacement to data
		public int pdisp;      // this pointer displacement
		public int vdisp;      // vbase table displacement
							// NULL = (,,0xffffffff)
	}

	//  representation of a 32 bit pointer to member function for a
	//  class with no virtual bases and a single address point

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_PMFR32_NVSA
	{
		public uint off;        // near address of function (NULL = 0L)
	}

	//  representation of a 32 bit pointer to member function for a
	//  class with no virtual bases and multiple address points

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_PMFR32_NVMA
	{
		public uint off;        // near address of function (NULL = 0L,x)
		public int disp;
	}

	//  representation of a 32 bit pointer to member function for a
	//  class with virtual bases

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_PMFR32_VBASE
	{
		public uint off;        // near address of function (NULL = 0L,x,x,x)
		public int mdisp;      // displacement to data
		public int pdisp;      // this pointer displacement
		public int vdisp;      // vbase table displacement
	}

	//////////////////////////////////////////////////////////////////////////////
	//
	//  The following type records are basically variant records of the
	//  above structure.  The "ushort leaf" of the above structure and
	//  the "ushort leaf" of the following type definitions are the same
	//  symbol.
	//

	//  Notes on alignment
	//  Alignment of the fields in most of the type records is done on the
	//  basis of the TYPTYPE record base.  That is why in most of the lf*
	//  records that the type is located on what appears to
	//  be a offset mod 4 == 2 boundary.  The exception to this rule are those
	//  records that are in a list (lfFieldList, lfMethodList), which are
	//  aligned to their own bases since they don't have the length field
	//

	//  Type record for LF_MODIFIER

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafModifier
	{
		// internal ushort leaf;      // LF_MODIFIER [TYPTYPE]
		public uint type;  // (type index) modified type
		public ushort attr;  // modifier attribute modifier_t CV_modifier
	}

	//  type record for LF_POINTER
	public enum LeafPointerAttr : uint
	{
		ptrtype = 0x0000001f,   // ordinal specifying pointer type (CV_ptrtype)
		ptrmode = 0x000000e0,   // ordinal specifying pointer mode (CV_ptrmode)
		isflat32 = 0x00000100,   // true if 0:32 pointer
		isvolatile = 0x00000200,   // TRUE if volatile pointer
		isconst = 0x00000400,   // TRUE if const pointer
		isunaligned = 0x00000800,   // TRUE if unaligned pointer
		isrestrict = 0x00001000,   // TRUE if restricted pointer (allow agressive opts)
	}
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafPointerAttr_
	{
		public LeafPointerAttr leafpointerAttr;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafPointer
	{
		public uint utype; // (type index) type index of the underlying type
		public uint attr; // LeafPointerAttr::Enum
	}

	//  type record for LF_ARRAY

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafArray
	{
		// internal ushort leaf;      // LF_ARRAY [TYPTYPE]
		public uint elemtype;   // (type index) type index of element type
		public uint idxtype;    // (type index) type index of indexing type
							 //internal byte[] data;       // variable length data specifying size in bytes
							 //internal string name;
	}

	//  type record for LF_CLASS, LF_STRUCTURE

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafClass
	{
		// internal ushort leaf;      // LF_CLASS, LF_STRUCT [TYPTYPE]
		public ushort count;      // count of number of elements in class
		public ushort prop;   // (CV_prop_t) property attribute field (prop_t)
		public uint field;      // (type index) type index of LF_FIELD descriptor list
		public uint derived;    // (type index) type index of derived from list if not zero
		public uint vshape;     // (type index) type index of vshape table for this class
							 //internal byte[] data;       // data describing length of structure in bytes
							 //internal string name;
	}

	//  type record for LF_UNION

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafUnion
	{
		// internal ushort leaf;      // LF_UNION [TYPTYPE]
		public ushort count;      // count of number of elements in class
		public ushort prop;   // (CV_prop_t) property attribute field
		public uint field;      // (type index) type index of LF_FIELD descriptor list
							 //internal byte[] data;       // variable length data describing length of
							 //internal string name;
	}

	//  type record for LF_ALIAS

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafAlias
	{
		// internal ushort leaf;      // LF_ALIAS [TYPTYPE]
		public uint utype;      // (type index) underlying type
							 //internal string name;       // alias name
	}

	//  type record for LF_MANAGED

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafManaged
	{
		public ushort leaf;      // LF_MANAGED [TYPTYPE]
		public string name;       // utf8, zero terminated managed type name
	}

	//  type record for LF_ENUM

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafEnum
	{
		// internal ushort leaf;      // LF_ENUM [TYPTYPE]
		public ushort count;      // count of number of elements in class
		public ushort prop;   // (CV_propt_t) property attribute field
		public uint utype;      // (type index) underlying type of the enum
		public uint field;      // (type index) type index of LF_FIELD descriptor list
							 //internal string name;       // length prefixed name of enum
	}

	//  Type record for LF_PROCEDURE

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafProc
	{
		// internal ushort leaf;      // LF_PROCEDURE [TYPTYPE]
		public uint rvtype;     // (type index) type index of return value
		public byte calltype;   // calling convention (CV_call_t)
		public byte reserved;   // reserved for future use
		public ushort parmcount;  // number of parameters
		public uint arglist;    // (type index) type index of argument list
	}

	//  Type record for member function

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafMFunc
	{
		// internal ushort leaf;      // LF_MFUNCTION [TYPTYPE]
		public uint rvtype;     // (type index) type index of return value
		public uint classtype;  // (type index) type index of containing class
		public uint thistype;   // (type index) type index of this pointer (model specific)
		public byte calltype;   // calling convention (call_t)
		public byte reserved;   // reserved for future use
		public ushort parmcount;  // number of parameters
		public uint arglist;    // (type index) type index of argument list
		public int thisadjust; // this adjuster (long because pad required anyway)
	}

	//  type record for virtual function table shape

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafVTShape
	{
		// internal ushort leaf;      // LF_VTSHAPE [TYPTYPE]
		public ushort count;      // number of entries in vfunctable
							 //internal byte[] desc;       // 4 bit (CV_VTS_desc) descriptors
	}

	//  type record for cobol0

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafCobol0
	{
		//public ushort leaf;      // LF_COBOL0 [TYPTYPE]
		public uint type;       // (type index) parent type record index
		//internal byte[] data;
	}

	//  type record for cobol1

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafCobol1
	{
		//public ushort leaf;      // LF_COBOL1 [TYPTYPE]
		//internal byte[] data;
	}

	//  type record for basic array

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafBArray
	{
		//public ushort leaf;      // LF_BARRAY [TYPTYPE]
		public uint utype;      // (type index) type index of underlying type
	}

	//  type record for assembler labels

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafLabel
	{
		//public ushort leaf;      // LF_LABEL [TYPTYPE]
		public ushort mode;       // addressing mode of label
	}

	//  type record for dimensioned arrays

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafDimArray
	{
		//public ushort leaf;      // LF_DIMARRAY [TYPTYPE]
		public uint utype;      // (type index) underlying type of the array
		public uint diminfo;    // (type index) dimension information
							 //internal string name;       // length prefixed name
	}

	//  type record describing path to virtual function table

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafVFTPath
	{
		//public ushort leaf;      // LF_VFTPATH [TYPTYPE]
		public uint count;      // count of number of bases in path
							 //internal uint[] bases;      // (type index) bases from root to leaf
	}

	//  type record describing inclusion of precompiled types

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafPreComp
	{
		// internal ushort leaf;      // LF_PRECOMP [TYPTYPE]
		public uint start;      // starting type index included
		public uint count;      // number of types in inclusion
		public uint signature;  // signature
							 //internal string name;       // length prefixed name of included type file
	}

	//  type record describing end of precompiled types that can be
	//  included by another file

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafEndPreComp
	{
		// internal ushort leaf;      // LF_ENDPRECOMP [TYPTYPE]
		public uint signature;  // signature
	}

	//  type record describing using of a type server

	//  description of type records that can be referenced from
	//  type records referenced by symbols

	//  type record for skip record

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafSkip
	{
		// internal ushort leaf;      // LF_SKIP [TYPTYPE]
		public uint type;       // (type index) next valid index
							 //internal byte[] data;       // pad data
	}

	//  argument list leaf

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafArgList
	{
		// internal ushort leaf;      // LF_ARGLIST [TYPTYPE]
		public uint count;      // number of arguments
							 //internal uint[] arg;        // (type index) number of arguments
	}

	//  derived class list leaf

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafDerived
	{
		// internal ushort leaf;      // LF_DERIVED [TYPTYPE]
		public uint count;      // number of arguments
							 //internal uint[] drvdcls;    // (type index) type indices of derived classes
	}

	//  leaf for default arguments

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafDefArg
	{
		// internal ushort leaf;      // LF_DEFARG [TYPTYPE]
		public uint type;       // (type index) type of resulting expression
							 //internal byte[] expr;       // length prefixed expression string
	}

	//  list leaf
	//      This list should no longer be used because the utilities cannot
	//      verify the contents of the list without knowing what type of list
	//      it is.  New specific leaf indices should be used instead.

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafList
	{
		// internal ushort leaf;      // LF_LIST [TYPTYPE]
		//internal byte[] data;       // data format specified by indexing type
	}

	//  field list leaf
	//  This is the header leaf for a complex list of class and structure
	//  subfields.

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafFieldList
	{
		// internal ushort leaf;      // LF_FIELDLIST [TYPTYPE]
		//internal char[] data;       // field list sub lists
	}

	//  type record for non-static methods and friends in overloaded method list

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct mlMethod
	{
		public ushort attr;       // (CV_fldattr_t) method attribute
		public ushort pad0;       // internal padding, must be 0
		public uint index;      // (type index) index to type record for procedure
							 //internal uint[] vbaseoff;   // offset in vfunctable if intro virtual
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafMethodList
	{
		// internal ushort leaf;      // LF_METHODLIST [TYPTYPE]
		//internal byte[] mList;      // really a mlMethod type
	}

	//  type record for LF_BITFIELD

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafBitfield
	{
		// internal ushort leaf;      // LF_BITFIELD [TYPTYPE]
		public uint type;       // (type index) type of bitfield
		public byte length;
		public byte position;
	}

	//  type record for dimensioned array with constant bounds

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafDimCon
	{
		// internal ushort leaf;      // LF_DIMCONU or LF_DIMCONLU [TYPTYPE]
		public uint typ;        // (type index) type of index
		public ushort rank;       // number of dimensions
							 //internal byte[] dim;        // array of dimension information with
							 // either upper bounds or lower/upper bound
	}

	//  type record for dimensioned array with variable bounds

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafDimVar
	{
		// internal ushort leaf;      // LF_DIMVARU or LF_DIMVARLU [TYPTYPE]
		public uint rank;       // number of dimensions
		public uint typ;        // (type index) type of index
							 //internal uint[] dim;        // (type index) array of type indices for either
							 // variable upper bound or variable
							 // lower/upper bound.  The count of type
							 // indices is rank or rank*2 depending on
							 // whether it is LFDIMVARU or LF_DIMVARLU.
							 // The referenced types must be
							 // LF_REFSYM or T_VOID
	}

	//  type record for referenced symbol

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafRefSym
	{
		// internal ushort leaf;      // LF_REFSYM [TYPTYPE]
		//internal byte[] Sym;        // copy of referenced symbol record
		// (including length)
	}

	//  the following are numeric leaves.  They are used to indicate the
	//  size of the following variable length data.  When the numeric
	//  data is a single byte less than 0x8000, then the data is output
	//  directly.  If the data is more the 0x8000 or is a negative value,
	//  then the data is preceded by the proper index.
	//

	//  signed character leaf

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafChar
	{
		// internal ushort leaf;      // LF_CHAR [TYPTYPE]
		public byte val;        // signed 8-bit value
	}

	//  signed short leaf

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafShort
	{
		// internal ushort leaf;      // LF_SHORT [TYPTYPE]
		public short val;        // signed 16-bit value
	}

	//  ushort leaf

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafUShort
	{
		// internal ushort leaf;      // LF_ushort [TYPTYPE]
		public ushort val;        // unsigned 16-bit value
	}

	//  signed (32-bit) long leaf

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafLong
	{
		// internal ushort leaf;      // LF_LONG [TYPTYPE]
		public int val;        // signed 32-bit value
	}

	//  uint    leaf

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafULong
	{
		// internal ushort leaf;      // LF_ULONG [TYPTYPE]
		public uint val;        // unsigned 32-bit value
	}

	//  signed quad leaf

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafQuad
	{
		// internal ushort leaf;      // LF_QUAD [TYPTYPE]
		public long val;        // signed 64-bit value
	}

	//  unsigned quad leaf

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafUQuad
	{
		// internal ushort leaf;      // LF_UQUAD [TYPTYPE]
		public ulong val;        // unsigned 64-bit value
	}

	//  signed int128 leaf

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafOct
	{
		// internal ushort leaf;      // LF_OCT [TYPTYPE]
		public ulong val0;
		public ulong val1;       // signed 128-bit value
	}

	//  unsigned int128 leaf

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafUOct
	{
		// internal ushort leaf;      // LF_UOCT [TYPTYPE]
		public ulong val0;
		public ulong val1;       // unsigned 128-bit value
	}

	//  real 32-bit leaf

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafReal32
	{
		// internal ushort leaf;      // LF_REAL32 [TYPTYPE]
		public float val;        // 32-bit real value
	}

	//  real 64-bit leaf

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafReal64
	{
		// internal ushort leaf;      // LF_REAL64 [TYPTYPE]
		public double val;        // 64-bit real value
	}

	//  real 80-bit leaf

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafReal80
	{
		// internal ushort leaf;      // LF_REAL80 [TYPTYPE]
		public float10 val;        // real 80-bit value
	}

	//  real 128-bit leaf

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafReal128
	{
		// internal ushort leaf;      // LF_REAL128 [TYPTYPE]
		public ulong val0;
		public ulong val1;       // real 128-bit value
	}

	//  complex 32-bit leaf

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafCmplx32
	{
		// internal ushort leaf;      // LF_COMPLEX32 [TYPTYPE]
		public float val_real;   // real component
		public float val_imag;   // imaginary component
	}

	//  complex 64-bit leaf

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafCmplx64
	{
		// internal ushort leaf;      // LF_COMPLEX64 [TYPTYPE]
		public double val_real;   // real component
		public double val_imag;   // imaginary component
	}

	//  complex 80-bit leaf

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafCmplx80
	{
		// internal ushort leaf;      // LF_COMPLEX80 [TYPTYPE]
		public float10 val_real;   // real component
		public float10 val_imag;   // imaginary component
	}

	//  complex 128-bit leaf

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafCmplx128
	{
		// internal ushort leaf;      // LF_COMPLEX128 [TYPTYPE]
		public ulong val0_real;
		public ulong val1_real;  // real component
		public ulong val0_imag;
		public ulong val1_imag;  // imaginary component
	}

	//  variable length numeric field

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafVarString
	{
		// internal ushort leaf;      // LF_VARSTRING [TYPTYPE]
		public ushort len;        // length of value in bytes
							 //internal byte[] value;      // value
	}

	//  index leaf - contains type index of another leaf
	//  a major use of this leaf is to allow the compilers to emit a
	//  long complex list (LF_FIELD) in smaller pieces.

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafIndex
	{
		// internal ushort leaf;      // LF_INDEX [TYPTYPE]
		public ushort pad0;       // internal padding, must be 0
		public uint index;      // (type index) type index of referenced leaf
	}

	//  subfield record for base class field

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafBClass
	{
		// internal ushort leaf;      // LF_BCLASS [TYPTYPE]
		public ushort attr;       // (CV_fldattr_t) attribute
		public uint index;      // (type index) type index of base class
							 //internal byte[] offset;     // variable length offset of base within class
	}

	//  subfield record for direct and indirect virtual base class field

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafVBClass
	{
		// internal ushort leaf;      // LF_VBCLASS | LV_IVBCLASS [TYPTYPE]
		public ushort attr;       // (CV_fldattr_t) attribute
		public uint index;      // (type index) type index of direct virtual base class
		public uint vbptr;      // (type index) type index of virtual base pointer
							 //internal byte[] vbpoff;     // virtual base pointer offset from address point
							 // followed by virtual base offset from vbtable
	}

	//  subfield record for friend class

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafFriendCls
	{
		// internal ushort leaf;      // LF_FRIENDCLS [TYPTYPE]
		public ushort pad0;       // internal padding, must be 0
		public uint index;      // (type index) index to type record of friend class
	}

	//  subfield record for friend function

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafFriendFcn
	{
		// internal ushort leaf;      // LF_FRIENDFCN [TYPTYPE]
		public ushort pad0;       // internal padding, must be 0
		public uint index;      // (type index) index to type record of friend function
							 //internal string name;       // name of friend function
	}

	//  subfield record for non-static data members

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafMember
	{
		// internal ushort leaf;      // LF_MEMBER [TYPTYPE]
		public ushort attr;       // (CV_fldattr_t)attribute mask
		public uint index;      // (type index) index of type record for field
							 //internal byte[] offset;     // variable length offset of field
							 //internal string name;       // length prefixed name of field
	}

	//  type record for static data members

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafSTMember
	{
		// internal ushort leaf;      // LF_STMEMBER [TYPTYPE]
		public ushort attr;       // (CV_fldattr_t) attribute mask
		public uint index;      // (type index) index of type record for field
							 //internal string name;       // length prefixed name of field
	}

	//  subfield record for virtual function table pointer

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafVFuncTab
	{
		// internal ushort leaf;      // LF_VFUNCTAB [TYPTYPE]
		public ushort pad0;       // internal padding, must be 0
		public uint type;       // (type index) type index of pointer
	}

	//  subfield record for virtual function table pointer with offset

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafVFuncOff
	{
		// internal ushort leaf;      // LF_VFUNCOFF [TYPTYPE]
		public ushort pad0;       // internal padding, must be 0.
		public uint type;       // (type index) type index of pointer
		public int offset;     // offset of virtual function table pointer
	}

	//  subfield record for overloaded method list

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafMethod
	{
		// internal ushort leaf;      // LF_METHOD [TYPTYPE]
		public ushort count;      // number of occurrences of function
		public uint mList;      // (type index) index to LF_METHODLIST record
							 //internal string name;       // length prefixed name of method
	}

	//  subfield record for nonoverloaded method

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafOneMethod
	{
		// internal ushort leaf;      // LF_ONEMETHOD [TYPTYPE]
		public ushort attr;       // (CV_fldattr_t) method attribute
		public uint index;      // (type index) index to type record for procedure
							 //internal uint[] vbaseoff;   // offset in vfunctable if intro virtual
							 //internal string name;
	}

	//  subfield record for enumerate

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafEnumerate
	{
		// internal ushort leaf;      // LF_ENUMERATE [TYPTYPE]
		public ushort attr;       // (CV_fldattr_t) access
							 //internal byte[] value;      // variable length value field
							 //internal string name;
	}

	//  type record for nested (scoped) type definition

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafNestType
	{
		// internal ushort leaf;      // LF_NESTTYPE [TYPTYPE]
		public ushort pad0;       // internal padding, must be 0
		public uint index;      // (type index) index of nested type definition
							 //internal string name;       // length prefixed type name
	}

	//  type record for nested (scoped) type definition, with attributes
	//  new records for vC v5.0, no need to have 16-bit ti versions.

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafNestTypeEx
	{
		// internal ushort leaf;      // LF_NESTTYPEEX [TYPTYPE]
		public ushort attr;       // (CV_fldattr_t) member access
		public uint index;      // (type index) index of nested type definition
							 //internal string name;       // length prefixed type name
	}

	//  type record for modifications to members

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafMemberModify
	{
		// internal ushort leaf;      // LF_MEMBERMODIFY [TYPTYPE]
		public ushort attr;       // (CV_fldattr_t) the new attributes
		public uint index;      // (type index) index of base class type definition
							 //internal string name;       // length prefixed member name
	}

	//  type record for pad leaf
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct LeafPad
	{
		public byte leaf;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct TypeRecord
	{
		public ushort length;
		public ushort leafType;
	}

	public enum Subsection : uint
	{
		Ignore = 0x80000000,   // if this bit is set in a subsection type then ignore the subsection contents)
		Symbols = 0xF1,
		Lines = 0xF2,
		StringTable = 0xF3,
		FileChecksums = 0xF4,
		FrameData = 0xF5,
		_Unknown1 = 0xF6,           // Encountered, don't know what it is though
	}
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Subsection_
	{
		public Subsection subsection;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CVFileChecksum
	{
		public uint name;           // Index of name in name table.
		public byte len;            // Hash length
		public byte type;           // Hash type
	}

	public enum SymbolDefs : uint
	{
		S_END = 0x0006,  // Block, procedure, "with" or thunk end
		S_OEM = 0x0404,  // OEM defined symbol

		S_REGISTER_ST = 0x1001,  // Register variable
		S_CONSTANT_ST = 0x1002,  // constant symbol
		S_UDT_ST = 0x1003,  // User defined type
		S_COBOLUDT_ST = 0x1004,  // special UDT for cobol that does not symbol pack
		S_MANYREG_ST = 0x1005,  // multiple register variable
		S_BPREL32_ST = 0x1006,  // BP-relative
		S_LDATA32_ST = 0x1007,  // Module-local symbol
		S_GDATA32_ST = 0x1008,  // Global data symbol
		S_PUB32_ST = 0x1009,  // a internal symbol (CV internal reserved)
		S_LPROC32_ST = 0x100a,  // Local procedure start
		S_GPROC32_ST = 0x100b,  // Global procedure start
		S_VFTABLE32 = 0x100c,  // address of virtual function table
		S_REGREL32_ST = 0x100d,  // register relative address
		S_LTHREAD32_ST = 0x100e,  // local thread storage
		S_GTHREAD32_ST = 0x100f,  // global thread storage

		S_LPROCMIPS_ST = 0x1010,  // Local procedure start
		S_GPROCMIPS_ST = 0x1011,  // Global procedure start

		// new symbol records for edit and continue information

		S_FRAMEPROC = 0x1012,  // extra frame and proc information
		S_COMPILE2_ST = 0x1013,  // extended compile flags and info

		// new symbols necessary for 16-bit enumerates of IA64 registers
		// and IA64 specific symbols

		S_MANYREG2_ST = 0x1014,  // multiple register variable
		S_LPROCIA64_ST = 0x1015,  // Local procedure start (IA64)
		S_GPROCIA64_ST = 0x1016,  // Global procedure start (IA64)

		// Local symbols for IL
		S_LOCALSLOT_ST = 0x1017,  // local IL sym with field for local slot index
		S_PARAMSLOT_ST = 0x1018,  // local IL sym with field for parameter slot index

		S_ANNOTATION = 0x1019,  // Annotation string literals

		// symbols to support managed code debugging
		S_GMANPROC_ST = 0x101a,  // Global proc
		S_LMANPROC_ST = 0x101b,  // Local proc
		S_RESERVED1 = 0x101c,  // reserved
		S_RESERVED2 = 0x101d,  // reserved
		S_RESERVED3 = 0x101e,  // reserved
		S_RESERVED4 = 0x101f,  // reserved
		S_LMANDATA_ST = 0x1020,
		S_GMANDATA_ST = 0x1021,
		S_MANFRAMEREL_ST = 0x1022,
		S_MANREGISTER_ST = 0x1023,
		S_MANSLOT_ST = 0x1024,
		S_MANMANYREG_ST = 0x1025,
		S_MANREGREL_ST = 0x1026,
		S_MANMANYREG2_ST = 0x1027,
		S_MANTYPREF = 0x1028,  // Index for type referenced by name from metadata
		S_UNAMESPACE_ST = 0x1029,  // Using namespace

		// Symbols w/ SZ name fields. All name fields contain utf8 encoded strings.
		S_ST_MAX = 0x1100,  // starting point for SZ name symbols

		S_OBJNAME = 0x1101,  // path to object file name
		S_THUNK32 = 0x1102,  // Thunk Start
		S_BLOCK32 = 0x1103,  // block start
		S_WITH32 = 0x1104,  // with start
		S_LABEL32 = 0x1105,  // code label
		S_REGISTER = 0x1106,  // Register variable
		S_CONSTANT = 0x1107,  // constant symbol
		S_UDT = 0x1108,  // User defined type
		S_COBOLUDT = 0x1109,  // special UDT for cobol that does not symbol pack
		S_MANYREG = 0x110a,  // multiple register variable
		S_BPREL32 = 0x110b,  // BP-relative
		S_LDATA32 = 0x110c,  // Module-local symbol
		S_GDATA32 = 0x110d,  // Global data symbol
		S_PUB32 = 0x110e,  // a internal symbol (CV internal reserved)
		S_LPROC32 = 0x110f,  // Local procedure start
		S_GPROC32 = 0x1110,  // Global procedure start
		S_REGREL32 = 0x1111,  // register relative address
		S_LTHREAD32 = 0x1112,  // local thread storage
		S_GTHREAD32 = 0x1113,  // global thread storage

		S_LPROCMIPS = 0x1114,  // Local procedure start
		S_GPROCMIPS = 0x1115,  // Global procedure start
		S_COMPILE2 = 0x1116,  // extended compile flags and info
		S_MANYREG2 = 0x1117,  // multiple register variable
		S_LPROCIA64 = 0x1118,  // Local procedure start (IA64)
		S_GPROCIA64 = 0x1119,  // Global procedure start (IA64)
		S_LOCALSLOT = 0x111a,  // local IL sym with field for local slot index
		S_SLOT = S_LOCALSLOT,  // alias for LOCALSLOT
		S_PARAMSLOT = 0x111b,  // local IL sym with field for parameter slot index

		// symbols to support managed code debugging
		S_LMANDATA = 0x111c,
		S_GMANDATA = 0x111d,
		S_MANFRAMEREL = 0x111e,
		S_MANREGISTER = 0x111f,
		S_MANSLOT = 0x1120,
		S_MANMANYREG = 0x1121,
		S_MANREGREL = 0x1122,
		S_MANMANYREG2 = 0x1123,
		S_UNAMESPACE = 0x1124,  // Using namespace

		// ref symbols with name fields
		S_PROCREF = 0x1125,  // Reference to a procedure
		S_DATAREF = 0x1126,  // Reference to data
		S_LPROCREF = 0x1127,  // Local Reference to a procedure
		S_ANNOTATIONREF = 0x1128,  // Reference to an S_ANNOTATION symbol
		S_TOKENREF = 0x1129,  // Reference to one of the many MANPROCSYM's

		// continuation of managed symbols
		S_GMANPROC = 0x112a,  // Global proc
		S_LMANPROC = 0x112b,  // Local proc

		// short, light-weight thunks
		S_TRAMPOLINE = 0x112c,  // trampoline thunks
		S_MANCONSTANT = 0x112d,  // constants with metadata type info

		// native attributed local/parms
		S_ATTR_FRAMEREL = 0x112e,  // relative to virtual frame ptr
		S_ATTR_REGISTER = 0x112f,  // stored in a register
		S_ATTR_REGREL = 0x1130,  // relative to register (alternate frame ptr)
		S_ATTR_MANYREG = 0x1131,  // stored in >1 register

		// Separated code (from the compiler) support
		S_SEPCODE = 0x1132,

		S_LOCAL = 0x1133,  // defines a local symbol in optimized code
		S_DEFRANGE = 0x1134,  // defines a single range of addresses in which symbol can be evaluated
		S_DEFRANGE2 = 0x1135,  // defines ranges of addresses in which symbol can be evaluated

		S_SECTION = 0x1136,  // A COFF section in a PE executable
		S_COFFGROUP = 0x1137,  // A COFF group
		S_EXPORT = 0x1138,  // A export

		S_CALLSITEINFO = 0x1139,  // Indirect call site information
		S_FRAMECOOKIE = 0x113a,  // Security cookie information

		S_DISCARDED = 0x113b,  // Discarded by LINK /OPT:REF (experimental, see richards)

		S_RECTYPE_MAX,              // one greater than last
		S_RECTYPE_LAST = S_RECTYPE_MAX - 1,
	}
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SymbolDefs_
	{
		public SymbolDefs symboldefs;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	// Struct for S_LPROC32 and S_GPROC32
	public struct ProcSym32
	{
		// internal ushort reclen;    // Record length [SYMTYPE]
		// internal ushort rectyp;    // S_GPROC32 or S_LPROC32
		public uint parent;     // pointer to the parent
		public uint end;        // pointer to this blocks end
		public uint next;       // pointer to next symbol
		public uint len;        // Proc length
		public uint dbgStart;   // Debug start offset
		public uint dbgEnd;     // Debug end offset
		public uint typind;     // (type index) Type index
		public uint off;
		public ushort seg;
		public byte flags;      // (CV_PROCFLAGS) Proc flags
							//internal string name;       // Length-prefixed name
	}

	// Struct for S_THUNK32
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ThunkSym32
	{
		// internal ushort reclen;    // Record length [SYMTYPE]
		// internal ushort rectyp;    // S_THUNK32
		public uint parent;     // pointer to the parent
		public uint end;        // pointer to this blocks end
		public uint next;       // pointer to next symbol
		public uint off;
		public ushort seg;
		public ushort len;        // length of thunk
		public byte ord;        // THUNK_ORDINAL specifying type of thunk
							//internal string name;       // Length-prefixed name
							//internal byte[] variant;    // variant portion of thunk
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct BlockSym32
	{
		public uint parent;
		public uint end;
		public uint len;
		public uint off;
		public ushort seg;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_LineSection
	{
		public uint off;
		public ushort sec;
		public ushort flags;
		public uint cod;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_SourceFile
	{
		public uint index;          // Index to file in checksum section.
		public uint count;          // Number of CV_Line records.
		public uint linsiz;         // Size of CV_Line records.
	}

	public enum CV_Line_Flags : uint
	{
		linenumStart = 0x00ffffff,   // line where statement/expression starts
		deltaLineEnd = 0x7f000000,   // delta to line where statement ends (optional)
		fStatement = 0x80000000,   // true if a statement linenumber, else an expression line num
	}
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_Line_Flags_
	{
		public CV_Line_Flags cv_line_flags;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_Line
	{
		public uint offset;         // Offset to start of code bytes for line number
		public uint flags;          // (CV_Line_Flags)
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct CV_Column
	{
		public ushort start;
		public ushort end;
	}
    [StructLayout(LayoutKind.Sequential,Pack =1)]
	public struct GlobalRecord
	{
		public ushort leafType;
		public uint symType;
		public uint offset;
		public ushort segment;
	}

	public enum FPOFlags : uint
	{
		SEH = 1,
		CPPEH = 2,
		fnStart = 4
	}
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct FPO_DATA
	{
		public uint ulOffStart;
		public uint cbProcSize;
		public uint cdwLocals;
		public ushort cdwParams;

		public ushort attributes;
		public ushort misc;
		//public ushort cbProlog_8;//  :8;
		//public ushort cbRegs_3;//  :3;
		//public ushort fHasSEH_1;//  :1;
		//public ushort fUseBP_1;//  :1;
		//public ushort reserved_1;//  :1;
		//public ushort cbFrame_2;//  :2;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct FPO_DATA_V2
	{
		public uint ulOffStart;
		public uint cbProcSize;
		public uint cbLocals;
		public uint cbParams;
		public uint maxStack;
		public uint ProgramStringOffset;
		public ushort cbProlog;
		public ushort cbSavedRegs;
		public FPOFlags flags;
	}

	public enum SYM_ENUM:uint
	{
		S_COMPILE = 0x0001,  // Compile flags symbol
		S_REGISTER_16t = 0x0002,  // Register variable
		S_CONSTANT_16t = 0x0003,  // constant symbol
		S_UDT_16t = 0x0004,  // User defined type
		S_SSEARCH = 0x0005,  // Start Search
		S_END = 0x0006,  // Block, procedure, "with" or thunk end
		S_SKIP = 0x0007,  // Reserve symbol space in $$Symbols table
		S_CVRESERVE = 0x0008,  // Reserved symbol for CV internal use
		S_OBJNAME_ST = 0x0009,  // path to object file name
		S_ENDARG = 0x000a,  // end of argument/return list
		S_COBOLUDT_16t = 0x000b,  // special UDT for cobol that does not symbol pack
		S_MANYREG_16t = 0x000c,  // multiple register variable
		S_RETURN = 0x000d,  // return description symbol
		S_ENTRYTHIS = 0x000e,  // description of this pointer on entry

		S_BPREL16 = 0x0100,  // BP-relative
		S_LDATA16 = 0x0101,  // Module-local symbol
		S_GDATA16 = 0x0102,  // Global data symbol
		S_PUB16 = 0x0103,  // a public symbol
		S_LPROC16 = 0x0104,  // Local procedure start
		S_GPROC16 = 0x0105,  // Global procedure start
		S_THUNK16 = 0x0106,  // Thunk Start
		S_BLOCK16 = 0x0107,  // block start
		S_WITH16 = 0x0108,  // with start
		S_LABEL16 = 0x0109,  // code label
		S_CEXMODEL16 = 0x010a,  // change execution model
		S_VFTABLE16 = 0x010b,  // address of virtual function table
		S_REGREL16 = 0x010c,  // register relative address

		S_BPREL32_16t = 0x0200,  // BP-relative
		S_LDATA32_16t = 0x0201,  // Module-local symbol
		S_GDATA32_16t = 0x0202,  // Global data symbol
		S_PUB32_16t = 0x0203,  // a public symbol (CV internal reserved)
		S_LPROC32_16t = 0x0204,  // Local procedure start
		S_GPROC32_16t = 0x0205,  // Global procedure start
		S_THUNK32_ST = 0x0206,  // Thunk Start
		S_BLOCK32_ST = 0x0207,  // block start
		S_WITH32_ST = 0x0208,  // with start
		S_LABEL32_ST = 0x0209,  // code label
		S_CEXMODEL32 = 0x020a,  // change execution model
		S_VFTABLE32_16t = 0x020b,  // address of virtual function table
		S_REGREL32_16t = 0x020c,  // register relative address
		S_LTHREAD32_16t = 0x020d,  // local thread storage
		S_GTHREAD32_16t = 0x020e,  // global thread storage
		S_SLINK32 = 0x020f,  // static link for MIPS EH implementation

		S_LPROCMIPS_16t = 0x0300,  // Local procedure start
		S_GPROCMIPS_16t = 0x0301,  // Global procedure start

		// if these ref symbols have names following then the names are in ST format
		S_PROCREF_ST = 0x0400,  // Reference to a procedure
		S_DATAREF_ST = 0x0401,  // Reference to data
		S_ALIGN = 0x0402,  // Used for page alignment of symbols

		S_LPROCREF_ST = 0x0403,  // Local Reference to a procedure
		S_OEM = 0x0404,  // OEM defined symbol

		// sym records with 32-bit types embedded instead of 16-bit
		// all have 0x1000 bit set for easy identification
		// only do the 32-bit target versions since we don't really
		// care about 16-bit ones anymore.
		S_TI16_MAX = 0x1000,

		S_REGISTER_ST = 0x1001,  // Register variable
		S_CONSTANT_ST = 0x1002,  // constant symbol
		S_UDT_ST = 0x1003,  // User defined type
		S_COBOLUDT_ST = 0x1004,  // special UDT for cobol that does not symbol pack
		S_MANYREG_ST = 0x1005,  // multiple register variable
		S_BPREL32_ST = 0x1006,  // BP-relative
		S_LDATA32_ST = 0x1007,  // Module-local symbol
		S_GDATA32_ST = 0x1008,  // Global data symbol
		S_PUB32_ST = 0x1009,  // a public symbol (CV internal reserved)
		S_LPROC32_ST = 0x100a,  // Local procedure start
		S_GPROC32_ST = 0x100b,  // Global procedure start
		S_VFTABLE32 = 0x100c,  // address of virtual function table
		S_REGREL32_ST = 0x100d,  // register relative address
		S_LTHREAD32_ST = 0x100e,  // local thread storage
		S_GTHREAD32_ST = 0x100f,  // global thread storage

		S_LPROCMIPS_ST = 0x1010,  // Local procedure start
		S_GPROCMIPS_ST = 0x1011,  // Global procedure start

		S_FRAMEPROC = 0x1012,  // extra frame and proc information
		S_COMPILE2_ST = 0x1013,  // extended compile flags and info

		// new symbols necessary for 16-bit enumerates of IA64 registers
		// and IA64 specific symbols

		S_MANYREG2_ST = 0x1014,  // multiple register variable
		S_LPROCIA64_ST = 0x1015,  // Local procedure start (IA64)
		S_GPROCIA64_ST = 0x1016,  // Global procedure start (IA64)

		// Local symbols for IL
		S_LOCALSLOT_ST = 0x1017,  // local IL sym with field for local slot index
		S_PARAMSLOT_ST = 0x1018,  // local IL sym with field for parameter slot index

		S_ANNOTATION = 0x1019,  // Annotation string literals

		// symbols to support managed code debugging
		S_GMANPROC_ST = 0x101a,  // Global proc
		S_LMANPROC_ST = 0x101b,  // Local proc
		S_RESERVED1 = 0x101c,  // reserved
		S_RESERVED2 = 0x101d,  // reserved
		S_RESERVED3 = 0x101e,  // reserved
		S_RESERVED4 = 0x101f,  // reserved
		S_LMANDATA_ST = 0x1020,
		S_GMANDATA_ST = 0x1021,
		S_MANFRAMEREL_ST = 0x1022,
		S_MANREGISTER_ST = 0x1023,
		S_MANSLOT_ST = 0x1024,
		S_MANMANYREG_ST = 0x1025,
		S_MANREGREL_ST = 0x1026,
		S_MANMANYREG2_ST = 0x1027,
		S_MANTYPREF = 0x1028,  // Index for type referenced by name from metadata
		S_UNAMESPACE_ST = 0x1029,  // Using namespace

		// Symbols w/ SZ name fields. All name fields contain utf8 encoded strings.
		S_ST_MAX = 0x1100,  // starting point for SZ name symbols

		S_OBJNAME = 0x1101,  // path to object file name
		S_THUNK32 = 0x1102,  // Thunk Start
		S_BLOCK32 = 0x1103,  // block start
		S_WITH32 = 0x1104,  // with start
		S_LABEL32 = 0x1105,  // code label
		S_REGISTER = 0x1106,  // Register variable
		S_CONSTANT = 0x1107,  // constant symbol
		S_UDT = 0x1108,  // User defined type
		S_COBOLUDT = 0x1109,  // special UDT for cobol that does not symbol pack
		S_MANYREG = 0x110a,  // multiple register variable
		S_BPREL32 = 0x110b,  // BP-relative
		S_LDATA32 = 0x110c,  // Module-local symbol
		S_GDATA32 = 0x110d,  // Global data symbol
		S_PUB32 = 0x110e,  // a public symbol (CV internal reserved)
		S_LPROC32 = 0x110f,  // Local procedure start
		S_GPROC32 = 0x1110,  // Global procedure start
		S_REGREL32 = 0x1111,  // register relative address
		S_LTHREAD32 = 0x1112,  // local thread storage
		S_GTHREAD32 = 0x1113,  // global thread storage

		S_LPROCMIPS = 0x1114,  // Local procedure start
		S_GPROCMIPS = 0x1115,  // Global procedure start
		S_COMPILE2 = 0x1116,  // extended compile flags and info
		S_MANYREG2 = 0x1117,  // multiple register variable
		S_LPROCIA64 = 0x1118,  // Local procedure start (IA64)
		S_GPROCIA64 = 0x1119,  // Global procedure start (IA64)
		S_LOCALSLOT = 0x111a,  // local IL sym with field for local slot index
		S_SLOT = S_LOCALSLOT,  // alias for LOCALSLOT
		S_PARAMSLOT = 0x111b,  // local IL sym with field for parameter slot index

		// symbols to support managed code debugging
		S_LMANDATA = 0x111c,
		S_GMANDATA = 0x111d,
		S_MANFRAMEREL = 0x111e,
		S_MANREGISTER = 0x111f,
		S_MANSLOT = 0x1120,
		S_MANMANYREG = 0x1121,
		S_MANREGREL = 0x1122,
		S_MANMANYREG2 = 0x1123,
		S_UNAMESPACE = 0x1124,  // Using namespace

		// ref symbols with name fields
		S_PROCREF = 0x1125,  // Reference to a procedure
		S_DATAREF = 0x1126,  // Reference to data
		S_LPROCREF = 0x1127,  // Local Reference to a procedure
		S_ANNOTATIONREF = 0x1128,  // Reference to an S_ANNOTATION symbol
		S_TOKENREF = 0x1129,  // Reference to one of the many MANPROCSYM's

		// continuation of managed symbols
		S_GMANPROC = 0x112a,  // Global proc
		S_LMANPROC = 0x112b,  // Local proc

		// short, light-weight thunks
		S_TRAMPOLINE = 0x112c,  // trampoline thunks
		S_MANCONSTANT = 0x112d,  // constants with metadata type info

		// native attributed local/parms
		S_ATTR_FRAMEREL = 0x112e,  // relative to virtual frame ptr
		S_ATTR_REGISTER = 0x112f,  // stored in a register
		S_ATTR_REGREL = 0x1130,  // relative to register (alternate frame ptr)
		S_ATTR_MANYREG = 0x1131,  // stored in >1 register

		// Separated code (from the compiler) support
		S_SEPCODE = 0x1132,

		S_LOCAL_2005 = 0x1133,  // defines a local symbol in optimized code
		S_DEFRANGE_2005 = 0x1134,  // defines a single range of addresses in which symbol can be evaluated
		S_DEFRANGE2_2005 = 0x1135,  // defines ranges of addresses in which symbol can be evaluated

		S_SECTION = 0x1136,  // A COFF section in a PE executable
		S_COFFGROUP = 0x1137,  // A COFF group
		S_EXPORT = 0x1138,  // A export

		S_CALLSITEINFO = 0x1139,  // Indirect call site information
		S_FRAMECOOKIE = 0x113a,  // Security cookie information

		S_DISCARDED = 0x113b,  // Discarded by LINK /OPT:REF (experimental, see richards)

		S_COMPILE3 = 0x113c,  // Replacement for S_COMPILE2
		S_ENVBLOCK = 0x113d,  // Environment block split off from S_COMPILE2

		S_LOCAL = 0x113e,  // defines a local symbol in optimized code
		S_DEFRANGE = 0x113f,  // defines a single range of addresses in which symbol can be evaluated
		S_DEFRANGE_SUBFIELD = 0x1140,           // ranges for a subfield

		S_DEFRANGE_REGISTER = 0x1141,           // ranges for en-registered symbol
		S_DEFRANGE_FRAMEPOINTER_REL = 0x1142,   // range for stack symbol.
		S_DEFRANGE_SUBFIELD_REGISTER = 0x1143,  // ranges for en-registered field of symbol
		S_DEFRANGE_FRAMEPOINTER_REL_FULL_SCOPE = 0x1144, // range for stack symbol span valid full scope of function body, gap might apply.
		S_DEFRANGE_REGISTER_REL = 0x1145, // range for symbol address as register + offset.

		// S_PROC symbols that reference ID instead of type
		S_LPROC32_ID = 0x1146,
		S_GPROC32_ID = 0x1147,
		S_LPROCMIPS_ID = 0x1148,
		S_GPROCMIPS_ID = 0x1149,
		S_LPROCIA64_ID = 0x114a,
		S_GPROCIA64_ID = 0x114b,

		S_BUILDINFO = 0x114c, // build information.
		S_INLINESITE = 0x114d, // inlined function callsite.
		S_INLINESITE_END = 0x114e,
		S_PROC_ID_END = 0x114f,

		S_DEFRANGE_HLSL = 0x1150,
		S_GDATA_HLSL = 0x1151,
		S_LDATA_HLSL = 0x1152,

		S_FILESTATIC = 0x1153,


    S_LOCAL_DPC_GROUPSHARED = 0x1154, // DPC groupshared variable
    S_LPROC32_DPC = 0x1155, // DPC local procedure start
    S_LPROC32_DPC_ID =  0x1156,
    S_DEFRANGE_DPC_PTR_TAG =  0x1157, // DPC pointer tag definition range
    S_DPC_SYM_TAG_MAP = 0x1158, // DPC pointer tag value to symbol record map


		S_ARMSWITCHTABLE = 0x1159,
		S_CALLEES = 0x115a,
		S_CALLERS = 0x115b,
		S_POGODATA = 0x115c,
		S_INLINESITE2 = 0x115d,      // extended inline site information

		S_HEAPALLOCSITE = 0x115e,    // heap allocation site

		S_MOD_TYPEREF = 0x115f,      // only generated at link time

		S_REF_MINIPDB = 0x1160,      // only generated at link time for mini PDB
		S_PDBMAP = 0x1161,      // only generated at link time for mini PDB

		S_GDATA_HLSL32 = 0x1162,
		S_LDATA_HLSL32 = 0x1163,

		S_GDATA_HLSL32_EX = 0x1164,
		S_LDATA_HLSL32_EX = 0x1165,

		S_RECTYPE_MAX,               // one greater than last
		S_RECTYPE_LAST = S_RECTYPE_MAX - 1,
		S_RECTYPE_PAD = S_RECTYPE_MAX + 0x100 // Used *only* to verify symbol record types so that current PDB code can potentially read
											  // future PDBs (assuming no format change, etc).

	}

}
