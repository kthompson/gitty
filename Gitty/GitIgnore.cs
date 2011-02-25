using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gitty
{
    /// <summary>
    /// Class for maintaining items to be excluded
    /// </summary>
    class GitIgnore
    {
        public GitIgnore Parent { get; private set; }

        public List<string> Excludes { get; private set; }
        public List<string> Overrides { get; private set; }

        public bool IsMatch(TreeEntry entry)
        {
            return false;
        }
    }
}
