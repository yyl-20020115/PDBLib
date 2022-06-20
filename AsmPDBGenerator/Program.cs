namespace AsmPDBGenerator
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            foreach(var arg in args)
            {
                if(Path.GetExtension(arg).ToLower() == ".yml")
                {
                    var generator = new NasmPDBGenerator();
                    if (generator.Load(arg))
                    {
                        generator.Generate(Path.ChangeExtension(arg, ".pdb"));
                    }
                }

            }
        }
    }
}