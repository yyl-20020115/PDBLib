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

public class PDBGeneratorWindows : PDBGenerator
{

    public PDBGeneratorWindows()
    {

    }

    public override bool Generate(string path)
    {
        var pef_path = "";
        if (!File.Exists(pef_path = Path.ChangeExtension(path, ".exe"))
            && !File.Exists(pef_path = Path.ChangeExtension(path, ".dll"))) 
            return false;
        
        using var pef_stream = new FileStream(pef_path,FileMode.Open, FileAccess.Read);
        using var pef_reader = new PEReader(pef_stream, PEStreamOptions.Default);

        using var pdb_stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var pdb_writer = SymUnmanagedWriterFactory.CreateWriter(
            new SymMetadataProvider(pef_reader.GetMetadataReader()), 
                PortablePdbConversionOptions.Default.WriterCreationOptions);

        var guid = Guid.Empty;
        int age = 1;
        uint stamp = 0;
        ushort fileMajor = 0;
        ushort fileMinor = 0;
        ushort fileBuild = 0;
        ushort fileRevision = 0;
        string language = "ASM";
        int entryToken = 0;
        int methodToken = 0;
        int startOffset = 0;
        int endOffset = 0;
        string name = "";
        Guid lang = Guid.Empty;
        Guid vender = Guid.Empty;
        Guid type = Guid.Empty;
        Guid algorithm = Guid.Empty;
        var checksum = Array.Empty<byte>();
        var source = Array.Empty<byte>();
       

        int documentIndex = pdb_writer.DefineDocument(name, lang, vender, type, algorithm, checksum, source);
        


        

        pdb_writer.OpenMethod(methodToken);
        pdb_writer.OpenScope(startOffset);

        //pdb_writer.DefineSequencePoints();

        pdb_writer.CloseScope(endOffset);
        pdb_writer.CloseMethod();
        pdb_writer.SetEntryPoint(entryToken);
        pdb_writer.AddCompilerInfo(fileMajor, fileMinor, fileBuild, fileRevision, language);
        pdb_writer.UpdateSignature(guid, stamp, age);
        pdb_writer.WriteTo(pdb_stream);
        return true;
    }

}
