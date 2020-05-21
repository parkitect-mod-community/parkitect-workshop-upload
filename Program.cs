using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CommandLine;

namespace parkitect_workshop_upload
{
    class Options
    {
        [Option('u', "username", Required = true, HelpText = "Steam Username")]
        public String SteamUsername { get; set; }

        [Option('p', "password", Required = true, HelpText = "Steam Password")]
        public String SteamPassword { get; set; }

        [Option('c', "project", HelpText = "Directory for Project Path", Default = "./")]
        public String Path { get; set; }

        [Option('o', "output", HelpText = "Directory for output Mod", Default = "./bin")]
        public String Output { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args).MapResult(
                (Options opts) => RunOptions(opts), errs => 1);
        }

        static void UpdateProjectHintsAndOutput(String projectPath, String assemblyPath, String output)
        {

            String csprojFile = null;
            foreach (var file in Directory.GetFiles(projectPath))
            {
                if (file.Contains(".csproj"))
                {
                    csprojFile = file;
                    break;
                }
            }

            XmlDocument document = new XmlDocument();
            Console.WriteLine("Opening csproj: {0}", Path.Combine(projectPath, csprojFile));
            document.Load(Path.Combine(projectPath, csprojFile));

            var manager = new XmlNamespaceManager(document.NameTable);
            manager.AddNamespace("x", "http://schemas.microsoft.com/developer/msbuild/2003");

            var toReference = document.SelectNodes("//x:Reference", manager)
                .Cast<XmlNode>();

            List<String> parkitectAssemblies = new List<string>();
            foreach (var file in Directory.GetFiles(assemblyPath))
            {
                if (file.EndsWith(".dll"))
                {
                    parkitectAssemblies.Add(  Path.GetFileName(file).Replace(".dll", "").Trim());
                }
            }

            foreach (var node in toReference)
            {
                var inc = node.Attributes["Include"];
                String targetAssembly = inc.Value.Split(',').FirstOrDefault();

                if (parkitectAssemblies.Contains(targetAssembly))
                {
                    var isPrivate = node.SelectNodes("//x:Private",manager);
                    if (isPrivate.Count == 0) {
                        var privateNode = document.CreateElement("Private",
                            "http://schemas.microsoft.com/developer/msbuild/2003");
                        privateNode.InnerText = "False";
                        node.AppendChild(privateNode);
                    }
                    else
                    {
                        foreach (var prv in isPrivate.Cast<XmlNode>())
                        {
                            prv.InnerText = "False";
                        }
                    }

                    var hints = node.SelectNodes("//x:HintPath",manager);
                    if (hints.Count > 0)
                    {
                        bool foundAssembly = false;
                        foreach (var hint in hints.Cast<XmlNode>())
                        {
                            String assembly = Path.GetFileName(hint.InnerText).Replace(".dll", "").Trim();
                            if (assembly.Equals(targetAssembly))
                            {
                                foundAssembly = true;
                                String target =  Path.Join(assemblyPath, targetAssembly + ".dll");
                                Console.WriteLine("Found Existing Assembly {0} -- Updating path: {1} to {2}", targetAssembly, hint.InnerText, target);
                                hint.InnerText = target;
                                break;
                            }
                        }

                        if (!foundAssembly)
                        {
                            var hint = document.CreateElement("HintPath",
                                "http://schemas.microsoft.com/developer/msbuild/2003");
                            hint.InnerText = Path.Join(assemblyPath, targetAssembly + ".dll");
                            node.AppendChild(hint);
                            Console.WriteLine("Resolved Assembly {0} -- path: {1}", targetAssembly, hint.InnerText);
                        }

                    }
                    else
                    {
                        var hint = document.CreateElement("HintPath",
                            "http://schemas.microsoft.com/developer/msbuild/2003");
                        hint.InnerText = Path.Join(assemblyPath, targetAssembly + ".dll");

                        Console.WriteLine("Resolved Assembly {0} -- path: {1}", targetAssembly, hint.InnerText);
                        node.AppendChild(hint);
                    }
                }
            }

            foreach (var node in document.SelectNodes("//x:OutputPath", manager).Cast<XmlNode>())
            {
                node.InnerText = output;
            }

            document.Save(Path.Join(projectPath, csprojFile));
        }


        static int RunOptions(Options options)
        {
            DepotDownloader downloader = new DepotDownloader();
            if (downloader.Login(options.SteamUsername, options.SteamPassword))
            {
                String ParkitectPath = Path.Combine(options.Path, "Game");
                downloader.DownloadDepot(ParkitectPath, 453090, 453094, "public", s => s.EndsWith(".dll")).Wait();
                UpdateProjectHintsAndOutput(options.Path, Path.Combine(ParkitectPath, "Parkitect_Data/Managed"),
                    options.Path + "/bin");
            }
            else
            {
                Console.WriteLine("Failed to login");
            }

            Console.WriteLine("Completed");
            return 0;
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            //handle errors
        }

    }
}
