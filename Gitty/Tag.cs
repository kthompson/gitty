using System;
using System.IO;
using Gitty.Storage;

namespace Gitty
{
    public class Tag : AbstractObject
    {
        private readonly ObjectStorage _storage;
        private readonly Action _loader;

        internal Tag(ObjectStorage storage, ObjectReader reader, string id)
            : base(id)
        {
            _storage = storage;
            _loader = () => LoadFromObjectReader(reader);
        }

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
        public string ObjectType
        {
            get
            {
                this.EnsureLoaded();
                return _type;
            }
        }

        private AbstractObject _object;
        public AbstractObject Object
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

            _loader();

            this._loaded = true;
        }

        private void LoadFromObjectReader(ObjectReader reader)
        {
            reader.Load(stream => LoadFromReader(new StreamReader(stream), reader.Size));
        }

        private void LoadFromReader(TextReader reader, long size)
        {
            var bytesRead = 0;
            string line;
            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                bytesRead += line.Length + 1; //add 1 for LF
                var parts = line.Split(new[] { ' ' }, 2);
                switch (parts[0])
                {
                    case "object":
                        this._object = _storage.Read(parts[1]);
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

            var messageSize = size - bytesRead;
            var buffer = new char[messageSize];
            var read = reader.Read(buffer, 0, buffer.Length);
            this._message = new string(buffer, 0, read);
        }

        public override ObjectType Type
        {
            get { return Gitty.ObjectType.Tag; }
        }
    }
}