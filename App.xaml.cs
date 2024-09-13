﻿/*
 * RyTuneX - A Windows 10 and 11 optimizer designed to enhance system performance.
 * Copyright (C) 2023 Rayen Ghanmi
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 *
 * Contact: ghanmirayen12@gmail.com
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

using RyTuneX.Activation;
using RyTuneX.Contracts.Services;
using RyTuneX.Core.Contracts.Services;
using RyTuneX.Core.Services;
using RyTuneX.Helpers;
using RyTuneX.Models;
using RyTuneX.Notifications;
using RyTuneX.Services;
using RyTuneX.ViewModels;
using RyTuneX.Views;
using Windows.Storage;

namespace RyTuneX;

public partial class App : Application
{
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar
    {
        get; set;
    }

    public App()
    {
        InitializeComponent();
        LogHelper.Log("___________ New Session ___________");
        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers
            services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

            // Services
            services.AddSingleton<IAppNotificationService, AppNotificationService>();
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            services.AddSingleton<IFileService, FileService>();

            // Views and ViewModels
            services.AddTransient<SettingsPage>();
            services.AddTransient<OptimizeSystemPage>();
            services.AddTransient<SystemInfoPage>();
            services.AddTransient<DebloatSystemPage>();
            services.AddTransient<HomePage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        //App.GetService<IAppNotificationService>().Initialize();

    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        if (ShouldShowWelcomeNotification())
        {
            await LogHelper.Log("Showing Welcome Notification");
            App.MainWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () =>
            {
                var welcomeMessage = "WelcomeNotice".GetLocalized().Replace("\\n", Environment.NewLine); ;
                App.MainWindow.ShowMessageDialogAsync(welcomeMessage, "WelcomeNoticeTitle".GetLocalized());
            });

            // Set the flag to indicate that the welcome notification has been shown
            SetWelcomeNotificationShown();
        }

        await App.GetService<IActivationService>().ActivateAsync(args);
    }

    private static bool ShouldShowWelcomeNotification()
    {
        // Check if the setting is not present or set to false
        if (!ApplicationData.Current.LocalSettings.Values.TryGetValue("WelcomeNotificationShown", out var value))
        {
            return true; // Show the notification if the setting is not present
        }

        // Check the value of the setting
        return !(bool)value;
    }

    private static void SetWelcomeNotificationShown()
    {
        // Set the flag to true to indicate that the welcome notification has been shown
        LogHelper.Log("Setting WelcomeNotificationShown");
        ApplicationData.Current.LocalSettings.Values["WelcomeNotificationShown"] = true;
    }
}
