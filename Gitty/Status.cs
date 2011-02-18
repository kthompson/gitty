using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Gitty
{
    /// <summary>
    /// Gets a status of all the files in the working directory
    /// </summary>
    public class Status
    {
        private readonly Repository _repository;

        /// <summary>
        /// Gets the head tree.
        /// </summary>
        public Tree HeadTree { get; private set; }
        /// <summary>
        /// Gets the working tree.
        /// </summary>
        public Tree WorkingTree { get; private set; }
        /// <summary>
        /// Gets the index tree.
        /// </summary>
        public Tree IndexTree { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Status"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public Status(Repository repository)
        {
            _repository = repository;
            
            //we need to know index entries and compare them to HEAD diff will be added items
            var dir = new DirectoryInfo(_repository.WorkingDirectoryLocation);

            this.WorkingTree = new WorkingTreeDirectory(dir);
            this.HeadTree = this._repository.Head.Tree;
            this.IndexTree = this._repository.Head.Tree;
        }


        //private IEnumerable<string> Changed { get; private set; }

        //not in Index, HEAD or Ignored
        //private IEnumerable<string> Untracked { get; private set; }


        //private IEnumerable<string> Ignored { get; private set; }


        //private IEnumerable<string> Updated { get; private set; }
        //private IEnumerable<string> Unmerged { get; private set; }
        //private IEnumerable<string> Changed { get; private set; }
    }
}
