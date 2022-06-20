using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBLib
{
    public class PDBStreamWriter
    {
        public MemoryStream Stream = new ();
        public PDBStreamWriter() { }
        public long Length => this.Stream.Length;
        public void WriteByte(byte b) => this.Stream.WriteByte(b);
        public void WriteBytes(byte[] bytes) => this.Stream.Write(bytes);
        public void Write<T>(T t) where T : struct => this.WriteBytes(Utils.From(t));
        public void Align(uint size)
        {
            long rem = this.Length % size;
            if (rem > 0)
            {
                for(int i = 0; i < (size-rem); i++)
                {
                    this.WriteByte(0);
                }
            }
        }
        public byte[] ToArray()=>this.Stream.ToArray();
    }
}
