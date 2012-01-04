namespace Gitty
{
    ///<summary>
    /// TreeEntry to represent filesystem symlinks
    ///</summary>
    public class Symlink : TreeEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Symlink"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="name">The name.</param>
        public Symlink(string id, Tree parent, string name)
            : base(id, parent, name, FileMode.SymlinkOctal)
        {
        }

        /// <summary>
        /// Gets the ObjectType.
        /// </summary>
        public override ObjectType Type
        {
            get { return ObjectType.Blob; }
        }
    }
}