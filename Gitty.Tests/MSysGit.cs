﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Gitty.Tests
{
    public class MSysGit : DynamicObject
    {
        private static readonly string MSysGitPath = Environment.GetEnvironmentVariable("MSYSGIT_PATH");

        private static void Exec(IEnumerable<string> arguments, out object result)
        {
            if (MSysGitPath == null)
                throw new InvalidOperationException("MSYSGIT_PATH was not specified. Please make sure to set the MSYSGIT_PATH environment variable to the bin directory of your MSysGit install.");

            var git = Path.Combine(MSysGitPath, "git.exe");
            if (!File.Exists(git))
                throw new InvalidOperationException("Could not find git.exe in MSYSGIT_PATH. Please make sure to set the MSYSGIT_PATH environment variable to the bin directory of your MSysGit install.");

            var start = new ProcessStartInfo(git, string.Join(" ", arguments))
                            {
                                RedirectStandardOutput = true,
                                UseShellExecute = false, 
                                WindowStyle =  ProcessWindowStyle.Hidden, 
                                CreateNoWindow =  true,
                            };
            var dir = Environment.GetEnvironmentVariable("GIT_WORK_TREE",
                                                         EnvironmentVariableTarget.Process);
            if (dir != null)
                start.WorkingDirectory = dir;

            using (var proc = Process.Start(start))
            {
                string[] output = {string.Empty};

                proc.OutputDataReceived += (sender, args) => output[0] += args.Data + Environment.NewLine;

                proc.BeginOutputReadLine();
                
                proc.WaitForExit();
                result = output[0];
            }
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = null;
            var command = ToCommand(binder.Name);
            Exec(new[] {command}.Concat(args.Select(a => a.ToString())), out result);
            return true;
        }

        private static string ToCommand(string name)
        {
            return Regex.Matches(name, "[A-Z][a-z]+")
                    .OfType<Match>()
                    .Select(match => match.Value)
                    .Aggregate((acc, b) => acc + "-" + b)
                    .TrimStart(' ')
                    .ToLower();
        }
    }
}
