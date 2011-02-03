using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;

namespace Gitty.Tests
{
    public class MSysGit : DynamicObject
    {
        private static readonly string MSysGitPath = Environment.GetEnvironmentVariable("MSYSGIT_PATH");

        private static void Exec(IEnumerable<string> arguments)
        {
            if (MSysGitPath == null)
                throw new InvalidOperationException("MSYSGIT_PATH was not specified. Please make sure to set the MSYSGIT_PATH environment variable to the bin directory of your MSysGit install.");

            var git = Path.Combine(MSysGitPath, "git.exe");
            if (!File.Exists(git))
                throw new InvalidOperationException("Could not find git.exe in MSYSGIT_PATH. Please make sure to set the MSYSGIT_PATH environment variable to the bin directory of your MSysGit install.");

            var proc = Process.Start(git, string.Join(" ", arguments));

            if (proc == null)
                return;

            proc.WaitForExit();
            proc.Dispose();
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = null;
            var command = binder.Name.ToLower();
            Exec(new[] {command}.Concat(args.Select(a => a.ToString())));
            return true;
        }
    }
}
