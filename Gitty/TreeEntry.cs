using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using Gitty.Storage;

namespace Gitty
{
    public class TreeEntry : AbstractObject
    {
        public string Mode { get; private set; }

        public Tree Parent { get; private set; }

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

        internal TreeEntry(ObjectType type, string id, Tree parent, string name, string mode) 
            : base(type, id)
        {
            this.Parent = parent;
            this.Name = name;
            this.Mode = mode;
        }
    }
}