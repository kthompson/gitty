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
        }

        public IEnumerable<Ref> Remotes
        {
            get
            {
                return Helper.GetLocations(Path.Combine(this.Location, Ref.Refs, Ref.Remotes)).Select(path => new Ref(this, path));
            }
        }

        public IEnumerable<Ref> Refs
        {
            get
            {
                return Helper.GetLocations(Path.Combine(this.Location, Ref.Refs)).Select(path => new Ref(this, path));
            }
        }

        public IEnumerable<Ref> Heads
        {
            get
            {
                return Helper.GetLocations(Path.Combine(this.Location, Ref.Refs, Ref.Heads)).Select(path => new Ref(this, path));
            }
        }

        public IEnumerable<Ref> Branches
        {
            get { return this.Heads; }
        }

        public IEnumerable<Ref> Tags
        {
            get
            {
                return Helper.GetLocations(Path.Combine(this.Location, Ref.Refs, Ref.Tags)).Select(path => new Ref(this, path));
            }
        }

        public string Location { get; private set; }

        public Head Head
        {
            get { return new Head(this); }
        }
    }
}