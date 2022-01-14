using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace TechnicalAnalysisTools.Shared.Helpers
{
    public static class AssemblyHelper
    {
        private static bool IsAppDomainCreationEnabled { get; } = false;

        private static Dictionary<string, AppDomain> AssemblyAppDomains { get; } = new Dictionary<string, AppDomain>();

        private static Dictionary<string, Assembly> Assemblies { get; } = new Dictionary<string, Assembly>();

        public static string CompiledAlarmDataFolder { get; set; }

        private static string GetFileUniqueName(string filePath)
        {
            var fileInfo = new FileInfo(filePath);

            return "Key" + fileInfo.Name.ToLower().GetHashCode().ToString();
        }

        public static bool LoadAssemblyFile(string filePath)
        {
            bool result;

            var key = GetFileUniqueName(filePath);

            if (IsAppDomainCreationEnabled)
            {
                var appDomain = AppDomain.CreateDomain("AppDomain" + key);

                try
                {
                    var assemblyName = new AssemblyName() { CodeBase = filePath };

                    var assembly = appDomain.Load(assemblyName);

                    AssemblyAppDomains[key] = appDomain;
                    Assemblies[key] = assembly;

                    result = true;
                }
                catch
                {
                    result = false;
                }

                try
                {
                    if (!result)
                    {
                        AppDomain.Unload(appDomain);

                        if (AssemblyAppDomains.ContainsKey(key))
                        {
                            AssemblyAppDomains.Remove(key);
                        }

                        if (Assemblies.ContainsKey(key))
                        {
                            Assemblies.Remove(key);
                        }
                    }
                }
                catch
                {

                }
            }
            else
            {
                try
                {
                    var assembly = Assembly.LoadFile(filePath);

                    AssemblyAppDomains[key] = AppDomain.CurrentDomain;
                    Assemblies[key] = assembly;

                    result = true;
                }
                catch
                {
                    result = false;
                }
            }

            return result;
        }

        public static bool UnloadAssemblyFile(string filePath)
        {
            var result = false;

            if (IsAppDomainCreationEnabled)
            {
                try
                {
                    var key = GetFileUniqueName(filePath);

                    if (AssemblyAppDomains.ContainsKey(key))
                    {
                        var appDomain = AssemblyAppDomains[key];

                        AppDomain.Unload(appDomain);

                        AssemblyAppDomains.Remove(key);
                        Assemblies.Remove(key);

                        result = true;
                    }
                }
                catch
                {

                }
            }

            return result;
        }

        public static Assembly GetAssembly(string fileName)
        {
            Assembly result = null;

            try
            {
                var filePath = Path.Combine(CompiledAlarmDataFolder, fileName);

                var key = GetFileUniqueName(filePath);

                if (Assemblies.ContainsKey(key))
                {
                    result = Assemblies[key];
                }
            }
            catch
            {
                result = null;
            }

            return result;
        }
    }
}
