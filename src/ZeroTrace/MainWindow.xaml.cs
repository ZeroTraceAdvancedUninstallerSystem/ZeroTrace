using System.Windows;
using System.Collections.Generic;
using ZeroTrace.Core.Discovery;
using ZeroTrace.Core.Logging;
using ZeroTrace.ViewModels;

namespace ZeroTrace;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // 1. Wir nutzen den IZeroTraceLogger (dein definierter Standard)
        // Falls FileLogger einen Fehler wirft, nutzen wir hier testweise einen NullLogger oder DebugLogger
        IZeroTraceLogger logger = new FileLogger(); 

        // 2. Registry-Provider initialisieren
        // HINWEIS: Falls dein Provider KEINEN Logger im Konstruktor hat, lösche das (logger) in der Klammer
        var registryProvider = new RegistryDiscoveryProvider(logger);

        var providers = new List<IDiscoveryProvider> { registryProvider };

        // 3. Den Service bauen
        var discoverySvc = new DiscoveryService(logger, providers);

        // 4. Alles ans ViewModel binden
        this.DataContext = new MainViewModel(discoverySvc);
    }
}