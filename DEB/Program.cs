using DotnetMakeDeb;
using DotnetMakeDeb.Deb;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace DEB
{
    class Program
    {
        static int Main(string[] args)
        {
            var arg = new ArgParser(args);
            var deb = new DebPackage();

            if (arg["o"] == null)
            {
                Console.WriteLine(help);
                return -1;
            }

            if (arg["file"] != null)
                deb.ReadSpecification(arg["file"].Trim('\\'));
            else if (arg["dir"] != null)
                deb.ReadSpecificationDeb(arg["dir"].Trim('\\'));
            else
            {
                Console.WriteLine(help);
                return -1;
            }


            // set version package
            if (arg["v"] != null)
            {
                deb.SetVersion(arg["ver"]);
            }
            else if (arg["fv"] != null)
            {
                var ver = FileVersionInfo.GetVersionInfo(arg["fv"]).ProductVersion;
                if (ver != null)
                {
                    deb.SetVersion(ver);
                }
            }

            // set output dir
            if (arg["dir"] != null)
            { 
                deb.OutDir = Path.GetDirectoryName(arg["o"]);
            }

            var debFile = arg["o"];
            if (arg["v"] != null || arg["fv"] != null)
                debFile = Path.GetFileNameWithoutExtension(arg["o"]) + "." + deb.controlParams.Version.TrimStart('.').TrimEnd('.') + ".deb";

            if (!debFile.EndsWith(".deb"))
               debFile += ".deb";

            using (FileStream file = File.OpenWrite(debFile))
            {
                deb.WritePackage(file);
            }

            return 0;
        }


        static string help = @"
+--------------------------------------+
| -v    set version package            |
| -fv   set file check version package |
| -o    set output file                |
| -file set file .debspec              |
| -dir  set directory package          |
+--------------------------------------+


";    }


    class ArgParser
    {
        private StringDictionary Parameters = new StringDictionary();

        public string this[string Param] => Parameters[Param];

        public ArgParser(string[] Args)
        {
            Regex regex = new Regex("^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Regex regex2 = new Regex("^['\"]?(.*?)['\"]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            string text = null;
            foreach (string input in Args)
            {
                string[] array = regex.Split(input, 3);
                switch (array.Length)
                {
                    case 1:
                        if (text != null)
                        {
                            if (!Parameters.ContainsKey(text))
                            {
                                array[0] = regex2.Replace(array[0], "$1");
                                Parameters.Add(text, array[0]);
                            }
                            text = null;
                        }
                        break;
                    case 2:
                        if (text != null && !Parameters.ContainsKey(text))
                        {
                            Parameters.Add(text, "true");
                        }
                        text = array[1];
                        break;
                    case 3:
                        if (text != null && !Parameters.ContainsKey(text))
                        {
                            Parameters.Add(text, "true");
                        }
                        text = array[1];
                        if (!Parameters.ContainsKey(text))
                        {
                            array[2] = regex2.Replace(array[2], "$1");
                            Parameters.Add(text, array[2]);
                        }
                        text = null;
                        break;
                }
            }
            if (text != null && !Parameters.ContainsKey(text))
            {
                Parameters.Add(text, "true");
            }
        }
    }
}
