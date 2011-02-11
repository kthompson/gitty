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


        public ObjectStorage(Repository repository)
        {
            this.Repository = repository;
            this.Location = repository.ObjectsLocation;
            this.PacksLocation = repository.PacksLocation;
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

        public T Read<T>(string id)
            where T : class
        {
            return Read(id) as T;
        }

        public AbstractObject Read(string id)
        {
            ObjectType type;
            return Read(id, out type);
        }

        public AbstractObject Read(string id, out ObjectType type)
        {
            var reader = ObjectReader.Create(this.Repository, id);
            if (reader == null)
            {
                type = ObjectType.Undefined;
                return null;
            }

            type = reader.Type;

            switch (reader.Type)
            {
                case ObjectType.Tree:
                    return CreateTree(id, reader);
                case ObjectType.Blob:
                    return CreateBlob(id, reader);
                case ObjectType.Commit:
                case ObjectType.Tag:
                case ObjectType.OffsetDelta:
                case ObjectType.ReferenceDelta:
                default:
                    throw new NotSupportedException(string.Format("Object Type ({0}) for object ({1}) not supported at this time.", reader.Type, id));
            }
        }

        private Tree CreateTree(string id, ObjectReader reader)
        {
            return new Tree(this, id, reader.Size, LoadDataFromReader(reader));
        }

        private Blob CreateBlob(string id, ObjectReader reader)
        {
            return new Blob(id, reader.Size, LoadDataFromReader(reader));
        }

        private Func<byte[]> LoadDataFromReader(ObjectReader reader)
        {
            return () =>
                       {
                           var bytes = new byte[reader.Size];
                           reader.Load(stream => stream.Read(bytes, 0, bytes.Length));
                           return bytes;
                       };
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
