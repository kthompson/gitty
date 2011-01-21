using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Gitty
{
    internal sealed class Helper
    {
        public static IEnumerable<string> GetLocations(string location)
        {
            return new DirectoryInfo(location)
                      .EnumerateFiles("*", SearchOption.AllDirectories)
                      .Select(f => f.FullName);
        }

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static String MakeRelativePath(string fromPath, string toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            if (fromPath[fromPath.Length - 1] != Path.DirectorySeparatorChar)
                fromPath += Path.DirectorySeparatorChar;

            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);

            return relativeUri.ToString();

        }

        public static string MakeAbsolutePath(string path)
        {
            if (File.Exists(path))
                return new FileInfo(path).FullName;
            
            if (Directory.Exists(path))
                return new DirectoryInfo(path).FullName;

            return null;
        }

        public static byte[] IdToByteArray(string id)
        {
            var array = new byte[20];

            for (int i = 0; i < 40; i+=2)
            {
                array[i/2] = byte.Parse(id.Substring(i, 2), NumberStyles.HexNumber);
            }
            return array;

        }
    }
}
