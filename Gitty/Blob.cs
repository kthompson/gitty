using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.AccessControl;
using System.Text;

namespace Gitty
{
    class Blob 
    {
        private readonly ObjectLoader _loader;

        public void GetContentStream(ObjectLoader.ContentLoader contentLoader)
        {
            this._loader.Load(contentLoader);
        }

        public string Id { get; private set; }

        internal Blob(Repository repository, ObjectLoader loader)
        {
            this.Id = loader.Id;
            this._loader = loader;
        }
    }


    public class Tree 
    {
        private readonly ObjectLoader _loader;

        public Tree(Repository repository, ObjectLoader loader)
        {
            _loader = loader;
            this.Id = loader.Id;
        }

        public TreeEntry[] Items { get; private set; }

        public string Id { get; private set; }

    }

    public class TreeEntry
    {
        public string Id { get; private set; }
        public string Type { get; private set; }
        public string Mode { get; private set; }
        public string Name { get; private set; }
        public object Entry { get; private set; }
    }

    public class Commit
    {
        private readonly Repository _repository;
        private readonly ObjectLoader _loader;

        private Tree _tree;
        public Tree Tree
        {
            get
            {
                this.EnsureLoaded();
                return _tree;
            }
        }

        private readonly List<Commit> _parents;
        public Commit[] Parents
        {
            get
            {
                this.EnsureLoaded();
                return _parents.ToArray();
            }
        }

        private string _commiter;
        public string Commiter
        {
            get
            {
                this.EnsureLoaded();
                return _commiter;
            }
        }

        private string _author;
        public string Author
        {
            get
            {
                this.EnsureLoaded(); 
                return _author;
            }
        }

        private string _message;
        public string Message
        {
            get
            {
                this.EnsureLoaded();
                return _message;
            }
        }

        public string Id { get; private set; }

        internal Commit(Repository repository, ObjectLoader loader)
        {
            _repository = repository;
            _loader = loader;

            this.Id = loader.Id;
            this._parents = new List<Commit>();
        }

        private bool _loaded;
        private void EnsureLoaded()
        {
            if (_loaded) 
                return;

            this._loader.Load((stream, info) =>
            {
                var bytesRead = 0;
                var reader = new StreamReader(stream);
                string line;
                while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    bytesRead += line.Length + 1; //add 1 for LF
                    var parts = line.Split(new[] {' '}, 2);
                    switch (parts[0])
                    {
                        case "tree":
                            this._tree = _repository.OpenObject(parts[1]) as Tree;
                            break;
                        case "author":
                            this._author = parts[1];
                            break;
                        case "committer":
                            this._commiter = parts[1];
                            break;
                        case "parent":
                            var parent = _repository.OpenObject(parts[1]) as Commit;
                            if(parent != null)
                                this._parents.Add(parent);
                            break;
                        default:
                            throw new NotSupportedException(string.Format("{0} is not a supported commit field.", parts[0]));
                    }
                }

                var messageSize = info.Size - bytesRead;
                var buffer = new char[messageSize];
                var read = reader.Read(buffer, 0, buffer.Length);
                this._message = new string(buffer, 0, read);
            });

            this._loaded = true;
        }
    }

    class Tag
    {
        private readonly ObjectLoader _loader;

        public Tag(Repository repository, ObjectLoader loader)
        {
            _loader = loader;
            this.Id = loader.Id;
        }

        public string Id { get; private set; }

        public string Name { get; private set; }
        
        public string Type { get; private set; }
        public object Object { get; private set; }

        public string Tagger { get; private set; }
        public DateTime TaggedAt { get; private set; }

        public string Message { get; private set; }
    }

    public class ObjectLoadInfo
    {
        public string Type { get; private set; }
        public int Size { get; private set; }

        public ObjectLoadInfo(string type, int size)
        {
            this.Type = type;
            this.Size = size;
        }
    }

    public abstract class ObjectLoader
    {
        public string Id { get; private set; }
        public Repository Repository { get; private set; }
        public abstract bool Exists { get; }

        protected ObjectLoader(Repository repository, string id)
        {
            this.Repository = repository;
            this.Id = id;
        }

        protected abstract Stream OpenStream();

        public delegate void ContentLoader(Stream stream, ObjectLoadInfo loadInfo);
        public ObjectLoadInfo Load(ContentLoader contentLoader = null)
        {
            
            var size = 0;
            var type = string.Empty;
            var inner = OpenStream();

            // this is a hack to get the DeflateStream to read properly
            inner.ReadByte();
            inner.ReadByte();

            using (var stream = new DeflateStream(inner, CompressionMode.Decompress))
            {
                var sb = new StringBuilder();
                var inHeader = true;

                while (inHeader)
                {
                    var c = (char) stream.ReadByte();
                    switch (c)
                    {
                        case ' ':
                            type = sb.ToString();
                            sb.Clear();
                            continue;
                        case '\0':
                            size = int.Parse(sb.ToString());
                            sb.Clear();
                            inHeader = false;
                            continue;
                    }
                    sb.Append(c);
                }
                var loadInfo = new ObjectLoadInfo(type, size);
                if(contentLoader != null)
                    contentLoader(stream, loadInfo);
                return loadInfo;
            }
        }

        public static ObjectLoader Create(Repository repository, string id)
        {
            ObjectLoader loader = new LooseObjectLoader(repository, id);
            if (loader.Exists)
                return loader;

            loader = new PackedObjectLoader(repository, id);
            return loader.Exists ? loader : null;
        }

        public static ContentLoader DefaultContentLoader(Stream stream)
        {
            return (read, loadInfo) =>
            {
                var size = loadInfo.Size;
                while (size > 0)
                {
                    var bsize = size > 4096 ? 4096 : size;
                    var buffer = new byte[bsize];
                    var rsize = read.Read(buffer, 0, buffer.Length);
                    if (rsize == 0)
                        return;

                    size -= rsize;
                    stream.Write(buffer, 0, rsize);
                }
                stream.Position = 0;
            };
        }
    }

    class LooseObjectLoader : ObjectLoader
    {
        public string Location { get; private set; }

        public LooseObjectLoader(Repository repository, string id)
            : base(repository, id)
        {
            this.Location = Path.Combine(repository.Location, "objects", id.Substring(0, 2), id.Substring(2));
        }

        public override bool Exists
        {
            get { return File.Exists(this.Location); }
        }

        protected override Stream OpenStream()
        {
            return new FileStream(this.Location, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }

    class PackedObjectLoader : ObjectLoader
    {
        public PackedObjectLoader(Repository repository, string id)
            : base(repository, id)
        {
        }

        public override bool Exists
        {
            get { throw new NotImplementedException(); }
        }

        protected override Stream OpenStream()
        {
            throw new NotImplementedException();
        }
    }
}
