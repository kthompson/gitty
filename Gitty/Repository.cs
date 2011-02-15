using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Gitty.Storage;

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
                                                "You must specify a workingDirectory or gitDirectory");

            if (create && !Directory.Exists(gitDirectory))
                Directory.CreateDirectory(gitDirectory);

            this.Location = Helper.MakeAbsolutePath(gitDirectory);

            this.InfoLocation = Path.Combine(this.Location, "info");

            this.HooksLocation = Path.Combine(this.Location, "hooks");

            this.ObjectsLocation = Path.Combine(this.Location, "objects");
            this.PacksLocation = Path.Combine(this.ObjectsLocation, "pack");

            this.RefsLocation = Path.Combine(this.Location, Ref.Refs);

            this.HeadsLocation = Path.Combine(this.RefsLocation, Ref.Heads);
            this.RemotesLocation = Path.Combine(this.RefsLocation, Ref.Remotes);
            this.TagsLocation = Path.Combine(this.RefsLocation, Ref.Tags);

            this._storage = new ObjectStorage(this.ObjectsLocation);

            if (!create) 
                return;

            //.git
            var configfile = this.WorkingDirectory == null
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
        public string HooksLocation { get; private set; }
        public string InfoLocation { get; private set; }

        private ObjectStorage _storage;

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

        //public object OpenObject(string id)
        //{
        //    var loader = OpenObjectLoader(id);
        //    if (loader == null)
        //        return null;

        //    switch (loader.Type)
        //    {
        //        case ObjectType.Commit:
        //            return new Commit(this, loader, id);
        //        case ObjectType.Tree:
        //            return new Tree(id, 0, null);
        //        case ObjectType.Blob:
        //            //TODO: fix this
        //            return new Blob(id, 0, null);
        //        case ObjectType.Tag:
        //            return new Tag(this, loader, id);
        //        case ObjectType.OffsetDelta:
        //        case ObjectType.ReferenceDelta:
        //        default:
        //            throw new NotSupportedException(string.Format("Object Type ({0}) for object ({1}) not supported at this time.", loader.Type, id));
        //    }
        //}

        

        //public TreeEntry<T> OpenTreeEntry<T>(string id, string name, string mode, Tree parent)
        //    where T : AbstractObject
        //{
        //    var loader = OpenObjectLoader(id);
        //    if (loader == null)
        //        return null;

        //    switch (loader.Type)
        //    {
        //        case ObjectType.Tree:
        //            return new Tree(this, id, name, mode, parent);
        //        case ObjectType.Blob:
        //            return new Blob(id, name, mode, parent);
        //        case ObjectType.Commit:
        //        case ObjectType.Tag:
        //        case ObjectType.OffsetDelta:
        //        case ObjectType.ReferenceDelta:
        //        default:
        //            throw new NotSupportedException(string.Format("Object Type ({0}) for object ({1}) not supported at this time.", loader.Type, id));
        //    }
        //}

        public AbstractObject OpenObject(string id)
        {
            return this._storage.Read(id);
        }
    }
}