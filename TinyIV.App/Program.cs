﻿using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using OxyPlot.Avalonia;
using TinyIV.App.ViewModels;
using TinyIV.App.Views;

namespace TinyIV.App
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args) => BuildAvaloniaApp().Start(AppMain, args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp() {
            OxyPlotModule.EnsureLoaded();
            return AppBuilder.Configure<App>()
                             .UsePlatformDetect()
                             .LogToDebug()
                             .BeforeStarting(_ => OxyPlotModule.Initialize())
                             .UseReactiveUI();
        }

        // Your application's entry point. Here you can initialize your MVVM framework, DI
        // container, etc.
        private static void AppMain(Application app, string[] args)
        {
            var window = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };

            app.Run(window);
        }
    }
}
