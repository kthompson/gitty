using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Gitty
{
    public class Head
    {
        public Repository Repository { get; private set; }
        public string Location { get; private set; }

        public bool IsDetached
        {
            get { return this.Ref == null; }
        }

        private Ref _ref;
        public Ref Ref
        {
            get
            {
                this.EnsureLoaded();
                return _ref;
            }
        }

        private string _id;
        public string Id
        {
            get
            {
                this.EnsureLoaded();
                return _id;
            }
        }

        private Tree _tree;
        public Tree Tree
        {
            get { return _tree ?? (_tree = this.Repository.ObjectStorage.Read<Tree>(this.Id)); }
        }

        public Head(Repository repository)
        {
            this.Repository = repository;
            this.Location = Path.Combine(repository.Location, "HEAD");
        }

        private void EnsureLoaded()
        {
            string data;
            using (var reader = new StreamReader(File.OpenRead(this.Location)))
            {
                data = reader.ReadToEnd().TrimEnd();
            }

            if (data.StartsWith("ref: "))
            {
                data = data.Substring("ref: ".Length);
                this._ref = this.Repository.Refs.Where(r => r.RelativePath == data).First();
                this._id = this._ref.Id;
            }
            else
            {
                this._id = data;
            }
        }
    }
}
