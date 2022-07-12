namespace PDBLib;

/// <summary>
/// | Stream No.			| Contents																								|Short Description
///|--------------|---------------------------------|-------------------
///| 1            | Pdb(header)                      | Version information, and information to connect this PDB to the EXE
///| 2	           | Tpi (Type manager)              | All the types used in the executable.
///| 3	           | Dbi (Debug information)            | Holds section contributions, and list of ‘Mods’
///| 4	           | NameMap                            | Holds a hashed string table
///| 4-(n+4)	     | n Mod’s (Module information)	   | Each Mod stream holds symbols and line numbers for one compiland
///| n+4	         | Global symbol hash	             | An index that allows searching in global symbols by name
///| n+5	         | Public symbol hash	             | An index that allows searching in public symbols by addresses
///| n+6	         | Symbol records                    | Actual symbol records of global and public symbols
///| n+7	         | Type hash                          | Hash used by the TPI stream.
/// </summary>
public enum PDBBits : uint
{
    Bits32 = 0,
    Bits64 = 1,
}
public class PDBDocument
{
    public string Creator = "";
    public string Version = "";
    public string Language = "";
    public string Machine = "";
    public PDBBits Bits = PDBBits.Bits64;
    public List<string> Names = new();
    public List<PDBGlobal> Globals = new();
    public List<PDBModule> Modules = new();
    public List<PDBFunction> Functions = new();
    public List<PDBType> Types = new();
    public List<IMAGE_SECTION_HEADER> SectionHeaders = new();
}
public class PDBGlobal
{
    public string Name = "";
    public string LeafType = "";
    public string SymType = "";
    public uint Offset = 0;
    public uint Segment = 0;
}
public class PDBType
{
    public string TypeName = "";
    public bool IsPointer = false;
    public Dictionary<string, string> Values = new();
    public Dictionary<string, PDBType> SubTypes = new();
    public HashSet<string> Collect(HashSet<string>? texts = null)
    {
        texts ??= new();
        texts.Add(this.TypeName);
        foreach (var st in this.SubTypes)
        {
            texts.Add(st.Key);
            st.Value.Collect(texts);
        }
        return texts;
    }
}
public class PDBLine
{
    public uint CodeOffset = 0;
    public uint LineNumber = 0; //0~2^24-1
    public byte DeltaLineEnd = 0; //0~128
    public bool IsExpression = false;
}

public class PDBModule
{
    public string ModuleName = "";
    public string ObjectName = "";
    public List<string> Sources = new();
    public List<string> FunctionNames = new();
}
public class PDBFunction
{
    public string Name = "";
    public string Source = "";
    public PDBType Type = new();
    public uint Segment = 0;
    public uint Offset = 0;
    public uint Length = 0;
    public uint ParamSize = 0;
    public uint Bits = 0;
    public List<PDBLine> Lines = new();
}
