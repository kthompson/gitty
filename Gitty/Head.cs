using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Gitty
{
    public class Head
    {
        public Repository Repository { get; private set; }
        public string Location { get; private set; }

        public bool IsDetached
        {
            get { return this.Ref == null; }
        }

        public Ref Ref { get; private set; }
        public string Id { get; private set; }

        public Head(Repository repository)
        {
            this.Repository = repository;
            this.Location = Path.Combine(repository.Location, "HEAD");

            this.LoadHead();
        }

        private void LoadHead()
        {
            string data;
            using (var reader = new StreamReader(File.OpenRead(this.Location)))
            {
                data = reader.ReadToEnd().TrimEnd();
            }

            if (data.StartsWith("ref: "))
            {
                data = data.Substring("ref: ".Length);
                this.Ref = this.Repository.Refs.Where(r => r.RelativePath == data).First();
                this.Id = this.Ref.Id;
            }
            else
            {
                this.Id = data;
            }
        }
    }
}
