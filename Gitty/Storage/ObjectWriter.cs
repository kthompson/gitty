using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Gitty.Storage
{
    class ObjectWriter
    {
        public static string ComputeId(Tree tree)
        {
            using (var md = new MessageDigest())
            {
                byte[] data;
                var ms = new MemoryStream();

                foreach (var item in tree.Items)
                {
                    data = Encoding.Default.GetBytes(string.Format("{0} {1}\0", item.Mode, item.Name));
                    ms.Write(data, 0, data.Length);

                    data = Helper.IdToByteArray(item.Id);
                    ms.Write(data, 0, data.Length);
                }

                data = Encoding.Default.GetBytes(string.Format("tree {0}\0", ms.Length));

                md.Update(data);
                md.Update(ms);
                
                var digest = md.Digest();

                return Helper.ByteArrayToId(digest);
            }
        }

        public static string ComputeId(Blob blob)
        {
            using (var md = new MessageDigest())
            {
                byte[] data = Encoding.Default.GetBytes(string.Format("blob {0}\0", blob.Size));

                md.Update(data);
                md.Update(blob.Data);

                var digest = md.Digest();

                return Helper.ByteArrayToId(digest);
            }
        }

        //private static string CalculateId(WorkingTreeFile file)
        //{
        //    using (var md = new MessageDigest())
        //    {
        //        byte[] data = Encoding.Default.GetBytes(string.Format("blob {0}\0", file.Length));

        //        md.Update(data);

        //        using (var stream = file.OpenRead())
        //        {
        //            md.Update(stream);
        //        }

        //        var digest = md.Digest();

        //        return Helper.ByteArrayToId(digest);
        //    }
        //}
        
    }
}
