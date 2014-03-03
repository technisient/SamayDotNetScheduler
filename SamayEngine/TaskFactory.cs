using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using Technisient.Properties;

namespace Technisient
{
    internal class TaskFactory : MarshalByRefObject
    {
        internal Dictionary<string, string> Initialize()
        {
              Dictionary<string, string> TaskAssemblyDict = new Dictionary<string, string>();

            //cache all Tasks assembly names in the beginning.
            DirectoryInfo dI = new DirectoryInfo(Technisient.SamayEngine.EnsureFullPath(Settings.Default.TasksDirectory));
            FileInfo[] files = dI.GetFiles("*.dll");

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            foreach (FileInfo f in files)
            {
                try
                {
                    Assembly assembly = System.Reflection.Assembly.LoadFrom(f.FullName);
                    foreach (Type classType in assembly.GetTypes())
                    {
                        if (!classType.IsClass)
                            continue;

                        if (classType.BaseType.FullName =="Technisient.TaskBase")
                        {
                            TaskAssemblyDict.Add(classType.FullName, f.FullName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    SamayLogger.LogError("Unable to Load Assembly: " + f.FullName + "\n" + ex.ToString(), SamayLogger.SamayEngineLogJobName, "Engine", SamayLogger.SamayEngineLoggingGUID, DateTime.Now);
                }

            }

            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            return TaskAssemblyDict;
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs resolveEventArgs)
        {
            try
            {
                string f = resolveEventArgs.Name.Substring(0, resolveEventArgs.Name.IndexOf(',')) + ".dll";
                Assembly assembly;

                if (File.Exists(Settings.Default.TasksDirectory + "\\" + f))
                    assembly = Assembly.LoadFrom(Settings.Default.TasksDirectory + "\\" + f);
                else
                    throw new Exception("Unable to find: " + f);

                return assembly;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        
    }
}
