namespace Gitty
{
    /// <summary>
    /// Git Link used to represent sub modules
    /// </summary>
    public class GitLink : TreeEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitLink"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="name">The name.</param>
        public GitLink(string id, Tree parent, string name) 
            : base(id, parent, name, FileMode.GitLinkOctal)
        {
        }

        /// <summary>
        /// Gets the ObjectType.
        /// </summary>
        public override ObjectType Type
        {
            get { return ObjectType.Commit; }
        }
    }
}