using NUnit.Engine;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace NUnitAdapter
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var engine = TestEngineActivator.CreateInstance())
            {
#if NETFRAMEWORK
                var binPath = args.FirstOrDefault() ?? @"D:\prog\git\hub\dedale\dummy-for-pipelines\.build\bin\net48";
#endif
#if NETCOREAPP
                var binPath = args.FirstOrDefault() ?? @"D:\prog\git\hub\dedale\dummy-for-pipelines\.build\bin\netcoreapp3.1";
#endif

                engine.WorkDirectory = binPath;
                engine.Initialize();

                foreach (var path in Directory.GetFiles(binPath, "*Tests.dll"))
                {
                    Console.WriteLine($"Exploring {path}...");

                    var package = new TestPackage(path);
#if NETCOREAPP
                    package.AddSetting("DomainUsage", "None");
#endif
#if NETFRAMEWORK
                    package.AddSetting("DomainUsage", "Single");
                    package.AddSetting("BasePath", binPath);
#endif
                    package.AddSetting("DisposeRunners", true);
                    try
                    {
                        using (var runner = engine.GetRunner(package))
                        {
                            var xml = runner.Explore(TestFilter.Empty);
                            var doc = XDocument.Parse(xml.OuterXml);
                            doc.XPathSelectElements("//test-run")
                                .ToList()
                                .ForEach(e => Console.WriteLine($"Explored {e.Attribute("name").Value}, found {e.Attribute("testcasecount").Value} test(s)"));
                            runner.Unload();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.ToString());
                    }
                }
            }
        }
    }
}
