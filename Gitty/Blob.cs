using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using Gitty.Storage;

namespace Gitty
{
    /// <summary>
    /// Represents the git object Blob that is used to represent files
    /// </summary>
    public class Blob : TreeEntry
    {
        #region Constructors
        internal Blob(string id, long size, Func<byte[]> loader, Tree parent = null, string name = null, string mode = null)
            : base(id, parent, name, mode)
        {
            _loader = new Lazy<byte[]>(loader);
            this.Size = size;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the data of the object.
        /// </summary>
        public byte[] Data
        {
            get { return _loader.Value; }
        }
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The SHA1 id of the object.
        /// </value>
        public override string Id
        {
            get { return base.Id ?? _id ?? (_id = ObjectWriter.ComputeId(this)); }
        }
        /// <summary>
        /// Gets the size of the blob.
        /// </summary>
        public long Size { get; private set; }

        /// <summary>
        /// Gets the <see cref="ObjectType"/>.
        /// </summary>
        public override ObjectType Type
        {
            get { return ObjectType.Blob; }
        }
        #endregion

        #region Private Variables
        private string _id;

        private readonly Lazy<byte[]> _loader;
        #endregion
    }
}
