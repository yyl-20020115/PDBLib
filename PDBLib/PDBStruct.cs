using System.Text;

namespace PDBLib
{
    public static class PDBConsts
	{
		public static string Signature = "Microsoft C/C++ MSF 7.00\r\n"+((char)26)+"DS\0\0\0";
		public static readonly byte[] SignatureBytes = Encoding.Latin1.GetBytes(Signature);
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
		public int Offset;
	}

	public struct StringVal
	{
		public uint Id;
		public uint Stream;
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

	public enum KnownStreams : uint
	{
		Root = 0,
		Streams = 1,
		TypeInfoStream = 2,
		DebugInfo = 3,
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
			if(other is FunctionRecord f)
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
		public DBIModuleInfo Info = new ();
		public List<string> Sources = new();
		public List<FunctionRecord> Functions = new();
		public Dictionary<uint,uint> SrcIndex = new();
		public string ModuleName = "";
		public string ObjectName = "";
	}
}
