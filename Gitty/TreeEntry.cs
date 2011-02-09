using System;

namespace Gitty
{
    public abstract class TreeEntry
    {
        protected readonly Repository Repository;
        protected readonly ObjectLoader Loader;

        public string Id { get; private set; }
        public string Mode { get; private set; }

        public Tree Parent { get; set; }

        public string Name { get; private set; }

        public abstract ObjectType Type { get; }

        private string _fullName;
        public string FullName
        {
            get
            {
                if (_fullName == null)
                {
                    if (this.Parent != null)
                    {
                        //TODO: should we be using Path.Combine here?

                        var parentName = this.Parent.FullName;
                        if (parentName != null)
                            parentName += '/';

                        _fullName = parentName + this.Name;
                    }
                    else
                    {
                        _fullName = this.Name;
                    }
                }

                return _fullName;
            }
        }

        internal TreeEntry(Repository repository, ObjectLoader loader, string id, string name, string mode, Tree parent)
        {
            this.Repository = repository;
            this.Loader = loader;

            this.Id = id;
            this.Name = name;
            this.Mode = mode;
            this.Parent = parent;
        }
    }
}