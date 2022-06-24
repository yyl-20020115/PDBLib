using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBLib
{
    public class PDBFileWriter : PDBStreamWriter
    {
        public PDBFileWriter(Stream stream)
            : base(stream)
        {
        }
    }
}
