using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gitty
{
    public abstract class AbstractObject
    {
        public virtual string Id { get; protected set; }
        public abstract ObjectType Type { get; }

        protected AbstractObject(string id = null)
        {
            this.Id = id;
        }
    }
}
