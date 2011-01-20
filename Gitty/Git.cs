using System.Text;

namespace Gitty
{
    public class Git
    {
        public static Repository Open(string directory, bool bare = false)
        {
            return new Repository(directory);
        }
    }
}
