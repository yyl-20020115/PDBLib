using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PDBLib
{
    public class PDBGenerator : IPDBGenerator
    {
        public static readonly int PDBHeaderLength = Marshal.SizeOf<PDBHeader>();
        public static readonly int DBIHeaderLength = Marshal.SizeOf<DBIHeader>();
        public static readonly int DBIDebugHeaderLength = Marshal.SizeOf<DBIDebugHeader>();
        public static readonly int TypeInfoHeaderLength = Marshal.SizeOf<TypeInfoHeader>();
        public int PageSize { get; protected set; } = PDBConsts.DefaultPageAlignmentSize;
        protected PDBHeader pdb_header = new();
        protected NameStreamHeader name_header = new();
        protected NameIndexHeader name_index_header = new();
        protected DBIHeader dbi_header = new();
        protected DBIDebugHeader dbi_debug_header = new();
        protected TypeInfoHeader type_info_header = new();
        protected List<IMAGE_SECTION_HEADER> sections = new();
        protected Dictionary<uint, UniqueSrc> unique = new();
        protected Dictionary<string, uint> name_indices = new();
        protected Dictionary<uint, string> stream_names = new();
        protected SortedDictionary<uint,PDBStreamWriter> streams = new();

        public PDBStreamWriter GetStream(KnownStream stream) => this.GetStream((uint)stream);
        public PDBStreamWriter GetStream(uint idx)
        {
            if(!this.streams.TryGetValue(idx, out var writer))
            {
                this.streams.Add(idx, writer = new());
            }
            return writer;
        }
        protected uint uid = 0;
        protected void RegisterStrings(params string[] texts)
        {
            this.RegisterStrings(texts as IEnumerable<string>);
        }
        protected void RegisterStrings(IEnumerable<string> texts)
        {
            foreach(var text in texts)
            {
                this.RegisterString(text);
            }
        }
        protected uint RegisterString(string text)
        {
            if(!this.name_indices.TryGetValue(text,out var id))
            {
                this.name_indices.Add(text, id = uid++);
            }
            return id;
        }
        protected Dictionary<string,uint> RegisterStrings(PDBDocument doc)
        {
            this.RegisterString(PDBConsts.NameStreamName);
            this.RegisterString(doc.Creator);
            this.RegisterString(doc.Version);
            this.RegisterString(doc.Language);
            this.RegisterString(doc.Machine);
            doc.Names.ForEach(n => this.RegisterString(n));
            doc.Globals.ForEach(g => this.RegisterStrings(g.Name, g.SymType, g.LeafType));
            doc.Modules.ForEach(m => { 
                this.RegisterStrings(m.ModuleName); 
                this.RegisterStrings(m.Sources);
                this.RegisterStrings(m.FunctionNames); 
            });
            doc.Functions.ForEach(f => this.RegisterStrings(f.Name,f.Source));
            doc.Types.ForEach(t => this.RegisterStrings(t.Collect()));
            return this.name_indices;
        }
        protected PDBStreamWriter BuildNameStream(PDBDocument doc)
        {
            var names = this.RegisterStrings(doc);
            if(names.TryGetValue(PDBConsts.NameStreamName, out var idx))
            {
                var stream = this.GetStream(idx);
                stream.Reserve<NameStreamHeader>();
                int offset = 0;
                var texts = names.Keys.ToArray();
                var offsets = new List<int>();
                for(int i = 0; i < texts.Length; i++)
                {
                    offsets.Add(offset);
                    offset += stream.Write(texts[i]);
                }
                stream.Align(4);
                //size
                stream.Write(texts.Length);
                //offsets
                for(int i = 0; i < offsets.Count; i++)
                {
                    stream.Write(offsets[i]);
                    offset += sizeof(int);
                }
                stream.Rewind();
                this.name_header.Sig = PDBConsts.NameStreamSignature;
                this.name_header.Version = PDBConsts.NameStreamVersion;
                this.name_header.OffsetsOffset =(int) offset;
                stream.Write(this.name_header);
                return stream;
            }
            return new();
        }
        protected PDBStreamWriter BuildDebugStream(PDBDocument doc, Dictionary<string,uint> names, ref ushort freeStreamIdx)
        {
            var stream = this.GetStream(KnownStream.DebugInfo);
            if (stream != null)
            {
                var infos = new Dictionary<PDBModule, DBIModuleInfo>();

                stream.Reserve<DBIHeader>();
                int offset = stream.Length;
                for(int i = 0; i < doc.Modules.Count; i++)
                {
                    var module = doc.Modules[i];
                    var info = new DBIModuleInfo();
                    info.cbSyms = 0;
                    info.cbOldLines = 0;
                    //TODO: fill info
                    info.stream = (short)freeStreamIdx++;

                    stream.Write(info);
                    stream.Write(module.ModuleName);
                    stream.Write(module.ObjectName);
                    stream.Align(4);
                    infos.Add(module,info);
                }
                this.dbi_header.moduleSize = (uint)(stream.Length - offset);

                stream.Rewind();
                this.dbi_header.age = 2;
                this.dbi_header.dbgHeaderSize = (uint)Marshal.SizeOf<DBIDebugHeader>();
                this.dbi_header.reserved = 0;
                //TODO: other things
                stream.Write(this.dbi_header);
                //TODO: fill debug headers
                stream.Write(this.dbi_debug_header);
                if (doc.SectionHeaders.Count > 0)
                {
                    var ss = this.GetStream(this.dbi_debug_header.sectionHdr = freeStreamIdx++);
                    for(int i= 0; i < doc.SectionHeaders.Count; i++)
                    {
                        ss.Write(doc.SectionHeaders[i]);
                    }
                }
                foreach(var pair in infos)
                {
                    var info = pair.Value;
                    var info_stream = this.GetStream((uint)info.stream);
                    if (info_stream != null)
                    {
                        const int sig = 4;
                        info_stream.Write(sig);

                        //TODO:
                        info.cbSyms = 0;
                        for(int i = 0; i < pair.Key.Sources.Count; i++)
                        {
                            var source = pair.Key.Sources[i];
                            if(names.TryGetValue(source, out var name_idx))
                            {
                                var subsection_header = new SubsectionHeader();
                                subsection_header.Sig = (int)Subsection.FileChecksums;
                                var of = info_stream.Length;
                                var fc = new CVFileChecksum();
                                fc.len = 16;
                                fc.type = 1;

                                fc.name = name_idx;
                                info_stream.Write(fc);
                                info_stream.Seek(of + fc.len);
                                info_stream.Align(4);
                            }
                        }
                    }
                }
                //TODO: module functions
                //TODO: global functions

            }
            return stream ?? new ();
        }
        
        protected PDBStreamWriter BuildTypeStream(PDBDocument doc) 
        {
            var stream = this.GetStream(KnownStream.TypeInfoStream)!;
            if (stream != null)
            {
                stream.Reserve<TypeInfoHeader>();
            //TODO:

                stream.Rewind();
                stream.Write(this.type_info_header);
            }
            return stream ?? new();
        }
        protected PDBStreamWriter BuildGlobalStream(PDBDocument doc)
        {
            var stream = this.GetStream(KnownStream.TypeInfoStream)!;
            if (stream != null)
            {

            }
            return stream ?? new();
        }
        protected PDBStreamWriter BuildStreamsNamesStream(Dictionary<uint,string> stream_names)
        {
            var streams_stream = this.GetStream(KnownStream.Streams)!;
            if (streams_stream != null)
            {
                this.name_index_header.age = 2;
                this.name_index_header.timeDateStamp = 0;
                this.name_index_header.guid = Guid.NewGuid();
                this.name_index_header.version = 20000404;
                this.name_index_header.names = 34;

                streams_stream.Write(this.name_index_header);
                var nameStart = streams_stream.Length;
                streams_stream.Write(stream_names.Count); //numOK
                //numOk
                //skp
                //deletedskip
                for(int i = 0; i < stream_names.Count >> 5; i++)
                {
                    streams_stream.Write(0xffffffff); //all oks
                }
                streams_stream.Write(stream_names.Count); //total_count
                var okOffset = streams_stream.Length;
                var skip = 0;
                streams_stream.Write(skip); //skip = 0
                streams_stream.Seek(okOffset + (skip + 1) * sizeof(uint));
                var deletedskip = 0;
                var deletedOffset = streams_stream.Length;
                streams_stream.Write(deletedskip);
                streams_stream.Seek(deletedOffset + (deletedskip + 1) * sizeof(uint));

                var record = streams_stream.Length;
                var offset = 0u;
                var texts = new List<string>();
                var sids = new List<StringStreamIds>();
                var ids = new List<uint>();
                foreach (var name in stream_names)
                {
                    var sid = new StringStreamIds();
                    sid.StreamId = name.Key;
                    texts.Add(name.Value);
                    ids.Add(sid.StringId = 0);
                    sids.Add(sid);
                }

                streams_stream.Reserve(stream_names.Count * Marshal.SizeOf<StringStreamIds>());

                for(int i = 0; i < texts.Count; i++)
                {
                    ids[i] = offset;
                    offset += (uint)streams_stream.Write(texts[i]);
                }
                streams_stream.Seek(record);
                for (int i = 0; i < texts.Count; i++)
                {
                    streams_stream.Write(sids[i]);
                }
            }
            return streams_stream ?? new();

        }
        //  PN  Type/Name   Description
        //  0   HDR hdr     page 0: master index
        //  1   FPM fpm0    first free page map
        //  2   FPM fpm1    second free page map
        public int GetMasterIndexPagesCount()
        {
            int total_size_of_all_aligned_streams
                = this.streams.Values.Sum(s => s.GetAlignedSize(this.PageSize));

            int total_pages_of_all_aligned_streams
                = Utils.GetNumPages(total_size_of_all_aligned_streams, this.PageSize);

            int total_pages_of_page_indices
                = Utils.GetNumPages(total_pages_of_all_aligned_streams * sizeof(uint), this.PageSize);

            int total_pages_of_master_index
                = 2/*FreePagesMap*/ + Utils.GetNumPages(PDBHeaderLength /*Header*/
                    + total_pages_of_page_indices * sizeof(uint), this.PageSize);
            return total_pages_of_master_index;
        }
        protected bool BuildRootStream(PDBFileWriter writer, SortedDictionary<int, List<int>> root_pages_offset_keys_and_page_indices)
        {
            //should not happen
            if (root_pages_offset_keys_and_page_indices.Count * sizeof(uint) > (this.PageSize - PDBHeaderLength))
                return false;

            int fpm_index = 0;
            int total_size_of_all_aligned_streams 
                = this.streams.Values.Sum(s => s.GetAlignedSize(this.PageSize));

            int total_pages_of_all_aligned_streams
                = Utils.GetNumPages(total_size_of_all_aligned_streams, this.PageSize);

            int total_pages_of_page_indices
                = Utils.GetNumPages(total_pages_of_all_aligned_streams * sizeof(uint), this.PageSize);

            int total_pages_of_master_index
                = 2/*FreePagesMap*/ + Utils.GetNumPages(PDBHeaderLength /*Header*/ 
                    + total_pages_of_page_indices * sizeof(uint), this.PageSize);

            int total_pages = total_pages_of_master_index + total_pages_of_all_aligned_streams;

            var all_page_indices = new List<int>();
            
            //Keep space for header
            writer.Reserve<PDBHeader>();
            //Master Index
            foreach(var root_page in root_pages_offset_keys_and_page_indices)
            {
                writer.Write(root_page.Key);
                int location = writer.Length;
                {
                    writer.Seek(root_page.Key * this.PageSize);
                    foreach (var local_page_index in root_page.Value)
                    {
                        writer.Write(local_page_index);
                        all_page_indices.Add(local_page_index);
                    }
                }
                writer.Seek(location);
            }
            //maybe zeros following
            writer.Align(this.PageSize);
            //
            fpm_index = Utils.GetNumPages(writer.Length, this.PageSize);           
            writer.Reserve(this.PageSize * 2);
            int directorySize = writer.Length;

            //streams and count
            int page_index = 0;
            int page_offset = (all_page_indices[page_index++]) * this.PageSize;
            
            //stream count and all stream sizes
            writer.Seek(page_offset);
            writer.Write(this.streams.Count); //save count of streams

            var stream_numbers = new List<uint>(this.streams.Keys);
            int index_count_for_one_page = this.PageSize / sizeof(uint);
            int stream_index = 0;
            int first = 1;
            //all sizes of streams
            while(page_index<all_page_indices.Count 
                && stream_index < stream_numbers.Count 
                && stream_index < index_count_for_one_page)
            {
                var stream_number = stream_numbers[stream_index++];
                if(this.streams.TryGetValue(stream_number,out var stream)){
                    writer.Write(stream.Length);
                }
                if((stream_index + first) % index_count_for_one_page == 0)
                {
                    writer.Seek(all_page_indices[page_index++] * this.PageSize);
                    first = 0;
                }
            }

            var post_page_table_offset = writer.Length;
            //write header
            {
                Array.Copy(PDBConsts.SignatureBytes, this.pdb_header.Signature, PDBConsts.SignatureBytes.Length);
                this.pdb_header.PageSize = PDBConsts.DefaultPageAlignmentSize;
                this.pdb_header.FreePageMapIndex = fpm_index; //NOTICE: USE Free Page Map at Page Index 1
                this.pdb_header.PagesUsed = total_pages; //NOTICE: all pages
                this.pdb_header.DirectorySize = directorySize;
                this.pdb_header.Reserved = 0;

                //Write pdb header
                writer.Rewind();
                writer.Write(this.pdb_header);
            }

            //write following streams
            writer.Seek(post_page_table_offset);
            {
                //write streams
                foreach (var s in this.streams.Values)
                {
                    writer.WriteBytes(s.ToArray());
                }
            }
            return true;
        }
        public bool Load(PDBDocument doc)
        {
            //setup streams
            if (doc != null)
            {
                ushort freeStreamIdx = 0;

                this.BuildNameStream(doc);
                this.BuildTypeStream(doc);  
                this.BuildDebugStream(doc, this.name_indices, ref freeStreamIdx);
                this.BuildGlobalStream(doc);
                //TODO: name streams
                this.BuildStreamsNamesStream(this.stream_names);

                return true;
            }
            return false;
        }
        public bool Generate(string path)
        {
            using var stream = File.OpenWrite(path);
            return this.Generate(stream);
        }
        public bool Generate(Stream stream)
        {
            int page_after_master_index_Pages = GetMasterIndexPagesCount();
            var dict = new SortedDictionary<int, List<int>>();
            var list = new List<int>();
            dict.Add(page_after_master_index_Pages, list);
            foreach (var _stream in this.streams)
            {
                list.AddRange(
                    _stream.Value.Complete(ref page_after_master_index_Pages, this.PageSize));
            }
            return this.BuildRootStream(new (stream), dict);
        }
    }
}
