using System;
using System.IO;

namespace Gitty
{
    abstract class PackedObjectLoader : ObjectLoader
    {
        public PackFile PackFile { get; private set; }

        public long ObjectOffset { get; private set; }
        public long DataOffset { get; private set; }

        private ObjectLoader _base;

        protected PackedObjectLoader(PackFile packFile, long objectOffset, long dataOffset, long size, ObjectType type)
        {
            this.PackFile = packFile;
            this.Type = type;
            this.ObjectOffset = objectOffset;
            this.DataOffset = dataOffset;
            this.Size = size;
        }

        public static PackedObjectLoader Create(PackFile packFile, long objectOffset)
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
                        return new WholePackedObjectLoader(packFile, objectOffset, file.Position, size, type);

                    case ObjectType.OffsetDelta:
                        var baseOffset = objectOffset - file.Read7BitEncodedInt();
                        return new DeltaOffsetPackedObjectLoader(packFile, objectOffset, file.Position, size, type, baseOffset);

                    case ObjectType.ReferenceDelta:
                        var baseId = file.ReadId();
                        return new DeltaReferencePackedObjectLoader(packFile, objectOffset, file.Position, size, type, baseId);

                    case ObjectType.Undefined:
                        throw new InvalidDataException("ObjectType was undefined.");
                    case ObjectType.Reserved:
                        throw new InvalidDataException("ObjectType is reserved.");
                    default:
                        throw new InvalidDataException("ObjectType is not valid.");
                }
                
            }
        }

        private void LoadDelta(FileStream file, ContentLoader contentLoader)
        {
            var offset = this.ObjectOffset - file.Read7BitEncodedInt();
            var dataOffset = file.Position;

            var baseObject = this.PackFile.GetObjectLoader(offset);
            baseObject.Load((stream, loadInfo) =>
                                        {
                                            this.Type = loadInfo.Type;
                                            //this.Size = loadInfo.Size;
                                            if(contentLoader != null)
                                                contentLoader(stream, loadInfo);
                                        });
        }
    }

    //V2 PackIndex
}