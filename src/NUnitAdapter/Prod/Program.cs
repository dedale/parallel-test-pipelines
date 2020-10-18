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
        private sealed class Listener : ITestEventListener
        {
            public void OnTestEvent(string report)
            {
            }
        }
        private static int CompareFixtures(string x, string y)
        {
            var xid = x.Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries).Last();
            var yid = y.Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries).Last();
            if (int.TryParse(xid, out var xi) && int.TryParse(yid, out var yi))
                return xi.CompareTo(yi);
            return x.CompareTo(y);
        }
        static void Main(string[] args)
        {
            using (var engine = TestEngineActivator.CreateInstance())
            {
#if NETCOREAPP
                var binPath = args.FirstOrDefault() ?? @"D:\prog\git\hub\dedale\dummy-for-pipelines\.build\bin\netcoreapp3.1";
#endif
#if NETFRAMEWORK
                var binPath = args.FirstOrDefault() ?? @"D:\prog\git\hub\dedale\dummy-for-pipelines\.build\bin\net48";
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
                    package.AddSetting("DomainUsage", "Single"); // Single
                    package.AddSetting("BasePath", binPath);
#endif
                    try
                    {
                        using (var runner = engine.GetRunner(package))
                        {
                            var xml = runner.Explore(TestFilter.Empty);
                            var doc = XDocument.Parse(xml.OuterXml);
                            doc.XPathSelectElements("//test-run")
                                .ToList()
                                .ForEach(e => Console.WriteLine($"Explored {e.Attribute("name").Value}, found {e.Attribute("testcasecount").Value} test(s)"));
                            var fixtures = doc.XPathSelectElements("//test-suite[@type='TestFixture']")
                                .Select(e => e.Attribute("fullname").Value)
                                .ToList();
                            fixtures.Sort(CompareFixtures);
                            fixtures.Reverse();
                            foreach (var fixture in fixtures)
                            {
                                Console.WriteLine($"Running {fixture}...");
                                xml = runner.Run(new Listener(), TestFilter.Empty);
                            }
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
