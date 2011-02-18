namespace Gitty
{
    /// <summary>
    /// Object Type used to represent the different object types used in storage.
    /// </summary>
    public enum ObjectType : byte
    {
        /// <summary>
        /// Used for an unspecified object type
        /// </summary>
        Undefined = 0,
        /// <summary>
        /// Commit object
        /// </summary>
        Commit = 1,
        /// <summary>
        /// Tree object
        /// </summary>
        Tree = 2,
        /// <summary>
        /// Blob object
        /// </summary>
        Blob = 3,
        /// <summary>
        /// Tag object
        /// </summary>
        Tag = 4,
        /// <summary>
        /// Reserved for future use
        /// </summary>
        Reserved = 5,
        /// <summary>
        /// Offset Delta objects
        /// </summary>
        OffsetDelta = 6,
        /// <summary>
        /// SHA1/Reference Delta objects
        /// </summary>
        ReferenceDelta = 7,
    }
}