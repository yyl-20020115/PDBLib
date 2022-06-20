using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBLib
{
    public class PDBGenerator
    {
        protected PDBFileWriter writer;
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
