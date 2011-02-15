using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Gitty
{
    class RefStorage
    {
        public string RefsLocation { get; private set; }
        public string HeadsLocation { get; private set; }
        public string RemotesLocation { get; private set; }
        public string TagsLocation { get; private set; }

        public RefStorage(string location, bool create)
        {
            this.RefsLocation = Path.Combine(location, Ref.Refs);

            this.HeadsLocation = Path.Combine(this.RefsLocation, Ref.Heads);
            this.RemotesLocation = Path.Combine(this.RefsLocation, Ref.Remotes);
            this.TagsLocation = Path.Combine(this.RefsLocation, Ref.Tags);

            if (!create) 
                return;

            //.git/refs
            Directory.CreateDirectory(this.RefsLocation);
            //.git/refs/heads
            Directory.CreateDirectory(this.HeadsLocation);
            //.git/refs/tags
            Directory.CreateDirectory(this.TagsLocation);
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
            return Helper.GetLocations(location).Select(path => new Ref(this.RefsLocation, path));
        }
    }
}
