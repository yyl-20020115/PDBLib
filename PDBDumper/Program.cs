using PDBLib;
using YamlDotNet.Serialization;
namespace PDBDumper
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var i = 0;
            if (args.Length == 0)
            {
                Console.WriteLine("PDBDumper generates .yml files from .pdb files");
                Console.WriteLine("Usage: PDBDumper [debug1.pdb] [debug2.pdb] ...");
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
                            var doc = parser.Parse();
                            if (doc != null)
                            {
                                var builder = new SerializerBuilder();
                                var serializer = builder.Build();
                                using var output = new StreamWriter(Path.ChangeExtension(arg, ".yml"));
                                serializer.Serialize(output, doc);
                                i++;
                            }
                        }
                    }
                }
            }
            return i == args.Length ? 0 : -i;
        }
    }
}