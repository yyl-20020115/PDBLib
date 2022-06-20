using System.Runtime.InteropServices;
using System.Text;
namespace PDBLib
{
	public class PDBParser
	{
		public Dictionary<uint, (string, GlobalRecord)> Globals => globals;
		public List<FunctionRecord> Functions => this.functions;
		public List<Module> Modules => this.modules;
		public Dictionary<uint, TypeInfo> Types => this.types;
		public NameStream Names => this.names;
		public Guid Guid => this.m_guid;
		public Dictionary<(uint, uint), FPO_DATA> Fpov1Data => this.fpov1Data;
		public Dictionary<(uint, uint), FPO_DATA_V2> Fpov2Data => this.fpov2Data;
		public byte[] Buffer => this.buffer;
		public uint PageSize => this.page_size;

		protected Dictionary<uint, (string, GlobalRecord)> globals = new();
        protected List<FunctionRecord> functions = new();
		protected Dictionary<uint, TypeInfo> types = new();
		protected List<Module> modules = new();
		protected Dictionary<(uint, uint), FPO_DATA> fpov1Data = new();
		protected Dictionary<(uint, uint), FPO_DATA_V2> fpov2Data = new();
		protected NameStream names = new();
		protected byte[] buffer = Array.Empty<byte>();
		protected uint page_size = 0;
		protected uint page_used = 0;
		protected long real_length = 0;
		protected string pdb_path ="";
		protected string exe_path = "";
		protected PDBHeader pdb_header = new();
		protected DBIHeader dbi_header = new();
		protected DBIDebugHeader dbi_debug_header = new();
		protected TypeInfoHeader type_info_header = new();
		protected List<IMAGE_SECTION_HEADER> sections = new();
		protected Dictionary<uint, UniqueSrc> unique = new();
		protected List<StreamPair> streams = new();
		protected Dictionary<string, uint> name_indices = new();

		protected Guid m_guid = new();        //!< Unique GUID for the PDB, found in the NameIndexHeader in the root stream, matches the guid returned by IDiaSession::get_globalScope()->get_guid()

		public StreamPair? GetStream(KnownStreams ks) => this.GetStream((uint)ks);
		public StreamPair? GetStream(uint index) => index<this.streams.Count ? this.streams[(int)index] : null;
		public PDBParser()
		{

		}

		public bool Load(string path)
		{
			var header_size = Marshal.SizeOf<PDBHeader>();

			if (path.ToLower().EndsWith(".pdb"))
			{
				var info = new FileInfo(this.pdb_path = path);
				if ((this.real_length = (int)info.Length) >= header_size)
				{
					var header_bytes = Utils.ReadFile(path, header_size);
					this.pdb_header = Utils.From<PDBHeader>(header_bytes);
					this.page_size = (uint)pdb_header.pageSize;
					this.page_used = (uint)pdb_header.pagesUsed; //this is all length of file in pages
					this.buffer = Utils.ReadFile(path, (int)this.real_length,
						new byte[Utils.GetAlignedLength((uint)this.real_length, this.page_size)]);

					this.exe_path =
						File.Exists(Path.ChangeExtension(path, ".exe"))
						? Path.ChangeExtension(path, ".exe")
						: File.Exists(Path.ChangeExtension(path, ".dll"))
						? Path.ChangeExtension(path, ".dll")
						: string.Empty;

					return this.ReadRootStream();
				}
			}
			return false;
		}
		public PDBDocument Parse()
        {
			var doc = new PDBDocument();
            if (this.ParseInternal())
            {
				doc.Names = new(this.names.Dict.Values);
				//TODO:
				doc.Globals = new();
				doc.Types = this.types.Select(t => Utils.StringizeTypeFast(t.Key, this.types, 0)).ToList();
				doc.Modules = this.modules.Select(m => new PDBModule() { ModuleName = m.ModuleName, ObjectName = m.ObjectName }).ToList();
				//TODO:functions
				doc.Functions = this.functions.Select(f => new PDBFunction()).ToList();
			}
			return doc;
        }
		protected internal bool ParseInternal()
		{
			var pair = GetStream(KnownStreams.DebugInfo);
			if (pair == null || pair.Size == 0) return false;

			var reader = new PDBStreamReader(pair, this);
			this.dbi_header = reader.Read<DBIHeader>();

			uint endOffset = reader.Offset + this.dbi_header.moduleSize;
			//load modules
			while (reader.Offset < endOffset)
			{
				var dbInfo = reader.Read<DBIModuleInfo>();
				var modName = reader.ReadString();
				var objName = reader.ReadString();

				if (dbInfo.stream != -1)
					modules.Add(new Module { Info = dbInfo, ModuleName = modName, ObjectName = objName });

				reader.Align(4);
			}

			reader.Seek(reader.Offset
				+ this.dbi_header.secConSize
				+ this.dbi_header.secMapSize
				+ this.dbi_header.fileInfoSize
				+ this.dbi_header.srcModuleSize
				+ this.dbi_header.ecInfoSize);
			//load debug header
			this.dbi_debug_header = reader.Read<DBIDebugHeader>();

			// Get the PE headers so that we can offset the functions to their correct
			// addresses in the actual executable
			if (this.dbi_debug_header.sectionHdr != 0xFFFF)
			{
				this.ReadSectionHeaders(this.dbi_debug_header.sectionHdr, this.sections);
			}

			//load files
			uint id = 1;
			foreach (var mod in this.modules)
			{
				if (mod.Info.stream < 0) continue;
				GetModuleFiles(mod.Info, ref id, unique, mod.SrcIndex);
			}

			//load names
			if (this.LoadNameStream(this.names))
			{
				foreach (var mod in this.modules)
				{
					foreach (var kv in mod.SrcIndex)
					{
						if (unique.TryGetValue(kv.Value, out var us) && 0 == us.Visited)
						{
							if (!names.Dict.TryGetValue(kv.Value, out var file))
							{
								// Handle bad file references...again...thank you microsoft
							}
							us.Visited = 1;
						}
					}
				}
			}

			//load types
			this.types = LoadTypeStream();

			// Check to see if we need to remap functions
			if (this.dbi_debug_header.tokenRidMap != 0 && this.dbi_debug_header.tokenRidMap != 0xffff)
				return false;

			//load global functions
			GetGlobalFunctions((ushort)this.dbi_header.symRecordStream, sections, globals);

			foreach (var mod in modules)
			{
				GetModuleFunctions(mod.Info, functions);
			}

			functions.Sort();

			foreach (var mod in modules)
			{
				ResolveFunctionLines(mod.Info, functions, unique, mod.SrcIndex);
			}

			if (this.dbi_debug_header.FPO != 0xffff)
			{
				ReadFPO(this.dbi_debug_header.FPO, fpov1Data);
			}

			if (this.dbi_debug_header.newFPO != 0xffff)
			{
				ReadFPO(this.dbi_debug_header.newFPO, fpov2Data);
			}

			// We cheat in the Function < operator so that we can sort
			// first, now iterate over the functions and remove the functions that are duplicates,
			// we don't actually remove the functions, just make it so that they are skipped from printing
			this.functions = functions.Distinct().ToList();

			// Offset functions by segment address and
			// try to fill in paramSize from FPO data.
			foreach (var func in functions)
			{
				if (func.Segment == 0xffffffff)
					continue;

				func.Offset += sections[(int)(func.Segment - 1)].VirtualAddress;
			}
			foreach (var func in functions)
			{
				if (func.Segment == 0xffffffff)
					continue;
				func.Offset += sections[(int)func.Segment - 1].VirtualAddress;
				if (!UpdateParamSize(func, fpov2Data))
				{
					if (!UpdateParamSize(func, fpov1Data))
					{
						UpdateParamSize(func, globals);
					}
				}
			}

			return true;
		}

		public bool ReadRootStream()
		{
			if (this.pdb_header.signature.SequenceEqual(PDBConsts.SignatureBytes))
			{
				var rootSize = (uint)this.pdb_header.directorySize;
				var numRootPages = Utils.GetNumPages(rootSize, this.page_size);
				var numRootIndexPages = Utils.GetNumPages(numRootPages * 4, this.page_size);
				int rootIndices = Marshal.SizeOf<PDBHeader>();
				var rootPageList = new List<uint>();
				for (int i = 0; i < numRootIndexPages; ++i)
				{
					var rootPages = BitConverter.ToUInt32(this.buffer, rootIndices + i * sizeof(uint)) * this.page_size;
					for (int j = 0; j < this.page_size / sizeof(uint); j++)
					{
						var rp = BitConverter.ToUInt32(this.buffer, (int)rootPages + j * sizeof(uint));
						rootPageList.Add(rp);
					}
				}

				uint pageIndex = 0;
				uint pageOffset = 0;
				var page = rootPageList[(int)pageIndex] * this.page_size;

				// The first 4 bytes are how many streams we actually need to read
				var numStreams = BitConverter.ToUInt32(this.buffer, (int)page);

				++pageOffset;
				this.streams = new List<StreamPair>((int)numStreams);

				var numItems = this.page_size / sizeof(uint);

				// Read all of the sizes for each stream, which directly determines how many
				// page indices we need to read after them
				{
					var streamIndex = 0;
					do
					{
						for (; streamIndex < numStreams && pageOffset < numItems; ++streamIndex, ++pageOffset)
						{
							var size = BitConverter.ToUInt32(this.buffer, (int)(page + pageOffset * sizeof(uint)));// page[pageOffset];
							if (size == 0xFFFFFFFF)
								streams.Add(new());
							else
								streams.Add(new(size));
						}

						// Advance to the next page
						if (pageOffset == numItems)
						{
							page = rootPageList[(int)++pageIndex] * this.page_size;
							pageOffset = 0;
						}
					} while (streamIndex < numStreams);
				}

				// For each stream, get the list of page indices associated with each one,
				// since any data associated with a stream are not necessarily adjacent
				for (int i = 0; i < numStreams; ++i)
				{
					var numPages = Utils.GetNumPages(streams[i].Size, this.page_size);

					if (numPages != 0)
					{
						streams[i].PageIndices.AddRange(Enumerable.Repeat<uint>(0,(int)numPages)); 

						var numToCopy = numPages;
						do
						{
							var num = Math.Min(numToCopy, numItems - pageOffset);

							//var src =this.buffer[(int)(page + pageOffset* sizeof(uint))..(int)(page + pageOffset* sizeof(uint) + sizeof(uint) * num)];

							for (int j = 0; j < num; ++j)
							{
                                streams[i].PageIndices[(int)(numPages - numToCopy + j)]=
									BitConverter.ToUInt32(
										this.buffer, (int)(page + pageOffset * sizeof(uint)+sizeof(uint)*j));
							}
							numToCopy -= num;
							pageOffset += num;
							if (pageOffset == numItems)
							{
								page = rootPageList[(int)++pageIndex] * this.page_size;
								pageOffset = 0;
							}
						} while (numToCopy != 0);
					}
				}

				uint numOk = 0;
				{
					PDBStreamReader nameReader = new(streams[1], this);

					var nameIndexHeader = nameReader.Read<NameIndexHeader>();
					this.m_guid = nameIndexHeader.guid;

					var nameStart = nameReader.Offset;

					PDBStreamReader mapReader = new(streams[1], this);
					mapReader.Seek(nameStart + nameIndexHeader.names);

					numOk = mapReader.Read<uint>();
					var count = mapReader.Read<uint>();

					var okOffset = mapReader.Offset;
					var skip = mapReader.Read<uint>();

					if ((count >> 5) > skip)
					{
						return false;
					}

					var okBits = mapReader.Reads<uint>((count >> 5) * sizeof(uint));
                    if (okBits.Length == 0)
                    {
						okBits = new uint[] { mapReader.Peak<uint>() };
                    }
					mapReader.Seek(okOffset + (skip + 1) * sizeof(uint));

					var deletedOffset = mapReader.Offset;
					var deletedskip = mapReader.Read<uint>();
					if (deletedskip != 0)
					{
						return false;
					}

					mapReader.Seek(deletedOffset + (deletedskip + 1) * sizeof(uint));

					for (var i = 0; i < count; ++i)
					{
                        if (0 != (okBits[i >> 5] & (1 << (i % 32))))
                            continue;

                        var val = mapReader.Read<StringVal>();
						nameReader.Seek(nameStart + val.Id);
						var name = nameReader.ReadString().ToUpper();

						name_indices.Add(name, val.Stream);
						numOk--;
					}
				}

				return numOk == 0;

			}

			return false;
		}

		// The name stream maps file indices with the path of the source file
		public bool LoadNameStream(NameStream names)
		{
			if (name_indices.TryGetValue("/NAMES", out var idx))
			{
				var ns = GetStream(idx)!;
				// Die in a fire microsoft.
				// Explanation - Every pdb I have tested puts streams in sequential order
				// so I assumed that was always the case, but no, apparently incorrect!
				// This was only encountered in one PDB in this one stream, so for now, just do this once.
				// Obviously the way to always be 100% correct all the time is to copy the entire stream into sequential
				// memory, but we don't want to do that if we don't have to
				names.Buffer = new byte[ns.Size];
				int last = ns.PageIndices.Count - 1;
				for (int i = 0, end = last; i < end; ++i)
				{
					//memcpy(tempData + i * m_pageSize, m_base + ns.pageIndices[i] * m_pageSize, m_pageSize);
					Array.Copy(this.buffer, ns.PageIndices[i] * this.page_size, names.Buffer, i * this.page_size, this.page_size);
				}

				// The last page may be shorter than the actual page size
				//memcpy(tempData + last * m_pageSize, m_base + ns.pageIndices[last] * m_pageSize, std::min(m_pageSize, ns.size - last * m_pageSize));
				Array.Copy(this.buffer, ns.PageIndices[last] * this.page_size, names.Buffer, last * this.page_size, Math.Min( this.page_size, ns.Size- last*this.page_size));

				var nsh = Utils.From<NameStreamHeader>(names.Buffer);

				if (nsh.Sig != 0xeffeeffe || nsh.Version != 1)
				{
					return false;
				}

				int nsl = Marshal.SizeOf<NameStreamHeader>();

				int offsets = nsl + nsh.Offset;

				uint size = BitConverter.ToUInt32(names.Buffer, offsets);
				offsets += sizeof(uint);
				int nameStart = nsl;

				for (int i = 0; i < size; ++i)
				{
					uint id = BitConverter.ToUInt32(names.Buffer, offsets);
					offsets += sizeof(uint);
					if (id != 0)
					{
						var text = Utils.ReadString(names.Buffer, (int)(nameStart + id));
						if (text!=null)
						{
							names.Dict.Add(id, text);
						}
					}
				}
			}
			return true;
		}
		// The type stream maps a type id to a description of that type
		public Dictionary<uint, TypeInfo> LoadTypeStream()
		{
			var ts = GetStream(KnownStreams.TypeInfoStream)!;
			if (ts.Size == 0) return new();

			PDBStreamReader reader = new(ts, this);

			this.type_info_header = reader.Read<TypeInfoHeader>();

			var map = new Dictionary<uint, TypeInfo>((int)(this.type_info_header.max - this.type_info_header.min));

			uint end = reader.Offset;

			for (int i = (int)this.type_info_header.min; i < this.type_info_header.max; ++i)
			{
				reader.Seek(end);
				reader.Align((uint)Marshal.SizeOf<TypeRecord>());

				var record = reader.Read<TypeRecord>();
				if (record.length == 0) return new();

				// No type, which can happen rarely, do NOT adjust when encountered
				if (record.leafType == 0)
					continue;

				end = reader.Offset + record.length - sizeof(ushort);

				var nfo = new TypeInfo();
				switch (nfo.Type = (LEAF)record.leafType)
				{
					case LEAF.LF_MODIFIER:
						nfo.Data = reader.ReadBytes(Marshal.SizeOf<LeafModifier>());
						break;
					case LEAF.LF_POINTER:
						nfo.Data = reader.ReadBytes(Marshal.SizeOf<LeafPointer>());
						break;
					case LEAF.LF_PROCEDURE:
						nfo.Data = reader.ReadBytes(Marshal.SizeOf<LeafProc>());
						break;
					case LEAF.LF_MFUNCTION:
						nfo.Data = reader.ReadBytes(Marshal.SizeOf<LeafMFunc>());
						break;
					case LEAF.LF_ARGLIST:
						{
							var count = reader.Peak<uint>();
							nfo.Data = reader.ReadBytes((int)(count + 1) * sizeof(uint));
						}
						break;
					case LEAF.LF_ARRAY:
						nfo.Data = reader.ReadBytes(Marshal.SizeOf<LeafArray>());
						break;
					case LEAF.LF_CLASS:
					case LEAF.LF_STRUCTURE:
						{
							reader.Seek(reader.Offset + (uint)Marshal.SizeOf<LeafClass>() + sizeof(ushort));
							nfo.Name = reader.ReadString();
						}
						break;
					case LEAF.LF_UNION:
						{
							reader.Seek(reader.Offset + (uint)Marshal.SizeOf<LeafUnion>() + sizeof(ushort));
							nfo.Name = reader.ReadString();
						}
						break;
					case LEAF.LF_ENUM:
						{
							reader.Seek(reader.Offset + (uint)Marshal.SizeOf<LeafEnum>());
							nfo.Name = reader.ReadString();
						}
						break;
					case LEAF.LF_ALIAS:
						{
							reader.Seek(reader.Offset + (uint)Marshal.SizeOf<LeafAlias>());
							nfo.Name = reader.ReadString();
						}
						break;
					case LEAF.LF_INDEX:
						nfo.Data = reader.ReadBytes(Marshal.SizeOf<LeafIndex>());
						break;
					default:
						{
							if (nfo.Type < LEAF.LF_NUMERIC || nfo.Type > LEAF.LF_UTF8STRING)
								continue;

							nfo.Data = reader.ReadBytes((int)(end - reader.Offset));
						}
						break;
				}
				map.Add((uint)i, nfo);
			}

			return map;
		}

		public void ReadSectionHeaders(uint headerStream, List<IMAGE_SECTION_HEADER> headers)
		{
			var hs = GetStream(headerStream)!;

			PDBStreamReader reader = new(hs, this);

			while (reader.Offset < hs.Size)
			{
				headers.Add(reader.Read<IMAGE_SECTION_HEADER>());
			}
		}
		public void ReadFPO(uint fpoStream, Dictionary<(uint, uint), FPO_DATA> fpoData)
		{
			var fs = GetStream(fpoStream)!;

			PDBStreamReader reader = new(fs, this);

			var last = new FPO_DATA();
			while (reader.Offset < fs.Size)
			{
				var fh = reader.Read<FPO_DATA>();
				// PDB files contain lots of duplicated FPO records.
				if (fh.ulOffStart != last.ulOffStart || fh.cbProcSize != last.cbProcSize
					|| (fh.misc & 0xff) != (last.misc & 0xff))
				{
					last = fh;
					fpoData.Add((fh.ulOffStart, fh.cbProcSize), fh);
				}
			}
		}
		public void ReadFPO(uint fpoStream, Dictionary<(uint, uint), FPO_DATA_V2> fpoData)
		{
			var fs = GetStream(fpoStream)!;

			PDBStreamReader reader = new(fs, this);

			var last = new FPO_DATA_V2();
			while (reader.Offset < fs.Size)
			{
				var fh = reader.Read<FPO_DATA_V2>();
				// PDB files contain lots of duplicated FPO records.
				if (fh.ulOffStart != last.ulOffStart || fh.cbProcSize != last.cbProcSize
					|| (fh.cbProlog) != (last.cbProlog))
				{
					last = fh;
					fpoData.Add((fh.ulOffStart, fh.cbProcSize), fh);
				}
			}
		}

		public void GetGlobalFunctions(ushort symRecStream, List<IMAGE_SECTION_HEADER> headers, Dictionary<uint, (string,GlobalRecord)> globals)
		{
			var pair = GetStream(symRecStream)!;
			PDBStreamReader reader = new(pair, this);

			while (reader.Offset < pair.Size)
			{
				var len = reader.Read<ushort>();
				var gcs = Marshal.SizeOf<GlobalRecord>();
				if (len >= gcs)
				{
					var rec = reader.Read<GlobalRecord>();
					var name = Encoding.Latin1.GetString(reader.ReadBytes(len - gcs));

					// Is function?
					if (rec.symType == 2)
					{
						var rva = rec.offset + headers[rec.segment - 1].VirtualAddress;
                        if (!globals.ContainsKey(rva))
                        {
							globals.Add(rva,(name,rec));
						}
					}
				}
				else
				{
					// Just skip this data, we don't know how to handle it.
					reader.Seek(reader.Offset + len);
				}
			}
		}
		public void ResolveFiles(ref uint id, PDBStreamReader reader, int sig, uint end, Dictionary<uint, UniqueSrc> unique, Dictionary<uint, uint> fileIndices)
		{
			uint index = reader.Offset;

			while (reader.Offset < end)
			{
				uint msid = reader.Offset - index;
				var fileChk = reader.Read<CVFileChecksum>();

				if (!unique.TryGetValue(fileChk.name, out var fiter))
				{
					var fileid = unique[fileChk.name] = new UniqueSrc();
					fileid.Id = id++;
				}
				else
				{
					id++;
				}
				fileIndices.Add(msid, fileChk.name);

				// Skip past the actual checksum itself
				reader.Seek(reader.Offset + fileChk.len);
				reader.Align(4);
			}

		}

		public bool GetModuleFiles(DBIModuleInfo module, ref uint id, Dictionary<uint, UniqueSrc> unique, Dictionary<uint, uint> fileIndices)
		{
			int section = (int)Subsection.FileChecksums;
			var pair = GetStream((uint)module.stream)!;

			PDBStreamReader reader = new(pair, this);
			var sig = reader.Read<int>();

			if (sig != 4)
				return false;

			// Skip functions
			reader.Seek((uint)(module.cbSyms + module.cbOldLines));
			uint endOffset = reader.Offset + (uint)module.cbLines;

			while (reader.Offset < endOffset)
			{
				var header = reader.Read<SubsectionHeader>();

				if (header.Sig == 0 || 0 != (header.Sig & (uint)Subsection.Ignore))
					continue;

				uint end = reader.Offset + (uint)header.Size;

				if (header.Sig == section)
                {
					ResolveFiles(ref id,reader, sig, end, unique, fileIndices);
				}

				reader.Seek(end);

				reader.Align(4);
			}
			return true;

		}

		public bool GetModuleFunctions(DBIModuleInfo module, List<FunctionRecord> funcs)
		{
			var pair = GetStream((uint)module.stream)!;

			PDBStreamReader reader = new(pair, this);
			var sig = reader.Read<int>();

			if (sig != 4)
				return false;


			uint end = (uint)module.cbSyms;

			while (reader.Offset < end)
			{
				var header = reader.Read<SymbolHeader>();

				uint offsetBeg = reader.Offset - sizeof(ushort);

				switch ((SymbolDefs)header.Type)
				{
					case SymbolDefs.S_GPROC32:
					case SymbolDefs.S_LPROC32:
						{
							var proc = reader.Read<ProcSym32>();
							var name = reader.ReadString();

							FunctionRecord rec = new(name);
							rec.Offset = proc.off;
							rec.Segment = proc.seg;
							rec.Length = proc.len;
							rec.TypeIndex = proc.typind;

							funcs.Add(rec);
						}
						break;
					case SymbolDefs.S_THUNK32:
						{
							var thunk = reader.Read<ThunkSym32>();
							var name = reader.ReadString();

							FunctionRecord rec = new(name);
							rec.Offset = thunk.off;
							rec.Segment = thunk.seg;
							rec.Length = thunk.parent != 0 ? (uint)thunk.len : 0;

							funcs.Add(rec);
						}
						break;
					default:
						break;
				}

				reader.Seek(offsetBeg + header.Size);
			}
			return true;
		}

		public bool ResolveFunctionLines(DBIModuleInfo module, List<FunctionRecord> funcs,
			Dictionary<uint, UniqueSrc> unique, Dictionary<uint, uint> fileIndex)
		{
			int section = (int)Subsection.Lines;

			var pair = GetStream((uint)module.stream)!;

			PDBStreamReader reader = new(pair, this);
			var sig = reader.Read<int>();

			if (sig != 4)
				return false;

			// Skip functions
			reader.Seek((uint)(module.cbSyms + module.cbOldLines));
			uint endOffset = reader.Offset + (uint)module.cbLines;

			while (reader.Offset < endOffset)
			{
				var header = reader.Read<SubsectionHeader>();

				if (header.Sig == 0 || 0 != (header.Sig & (uint)Subsection.Ignore))
					continue;

				uint end = reader.Offset + (uint)header.Size;

				if (header.Sig == section)
                {
					ResolveLines(reader,sig,end,funcs,unique,fileIndex);
				}

				reader.Seek(end);

				reader.Align(4);
			}
			return true;

		}

		protected void ResolveLines(PDBStreamReader reader, int sig, uint end, List<FunctionRecord> funcs, Dictionary<uint, UniqueSrc> unique, Dictionary<uint, uint> fileIndex)
		{
			var line_section = reader.Read<CV_LineSection>();

			int min = 0;
			int max = funcs.Count - 1;
			while (min < max)
			{
				int mid = (min + max) >> 1;

				var func = funcs[mid];
				if (func.Segment < line_section.sec || (func.Segment == line_section.sec && func.Offset < line_section.off))
					min = mid + 1;
				else
					max = mid;
			}

			var function = funcs[min];
			if (function.LineOffset != 0 && (function.LineOffset - function.Offset < line_section.off - function.Offset
				|| 0 != (function.LineCount & 0xF0000000))) // This means the first function always wins, which seems to be the behavior of the original Breakpad implementation
				return;

			var srcfile = reader.Read<CV_SourceFile>();

			// First find the module specific file offset
			var fileChk = fileIndex[srcfile.index];

			// Next get the unique id that is paired with that particular file
			function.FileIndex = unique[fileChk].Id;

			function.LineCount = srcfile.count;
			function.LineOffset = line_section.off;

			if (function.LineCount > 0)
				function.Lines = reader.ReadBytes((int)srcfile.count * Marshal.SizeOf<CV_Line>());

			// Mark that the function has been encountered
			function.LineProcessed = true;

		}
		protected bool UpdateParamSize(FunctionRecord func, Dictionary<(uint, uint), FPO_DATA> fpoData)
		{
			var p = (func.Offset, func.Length);
			if (fpoData.TryGetValue(p,out var it))
			{
				UpdateParamSize(func, it);
				return true;
			}
			return false;
		}
		protected bool UpdateParamSize(FunctionRecord func, Dictionary<(uint, uint), FPO_DATA_V2> fpoData)
		{
			var p = (func.Offset, func.Length);
			if (fpoData.TryGetValue(p, out var it))
			{
				UpdateParamSize(func, it);
				return true;
			}
			return false;
		}
		protected bool UpdateParamSize(FunctionRecord func, Dictionary<uint, (string name, GlobalRecord record)> globals)
		{
			if (globals.TryGetValue(func.Offset,out var g))
			{
				var name = g.name;
				// stdcall and fastcall functions have their param size embedded in the decorated name
				if (!string.IsNullOrEmpty(name) &&(name[0] == '@' || name[0] == '_'))
				{
					int i = name.LastIndexOf('@');
					if (i>=0)
					{
						if (uint.TryParse(name[i..],out var val))
						{
							func.ParamSize = val;
							// fastcall functions accept up to 8 bytes of parameters in registers
							if (name[0] == '@')
							{
								if (val > 8)
								{
									func.ParamSize -= 8;
								}
								else
								{
									func.ParamSize = 0;
								}
							}
							return true;
						}
					}
				}
			}
			return false;
		}
		protected void UpdateParamSize(FunctionRecord func, FPO_DATA fpoData)
		{
			func.ParamSize =(uint) fpoData.cdwParams * 4;
		}
		protected void UpdateParamSize(FunctionRecord func, FPO_DATA_V2 fpoData)
		{
			func.ParamSize = fpoData.cbParams;
		}
	}
}
