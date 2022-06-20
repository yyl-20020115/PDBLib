using PDBLib;
using YamlDotNet.Serialization;
namespace PDBDumper
{
    public class Program
    {
        public static void Main(string[] args)
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
                            using var output = new StreamWriter(Path.ChangeExtension(arg,".yml"));
                            serializer.Serialize(output, doc);
                        }
                    }
                }
            }
        }
    }
}