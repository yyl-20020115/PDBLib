﻿namespace PDBLib;
public interface IPDBGenerator
{
    bool Generate(string pdb_path);
    bool Generate(Stream stream);
}
