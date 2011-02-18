using System.IO;

namespace Gitty.Storage
{
    abstract class DeltaPackedObjectReader : PackedObjectReader
    {
        public abstract ObjectType RawType { get; }
        public long RawSize { get; protected set; }
        public PackedObjectReader Base { get; private set; }

        public override long Size
        {
            get { return this.Data.Length; }
            protected set { }
        }

        private byte[] _data;
        public byte[] Data
        {
            get { return _data ?? (_data = LoadData()); }
        }

        public override ObjectType Type
        {
            get { return this.Base.Type; }
        }

        protected DeltaPackedObjectReader(PackFile packFile, long objectOffset, long dataOffset, long size, PackedObjectReader baseReader)
            : base(packFile, objectOffset, dataOffset, size, ObjectType.Undefined)
        {
            this.Base = baseReader;
            this.RawSize = size;
        }

        public override void Load(ContentLoader contentLoader = null)
        {
            if (contentLoader == null) 
                return;

            using (var stream = new MemoryStream(this.Data))
            {
                contentLoader(stream);
            }
        }

        private byte[] LoadData()
        {
            var baseData = new byte[this.Base.Size];
            this.Base.Load(stream => stream.Read(baseData, 0, baseData.Length));
            
            using (var file = File.OpenRead(this.PackFile.Location))
            {
                file.Seek(this.DataOffset, SeekOrigin.Begin);

                var stream = new CompressionStream(file, leaveOpen: true);
                var delta = new byte[this.RawSize];
                stream.Read(delta, 0, delta.Length);
                return BinaryDelta.Apply(baseData, delta);
            }
        }
    }

    class DeltaOffsetPackedObjectReader : DeltaPackedObjectReader
    {
        
        public DeltaOffsetPackedObjectReader(PackFile packFile, long objectOffset, long dataOffset, long size, long baseOffset)
            : base(packFile, objectOffset, dataOffset, size, packFile.GetObjectLoader(baseOffset))
        {
        }

        public override ObjectType RawType
        {
            get { return ObjectType.OffsetDelta; }
        }
    }

    class DeltaReferencePackedObjectReader : DeltaPackedObjectReader
    {
        public DeltaReferencePackedObjectReader(PackFile packFile, long objectOffset, long dataOffset, long size, string baseId)
            : base(packFile, objectOffset, dataOffset, size, packFile.GetObjectLoader(baseId))
        {
        }

        public override ObjectType RawType
        {
            get { return ObjectType.ReferenceDelta; }
        }
    }
}