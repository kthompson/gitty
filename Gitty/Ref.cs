using System;
using System.IO;

namespace Gitty
{

    /// <summary>
    /// Used to represent the different types of Refs
    /// </summary>
    public enum RefType
    {
        /// <summary>
        /// Head/Branches
        /// </summary>
        Head,
        /// <summary>
        /// Tags
        /// </summary>
        Tag,
        /// <summary>
        /// Remotes
        /// </summary>
        Remote
    }

    /// <summary>
    /// Object used to represent the named pointers to commits in the repo(Refs).
    /// </summary>
    public sealed class Ref
    {
        /// <summary>
        /// Constant for Refs path
        /// </summary>
        public const string Refs = "refs";
        /// <summary>
        /// Constant for Heads path
        /// </summary>
        public const string Heads = "heads";
        /// <summary>
        /// Constant for Remotes path
        /// </summary>
        public const string Remotes = "remotes";
        /// <summary>
        /// Constant for Tags path
        /// </summary>
        public const string Tags = "tags";


        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the name of the remote.
        /// </summary>
        /// <value>
        /// The name of the remote.
        /// </value>
        public string RemoteName { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this instance is packed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is packed; otherwise, <c>false</c>.
        /// </value>
        public bool IsPacked { get; private set; }
        /// <summary>
        /// Gets the type of ref.
        /// </summary>
        public RefType Type { get; private set; }
        /// <summary>
        /// Gets the id that the ref points to.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// Gets the relative path or "Git" name.
        /// </summary>
        public string RelativePath { get; private set; }
        /// <summary>
        /// Gets the location of the ref.
        /// </summary>
        public string Location { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ref"/> class.
        /// </summary>
        /// <param name="refsLocation">The refs location.</param>
        /// <param name="location">The location.</param>
        /// <param name="id">The id.</param>
        public Ref(string refsLocation, string location, string id = null)
        {
            this.Location = location;
            this.IsPacked = id != null;
            this.Id = id;

            var relPath = Helper.MakeRelativePath(refsLocation, location).Replace('\\', '/');

            if (relPath.StartsWith(Tags))
            {
                this.Type = RefType.Tag;
                this.Name = relPath.Substring(Tags.Length + 1);
            }
            else if (relPath.StartsWith(Heads))
            {
                this.Type = RefType.Head;
                this.Name = relPath.Substring(Heads.Length + 1);
            }
            else if (relPath.StartsWith(Remotes))
            {
                this.Type = RefType.Remote;
                var temp = relPath.Substring(Remotes.Length + 1).Split(new[] { '/', '\\' }, 2);

                this.RemoteName = temp[0];
                this.Name = relPath;
            }
            else
            {
                throw new ArgumentException("The location provided does not appear to be in the repository.");
            }

            this.RelativePath = Refs + "/" + relPath; //use forwardslash because we use relativePath for lookup purposes
            
            LoadId();
        }

        private void LoadId()
        {
            if (this.IsPacked)
                return;

            using (var stream = File.OpenRead(this.Location))
            {
                var reader = new StreamReader(stream);

                this.Id = reader.ReadToEnd().TrimEnd();
            }
        }
    }
}