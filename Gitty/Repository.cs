using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Gitty
{
    public class Repository
    {
        public Repository(string gitDir)
        {
            this.Location = Helper.MakeAbsolutePath(gitDir);

            this.ObjectsLocation = Path.Combine(this.Location, "objects");
            this.PacksLocation = Path.Combine(this.ObjectsLocation, "pack");

            this.RefsLocation = Path.Combine(this.Location, Ref.Refs);

            this.HeadsLocation = Path.Combine(this.RefsLocation, Ref.Heads);
            this.RemotesLocation = Path.Combine(this.RefsLocation, Ref.Remotes);
            this.TagsLocation = Path.Combine(this.RefsLocation, Ref.Tags);
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

        public string Location { get; private set; }
        public string RefsLocation { get; private set; }
        public string HeadsLocation { get; private set; }
        public string RemotesLocation { get; private set; }
        public string TagsLocation { get; private set; }
        public string PacksLocation { get; private set; }

        public Head Head
        {
            get { return new Head(this); }
        }

        public string ObjectsLocation { get; private set; }

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