namespace AsmPDBGenerator;
public static class Program
{
    public static int Main(string[] args)
    {
        var i = 0;
        if (args.Length == 0)
        {
            Console.WriteLine("AsmPDBGenerator generates .pdb files from .yml files (produced by NASM etc)");
            Console.WriteLine("Usage: AsmPDBGenerator [debug1.yml] [debug2.yml] ...");
        }
        else
        {
            foreach (var arg in args)
            {
                if (Path.GetExtension(arg).ToLower() == ".yml")
                {
                    var generator = new NAsmPDBGenerator();
                    if (generator.Load(arg) && generator.Generate(Path.ChangeExtension(arg, ".pdb")))
                    {
                        i++;
                    }
                }
            }
        }
        return i == args.Length ? 0 : -i;
    }                       
}