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

        public Blob(ObjectLoader loader)
        {
            this.Id = loader.Id;
            this._loader = loader;
        }
    }


    public class Tree 
    {
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
        public Tree Tree { get; private set; }
        public Commit[] Parents { get; private set; }

        public string Commiter { get; private set; }
        public DateTime CommitedAt { get; private set; }

        public string Author { get; private set; }
        public DateTime AuthoredAt { get; private set; }

        public string Message { get; private set; }

        public string Id
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        internal Commit()
        {
            
        }
    }

    class Tag
    {
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
        public ObjectLoadInfo Load(ContentLoader contentLoader)
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
