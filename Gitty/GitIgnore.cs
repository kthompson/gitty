using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gitty
{
    /// <summary>
    /// Class for maintaining items to be excluded
    /// 
    /// Patterns have the following format:
    ///    
    /// - A blank line matches no files, so it can serve as a separator for readability.
    /// - A line starting with # serves as a comment.
    /// - An optional prefix ! which negates the pattern; any matching file excluded by a previous pattern will become included again. If a negated pattern matches, this will override lower precedence patterns sources.
    /// - If the pattern ends with a slash, it is removed for the purpose of the following description, but it would only find a match with a directory. In other words, foo/ will match a directory foo and paths underneath it, but will not match a regular file or a symbolic link foo (this is consistent with the way how pathspec works in general in git).
    /// - If the pattern does not contain a slash /, git treats it as a shell glob pattern and checks for a match against the pathname relative to the location of the .gitignore file (relative to the toplevel of the work tree if not from a .gitignore file).
    /// - Otherwise, git treats the pattern as a shell glob suitable for consumption by fnmatch(3) with the FNM_PATHNAME flag: wildcards in the pattern will not match a / in the pathname. For example, "Documentation/*.html" matches "Documentation/git.html" but not "Documentation/ppc/ppc.html" or "tools/perf/Documentation/perf.html".
    /// - A leading slash matches the beginning of the pathname. For example, "/*.c" matches "cat-file.c" but not "mozilla-sha1/sha1.c".
    /// </summary>
    class GitIgnore
    {
        public GitIgnore Parent { get; private set; }

        public List<string> Excludes { get; private set; }
        public List<string> Overrides { get; private set; }

        public GitIgnore()
        {
            this.Excludes = new List<string>();
            this.Overrides = new List<string>();
        }

        public bool IsMatch(TreeEntry entry)
        {
            return false;
        }
    }
}
