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
        /// Gets a value indicating whether this is the initial commit.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is initial commit; otherwise, <c>false</c>.
        /// </value>
        public bool IsInitialCommit { get; private set; }

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
            /*	
                wt_status_collect_changes_worktree(s);

                if (s->is_initial)
                    wt_status_collect_changes_initial(s);
                else
                    wt_status_collect_changes_index(s);
                wt_status_collect_untracked(s);
             */
            var indexEntries = this.Index.Entries.ToDictionary(entry => entry.Name);
            var headEnties = this.HeadTree.EnumerateItems(true).ToDictionary(entry => entry.FullName);

            foreach (var wtFile in this.WorkingTree.EnumerateItems(true))
            {
                var name = wtFile.FullName;
                var indexEntry = PopItem(indexEntries, name);
                var headEntry = PopItem(headEnties, name);

                var isNew = headEntry == null;
                var untracked = headEntry == null && indexEntry == null;
                var added = headEntry == null && indexEntry != null;


                //if(index == null)
                //{
                    
                //}
            }

            //var query = from wtree in workingTreeFiles
            //            join indexEntry in this.Index.Entries on wtree.FullName equals indexEntry.Name into gj
            //            from headFileOrNull in gj.DefaultIfEmpty()
            //            select new { WorkingTreeEntry = wtree, HeadEntry = headFileOrNull };

            //var entries = query.ToList();
        }

        private static T PopItem<T>(Dictionary<string, T> indexEntries, string key)
             where T : class
        {
            T indexEntry = null;
            if (indexEntries.ContainsKey(key))
            {
                indexEntry = indexEntries[key];
                indexEntries.Remove(key);
            }
            return indexEntry;
        }

        class StatusEntry
        {
            
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
