using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBLib
{
    public interface IPDBGenerator
    {
        bool Generate(string pdb_path);
        bool Generate(Stream stream);
    }
}
