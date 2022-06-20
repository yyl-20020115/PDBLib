using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PDBLib
{
    public class PDBGenerator : IPDBGenerator
    {
        public static readonly int PDBHeaderLength = Marshal.SizeOf<PDBHeader>();
        public static readonly int DBIHeaderLength = Marshal.SizeOf<DBIHeader>();
        public static readonly int DBIDebugHeaderLength = Marshal.SizeOf<DBIDebugHeader>();
        public static readonly int TypeInfoHeaderLength = Marshal.SizeOf<TypeInfoHeader>();

        protected PDBHeader pdb_header = new();
        protected DBIHeader dbi_header = new();
        protected DBIDebugHeader dbi_debug_header = new();
        protected TypeInfoHeader type_info_header = new();
        protected List<IMAGE_SECTION_HEADER> sections = new();
        protected Dictionary<uint, UniqueSrc> unique = new();
        protected Dictionary<string, uint> name_indices = new();
        protected List<PDBStreamWriter> streams = new();

        protected uint uid = 0;
        protected void RegisterStrings(params string[] texts)
        {
            this.RegisterStrings(texts as IEnumerable<string>);
        }
        protected void RegisterStrings(IEnumerable<string> texts)
        {
            foreach(var text in texts)
            {
                this.RegisterString(text);
            }
        }
        protected uint RegisterString(string text)
        {
            if(!this.name_indices.TryGetValue(text,out var id))
            {
                this.name_indices.Add(text, id = uid++);
            }
            return id;
        }
        protected void RegisterStrings(PDBDocument doc)
        {
            this.RegisterString(PDBConsts.NameStreamName);
            this.RegisterString(doc.Creator);
            this.RegisterString(doc.Version);
            this.RegisterString(doc.Language);
            this.RegisterString(doc.Machine);
            doc.Names.ForEach(n => this.RegisterString(n));
            doc.Globals.ForEach(g => this.RegisterStrings(g.Name, g.SymType, g.LeafType));
            doc.Modules.ForEach(m => { 
                this.RegisterStrings(m.ModuleName); 
                this.RegisterStrings(m.Sources);
                this.RegisterStrings(m.FunctionNames); 
            });
            doc.Functions.ForEach(f => this.RegisterStrings(f.Name,f.Source));
            doc.Types.ForEach(t => this.RegisterStrings(t.Collect()));
        }
        public bool Load(PDBDocument doc)
        {
            //setup streams
            if (doc != null)
            {
                this.RegisterStrings(doc);





                //TODO:
                return true;
            }
            return false;
        }
        public bool Generate(string path)
        {
            using var stream = File.OpenWrite(path);
            return this.Generate(stream);
        }
        public bool Generate(Stream stream)
        {
            var writer = new PDBFileWriter(stream);
            //build streams and compile

            var total_streams_size = (uint)this.streams.Sum(s => s.GetAlignedSize());


            //write headers
            Array.Copy(PDBConsts.SignatureBytes, this.pdb_header.signature, PDBConsts.SignatureBytes.Length);

            this.pdb_header.pageSize = (int)PDBStreamWriter.PageAlignmentSize;
            this.pdb_header.freePageMap = 0;
            this.pdb_header.pagesUsed =(int) Utils.GetNumPages(total_streams_size,(uint)this.pdb_header.pageSize);
            //TODO:
            this.pdb_header.directorySize = 0;
            this.pdb_header.reserved = 0;
            //write pdb header
            writer.Write(this.pdb_header);



            //write streams
            foreach(var s in this.streams)
            {
                s.Complete();
                writer.Write(s.ToArray());
            }
            return true;
        }
    }
}
