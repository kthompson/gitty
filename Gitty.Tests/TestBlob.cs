using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gitty.Tests
{
    class TestBlob : Blob
    {
        internal TestBlob(Tree parent, string name)
            : base(null, 0, null, parent, name)
        {
        }
    }
}
