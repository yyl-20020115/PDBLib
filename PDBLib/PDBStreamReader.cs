using System.Runtime.InteropServices;
using System.Text;

namespace PDBLib
{
	public class PDBStreamReader
	{
	 	protected StreamPair m_stream;
		protected PDBParser m_parser;

		protected uint m_offset = 0;
		protected uint m_pageIndex = 0;
		protected uint m_pageDelta = 0;
		protected uint m_pageLast = 0xffffffff;
		protected uint m_pageBase = 0xffffffff;
		protected uint m_realOffset = 0xffffffff;

		protected SortedDictionary<uint, uint> m_pageTable = new ();
		public PDBStreamReader(StreamPair stream, PDBParser parser, uint offset = 0)
		{
			this.m_stream = stream;
			this.m_parser = parser;
			uint index = 0;
			foreach(var page in this.m_stream.PageIndices)
            {
				m_pageTable.Add(index++, page * this.m_parser.PageSize);
            }
			this.Seek(this.m_offset = offset);
		}

        public uint Offset => this.m_offset;
        public byte[] Data => this.m_parser.Buffer;

        public void Align(uint align)
		{
			uint diff = m_offset % align;

			if (diff != 0)
            {
				Seek(m_offset + align - diff);
			}
		}

		public void Seek(uint offset)
		{
			this.m_offset = offset;
			this.m_pageIndex = this.m_offset / m_parser.PageSize;
			this.m_pageDelta = this.m_offset % m_parser.PageSize;
            if (this.m_pageIndex != this.m_pageLast)
            {
				this.m_realOffset = (this.m_pageBase = this.m_pageTable[
					this.m_pageLast = this.m_pageIndex]) + this.m_pageDelta;
            }
            else
            {
				this.m_realOffset = this.m_pageBase + this.m_pageDelta;
            }
		}

		public T Peak<T>() where T : struct
		{
			var offset = this.m_offset;
			var ret = this.Read<T>();
			this.Seek(offset);
			return ret;
		}
		public byte Read()
        {
			var b = this.m_realOffset<this.Data.Length ? this.Data[this.m_realOffset] : (byte)0;
			this.Seek(this.m_offset + 1);
			return b;
        }
		public byte[] ReadBytes(int count)
        {
			var data = new byte[count];
			for(int i = 0; i < count; i++)
            {
				data[i] = Read();
            }
			return data;
        }
		public T Read<T>() where T : struct
        {
			var size = Marshal.SizeOf<T>();
			var data = new byte[size];
			for(int i = 0; i < size; i++)
            {
				data[i] = this.Read();
            }
			return Utils.From<T>(data);
		}

		public T[] Reads<T>(uint size) where T : struct
		{
			var ts = new T[size];
			for(int i = 0; i < size; i++)
            {
				ts[i] = Read<T>();
            }
			return ts;
		}

		public string ReadString()
		{
			uint origOffset = m_offset;
			var buffer = new MemoryStream();
			byte b = 0;
            while ((b = this.Read()) != 0)
            {
				buffer.WriteByte(b);
            }
			return PDBConsts.DefaultEncoding.GetString(buffer.ToArray());
		}	
	}

}
