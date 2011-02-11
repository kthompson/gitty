using System;
using System.Collections.Generic;
using Gitty.Storage;

namespace Gitty
{
    public class TreeEntry<T> : AbstractObject, ITreeEntry<T>
        where T : AbstractObject
    {
        public string Mode { get; private set; }

        public Tree Parent { get; private set; }

        public T Entry { get; set; }
        public string Name { get; private set; }

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

        internal TreeEntry(T entry, string name, string mode, Tree parent) 
            : base(entry.Type, entry.Id)
        {
            this.Entry = entry;
            this.Name = name;
            this.Mode = mode;
            this.Parent = parent;
        }
    }

    public interface ITreeEntry<out T>
    {
        Tree Parent { get; }
        string Mode { get; }
        string Name { get; }

        T Entry { get; }

        string FullName { get; }
    }
}