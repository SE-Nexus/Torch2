using InstanceUtils.Models.Server;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace InstanceUtils.Services
{
    /// <summary>
    /// Provides functionality for resolving and managing assemblies within the application.
    /// </summary>
    /// <remarks>This service is intended to assist with dynamic assembly resolution, enabling scenarios such
    /// as loading assemblies at runtime or resolving dependencies. It extends the <see cref="ServiceBase"/> class to
    /// integrate with the application's service infrastructure.</remarks>
    public class AssemblyResolverService : ServiceBase
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private List<string> _searchDirs = new List<string>();


        public AssemblyResolverService(IConfigService configs)
        {

            AddRelativeSearchDir(configs.SteamCMDPath);
            AddRelativeSearchDir(configs.GamePath);

            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembliesFromFolders;
        }

        /// <summary>
        /// Adds a relative search directory to the collection if it is not already present. You will need to register paths before the state is initing.
        /// </summary>
        /// <remarks>If the specified directory is already in the collection, it will not be added again. 
        /// If <paramref name="dir"/> is null, empty, or whitespace, the method performs no action.</remarks>
        /// <param name="dir">The relative directory path to add. This value cannot be null, empty, or consist only of whitespace.</param>
        public void AddRelativeSearchDir(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir))
                return;

            if (!_searchDirs.Contains(dir))
                _searchDirs.Add(dir);
        }

        private Assembly ResolveAssembliesFromFolders(object sender, ResolveEventArgs args)
        {
            string assemblyName = new AssemblyName(args.Name).Name + ".dll";

            foreach (string dir in _searchDirs)
            {
                try
                {


                    string candidatePath = Directory.GetFiles(dir, assemblyName, SearchOption.AllDirectories).FirstOrDefault();

                    if (string.IsNullOrEmpty(candidatePath))
                        continue;

                    //We should put this on a debug level, but for now we want to see it
                    //_logger.Info($"Resolving assembly {assemblyName} from directory {dir}...");
                    return Assembly.LoadFrom(candidatePath);



                }
                catch (Exception ex)
                {
                    _logger.Fatal(ex, $"Failed to resolve assembly {assemblyName} from directory {dir}.");
                }
            }

            //Only log if we have a valid requesting assembly
            if (args.RequestingAssembly != null)
                _logger.Warn($"Failed to resolve assembly {assemblyName} from any configured directories. Requesting Assembly: {args.RequestingAssembly}");

            return null;
        }
    }
}
