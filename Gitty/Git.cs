using System;
using System.IO;
using System.Text;

namespace Gitty
{
    public class Git
    {
        public static Repository Open(string workingDirectory, string gitDirectory = null)
        {
            return new Repository(workingDirectory, gitDirectory);
        }

        public static Repository Init(string directory, bool bare = false)
        {
            if (bare)
                return new Repository(null, directory, true);

            return new Repository(directory, null, true);
        }
    }
}
