using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gitty
{
    public class RepositoryState
    {
        public bool CanCheckout { get; private set; }
        public bool CanResetHead { get; private set; }
        public bool CanCommit { get; private set; }
        public string Description { get; private set; }

        private RepositoryState(bool canCheckout, bool canResetHead,bool canCommit, string description)
        {
            this.CanCheckout = canCheckout;
            this.CanResetHead = canResetHead;
            this.CanCommit = canCommit;
            this.Description = description;
        }
        public static readonly RepositoryState Bare = new RepositoryState(false, false, false, "Normal");
        public static readonly RepositoryState Safe = new RepositoryState(true, true, true, "Normal");
        public static readonly RepositoryState Merging = new RepositoryState(false, false, false, "Conflicts");
        public static readonly RepositoryState MergingResolved = new RepositoryState(true, true, true, "Merging Resolved");
        public static readonly RepositoryState Rebasing = new RepositoryState(false, false, true, "Rebase/Apply mailbox");
        public static readonly RepositoryState RebasingRebasing = new RepositoryState(false, false, true, "Rebase");
        public static readonly RepositoryState Apply = new RepositoryState(false, false, true, "Apply mailbox");
        public static readonly RepositoryState RebasingMerge = new RepositoryState(false, false, true, "Rebase w/merge");
        public static readonly RepositoryState RebasingInteractive = new RepositoryState(false, false, true, "Rebase interactive");
        public static readonly RepositoryState Bisecting = new RepositoryState(true, false, true, "Bisecting");

    }
}
