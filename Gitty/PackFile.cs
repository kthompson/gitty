﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Gitty
{
    class PackFile
    {
        
        private int _entryCount;
        public int EntryCount
        {
            get
            {
                this.EnsureLoaded();
                return _entryCount;
            }
        }

        private PackIndex _index;
        public PackIndex Index
        {
            get
            {
                this.EnsureLoaded();
                return _index;
            }
        }

        private int _version;
        public int Version
        {
            get
            {
                this.EnsureLoaded();
                return _version;
            }
        }

        public string Location { get; private set; }
        public string IndexLocation { get; private set; }

        public PackFile(string packFile)
        {
            this.Location = Helper.MakeAbsolutePath(packFile);
            if (this.Location.EndsWith(".pack"))
                this.IndexLocation = this.Location.Substring(0, this.Location.Length - 4) + "idx";
        }

        private bool _loaded;
        private void EnsureLoaded()
        {
            if (_loaded)
                return;

            using (var reader = new BinaryReader(File.OpenRead(this.Location)))
            {
                var sig = reader.ReadBytes(4);

                if (!(sig[0] == 'P' &&
                      sig[1] == 'A' &&
                      sig[2] == 'C' &&
                      sig[3] == 'K'))
                {
                    throw new InvalidOperationException("not a pack file");
                }

                this._version = reader.ReadInt32();
                this._entryCount = reader.ReadInt32();
                this._index = new PackIndex(this.IndexLocation, this.EntryCount);
            }
            this._loaded = true;
        }

        internal static IEnumerable<PackFile> FindAll(Repository repository)
        {
            var packs = new DirectoryInfo(repository.PacksLocation);
            if(packs.Exists)
                return packs
                    .EnumerateFiles("*.pack")
                    .Select(pf => new PackFile(pf.FullName));

            return new PackFile[] {};
        }

        public bool HasEntry(string id)
        {
            return this.Index.GetEntry(id) != null;
        }

        public ObjectLoader GetObjectLoader(string id)
        {
            return new PackedObjectLoader(this, id);
        }
    }
}