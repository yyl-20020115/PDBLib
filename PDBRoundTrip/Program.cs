using YamlDotNet.Serialization;
using PDBLib;
namespace PDBRoundTrip;
public class Program
{
    public static int Main(string[] args)
    {
        var i = 0;
        if (args.Length == 0)
        {
            Console.WriteLine("PDBRoundTrip generates .pdb files from .yml files and re-generates .pdb backwards");
            Console.WriteLine("Usage: PDBRoundTrip [debug1.yml] [debug2.yml] ...");
        }
        else
        {
            foreach (var arg in args)
            {
                if (Path.GetExtension(arg).ToLower() == ".pdb")
                {
                    var parser = new PDBParser();
                    if (parser.Load(arg))
                    {
                        var sdoc = parser.Parse();
                        if (sdoc != null)
                        {
                            var sbuilder = new SerializerBuilder();
                            var rbuilder = new DeserializerBuilder();
                            var serializer = sbuilder.Build();
                            var deserializer = rbuilder.Build();
                            var yml_path = Path.ChangeExtension(arg, ".yml");
                            using (var output = new StreamWriter(yml_path))
                            {
                                serializer.Serialize(output, sdoc);
                            }
                            using (var input = new StreamReader(yml_path))
                            {
                                var back_path = Path.GetFileNameWithoutExtension(arg) + "_back.pdb";
                                var rdoc = deserializer.Deserialize<PDBDocument>(input);
                                var generator = new PDBGenerator();
                                if (generator.Load(rdoc) && generator.Generate(back_path))
                                {
                                    i++;
                                }
                            }
                        }
                    }
                }
            }
        }
        return i == args.Length ? 0 : -i;
    }
}