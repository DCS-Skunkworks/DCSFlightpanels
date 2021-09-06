namespace NonVisuals.Plugin
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using ClassLibraryCommon;

    using MEF;

    public class PluginManager
    {
        [ImportMany(typeof(IPanelEventHandler))]
        private IEnumerable<Lazy<IPanelEventHandler, IPanelEventHandlerMetaData>> _pluginList;

        private static PluginManager _pluginManager;
        
        private CompositionContainer _container;

        // [Import(typeof(IPanelEventHandler))]
        // public IPanelEventHandler PanelEventHandler { get; set; }

        public static bool PlugSupportActivated { get; set; }

        public static bool DisableKeyboardAPI { get; set; }

        public static bool HasPlugin()
        {
            return Get().Plugins != null && Get().Plugins.Any();
        }

        public IEnumerable<Lazy<IPanelEventHandler, IPanelEventHandlerMetaData>> Plugins => _pluginList;

        public static PluginManager Get()
        {
            if (_pluginManager == null)
            {
                _pluginManager = new PluginManager();
                _pluginManager.LoadPlugins();
            }

            return _pluginManager;
        }

        public static void DoEvent(string profile, string panelHidId, int panelId, int switchId, bool pressed, SortedList<int, IKeyPressInfo> keySequence)
        {
            if (Get().Plugins == null)
            {
                return;
            }

            foreach (Lazy<IPanelEventHandler, IPanelEventHandlerMetaData> plugin in Get().Plugins)
            {
                plugin.Value.PanelEvent(profile, panelHidId, panelId, switchId, pressed, keySequence);
            }
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
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex, "Failed to load plugin");
            }
        }


    }

}
