using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NonVisuals.Plugin
{
    public static class PluginManager
    {

        static void foo()
        {
            string[] pluginPaths = new string[]
            {
                // Paths to plugins to load.
            };
            IEnumerable<IPanelEventPlugin> panelEventPlugins = pluginPaths.SelectMany(pluginPath =>
            {
                Assembly pluginAssembly = LoadPlugin(pluginPath);
                return LoadPlugin(pluginAssembly);
            }).ToList();
        }

        static Assembly LoadPlugin(string relativePath)
        {
            throw new NotImplementedException();
        }

        static IEnumerable<IPanelEventPlugin> LoadPlugin(Assembly assembly)
        {
            var count = 0;

            foreach (var type in assembly.GetTypes())
            {
                if (typeof(IPanelEventPlugin).IsAssignableFrom(type))
                {
                    if (!(Activator.CreateInstance(type) is IPanelEventPlugin result))
                    {
                        continue;
                    }
                    count++;
                    yield return result;
                }
            }

            if (count == 0)
            {
                var availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
                throw new ApplicationException(
                    $"Can't find a plugin type which implements IPanelEventPlugin in {assembly} from {assembly.Location}.\n" +
                    $"Available types: {availableTypes}");
            }
        }
    }
}
