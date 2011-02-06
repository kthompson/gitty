using System;

namespace Gitty
{
    public class TreeEntry : ITreeEntry
    {
        private readonly Repository _repository;

        public string Id { get; private set; }
        public string Mode { get; private set; }

        public ITreeEntry Parent { get; set; }

        public string Name { get; private set; }

        public string Type { get; private set; }

        public ITreeEntry Entry { get; private set; }

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

        internal TreeEntry(Repository repository, string id, string name, string mode)
        {
            this._repository = repository;

            this.Id = id;
            this.Name = name;
            this.Mode = mode;
            this.Entry = (ITreeEntry)this._repository.OpenObject(id);
            this.Entry.Parent = this;

            this.Type = this.Entry is Tree ? "tree" : "blob";

        }
    }

    public interface ITreeEntry
    {
        ITreeEntry Parent { get; set; }
        string Name { get;  }
        string FullName { get; }
    }
}