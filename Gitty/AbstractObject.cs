using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gitty
{
    public abstract class AbstractObject
    {
        public string Id { get; protected set; }
        public ObjectType Type { get; protected set; }

        protected AbstractObject(ObjectType type, string id = null)
        {
            this.Type = type;
            this.Id = id;
        }
    }
}
