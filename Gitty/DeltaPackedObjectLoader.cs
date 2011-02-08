using System;

namespace Gitty
{
    abstract class DeltaPackedObjectLoader : PackedObjectLoader
    {
        public ObjectType RawType { get; private set; }
        public PackedObjectLoader Base { get; private set; }

        public override ObjectType Type
        {
            get { return this.Base.Type; }
        }

        protected DeltaPackedObjectLoader(PackFile packFile, long objectOffset, long dataOffset, long size, ObjectType type, PackedObjectLoader baseLoader)
            : base(packFile, objectOffset, dataOffset, size, type)
        {
            this.Base = baseLoader;
            this.RawType = type;
        }
    }

    class DeltaOffsetPackedObjectLoader : DeltaPackedObjectLoader
    {
        public DeltaOffsetPackedObjectLoader(PackFile packFile, long objectOffset, long dataOffset, long size, ObjectType type, long baseOffset)
            : base(packFile, objectOffset, dataOffset, size, type, packFile.GetObjectLoader(baseOffset))
        {
        }

        public override void Load(ContentLoader contentLoader = null)
        {
            throw new NotImplementedException();
        }
    }

    class DeltaReferencePackedObjectLoader : DeltaPackedObjectLoader
    {
        public DeltaReferencePackedObjectLoader(PackFile packFile, long objectOffset, long dataOffset, long size, ObjectType type, string baseId)
            : base(packFile, objectOffset, dataOffset, size, type, packFile.GetObjectLoader(baseId))
        {
        }

        public override void Load(ContentLoader contentLoader = null)
        {
            throw new NotImplementedException();
        }
    }
}