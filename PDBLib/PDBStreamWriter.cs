using System.Runtime.InteropServices;
namespace PDBLib;
public class PDBStreamWriter
{
    public List<int> Pages = new();
    public Stream Stream => this.stream;
    public bool IsMemoryBased => this.stream is MemoryStream;
    protected Stream stream;
    public PDBStreamWriter(Stream? stream = null)
        => this.stream = stream ?? new MemoryStream();
    public int Length => (int)this.stream.Length;
    public uint GetAlignedSize(uint page = PDBConsts.DefaultPageAlignmentSize)
        => Utils.GetAlignedLength((uint)this.Length, page);
    public int GetAlignedSize(int page = PDBConsts.DefaultPageAlignmentSize)
        => Utils.GetAlignedLength((int)this.Length, page);
    public int GetAlignedPages(int page = PDBConsts.DefaultPageAlignmentSize)
        => Utils.GetNumPages((int)this.Length, page);
    public int WriteByte(byte b)
    {
        this.stream.WriteByte(b);
        return sizeof(byte);
    }
    public int WriteBytes(byte[] bytes)
    {
        this.stream.Write(bytes ??= Array.Empty<byte>());
        return bytes.Length;
    }
    public int WriteUInts(uint[] uints)
    {
        for(int i = 0; i < uints.Length; i++)
            this.Write(uints[i]);
        return uints.Length * sizeof(uint);
    }
    public int WriteInts(uint[] uints)
    {
        for (int i = 0; i < uints.Length; i++)
            this.Write(uints[i]);
        return uints.Length * sizeof(uint);
    }
    public int Write<T>(T t) where T : struct => this.WriteBytes(Utils.From(t));
    public int Write(int data) => this.WriteBytes(BitConverter.GetBytes(data));
    public int Write(uint data) => this.WriteBytes(BitConverter.GetBytes(data));
    public int Write(short data) => this.WriteBytes(BitConverter.GetBytes(data));
    public int Write(ushort data) => this.WriteBytes(BitConverter.GetBytes(data));
    public int Write(string text)
    {
        var offset = this.Length;
        text ??= string.Empty;
        var buffer = new byte[text.Length + 1];
        if (text.Length > 0)
        {
            PDBConsts.DefaultEncoding.GetBytes(text, 0, text.Length, buffer, 0);
        }
        buffer[text.Length] = 0;
        this.WriteBytes(buffer);
        return this.Length - offset;
    }
    public void Seek(long offset) => this.stream.Seek(offset, SeekOrigin.Begin);
    public void Reserve(uint length) => this.Seek(this.stream.Position + length);
    public void Reserve(int length) => this.Seek(this.stream.Position + length);
    public void Reserve<T>() where T : struct => this.Reserve(Marshal.SizeOf<T>());
    public void Rewind() => this.Seek(0);
    public void Align(int size = 4)
    {
        var rem = this.Length % size;
        for (int i = 0; rem > 0 && i < (size - rem); i++)
            this.WriteByte(0);
    }
    public List<int> Complete(ref int page, int alignment = PDBConsts.DefaultPageAlignmentSize)
    {
        this.Pages.Clear();
        this.Align(alignment);
        for(int i = 0; i < this.Length; i+=alignment)
            this.Pages.Add(page++);
        return this.Pages;
    }
    public byte[] ToArray()=>this.stream is MemoryStream m ? m.ToArray():Array.Empty<byte>();
}
