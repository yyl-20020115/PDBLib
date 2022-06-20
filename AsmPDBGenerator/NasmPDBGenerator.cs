using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using PDBLib;
namespace AsmPDBGenerator
{
    public class NasmPDBGenerator
    {
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
        protected void ProcessInfo(YamlNode info)
        {
            if(info is YamlMappingNode mapping)
            {
                document.Creator = (string?)mapping["creator"] ?? "";
                document.Version = (string?)mapping["version"] ?? "";
                document.Language = (string?)mapping["language"] ?? "";
                document.Machine = (string?)mapping["machine"] ?? "";

                module.ModuleName = (string?)mapping["output"] ?? "";
            }
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
                                if(uint.TryParse((string?)loc["file-offset"] ?? "", out var file_offset)
                                    && uint.TryParse((string?)loc["line-number"]??"",out var line_number))
                                {
                                    this.function.Lines.Add(new PDBLine
                                        { CodeOffset = file_offset, LineNumber = line_number });
                                }
                            }
                        }
                    }
                }
            }
        }
        protected string ConvertTypeName(string type)
        {
            //TODO:
            return type;
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

                    if(type=="PROC")
                    {
                        this.function.Name = name; //this is the single name, maybe main
                        uint.TryParse(section, out this.function.segment);
                        uint.TryParse(offset, out this.function.offset);
                        uint.TryParse(size, out this.function.length);

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
                                Segment = segment, Type = new(){ TypeName = ConvertTypeName(stype) }, 
                                Size = _size == 0 ? (bits == "32"? 4u:8u) : _size
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
                if(doc.RootNode is YamlMappingNode root)
                {
                    if (root.Children.TryGetValue("info", out var info))
                    {
                        this.ProcessInfo(info);
                    }
                    if (root.Children.TryGetValue("symbols", out var symbols))
                    {
                        this.ProcessSymbols(symbols);
                    }
                    if (root.Children.TryGetValue("source-files",out var source_files))
                    {
                        this.ProcessSourceFiles(source_files);
                    }
                }
            }
            return true;
        }
        public bool Generate(string pdb_path) 
            => this.generator.Load(this.document) && this.generator.Generate(pdb_path);

    }
}
