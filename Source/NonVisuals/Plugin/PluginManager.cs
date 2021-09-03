namespace NonVisuals.Plugin
{
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics;
    using System.Reflection;
    using ClassLibraryCommon;

    using MEF;

    public class PluginManager
    {
        private static PluginManager _pluginManager;
        
        private CompositionContainer _container;

        [Import(typeof(IPanelEventHandler))]
        public IPanelEventHandler PanelEventHandler { get; set; }

        public static bool PlugSupportActivated { get; set; }

        public static bool HasPlugin()
        {
            return Get().PanelEventHandler != null;
        }

        public static PluginManager Get()
        {
            if (_pluginManager == null)
            {
                _pluginManager = new PluginManager();
                _pluginManager.LoadPlugins();
            }

            return _pluginManager;
        }

        private void LoadPlugins()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();

                // An aggregate catalog that combines multiple catalogs.
                var catalog = new AggregateCatalog();

                // Adds all the parts found in the same assembly as the Program class.
                // catalog.Catalogs.Add(new AssemblyCatalog(assembly));
                Debug.WriteLine(AppDomain.CurrentDomain.BaseDirectory + "Extensions");
                catalog.Catalogs.Add(new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory + "Extensions"));
                // catalog.Catalogs.Add(new DirectoryCatalog(@"C:\dev\repos\DCSFlightpanels\Source\SamplePanelEventHandlerPlugin\bin\Debug"));

                // Create the CompositionContainer with the parts in the catalog.
                _container = new CompositionContainer(catalog);
                _container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                Common.ShowErrorMessageBox(compositionException, "Failed to load plugin");
            }
        }


    }

}
