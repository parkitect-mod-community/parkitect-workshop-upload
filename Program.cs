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
            CommandLine.Parser.Default.ParseArguments<Options>(args).WithParsed(RunOptions)
                .WithNotParsed(HandleParseError);
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
            document.Load(Path.Combine(projectPath, csprojFile));

            var manager = new XmlNamespaceManager(document.NameTable);
            var toReference = document.SelectNodes("//x:Reference", manager)
                .Cast<XmlNode>();

            List<String> parkitectAssemblies = new List<string>();
            foreach (var file in Directory.GetFiles(assemblyPath))
            {
                if (file.EndsWith(".dll"))
                {
                    parkitectAssemblies.Add(file.Replace(".dll", "").Trim());
                }
            }

            foreach (var node in toReference)
            {
                var inc = node.Attributes["Include"];
                String targetAssembly = inc.Value.Split(',').FirstOrDefault();
                if (parkitectAssemblies.Contains(targetAssembly))
                {
                    var hints = node.SelectNodes("//x:Hint");
                    if (hints.Count > 0)
                    {
                        bool foundAssembly = false;
                        foreach (var hint in hints.Cast<XmlNode>())
                        {
                            String assembly = Path.GetFileName(hint.InnerText).Replace(".dll", "");
                            if (assembly == targetAssembly)
                            {
                                foundAssembly = true;
                                hint.InnerText = Path.Join(assemblyPath, targetAssembly + ".dll");
                                Console.WriteLine("Resolved Assembly {0} -- path: {1}", targetAssembly, hint.InnerText);
                                break;
                            }
                        }

                        if (!foundAssembly)
                        {
                            var hint = document.CreateElement("HintPath", document.NamespaceURI);
                            hint.InnerText = Path.Join(assemblyPath, targetAssembly + ".dll");
                            node.AppendChild(hint);
                            Console.WriteLine("Resolved Assembly {0} -- path: {1}", targetAssembly, hint.InnerText);
                        }

                    }
                    else
                    {
                        var hint = document.CreateElement("HintPath", document.NamespaceURI);
                        hint.InnerText = Path.Join(assemblyPath, targetAssembly + ".dll");

                        Console.WriteLine("Resolved Assembly {0} -- path: {1}", targetAssembly, hint.InnerText);
                        node.AppendChild(hint);
                    }
                }
            }

            foreach (var node in document.SelectNodes("//x:OutputPath").Cast<XmlNode>())
            {
                node.InnerText = output;
            }

            document.Save(Path.Join(projectPath, csprojFile));
        }


        static void RunOptions(Options options)
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
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            //handle errors
        }

    }
}
