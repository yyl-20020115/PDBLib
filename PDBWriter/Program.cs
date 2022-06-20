using YamlDotNet.Serialization;
using PDBLib;
namespace PDBWriter
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var i = 0;
            if (args.Length == 0)
            {
                Console.WriteLine("PDBWriter generates .pdb files from .yml files");
                Console.WriteLine("Usage: PDBWriter [debug1.yml] [debug2.yml] ...");
            }
            else
            {
                foreach (var arg in args)
                {
                    if (Path.GetExtension(arg).ToLower() == ".yml")
                    {
                        var builder = new DeserializerBuilder();
                        var deserializer = builder.Build();
                        using var input = new StreamReader(arg);

                        var doc = deserializer.Deserialize<PDBDocument>(input);
                        var generator = new PDBGenerator();

                        if (generator.Load(doc) && generator.Generate(Path.ChangeExtension(arg, ".pdb")))
                        {
                            //DONE
                            i++;
                        }
                    }
                }
            }
            return i == args.Length ? 0 : -i;
        }
    }
}