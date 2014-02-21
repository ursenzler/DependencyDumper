// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Appccelerate">
//   Copyright (c) 2008-2013
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DependecyDumper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    public class Program
    {
        private static HashSet<Type> types;
        private static HashSet<Tuple<Type, Type>> dependencies;
        private static HashSet<Type> allTypes;

        public static void Main(string[] args)
        {
            if (args.Count() != 6)
            {
                Console.WriteLine("usage DependencyDumper assemblyFolder assemblyName typePattern minDepth roots absoluteOutputPath");
                Console.WriteLine("you provided: " + Environment.CommandLine);
                Console.ReadLine();
                return;
            }
            
            string assemblyFolder = args[0];
            string assemblyName = args[1];
            Regex typePattern = new Regex(args[2]);
            int minDepth = int.Parse(args[3]);
            string[] roots = args[4].Split(',');
            string outputPath = args[5];

            RegisterAssemblyResolveHandler(assemblyFolder);

            types = new HashSet<Type>();
            dependencies = new HashSet<Tuple<Type, Type>>();
            allTypes = new HashSet<Type>();

            Console.WriteLine("dumping dependencies of " + assemblyName + " and all its referenced assemblies.");
            Console.WriteLine("using assemblies from folder " + assemblyFolder);
            Console.WriteLine("using only types matching " + typePattern);
            Console.WriteLine("dumping only dependency chains longer or equal to " + minDepth);
            Console.WriteLine("treating the following classes as roots (even if they are used by other classes): " + roots.Aggregate((a, b) => a + ", " + b));
            Console.WriteLine("writing output to (should be a .tgf file)" + outputPath);
            Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine("loading types from assemblies...");
            Assembly assembly = Assembly.LoadFrom(Path.Combine(assemblyFolder, assemblyName));

            var allAssemblies = new List<Assembly>();
            AddAssemblies(assembly, allAssemblies);

            foreach (var a in allAssemblies)
            {
                var t = a.GetTypes();
                foreach (var type in t)
                {
                    if (typePattern.IsMatch(type.FullName))
                    {
                        if (!type.Name.Contains("__"))
                        {
                            allTypes.Add(type);
                        }
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("processing types:");
            foreach (Type type in allTypes)
            {
                Console.WriteLine("processing type " + type.FullName);

                Step(type);
            }

            // explicit roots
            Console.WriteLine();
            Console.WriteLine("processing explicit roots:");
            foreach (string root in roots)
            {
                Console.WriteLine("removing dependencies on " + root);
                var regex = new Regex(root);
                var toBreak = dependencies.Where(t => t.Item2.FullName != null && regex.IsMatch(t.Item2.FullName)).ToList();

                foreach (Tuple<Type, Type> tuple in toBreak)
                {
                    dependencies.Remove(tuple);
                }
            }

            Console.WriteLine();
            Console.WriteLine("calculating dependency depths");
            Dictionary<Type, int> levels = CalculateNodeDepth();

            Console.WriteLine();
            Console.WriteLine("removing types that are not in a deep enough dependency chain:");
            List<Type> remove = new List<Type>();
            do
            {
                remove.Clear();
                foreach (Type type in types)
                {
                    // root type and not enough depth
                    if (dependencies.All(t => t.Item2 != type) && levels[type] < minDepth)
                    {
                        remove.Add(type);   
                    }
                }

                foreach (Type type in remove)
                {
                    Console.WriteLine("removing type " + type);
                    types.Remove(type);
                    dependencies.RemoveWhere(t => t.Item1 == type);
                }
            }
            while (remove.Count > 0);

            Console.WriteLine();
            Console.WriteLine("writing output file...");
            var list = new List<Type>(types);
            using (StreamWriter writer = new StreamWriter(outputPath))
            {
                foreach (var type in list)
                {
                    var i = levels.ContainsKey(type) ? levels[type] : 0;
                    writer.WriteLine(new List<Type>(types).IndexOf(type) + " " + type.NameToString() + " " + i);
                }

                writer.WriteLine("#");

                foreach (var dependency in dependencies)
                {
                    if (list.IndexOf(dependency.Item1) >= 0 && list.IndexOf(dependency.Item2) >= 0)
                    {
                        writer.WriteLine(
                            list.IndexOf(dependency.Item1) + " " +
                            list.IndexOf(dependency.Item2));
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine(@"opening yEd (C:\Program Files (x86)\yWorks\yEd\yEd.exe)");
            Process yEd = new Process
            {
                StartInfo = 
                { 
                    FileName = @"C:\Program Files (x86)\yWorks\yEd\yEd.exe", 
                    Arguments = outputPath 
                }
            };
            yEd.Start();

            Console.WriteLine("done. Output file is at " + outputPath);
        }

        private static void RegisterAssemblyResolveHandler(string assemblyFolder)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) =>
            {
                string n = eventArgs.Name.Split(',').First();

                string path = Path.Combine(assemblyFolder, n + ".dll");

                var asm = Assembly.LoadFile(path);

                return asm;
            };
        }

        private static Dictionary<Type, int> CalculateNodeDepth()
        {
            var levels = new Dictionary<Type, int>();
            
            foreach (Type type in types)
            {
                CalculateNodeDepth(type, levels, new List<Type>());
            }

            return levels;
        }

        private static int CalculateNodeDepth(Type type, Dictionary<Type, int> levels, List<Type> currentChain)
        {
            if (levels.ContainsKey(type))
            {
                return levels[type];
            }

            if (currentChain.Contains(type))
            {
                return 0;
            }

            currentChain.Add(type);

            var d = dependencies.Where(t => t.Item1 == type);

            int current = 0;
            foreach (Tuple<Type, Type> tuple in d)
            {
                int n = CalculateNodeDepth(tuple.Item2, levels, currentChain) + 1;
                current = Math.Max(current, n);
            }

            levels.Add(type, current);

            return current;
        }

        private static void AddAssemblies(Assembly current, List<Assembly> list)
        {
            if (list.Contains(current))
            {
                return;
            }

            list.Add(current);

            foreach (var assemblyName in current.GetReferencedAssemblies())
            {
                try
                {
                    var assembly = Assembly.Load(assemblyName);

                    Console.WriteLine("loaded assembly " + current.FullName);

                    AddAssemblies(assembly, list);   
                }
                catch
                {
                    Console.WriteLine("skipping assembly because it is not found: " + assemblyName.Name);
                }
            }
        }

        private static void Step(Type current)
        {
            if (types.Contains(current) || 
                current.Name.StartsWith("<") ||
                (current.FullName != null && current.FullName.StartsWith("System") && !(current.IsGenericType && current.GetGenericTypeDefinition() == typeof(IEnumerable<>))))
            {
                return;
            }

            types.Add(current);

            if (current.IsInterface)
            {
                foreach (var type in allTypes)
                {
                    if (type.GetInterfaces().Contains(current))
                    {
                        dependencies.Add(new Tuple<Type, Type>(current, type));
                        Step(type);
                    }
                }

                if (current.IsGenericType)
                {
                    foreach (var type in allTypes)
                    {
                        if (type.GetInterfaces().Where(i => i.IsGenericType).Select(i => i.GetGenericTypeDefinition()).Contains(current.GetGenericTypeDefinition()))
                        {
                            dependencies.Add(new Tuple<Type, Type>(current, type));
                            Step(type);
                        }
                    }
                }
            }

            if (current.IsGenericType)
            {
                var genericArguments = current.GetGenericArguments();

                foreach (Type genericArgument in genericArguments)
                {
                    dependencies.Add(new Tuple<Type, Type>(current, genericArgument));
                    
                    Step(genericArgument);
                }
            }

            if (current.IsGenericParameter)
            {
                foreach (var parameter in current.GetGenericParameterConstraints())
                {
                    dependencies.Add(new Tuple<Type, Type>(current, parameter));
                    Step(parameter);

                    foreach (var type in allTypes)
                    {
                        if (type.BaseType == parameter)
                        {
                            dependencies.Add(new Tuple<Type, Type>(current, type));
                            Step(type);
                        }

                        foreach (Type @interface in type.GetInterfaces())
                        {
                            if (@interface == parameter)
                            {
                                dependencies.Add(new Tuple<Type, Type>(current, @interface));
                                Step(@interface);
                            }

                            if (@interface.IsGenericType)
                            {
                                if (@interface.GetGenericTypeDefinition() == parameter)
                                {
                                    dependencies.Add(new Tuple<Type, Type>(current, @interface));
                                    Step(@interface);
                                }
                            }
                        }
                    }
                }
            }

            if (current.Name.EndsWith("Factory"))
            {
                MethodInfo[] methods = current.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                foreach (MethodInfo method in methods)
                {
                    dependencies.Add(new Tuple<Type, Type>(current, method.ReturnType));
                    Step(method.ReturnType);
                }
            }
            
            var constructors = current.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors.Count() == 1)
            {
                var parameters = constructors.Single().GetParameters();

                foreach (var parameter in parameters)
                {
                    dependencies.Add(new Tuple<Type, Type>(current, parameter.ParameterType));

                    Step(parameter.ParameterType);
                }
            }
        }
    }
}
