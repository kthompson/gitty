using System;
using System.Collections.Generic;
using System.IO;

namespace Gitty
{
    public class Commit
    {
        private readonly Repository _repository;
        private readonly ObjectLoader _loader;

        private Tree _tree;
        public Tree Tree
        {
            get
            {
                this.EnsureLoaded();
                return _tree;
            }
        }

        private readonly List<Commit> _parents;
        public Commit[] Parents
        {
            get
            {
                this.EnsureLoaded();
                return _parents.ToArray();
            }
        }

        private string _commiter;
        public string Commiter
        {
            get
            {
                this.EnsureLoaded();
                return _commiter;
            }
        }

        private string _author;
        public string Author
        {
            get
            {
                this.EnsureLoaded(); 
                return _author;
            }
        }

        private string _message;
        public string Message
        {
            get
            {
                this.EnsureLoaded();
                return _message;
            }
        }

        public string Id { get; private set; }

        internal Commit(Repository repository, ObjectLoader loader, string id)
        {
            _repository = repository;
            _loader = loader;

            this.Id = id;
            this._parents = new List<Commit>();
        }

        private bool _loaded;
        private void EnsureLoaded()
        {
            if (_loaded) 
                return;

            this._loader.Load((stream, info) =>
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
                                                  this._tree = _repository.OpenObject(parts[1]) as Tree;
                                                  break;
                                              case "author":
                                                  this._author = parts[1];
                                                  break;
                                              case "committer":
                                                  this._commiter = parts[1];
                                                  break;
                                              case "parent":
                                                  var parent = _repository.OpenObject(parts[1]) as Commit;
                                                  if(parent != null)
                                                      this._parents.Add(parent);
                                                  break;
                                              default:
                                                  throw new NotSupportedException(string.Format("{0} is not a supported commit field.", parts[0]));
                                          }
                                      }

                                      var messageSize = info.Size - bytesRead;
                                      var buffer = new char[messageSize];
                                      var read = reader.Read(buffer, 0, buffer.Length);
                                      this._message = new string(buffer, 0, read);
                                  });

            this._loaded = true;
        }
    }
}