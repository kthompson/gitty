using System;
using System.IO;
using System.Text;

namespace Gitty
{
    /// <summary>
    /// Helper class for opening and creating Git Repositories
    /// </summary>
    public static class Git
    {
        /// <summary>
        /// Opens the specified working directory.
        /// </summary>
        /// <param name="workingDirectory">The working directory.</param>
        /// <param name="gitDirectory">The git directory.</param>
        /// <returns></returns>
        public static Repository Open(string workingDirectory, string gitDirectory = null)
        {
            return new Repository(workingDirectory, gitDirectory);
        }

        /// <summary>
        /// Initializes a new git repository in the specified directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="bare">if set to <c>true</c> [bare].</param>
        /// <returns></returns>
        public static Repository Init(string directory, bool bare = false)
        {
            if (bare)
                return new Repository(null, directory, true);

            return new Repository(directory, null, true);
        }
    }
}
