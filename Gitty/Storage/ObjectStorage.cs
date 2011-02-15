using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Gitty.Storage
{
    class ObjectStorage
    {
        public Repository Repository { get; private set; }
        public string Location { get; private set; }
        public string PacksLocation { get; private set; }


        public ObjectStorage(string objectsLocation)
        {
            this.Location = objectsLocation;
            this.PacksLocation = Path.Combine(objectsLocation, "pack");
        }

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
            {
                type = ObjectType.Undefined;
                return null;
            }

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
            //TODO: need to get rid of repository and just use ObjectsLocation
            var loader = LooseObjectReader.GetObjectLoader(this.Location, id);
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

        #region helper methods


        private bool LooseObjectExists(string id)
        {
            var location = Path.Combine(this.Location, id.Substring(0, 2), id.Substring(2));
            return File.Exists(location);
        }
        #endregion

    }
}
