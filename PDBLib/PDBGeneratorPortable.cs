using Microsoft.CodeAnalysis.Debugging;
using Microsoft.DiaSymReader;
using Microsoft.DiaSymReader.Tools;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
namespace PDBLib;
public class PDBGeneratorPortable : PDBGenerator
{
    public static readonly Guid None = new ("00000000-0000-0000-0000-000000000000");
    public static readonly Guid C = new ("63a08714-fc37-11d2-904c-00c04fa302a1");
    public static readonly Guid Cpp = new ("3a12d0b7-c26c-11d0-b442-00a0244a1dd2");
    public static readonly Guid CSharp = new ("3f5162f8-07c6-11d3-9053-00c04fa302a1");
    public static readonly Guid Basic = new ("3a12d0b8-c26c-11d0-b442-00a0244a1dd2");
    public static readonly Guid Java = new ("3a12d0b4-c26c-11d0-b442-00a0244a1dd2");
    public static readonly Guid Cobol = new ("af046cd1-d0e1-11d2-977c-00a0c9b4d50c");
    public static readonly Guid Pascal = new ("af046cd2-d0e1-11d2-977c-00a0c9b4d50c");
    public static readonly Guid CIL = new ("af046cd3-d0e1-11d2-977c-00a0c9b4d50c");
    public static readonly Guid JScript = new ("3a12d0b6-c26c-11d0-b442-00a0244a1dd2");
    public static readonly Guid SMC = new ("0d9b9f7b-6611-11d3-bd2a-0000f80849bd");
    public static readonly Guid MCpp = new ("4b35fde8-07c6-11d3-9053-00c04fa302a1");
    //vendor
    public static readonly Guid Other = new ("00000000-0000-0000-0000-000000000000");
    public static readonly Guid Microsoft = new ("994b45c4-e6e9-11d2-903f-00c04fa302a1");

    //harsh
    public static readonly Guid SHA1 = new ("ff1816ec-aa5e-4d10-87f7-6f4963833460");
    public static readonly Guid SHA256 = new ("8829d00f-11b8-4213-878b-770e8597ac16");
    //document type
    public static readonly Guid Text = new ("{5a869d0b-6611-11d3-bd2a-0000f80849bd}");
    public static bool IsPortable(Stream stream)
    {
        stream.Position = 0;

        var isPortable =false;

        if (stream.Length >= 4)
        {
            isPortable 
                 = stream.ReadByte() == 'B' 
                && stream.ReadByte() == 'S' 
                && stream.ReadByte() == 'J' 
                && stream.ReadByte() == 'B'
                ;
        }

        stream.Position = 0;

        return isPortable;
    }

    public static void GetWindowsPdbSignature(ImmutableArray<byte> bytes, out Guid guid, out uint timestamp, out int age)
    {
        var guidBytes = new byte[16];
        bytes.CopyTo(0, guidBytes, 0, guidBytes.Length);
        guid = new Guid(guidBytes);

        int n = guidBytes.Length;
        timestamp = ((uint)bytes[n + 3] << 24) | ((uint)bytes[n + 2] << 16) | ((uint)bytes[n + 1] << 8) | bytes[n];
        age = 1;
    }


    public static bool TryReadPdbId(PEReader peReader, out BlobContentId id, out int age)
    {
        var codeViewEntry = peReader.ReadDebugDirectory().LastOrDefault(entry => entry.Type == DebugDirectoryEntryType.CodeView);
        if (codeViewEntry.DataSize == 0)
        {
            id = default;
            age = 0;
            return false;
        }

        var codeViewData = peReader.ReadCodeViewDebugDirectoryData(codeViewEntry);

        id = new BlobContentId(codeViewData.Guid, codeViewEntry.Stamp);
        age = codeViewData.Age;
        return true;
    }
    public ImmutableArray<int> GetRowCounts(params int[] rc)
    {
        var builder = ImmutableArray.CreateBuilder<int>(MetadataTokens.TableCount);
        for (int i = 0; i < MetadataTokens.TableCount; i++)
        {
            builder.Add(rc[i]);
        }

        return builder.MoveToImmutable();
    }
    private MethodDefinitionHandle GetEntryPointHandle(int token)
    {
        var handle = MetadataTokens.EntityHandle(token);
        if (handle.IsNil)
        {
            return default;
        }

        if (handle.Kind != HandleKind.MethodDefinition)
        {
            return default;
        }

        return (MethodDefinitionHandle)handle;
    }
    public override bool Generate(string path)
    {
        var pef_path = "";
        if (!File.Exists(pef_path = Path.ChangeExtension(path, ".exe"))
            && !File.Exists(pef_path = Path.ChangeExtension(path, ".dll")))
            return false;

        using var pef_stream = new FileStream(pef_path, FileMode.Open, FileAccess.Read);
        using var pef_reader = new PEReader(pef_stream, PEStreamOptions.Default);

        using var pdb_stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var pdb_writer = SymUnmanagedWriterFactory.CreateWriter(
            new SymMetadataProvider(pef_reader.GetMetadataReader()),
                PortablePdbConversionOptions.Default.WriterCreationOptions);
        var hash = Array.Empty<byte>();

        if (!TryReadPdbId(pef_reader, out var pdbId, out int age))
        {
            throw new InvalidDataException(ConverterResources.SpecifiedPEFileHasNoAssociatedPdb);
        }

        var metadataBuilder = new MetadataBuilder();

        string name = "";
        Guid language = Guid.Empty;
        Guid vender = Guid.Empty;
        Guid type = Guid.Empty;
        Guid hashAlgorithm = Guid.Empty;
        var checksum = Array.Empty<byte>();
        var sourceBlob = Array.Empty<byte>();
        var checksumHandle = (hashAlgorithm != default) ? metadataBuilder.GetOrAddBlob(checksum) : default;
        var documentIndex = new Dictionary<string, DocumentHandle>(StringComparer.Ordinal);


        var documentHandle = metadataBuilder.AddDocument(
            name: metadataBuilder.GetOrAddDocumentName(name),
            hashAlgorithm: metadataBuilder.GetOrAddGuid(hashAlgorithm),
            hash: checksumHandle,
            language: metadataBuilder.GetOrAddGuid(language));

        metadataBuilder.AddCustomDebugInformation(
            documentHandle,
            metadataBuilder.GetOrAddGuid(PortableCustomDebugInfoKinds.EmbeddedSource),
            metadataBuilder.GetOrAddBlob(sourceBlob));
        documentIndex.Add(name, documentHandle);
        var typeSystemRowCounts = GetRowCounts(1,1,1,1,1);//TODO:
        var debugEntryPointToken = GetEntryPointHandle(0);



        //TODO:


        var serializer = new PortablePdbBuilder(
            metadataBuilder, 
            typeSystemRowCounts, 
            debugEntryPointToken, 
            idProvider: _ => pdbId);

        var blobBuilder = new BlobBuilder();
        serializer.Serialize(blobBuilder);
        blobBuilder.WriteContentTo(pdb_stream);

        return true;
    }
}
