using PDBLib;
using YamlDotNet.RepresentationModel;
namespace AsmPDBGenerator
{
    public class NasmPDBGenerator : IPDBGenerator
    {
        public static readonly Dictionary<string, string> Table = new()
        {
            ["NONE"] = "T_NOTYPE",
            ["BYTE"] = "T_UINT1",
            ["WORD"] = "T_UINT2",
            ["DWORD"] = "T_UINT4",
            ["QUAD"] = "T_UINT8",
            ["REAL32"] = "T_REAL32",
            ["REAL64"] = "T_REAL64",
            ["REAL80"] = "T_REAL80",
            ["REAL128"] = "T_REAL128",
            ["REAL256"] = "T_REAL256",
            ["REAL512"] = "T_REAL512",
        };
        public static string ConvertTypeName(string type)
            => Table.TryGetValue(type, out var ret) ? ret : "T_NOTYPE";

        public const string NasmCreator = "The Netwide Assembler";
        public const string NasmLanguage = "ASM";
        public PDBDocument PDBDocument => this.document;
        public PDBModule PDBModule => this.module;
        public PDBFunction PDBFunction => this.function;
        public PDBGenerator Generator => this.generator;

        protected PDBDocument document = new();
        protected PDBGenerator generator = new ();
        protected PDBModule module = new ();
        protected PDBFunction function = new ();
        public NasmPDBGenerator()
        {
            this.document.Functions.Add(this.function);
            this.document.Modules.Add(this.module);
        }
        protected bool ProcessInfo(YamlNode info)
        {
            if(info is YamlMappingNode mapping)
            {
                var creator = (string?)mapping["creator"] ?? "";
                var language = (string?)mapping["language"] ?? "";
                if (creator.StartsWith(NasmCreator) && NasmLanguage == language)
                {
                    document.Creator = creator;
                    document.Language = language;
                    document.Version = (string?)mapping["version"] ?? "";
                    document.Machine = (string?)mapping["machine"] ?? "";
                    module.ModuleName = (string?)mapping["output"] ?? "";
                    return true;
                }
            }
            return false;
        }
        protected void ProcessSourceFiles(YamlNode source_files)
        {
            if (source_files is YamlSequenceNode files)
            {
                foreach (var file in files.Children)
                {                    
                    var line_count = (string?)file["line-count"]??"";
                    
                    if(file["locations"] is YamlSequenceNode locations)
                    {
                        foreach(var location in locations)
                        {
                            if(location is YamlMappingNode loc)
                            {
                                if(uint.TryParse((string?)loc["code-offset"] ?? "", out var code_offset)
                                    && uint.TryParse((string?)loc["line-number"]??"",out var line_number))
                                {
                                    //line number is related to function start line
                                    //however we only have one function for Nasm.
                                    this.function.Lines.Add(new PDBLine
                                        { CodeOffset = code_offset, LineNumber = line_number });
                                }
                            }
                        }
                    }
                }
            }
        }
        protected void ProcessSymbols(YamlNode symbols)
        {
            if(symbols is YamlSequenceNode sequence)
            {
                foreach(var symbol in sequence.Children)
                {
                    var name = (string?)symbol["name"] ?? "";
                    var section = (string?)symbol["section"] ?? "";
                    var offset = (string?)symbol["offset"] ?? "";
                    var size = (string?)symbol["size"] ?? "";
                    var type = (string?)symbol["type"] ?? "";
                    var stype = (string?)symbol["stype"] ?? "";
                    var bits = (string?)symbol["bits"] ?? "";

                    if(type=="PROC" || type=="CODE")
                    {
                        this.function.Name = name; //this is the single name, maybe main
                        uint.TryParse(section, out this.function.Segment);
                        uint.TryParse(offset, out this.function.Offset);
                        uint.TryParse(size, out this.function.Length);
                        uint.TryParse(bits, out this.function.Bits);

                        if (bits == "32")
                        {
                            this.PDBDocument.Bits = PDBBits.Bits32;
                        }else if(bits == "64")
                        {
                            this.PDBDocument.Bits = PDBBits.Bits64;
                        }
                    }
                    else if(type=="LDATA" || type=="GDATA")
                    {
                        uint.TryParse(offset, out var _offset);
                        uint.TryParse(section, out var segment);
                        uint.TryParse(size, out var _size);
                        this.document.Globals.Add(
                            new PDBGlobal { Name = name, Offset=_offset,
                                Segment = segment, LeafType = ConvertTypeName(stype),
                                SymType = type,
                            });
                    }
                }
            }
        }
        public bool Load(string yaml_path)
        {
            using var reader = new StreamReader(yaml_path);
            var stream = new YamlStream();
            stream.Load(reader);
            foreach(var doc in stream.Documents)
            {
                if (doc.RootNode is YamlMappingNode root 
                    && root.Children.TryGetValue("info", out var info))
                {
                    if (this.ProcessInfo(info))
                    {
                        if (root.Children.TryGetValue("symbols", out var symbols))
                        {
                            this.ProcessSymbols(symbols);
                        }
                        if (root.Children.TryGetValue("source-files", out var source_files))
                        {
                            this.ProcessSourceFiles(source_files);
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        public bool Generate(string pdb_path)
            => this.generator.Load(this.document) && this.generator.Generate(pdb_path);
        public bool Generate(Stream stream)
            => this.generator.Load(this.document) && this.generator.Generate(stream);
    }
}
