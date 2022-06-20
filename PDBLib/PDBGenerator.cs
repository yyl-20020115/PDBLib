using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PDBLib
{
    public class PDBGenerator
    {
        public static readonly int PDBHeaderLength = Marshal.SizeOf<PDBHeader>();
        public static readonly int DBIHeaderLength = Marshal.SizeOf<DBIHeader>();
        public static readonly int DBIDebugHeaderLength = Marshal.SizeOf<DBIDebugHeader>();
        public static readonly int TypeInfoHeaderLength = Marshal.SizeOf<TypeInfoHeader>();
        
        protected PDBFileWriter writer;
        protected List<PDBStreamWriter> streams = new();
        public bool Load(PDBDocument doc)
        {
            return false;
        }
        public bool Generate(string path)
        {
            using var stream = File.OpenWrite(path);
            return this.Generate(stream);
        }
        public bool Generate(Stream stream)
        {
            this.writer = new PDBFileWriter(stream);



            return false;
        }



    }
}
