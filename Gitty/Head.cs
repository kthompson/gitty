using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Gitty
{
    /// <summary>
    /// A class to represent the HEAD in a git Repository
    /// </summary>
    public class Head
    {
        /// <summary>
        /// Gets the repository.
        /// </summary>
        public Repository Repository { get; private set; }
        /// <summary>
        /// Gets the location of the .git/HEAD.
        /// </summary>
        public string Location { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the repository is detached and not on any branch.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is detached; otherwise, <c>false</c>.
        /// </value>
        public bool IsDetached
        {
            get { return this.Ref == null; }
        }

        private Ref _ref;
        /// <summary>
        /// Gets the Ref that the HEAD may point to.
        /// </summary>
        public Ref Ref
        {
            get
            {
                this.EnsureLoaded();
                return _ref;
            }
        }

        private string _id;
        /// <summary>
        /// Gets the SHA1 id of the object we are pointing to.
        /// </summary>
        public string Id
        {
            get
            {
                this.EnsureLoaded();
                return _id;
            }
        }

        private Commit _commit;
        /// <summary>
        /// Gets the commit that this head is pointing to.
        /// </summary>
        public Commit Commit
        {
            get
            {
                return _commit ?? (_commit = this.Repository.ObjectStorage.Read<Commit>(this.Id));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Head"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public Head(Repository repository)
        {
            this.Repository = repository;
            this.Location = Path.Combine(repository.Location, "HEAD");
        }

        private void EnsureLoaded()
        {
            string data;
            using (var stream = File.OpenRead(this.Location))
            {
                var reader = new StreamReader(stream);
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
