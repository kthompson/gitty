using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Gitty.Storage;

namespace Gitty
{
    /// <summary>
    /// Object to represent a Git repository
    /// </summary>
    public class Repository
    {
        /// <summary>
        /// Gets the working directory location.
        /// </summary>
        public string WorkingDirectoryLocation { get; private set; }

        internal Repository(string workingDirectory = null, string gitDirectory = null, bool create = false)
        {
            if (workingDirectory != null)
            {
                this.WorkingDirectoryLocation = Helper.MakeAbsolutePath(workingDirectory);

                gitDirectory = gitDirectory ?? Path.Combine(workingDirectory, ".git");
            }

            if (gitDirectory == null)
                throw new ArgumentNullException("gitDirectory",
                                                "You must specify a workingDirectory or gitDirectory");

            if (create && !Directory.Exists(gitDirectory))
                Directory.CreateDirectory(gitDirectory);

            this.Location = Helper.MakeAbsolutePath(gitDirectory);

            this.InfoLocation = Path.Combine(this.Location, "info");

            this.HooksLocation = Path.Combine(this.Location, "hooks");

            this.CreateGitDirectory(create);

            this.ObjectStorage = new ObjectStorage(this.Location, create);
            this._refStorage = new RefStorage(this.Location, create);
        }

        private void CreateGitDirectory(bool create)
        {
            if (!create) 
                return;

            //.git
            var configfile = this.WorkingDirectoryLocation == null
                                 ? "Gitty.Content.config_bare"
                                 : "Gitty.Content.config";

            EmbeddedToFile(configfile, Path.Combine(this.Location, "config"));
            EmbeddedToFile("Gitty.Content.description", Path.Combine(this.Location, "description"));
            EmbeddedToFile("Gitty.Content.HEAD", Path.Combine(this.Location, "HEAD"));
            
            //.git/hooks/
            Directory.CreateDirectory(this.HooksLocation);
            EmbeddedHookToFile("applypatch-msg.sample");
            EmbeddedHookToFile("commit-msg.sample");
            EmbeddedHookToFile("post-commit.sample");
            EmbeddedHookToFile("post-receive.sample");
            EmbeddedHookToFile("post-update.sample");
            EmbeddedHookToFile("pre-applypatch.sample");
            EmbeddedHookToFile("pre-commit.sample");
            EmbeddedHookToFile("pre-rebase.sample");
            EmbeddedHookToFile("prepare-commit-msg.sample");
            EmbeddedHookToFile("update.sample");

            //.git/info/
            Directory.CreateDirectory(this.InfoLocation);
            EmbeddedToFile("Gitty.Content.info.exclude", Path.Combine(this.InfoLocation, "exclude"));

        }

        private void EmbeddedHookToFile(string file)
        {
            EmbeddedToFile("Gitty.Content.hooks." + file, Path.Combine(this.HooksLocation, file));
        }

        private static void EmbeddedToFile(string resource, string file)
        {
            using (var stream = File.Create(file))
            {
                using(var res = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
                {
                    if (res == null) 
                        throw new ArgumentException("not a valid resource", "resource");
                    
                    res.CopyTo(stream);
                }
            }
        }

        /// <summary>
        /// Gets the remotes.
        /// </summary>
        public IEnumerable<Ref> Remotes
        {
            get { return this._refStorage.Remotes; }
        }

        /// <summary>
        /// Gets the refs.
        /// </summary>
        public IEnumerable<Ref> Refs
        {
            get { return this._refStorage.Refs; }
        }

        /// <summary>
        /// Gets the heads.
        /// </summary>
        public IEnumerable<Ref> Heads
        {
            get { return this._refStorage.Heads; }
        }

        /// <summary>
        /// Gets the branches.
        /// </summary>
        public IEnumerable<Ref> Branches
        {
            get { return this._refStorage.Branches; }
        }

        /// <summary>
        /// Gets the tags.
        /// </summary>
        public IEnumerable<Ref> Tags
        {
            get { return this._refStorage.Tags; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is bare.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is bare; otherwise, <c>false</c>.
        /// </value>
        public bool IsBare
        {
            get { return this.WorkingDirectoryLocation == null; }
        }

        /// <summary>
        /// Gets the location of the repo.
        /// </summary>
        public string Location { get; private set; }

        /// <summary>
        /// Gets the hooks location.
        /// </summary>
        public string HooksLocation { get; private set; }

        /// <summary>
        /// Gets the info location.
        /// </summary>
        public string InfoLocation { get; private set; }

        internal ObjectStorage ObjectStorage { get; private set; }

        private readonly RefStorage _refStorage;

        /// <summary>
        /// Gets the head.
        /// </summary>
        public Head Head
        {
            get { return new Head(this); }
        }

        /// <summary>
        /// Gets the index.
        /// </summary>
        public Index Index
        {
            get{ return new Index(Path.Combine(this.Location, "index"));}
        }

        /// <summary>
        /// Gets the status.
        /// </summary>
        public Status Status
        {
            get
            {
                return new Status(this);
            }
        }

        /// <summary>
        /// Gets the state.
        /// </summary>
        public RepositoryState State
        {
            get
            {
                if(this.IsBare)
                    return RepositoryState.Bare;

                if (File.Exists(Path.Combine(this.WorkingDirectoryLocation, ".dotest")))
                    return RepositoryState.Rebasing;

                if (File.Exists(Path.Combine(this.WorkingDirectoryLocation, ".dotest-merge")))
                    return RepositoryState.RebasingInteractive;

                if (File.Exists(Path.Combine(this.WorkingDirectoryLocation, "rebase-apply", "rebasing")))
                    return RepositoryState.RebasingRebasing;

                if (File.Exists(Path.Combine(this.WorkingDirectoryLocation, "rebase-apply", "applying")))
                    return RepositoryState.Apply;

                if (Directory.Exists(Path.Combine(this.WorkingDirectoryLocation, "rebase-apply")))
                    return RepositoryState.Rebasing;

                if (File.Exists(Path.Combine(this.WorkingDirectoryLocation, "rebase-merge", "interactive")))
                    return RepositoryState.RebasingInteractive;

                if (Directory.Exists(Path.Combine(this.WorkingDirectoryLocation, "rebase-merge")))
                    return RepositoryState.RebasingMerge;

                if (File.Exists(Path.Combine(this.WorkingDirectoryLocation, "MERGE_HEAD")))
                {
                    if(this.Index.HasUnmergedPaths)
                        return RepositoryState.Merging;

                    return RepositoryState.MergingResolved;
                }

                if (File.Exists(Path.Combine(this.WorkingDirectoryLocation, "BISECT_LOG")))
                    return RepositoryState.Bisecting;

                return RepositoryState.Safe;
            }
        }

        /// <summary>
        /// Opens the object.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        [Obsolete]
        public AbstractObject OpenObject(string id)
        {
            //TODO: we should remove this as we should only have access through other fields
            return this.ObjectStorage.Read(id);
        }
    }
}