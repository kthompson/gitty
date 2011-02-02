using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Gitty
{
    public class Repository
    {
        public string WorkingDirectory { get; private set; }

        internal Repository(string workingDirectory = null, string gitDirectory = null, bool create = false)
        {
            if (workingDirectory != null)
            {
                this.WorkingDirectory = Helper.MakeAbsolutePath(workingDirectory);

                gitDirectory = gitDirectory ?? Path.Combine(workingDirectory, ".git");
            }

            if (gitDirectory == null)
                throw new ArgumentNullException("gitDirectory",
                                                "You must specify at least workingDirectory or gitDirectory");

            this.Location = Helper.MakeAbsolutePath(gitDirectory);

            this.ObjectsLocation = Path.Combine(this.Location, "objects");
            this.PacksLocation = Path.Combine(this.ObjectsLocation, "pack");

            this.RefsLocation = Path.Combine(this.Location, Ref.Refs);

            this.HeadsLocation = Path.Combine(this.RefsLocation, Ref.Heads);
            this.RemotesLocation = Path.Combine(this.RefsLocation, Ref.Remotes);
            this.TagsLocation = Path.Combine(this.RefsLocation, Ref.Tags);

            if (!create) 
                return;

            //.git
            Directory.CreateDirectory(this.Location);
            EmbeddedToFile("Gitty.Content.config", Path.Combine(this.Location, "config"));
            EmbeddedToFile("Gitty.Content.description", Path.Combine(this.Location, "description"));
            EmbeddedToFile("Gitty.Content.HEAD", Path.Combine(this.Location, "HEAD"));
            //.git/hooks/
            Directory.CreateDirectory(Path.Combine(this.Location, "hooks"));
            //.git/info/
            var info = Path.Combine(this.Location, "info");
            Directory.CreateDirectory(info);
            EmbeddedToFile("Gitty.Content.info.exclude", Path.Combine(info, "exclude"));
            //.git/objects
            Directory.CreateDirectory(this.ObjectsLocation);
            //.git/objects/info
            Directory.CreateDirectory(Path.Combine(this.ObjectsLocation, "info"));
            //.git/objects/pack
            Directory.CreateDirectory(this.PacksLocation);
            //.git/refs
            Directory.CreateDirectory(this.RefsLocation);
            //.git/refs/heads
            Directory.CreateDirectory(this.HeadsLocation);
            //.git/refs/tags
            Directory.CreateDirectory(this.TagsLocation);
            //.git/hooks/
            var hooks = Path.Combine(this.Location, "hooks");
            EmbeddedToFile("Gitty.Content.hooks.applypatch-msg.sample", Path.Combine(hooks, "applypatch-msg.sample"));
            EmbeddedToFile("Gitty.Content.hooks.commit-msg.sample", Path.Combine(hooks, "commit-msg.sample"));
            EmbeddedToFile("Gitty.Content.hooks.post-commit.sample", Path.Combine(hooks, "post-commit.sample"));
            EmbeddedToFile("Gitty.Content.hooks.post-receive.sample", Path.Combine(hooks, "post-receive.sample"));
            EmbeddedToFile("Gitty.Content.hooks.post-update.sample", Path.Combine(hooks, "post-update.sample"));
            EmbeddedToFile("Gitty.Content.hooks.pre-applypatch.sample", Path.Combine(hooks, "pre-applypatch.sample"));
            EmbeddedToFile("Gitty.Content.hooks.pre-commit.sample", Path.Combine(hooks, "pre-commit.sample"));
            EmbeddedToFile("Gitty.Content.hooks.pre-rebase.sample", Path.Combine(hooks, "pre-rebase.sample"));
            EmbeddedToFile("Gitty.Content.hooks.prepare-commit-msg.sample", Path.Combine(hooks, "prepare-commit-msg.sample"));
            EmbeddedToFile("Gitty.Content.hooks.update.sample", Path.Combine(hooks, "update.sample"));
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

        public IEnumerable<Ref> Remotes
        {
            get { return RefsFromPath(this.RemotesLocation); }
        }

        public IEnumerable<Ref> Refs
        {
            get { return RefsFromPath(this.RefsLocation); }
        }

        public IEnumerable<Ref> Heads
        {
            get { return RefsFromPath(this.HeadsLocation); }
        }

        public IEnumerable<Ref> Branches
        {
            get { return RefsFromPath(this.HeadsLocation); }
        }

        public IEnumerable<Ref> Tags
        {
            get { return RefsFromPath(this.TagsLocation); }
        }

        private IEnumerable<Ref> RefsFromPath(string location)
        {
            return Helper.GetLocations(location).Select(path => new Ref(this.Location, path));
        }

        public bool IsBare
        {
            get { return this.WorkingDirectory == null; }
        }

        public string Location { get; private set; }
        public string RefsLocation { get; private set; }
        public string HeadsLocation { get; private set; }
        public string RemotesLocation { get; private set; }
        public string TagsLocation { get; private set; }
        public string PacksLocation { get; private set; }
        public string ObjectsLocation { get; private set; }

        public Head Head
        {
            get { return new Head(this); }
        }

        public Index Index
        {
            get{ return new Index(Path.Combine(this.Location, "index"));}
        }

        public Status Status
        {
            get
            {
                return new Status(this);
            }
        }

        public RepositoryState State
        {
            get
            {
                if(this.IsBare)
                    return RepositoryState.Bare;

                if (File.Exists(Path.Combine(this.WorkingDirectory, ".dotest")))
                    return RepositoryState.Rebasing;

                if (File.Exists(Path.Combine(this.WorkingDirectory, ".dotest-merge")))
                    return RepositoryState.RebasingInteractive;

                if (File.Exists(Path.Combine(this.WorkingDirectory, "rebase-apply", "rebasing")))
                    return RepositoryState.RebasingRebasing;

                if (File.Exists(Path.Combine(this.WorkingDirectory, "rebase-apply", "applying")))
                    return RepositoryState.Apply;

                if (Directory.Exists(Path.Combine(this.WorkingDirectory, "rebase-apply")))
                    return RepositoryState.Rebasing;

                if (File.Exists(Path.Combine(this.WorkingDirectory, "rebase-merge", "interactive")))
                    return RepositoryState.RebasingInteractive;

                if (Directory.Exists(Path.Combine(this.WorkingDirectory, "rebase-merge")))
                    return RepositoryState.RebasingMerge;

                if (File.Exists(Path.Combine(this.WorkingDirectory, "MERGE_HEAD")))
                {
                    if(this.Index.HasUnmergedPaths)
                        return RepositoryState.Merging;

                    return RepositoryState.MergingResolved;
                }

                if (File.Exists(Path.Combine(this.WorkingDirectory, "BISECT_LOG")))
                    return RepositoryState.Bisecting;

                return RepositoryState.Safe;
            }
        }

        public object OpenObject(string id)
        {
            var loader = ObjectLoader.Create(this, id);
            if (loader == null)
                return null;

            var info = loader.Load();
            switch (info.Type)
            {
                case "commit":
                    return new Commit(this, loader);
                case "tree":
                    return new Tree(this, loader);
                case "blob":
                    return new Blob(loader);
                case "tag":
                    return new Tag(this, loader);
            }
            return null;
        }
    }
}