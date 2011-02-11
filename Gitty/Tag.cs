﻿using System;
using System.IO;
using Gitty.Storage;

namespace Gitty
{
    public class Tag
    {
        private readonly Repository _repository;
        private readonly ObjectReader _reader;

        internal Tag(Repository repository, ObjectReader reader, string id)
        {
            _repository = repository;
            _reader = reader;
            this.Id = id;
        }

        public string Id { get; private set; }

        private string _name;
        public string Name
        {
            get
            {
                this.EnsureLoaded();
                return _name;
            }
        }

        private string _type;
        public string Type
        {
            get
            {
                this.EnsureLoaded();
                return _type;
            }
        }

        private object _object;
        public object Object
        {
            get
            {
                this.EnsureLoaded();
                return _object;
            }
        }

        private string _tagger;
        public string Tagger
        {
            get
            {
                this.EnsureLoaded();
                return _tagger;
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
                                          var parts = line.Split(new[] { ' ' }, 2);
                                          switch (parts[0])
                                          {
                                              case "object":
                                                  this._object = _repository.OpenObject(parts[1]);
                                                  break;
                                              case "type":
                                                  this._type = parts[1];
                                                  break;
                                              case "tag":
                                                  this._name = parts[1];
                                                  break;
                                              case "tagger":
                                                  this._tagger = parts[1];
                                                  break;
                                              default:
                                                  throw new NotSupportedException(string.Format("{0} is not a supported tag field.", parts[0]));
                                          }
                                      }

                                      var messageSize = _reader.Size - bytesRead;
                                      var buffer = new char[messageSize];
                                      var read = reader.Read(buffer, 0, buffer.Length);
                                      this._message = new string(buffer, 0, read);
                                  });

            this._loaded = true;
        }
    }
}