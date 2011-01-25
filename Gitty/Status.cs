using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gitty
{
    public class Status
    {
        private readonly Repository _repository;

        public Status(Repository repository)
        {
            _repository = repository;
        }

        //private IEnumerable<string> Changed { get; private set; }
        //private IEnumerable<string> Untracked { get; private set; }
        //private IEnumerable<string> Ignored { get; private set; }


        //private IEnumerable<string> Updated { get; private set; }
        //private IEnumerable<string> Unmerged { get; private set; }
        //private IEnumerable<string> Changed { get; private set; }
    }
}
