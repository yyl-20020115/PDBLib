using Microsoft.DiaSymReader;
using Microsoft.DiaSymReader.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace PDBLib;

public class PDBGeneratorMS : PDBGenerator
{

    public PDBGeneratorMS()
    {

    }

    public override bool Generate(string path)
    {
        var pe_path = Path.ChangeExtension(path, ".exe");
        if (!File.Exists(pe_path))
        {
            pe_path = Path.ChangeExtension(path, ".dll");
            if (!File.Exists(pe_path))
                return false;
        }
        using var pdb_stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var pe_stream = new FileStream(pe_path,FileMode.Open, FileAccess.Read);
        using var pe_reader = new PEReader(pe_stream, PEStreamOptions.Default);

        using var pdbWriter = SymUnmanagedWriterFactory.CreateWriter(
            new SymMetadataProvider(pe_reader.GetMetadataReader()), (PortablePdbConversionOptions.Default).WriterCreationOptions);


        return base.Generate(path);
    }
    public override bool Generate(Stream stream)
    {

        return base.Generate(stream);
    }
}
