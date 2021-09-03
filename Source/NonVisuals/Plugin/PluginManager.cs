using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using ClassLibraryCommon;

namespace NonVisuals.Plugin
{
    public class PluginManager
    {
        private CompositionContainer _container;

        [Import(typeof(IPanelEventHandler))]
        public IPanelEventHandler PanelEventHandler;

        public void LoadPlugins()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                // An aggregate catalog that combines multiple catalogs.
                var catalog = new AggregateCatalog();
                // Adds all the parts found in the same assembly as the Program class.
                catalog.Catalogs.Add(new AssemblyCatalog(assembly));

                // Create the CompositionContainer with the parts in the catalog.
                _container = new CompositionContainer(catalog);
                _container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                Common.LogError(compositionException, "Failed to load plugins");
            }
        }
    }
}
