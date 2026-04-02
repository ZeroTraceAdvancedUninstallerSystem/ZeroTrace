// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Windows;
using ZeroTrace.Core.Discovery;
using ZeroTrace.Core.Logging;
using ZeroTrace.ViewModels;

namespace ZeroTrace.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var logger = new FileLogger();
        var registryProvider = new RegistryDiscoveryProvider(logger);
        var providers = new List<IDiscoveryProvider> { registryProvider };
        var discoverySvc = new DiscoveryService(logger, providers);

        DataContext = new MainViewModel(discoverySvc, logger);
    }
}
