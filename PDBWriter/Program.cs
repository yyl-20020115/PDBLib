using YamlDotNet.Serialization;
using PDBLib;
namespace PDBWriter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            foreach(var arg in args)
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
                    }
                }
            }
        }
    }
}