using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gitty;
using Gitty.Storage;

namespace Gitty.Tests
{
    class TestTreeEntry : List<TestTreeEntry>
    {
        public ObjectType Type { get; set; }
        public string Name { get; set; }

        public TestTreeEntry(string name, ObjectType type = ObjectType.Blob)
        {
            this.Name = name;
            this.Type = type;
        }
    }

    class TestTree : Tree, IEnumerable<TestTreeEntry>
    {
        private readonly List<TreeEntry> _items = new List<TreeEntry>();

        internal TestTree(Tree parent, string name)
            : base(null, null, 0, null, parent, name)
        {
        }

        public void Add(TestTreeEntry entry)
        {
            if (entry.Type == ObjectType.Tree)
            {
                var tree = new TestTree(this, entry.Name);
                foreach (var subentry in entry)
                    tree.Add(subentry);

                _items.Add(tree);
            }
            else
            {
                _items.Add(new TestBlob(this, entry.Name));
            }
        }

        protected override IEnumerable<TreeEntry> ItemsInternal
        {
            get
            {
                return _items;
            }
        }

        public IEnumerator<TreeEntry> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
