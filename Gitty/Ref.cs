using System;
using System.IO;

namespace Gitty
{

    public enum RefType
    {
        Head,
        Tag,
        Remote
    }

    public sealed class Ref
    {
        public static readonly string Refs = "refs";
        public static readonly string Heads = "heads";
        public static readonly string Remotes = "remotes";
        public static readonly string Tags = "tags";

        
        public string Name { get; private set; }
        public string RemoteName { get; private set; }
        public bool IsPacked { get; private set; }
        public RefType Type { get; private set; }
        public Repository Repository { get; private set; }
        public string Id { get; private set; }
        public string RelativePath { get; private set; }
        public string Location { get; private set; }

        public Ref(Repository repository, string location, string id = null)
        {
            this.Repository = repository;
            this.Location = location;
            this.IsPacked = id != null;
            this.Id = id;

            var relPath = Helper.MakeRelativePath(Path.Combine(this.Repository.Location, Refs), location).Replace('\\', '/');

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

            using (var reader = new StreamReader(File.OpenRead(this.Location)))
            {
                this.Id = reader.ReadToEnd().TrimEnd();
            }
        }
    }
}