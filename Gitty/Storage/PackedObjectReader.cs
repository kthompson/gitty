using System.IO;

namespace Gitty.Storage
{
    abstract class PackedObjectReader : ObjectReader
    {
        public PackFile PackFile { get; private set; }

        public long ObjectOffset { get; private set; }
        public long DataOffset { get; private set; }

        protected PackedObjectReader(PackFile packFile, long objectOffset, long dataOffset, long size, ObjectType type)
            : base(type, size)
        {
            this.PackFile = packFile;
            this.ObjectOffset = objectOffset;
            this.DataOffset = dataOffset;
        }

        public static PackedObjectReader Create(PackFile packFile, long objectOffset)
        {
            using (var file = File.OpenRead(packFile.Location))
            {
                file.Seek(objectOffset, SeekOrigin.Begin);

                var b = file.ReadByte();
                var typeCode = (b & 0x70) >> 4;
                var type = (ObjectType) typeCode;

                var size = b & 0xF;
                var bits = 4;

                while((b & 0x80) == 0x80)
                {
                    b = file.ReadByte();
                    size += (b & 0x7f) << bits;
                    bits += 7;
                }

                switch(type)
                {
                    case ObjectType.Blob:
                    case ObjectType.Commit:
                    case ObjectType.Tag:
                    case ObjectType.Tree:
                        return new WholePackedObjectReader(packFile, objectOffset, file.Position, size, type);

                    case ObjectType.OffsetDelta:
                        var baseOffset = objectOffset - file.Read7BitEncodedInt();
                        return new DeltaOffsetPackedObjectReader(packFile, objectOffset, file.Position, size, baseOffset);

                    case ObjectType.ReferenceDelta:
                        var baseId = file.ReadId();
                        return new DeltaReferencePackedObjectReader(packFile, objectOffset, file.Position, size, baseId);

                    case ObjectType.Undefined:
                        throw new InvalidDataException("ObjectType was undefined.");
                    case ObjectType.Reserved:
                        throw new InvalidDataException("ObjectType is reserved.");
                    default:
                        throw new InvalidDataException("ObjectType is not valid.");
                }
                
            }
        }
    }

    //V2 PackIndex
}