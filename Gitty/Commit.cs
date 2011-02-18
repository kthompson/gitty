using System;
using System.Collections.Generic;
using System.IO;
using Gitty.Storage;

namespace Gitty
{
    /// <summary>
    /// Represents the Commit objects in git.
    /// </summary>
    public class Commit : AbstractObject
    {
        private readonly ObjectStorage _storage;
        private readonly ObjectReader _reader;

        private Tree _tree;

        /// <summary>
        /// Gets the tree that this commit points to.
        /// </summary>
        public Tree Tree
        {
            get
            {
                this.EnsureLoaded();
                return _tree;
            }
        }

        private readonly List<Commit> _parents;
        /// <summary>
        /// Gets parent commits, if any exist.
        /// </summary>
        public Commit[] Parents
        {
            get
            {
                this.EnsureLoaded();
                return _parents.ToArray();
            }
        }

        private string _commiter;
        /// <summary>
        /// Gets the commiter name and email address.
        /// </summary>
        public string Commiter
        {
            get
            {
                this.EnsureLoaded();
                return _commiter;
            }
        }

        private string _author;
        /// <summary>
        /// Gets the author name and email address.
        /// </summary>
        public string Author
        {
            get
            {
                this.EnsureLoaded(); 
                return _author;
            }
        }

        private string _message;
        /// <summary>
        /// Gets the message of the commit.
        /// </summary>
        public string Message
        {
            get
            {
                this.EnsureLoaded();
                return _message;
            }
        }

        internal Commit(ObjectStorage storage, ObjectReader reader, string id)
            : base(id)
        {
            _storage = storage;
            _reader = reader;

            this._parents = new List<Commit>();
        }

        private bool _loaded;
        private void EnsureLoaded()
        {
            if (_loaded) 
                return;

            this._reader.Load(stream =>
                                  {
                                      var bytesRead = 0;
                                      var reader = new StreamReader(stream);
                                      string line;
                                      while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                                      {
                                          bytesRead += line.Length + 1; //add 1 for LF
                                          var parts = line.Split(new[] {' '}, 2);
                                          switch (parts[0])
                                          {
                                              case "tree":
                                                  this._tree = _storage.Read<Tree>(parts[1]);
                                                  break;
                                              case "author":
                                                  this._author = parts[1];
                                                  break;
                                              case "committer":
                                                  this._commiter = parts[1];
                                                  break;
                                              case "parent":
                                                  var parent = _storage.Read<Commit>(parts[1]);
                                                  if(parent != null)
                                                      this._parents.Add(parent);
                                                  break;
                                              default:
                                                  throw new NotSupportedException(string.Format("{0} is not a supported commit field.", parts[0]));
                                          }
                                      }

                                      var messageSize = this._reader.Size - bytesRead;
                                      var buffer = new char[messageSize];
                                      var read = reader.Read(buffer, 0, buffer.Length);
                                      this._message = new string(buffer, 0, read);
                                  });

            this._loaded = true;
        }

        /// <summary>
        /// Gets the ObjectType.
        /// </summary>
        public override ObjectType Type
        {
            get { return ObjectType.Commit; }
        }
    }
}