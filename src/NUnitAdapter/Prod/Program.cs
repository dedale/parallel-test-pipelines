using NUnit.Engine;
using NUnit.Engine.Runners;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NUnitAdapter
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var engine = TestEngineActivator.CreateInstance())
            //using (var engine = new TestEngine())
            {
#if NETFRAMEWORK
                var binPath = args.FirstOrDefault() ?? @"D:\prog\git\hub\dedale\dummy-for-pipelines\.build\bin\net48";
#endif
#if NETCOREAPP
                var binPath = args.FirstOrDefault() ?? @"D:\prog\git\hub\dedale\dummy-for-pipelines\.build\bin\netcoreapp3.1";
#endif

                engine.WorkDirectory = binPath;
                //engine.InternalTraceLevel = InternalTraceLevel.Verbose;
                engine.Initialize();

                //engine.Services.GetService<IDriverService>().GetDriver()

                foreach (var path in Directory.GetFiles(binPath, "*Tests.dll"))
                {
                    Console.WriteLine(path);

                    //var assembly = Assembly.LoadFrom(path);
                    //Console.WriteLine(assembly.FullName);

                    var package = new TestPackage(path);
                    package.AddSetting("DomainUsage", "None"); // Single? None?
                    //package.AddSetting("ProcessModel", "InProcess"); // Single? InProcess?
                    package.AddSetting("DisposeRunners", true);
                    //package.Settings["BasePath"] = binPath;
                    //package.Settings["InternalTraceLevel"] = "Verbose";
                    try
                    {
                        using (var runner = engine.GetRunner(package))
                        {
                            Console.WriteLine(runner.GetType().FullName);
                            //var driverService = engine.Services.GetService<IDriverService>();
                            //var driver = driverService.GetDriver(AppDomain.CurrentDomain, path, "netcoreapp3.1", true);
                            //Console.WriteLine(driver.GetType().FullName);
                            //var loaded = driver.Load(path, package.Settings);
                            //Console.WriteLine(loaded);
                            //var explored = driver.Explore(null);
                            //Console.WriteLine(explored);

                            var xml = runner.Explore(TestFilter.Empty);
                            Console.WriteLine(xml.OuterXml);
                            runner.Unload();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine($"{e.GetType().Name} {e.Message}");
                        if (e.InnerException != null)
                            Console.Error.WriteLine($"  {e.InnerException.GetType().Name} {e.InnerException.Message}");
                    }
                }
            }
        }
    }
}
