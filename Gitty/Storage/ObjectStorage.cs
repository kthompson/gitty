using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Gitty.Storage
{
    class ObjectStorage
    {
        #region Properties
        public string InfoLocation { get; private set; }
        public string ObjectsLocation { get; private set; }

        public IEnumerable<PackFile> PackFiles
        {
            get
            {
                //TODO we should be holding on to these pack file instances
                var packs = new DirectoryInfo(this.PacksLocation);
                if (packs.Exists)
                    return packs
                        .EnumerateFiles("*.pack")
                        .Select(pf => new PackFile(pf.FullName));

                return new PackFile[] { };
            }
        }
        public string PacksLocation { get; private set; }
        #endregion
        
        #region Constructors
        public ObjectStorage(string gitLocation, bool create)
        {
            this.ObjectsLocation = Path.Combine(gitLocation, "objects");
            this.PacksLocation = Path.Combine(this.ObjectsLocation, "pack");
            this.InfoLocation = Path.Combine(this.ObjectsLocation, "info");

            if (!create) 
                return;

            //.git/objects
            Directory.CreateDirectory(this.ObjectsLocation);
            //.git/objects/pack
            Directory.CreateDirectory(this.PacksLocation);
            //.git/objects/info
            Directory.CreateDirectory(this.InfoLocation);
        }
        #endregion

        #region readers

        public T Read<T>(string id, Tree parent = null, string name = null, string mode = null)
            where T : class
        {
            return Read(id, parent, name, mode) as T;
        }

        public AbstractObject Read(string id, Tree parent = null, string name = null, string mode = null)
        {
            ObjectType type;
            return Read(id, out type, parent, name, mode);
        }

        public AbstractObject Read(string id, out ObjectType type, Tree parent = null, string name = null, string mode = null)
        {
            var reader = CreateReader(id);
            if (reader == null)
                return ReadWithoutStorage(id, parent, name, mode, out type);

            type = reader.Type;

            switch (reader.Type)
            {
                case ObjectType.Tree:
                    return CreateTree(id, reader, parent, name, mode);
                case ObjectType.Blob:
                    return CreateBlob(id, reader, parent, name, mode);
                case ObjectType.Commit:
                    return CreateCommit(id, reader);
                case ObjectType.Tag:
                    return CreateTag(id, reader);
                case ObjectType.OffsetDelta:
                case ObjectType.ReferenceDelta:
                default:
                    throw new NotSupportedException(string.Format("Object Type ({0}) for object ({1}) not supported at this time.", reader.Type, id));
            }
        }

        private static AbstractObject ReadWithoutStorage(string id, Tree parent, string name, string mode, out ObjectType type)
        {
            TreeEntry entry = null;
            if (!string.IsNullOrEmpty(mode))
            {
                var modeValue = FileMode.FromOctal(mode);

                if (FileMode.IsGitLink(modeValue))
                {
                    entry = new GitLink(id, parent, name);
                }
                else if (FileMode.IsSymlink(modeValue))
                {
                    entry = new Symlink(id, parent, name);
                }
            }

            if(entry != null)
            {
                type = entry.Type;
                return entry;
            }

            type = ObjectType.Undefined;
            return null;
        }

        private Tag CreateTag(string id, ObjectReader reader)
        {
            return new Tag(this, reader, id);
        }

        private Commit CreateCommit(string id, ObjectReader reader)
        {
            return new Commit(this, reader, id);
        }

        private Tree CreateTree(string id, ObjectReader reader, Tree parent = null, string name = null, string mode = null)
        {
            return new Tree(this, id, reader.Size, LoadDataFromReader(reader), parent, name, mode);
        }

        private static Blob CreateBlob(string id, ObjectReader reader, Tree parent = null, string name = null, string mode = null)
        {
            return new Blob(id, reader.Size, LoadDataFromReader(reader), parent, name, mode);
        }

        private static Func<byte[]> LoadDataFromReader(ObjectReader reader)
        {
            return () =>
                       {
                           var bytes = new byte[reader.Size];
                           reader.Load(stream => stream.Read(bytes, 0, bytes.Length));
                           return bytes;
                       };
        }

        private ObjectReader CreateReader(string id)
        {
            var loader = LooseObjectReader.GetObjectLoader(this.ObjectsLocation, id);
            if (loader != null)
                return loader;

            var pf = this.PackFiles.Where(pack => pack.HasEntry(id)).FirstOrDefault();
            if (pf != null)
                return pf.GetObjectLoader(id);

            return null;
        }

        #endregion

        #region writers

        public void Write(Commit commit)
        {
            throw new NotImplementedException();
        }

        public void Write(Tree tree)
        {
            throw new NotImplementedException();
        }

        public void Write(Blob blob)
        {
            throw new NotImplementedException();
        }

        public void Write(Tag tag)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
