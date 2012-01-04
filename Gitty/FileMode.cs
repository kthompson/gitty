using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gitty
{
    class FileMode
    {

        private static readonly int TypeMask = FromOctal("0170000");

        public static readonly int GitLink = FromOctal(GitLinkOctal);
        public static readonly int Symlink = FromOctal(SymlinkOctal);
        public static readonly int File    = FromOctal(FileOctal);
        public static readonly int Tree    = FromOctal(TreeOctal);

        public const string GitLinkOctal = "0160000";
        public const string SymlinkOctal = "0120000";
        public const string FileOctal = "0100000";
        public const string TreeOctal = "0040000";


        public static int FromOctal(string octal)
        {
            var value = 0;
            foreach (var letter in octal)
            {
                value <<= 3;
                value += letter - '0';    
            }

            return value;
        }

        public static bool IsGitLink(string mode)
        {
            return IsGitLink(FromOctal(mode));
        }

        public static bool IsGitLink(int mode)
        {
            return (mode & TypeMask) == GitLink;
        }

        public static bool IsFile(string mode)
        {
            return IsFile(FromOctal(mode));
        }

        public static bool IsFile(int mode)
        {
            return (mode & TypeMask) == File;
        }

        public static bool IsSymlink(string mode)
        {
            return IsSymlink(FromOctal(mode));
        }

        public static bool IsSymlink(int mode)
        {
            return (mode & TypeMask) == Symlink;
        }

        public static bool IsTree(string mode)
        {
            return IsTree(FromOctal(mode));
        }

        public static bool IsTree(int mode)
        {
            return (mode & TypeMask) == Tree;
        }
    }
}
