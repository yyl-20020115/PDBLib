using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmPDBGenerator
{
    public interface IAsmPDBGenerator
    {
        bool Load(string yml);
        bool Generate(string pdb);
    }
}
