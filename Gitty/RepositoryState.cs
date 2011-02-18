using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gitty
{
    /// <summary>
    /// Object to represent the state of our repository and whether we can do various tasks or not
    /// </summary>
    public class RepositoryState
    {
        /// <summary>
        /// Gets a value indicating whether this instance can checkout.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance can checkout; otherwise, <c>false</c>.
        /// </value>
        public bool CanCheckout { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this instance can reset the head.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance can reset head; otherwise, <c>false</c>.
        /// </value>
        public bool CanResetHead { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this instance can commit.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance can commit; otherwise, <c>false</c>.
        /// </value>
        public bool CanCommit { get; private set; }
        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; private set; }

        private RepositoryState(bool canCheckout, bool canResetHead,bool canCommit, string description)
        {
            this.CanCheckout = canCheckout;
            this.CanResetHead = canResetHead;
            this.CanCommit = canCommit;
            this.Description = description;
        }

        /// <summary>
        /// Bare repository
        /// </summary>
        public static readonly RepositoryState Bare = new RepositoryState(false, false, false, "Normal");
        /// <summary>
        /// Repository is safe to do anything
        /// </summary>
        public static readonly RepositoryState Safe = new RepositoryState(true, true, true, "Normal");
        /// <summary>
        /// Repository is in a Merging state with Conflicts
        /// </summary>
        public static readonly RepositoryState Merging = new RepositoryState(false, false, false, "Conflicts");
        /// <summary>
        /// Repository is in a Merging state
        /// </summary>
        public static readonly RepositoryState MergingResolved = new RepositoryState(true, true, true, "Merging Resolved");
        /// <summary>
        /// Repository is rebasing
        /// </summary>
        public static readonly RepositoryState Rebasing = new RepositoryState(false, false, true, "Rebase/Apply mailbox");
        /// <summary>
        /// Repository is rebasing
        /// </summary>
        public static readonly RepositoryState RebasingRebasing = new RepositoryState(false, false, true, "Rebase");
        /// <summary>
        /// Repository is applying
        /// </summary>
        public static readonly RepositoryState Apply = new RepositoryState(false, false, true, "Apply mailbox");
        /// <summary>
        /// Repository is rebasing
        /// </summary>
        public static readonly RepositoryState RebasingMerge = new RepositoryState(false, false, true, "Rebase w/merge");
        /// <summary>
        /// Repository is in an interactive rebase
        /// </summary>
        public static readonly RepositoryState RebasingInteractive = new RepositoryState(false, false, true, "Rebase interactive");
        /// <summary>
        /// Repository is bisecting
        /// </summary>
        public static readonly RepositoryState Bisecting = new RepositoryState(true, false, true, "Bisecting");
    }
}
