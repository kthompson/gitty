using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using Gitty.Storage;

namespace Gitty
{
    /// <summary>
    /// Represents an object in a Tree
    /// </summary>
    public abstract class TreeEntry : AbstractObject
    {
        /// <summary>
        /// Gets the mode.
        /// </summary>
        public string Mode { get; private set; }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        public Tree Parent { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        private string _fullName;
        /// <summary>
        /// Gets the full name.
        /// </summary>
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

        internal TreeEntry(string id, Tree parent, string name, string mode) 
            : base(id)
        {
            this.Parent = parent;
            this.Name = name;
            this.Mode = mode;
        }
    }
}