using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gitty.Storage;

namespace Gitty
{
    class WorkingTreeDirectory : Tree
    {
        public DirectoryInfo Directory { get; set; }

        internal WorkingTreeDirectory(DirectoryInfo root)
            : this(root, null, null, ModeFromFileSystemInfo(root))
        {
        }

        internal WorkingTreeDirectory(DirectoryInfo directory, Tree parent, string name, string mode)
            : base(null, null, 0, null, parent, name, mode)
        {
            this.Directory = directory;
        }

        public override IEnumerable<TreeEntry> Items
        {
            get { return this.Directory.EnumerateFileSystemInfos().Select(FileSystemInfoToWorkingTree); }
        }

        private TreeEntry FileSystemInfoToWorkingTree(FileSystemInfo info)
        {
            var mode = ModeFromFileSystemInfo(info);
            var name = info.Name;

            if (info is FileInfo)
                return new WorkingTreeFile((FileInfo) info, this, name, mode);

            if (info is DirectoryInfo)
                return new WorkingTreeDirectory((DirectoryInfo) info, this, name, mode);

            throw new ArgumentException("info is not of this world.");
        }

        private static string ModeFromFileSystemInfo(FileSystemInfo info)
        {
            //TODO implement Mode From FileSystemInfo
            return string.Empty;
        }

        public static bool NotIgnored(Repository repo, FileSystemInfo info)
        {
            //TODO: add support for .gitignore
            return true;
        }
    }

    class WorkingTreeFile : Blob
    {
        public FileInfo File { get; private set; }

        internal WorkingTreeFile(FileInfo file, Tree parent, string name, string mode)
            : base(null, 0, null, parent, name, mode)
        {
            this.File = file;
        }
    }
}
