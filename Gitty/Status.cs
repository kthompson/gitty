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
        public Index Index { get; private set; }

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
            this.HeadTree = this._repository.Head.Commit.Tree;
            this.Index = this._repository.Index;

            this.BuildEntries();
        }

        private void BuildEntries()
        {
            var workingTreeFiles = this.WorkingTree.EnumerateItems(true);
            var headFiles = this.HeadTree.EnumerateItems(true);
            var indexFiles = this.Index.Entries;

            var query = from wtree in workingTreeFiles
                        join headFile in headFiles on wtree.FullName equals headFile.FullName into gj
                        from headFileOrNull in gj.DefaultIfEmpty()
                        select new { WorkingTreeEntry = wtree, HeadEntry = headFileOrNull };

            var entries = query.ToList();
        }

        ///<summary>
        /// Changed but not updated
        /// 
        /// HEAD:   v1
        /// Index:  v1
        /// WT:     v2
        ///</summary>
        public IEnumerable<string> Changed { get; private set; }

        ///<summary>
        /// Files that are not in the repository yet.
        /// 
        /// HEAD:   none
        /// Index:  none
        /// WT:     v1
        ///</summary>
        public IEnumerable<string> Untracked { get; private set; }

        ///<summary>
        /// Files that are in the working tree but ignored
        /// 
        /// HEAD:   none
        /// Index:  none
        /// WT:     none
        ///</summary>
        public IEnumerable<string> Ignored { get; private set; }

        ///<summary>
        /// Changes to be committed
        /// 
        /// HEAD:   none
        /// Index:  none
        /// WT:     none
        ///</summary>
        public IEnumerable<string> Updated { get; private set; }

    }
}
