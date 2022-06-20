using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBLib
{
    public class PDBFileWriter
    {
        protected Stream stream;
        public PDBFileWriter(Stream stream)
        {
            this.stream = stream;
        }

        public void Seek(long offset)
        {
            this.stream.Seek(offset, SeekOrigin.Begin);
        }
        public void Reserve(uint length)
        {
            this.Seek(this.stream.Position + length);
        }
        public void Write<T>(T t) where T : struct
        {
            this.Write(Utils.From(t)); 
        }
        public void Write(byte[] bytes)
        {
            this.stream.Write(bytes);
        }
        public void WriteByte(byte b)
        {
            this.stream.WriteByte(b);   
        }
    }
}
