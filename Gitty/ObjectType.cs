namespace Gitty
{
    public enum ObjectType : byte
    {
        Undefined = 0,
        Commit = 1,
        Tree = 2,
        Blob = 3,
        Tag = 4, 
        Reserved = 5,
        OffsetDelta = 6,
        ReferenceDelta = 7,
    }
}