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
        public bool Load(PDBDocument doc)
        {
            //setup streams
            if (doc != null)
            {
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
