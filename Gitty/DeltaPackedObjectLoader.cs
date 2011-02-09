using System;

namespace Gitty
{
    abstract class DeltaPackedObjectLoader : PackedObjectLoader
    {
        public abstract ObjectType RawType { get; }
        public PackedObjectLoader Base { get; private set; }

        public override ObjectType Type
        {
            get { return this.Base.Type; }
        }

        protected DeltaPackedObjectLoader(PackFile packFile, long objectOffset, long dataOffset, long size, PackedObjectLoader baseLoader)
            : base(packFile, objectOffset, dataOffset, size, ObjectType.Undefined)
        {
            this.Base = baseLoader;
        }

        public override void Load(ContentLoader contentLoader = null)
        {
            //TODO: implement delta base loading and BinaryDelta.Apply
            this.Base.Load(stream => { });

        }
    }

    class DeltaOffsetPackedObjectLoader : DeltaPackedObjectLoader
    {
        
        public DeltaOffsetPackedObjectLoader(PackFile packFile, long objectOffset, long dataOffset, long size, long baseOffset)
            : base(packFile, objectOffset, dataOffset, size, packFile.GetObjectLoader(baseOffset))
        {
        }

        public override ObjectType RawType
        {
            get { return ObjectType.OffsetDelta; }
        }
    }

    class DeltaReferencePackedObjectLoader : DeltaPackedObjectLoader
    {
        public DeltaReferencePackedObjectLoader(PackFile packFile, long objectOffset, long dataOffset, long size, string baseId)
            : base(packFile, objectOffset, dataOffset, size, packFile.GetObjectLoader(baseId))
        {
        }

        public override ObjectType RawType
        {
            get { return ObjectType.ReferenceDelta; }
        }
    }
}