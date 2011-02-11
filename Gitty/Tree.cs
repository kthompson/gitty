﻿using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Gitty.Storage;

namespace Gitty
{
    public class Tree : TreeEntry
    {
        internal Tree(Repository repository, ObjectReader reader, string id, string name = null, string mode = null, Tree parent = null)
            : base(repository, reader, id, name, mode, parent)
        {
        }

        private readonly List<TreeEntry> _items = new List<TreeEntry>();
        public virtual IEnumerable<TreeEntry> Items
        {
            get
            {
                this.EnsureLoaded();
                return _items.AsReadOnly();
            }
        }

        private static IEnumerable<T> FlattenTree<T>(IEnumerable<T> entries, Func<T,bool> filter, Func<T, IEnumerable<T>> selector)
        {
            foreach (var entry in entries)
            {
                if (filter(entry))
                {
                    yield return entry;
                }
                else
                {
                    foreach (var subentry in FlattenTree(selector(entry), filter, selector))
                    {
                        yield return subentry;
                    }
                }
            }
        }

        public IEnumerable<TreeEntry> EnumerateItems(bool recursive = false)
        {
            if (!recursive)
                return this.Items;

            return FlattenTree(this.Items, entry => entry.Type == ObjectType.Blob,
                                           entry => ((Tree) entry).EnumerateItems(recursive));
        }

        private bool _loaded;

        public override ObjectType Type
        {
            get { return ObjectType.Tree; }
        }

        protected virtual void EnsureLoaded()
        {
            if (_loaded)
                return;

            this.Reader.Load(stream =>
            {
                var bytesRead = 0;
                
                while (bytesRead < Reader.Size)
                {
                    //read until space for mode
                    var mode = stream.ReadUntil(c => c == ' ');
                    bytesRead += mode.Length + 1;
                    var name = stream.ReadUntil(c => c == '\0');
                    bytesRead += name.Length + 1;
                    var id = stream.ReadId();
                    bytesRead += 20;
                    var entry = this.Repository.OpenTreeEntry(id, name, mode, this);
                    this._items.Add(entry);
                }
            });

            this._loaded = true;
        }
    }

    class WorkingTreeHelper
    {
        public static TreeEntry GetWorkingTreeEntry(Repository repo, Tree parent, FileSystemInfo info)
        {
            if (info is FileInfo)
                return new WorkingTreeBlob(repo, (FileInfo)info, parent);

            return new WorkingTreeTree(repo, (DirectoryInfo)info, parent);
        }

        public static bool NotIgnored(Repository repo, FileSystemInfo info)
        {
            //TODO: add support for .gitignore
            return true;
        }

        public static string ModeForFileSystemInfo(FileSystemInfo info)
        {
            throw new NotImplementedException();
        }
    }

    class WorkingTreeTree : Tree
    {
        public DirectoryInfo Directory { get; private set; }

        public WorkingTreeTree(Repository repository, DirectoryInfo info, Tree parent)
            : base(repository, null, null, info.Name, WorkingTreeHelper.ModeForFileSystemInfo(info), parent)
        {
            this.Directory = info;
        }

        public override IEnumerable<TreeEntry> Items
        {
            get
            {
                var repo = this.Repository;

                return this.Directory
                        .EnumerateFileSystemInfos()
                        .Where(info => WorkingTreeHelper.NotIgnored(repo, info))
                        .Select(info => WorkingTreeHelper.GetWorkingTreeEntry(repo, this, info));
            }
        }

        protected override void EnsureLoaded()
        {
            //dont use the object reader in the base class
        }
    }

    class WorkingTreeBlob : Blob
    {
        public FileInfo File { get; private set; }

        public WorkingTreeBlob(Repository repository, FileInfo info, Tree parent)
            : base(repository, null, null, info.Name, WorkingTreeHelper.ModeForFileSystemInfo(info), parent)
        {
            File = info;
        }

        public override void GetContentStream(Action<Stream, IObjectInfo> contentLoader)
        {
            //dont use the object reader in the base class

        }
    }y
}