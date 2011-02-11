namespace Gitty.Storage
{
    abstract class DeltaPackedObjectReader : PackedObjectReader
    {
        public abstract ObjectType RawType { get; }
        public PackedObjectReader Base { get; private set; }

        public override ObjectType Type
        {
            get { return this.Base.Type; }
        }

        protected DeltaPackedObjectReader(PackFile packFile, long objectOffset, long dataOffset, long size, PackedObjectReader baseReader)
            : base(packFile, objectOffset, dataOffset, size, ObjectType.Undefined)
        {
            this.Base = baseReader;
        }

        public override void Load(ContentLoader contentLoader = null)
        {
            //TODO: implement delta base loading and BinaryDelta.Apply
            this.Base.Load(stream => { });

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