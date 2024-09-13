﻿using RyTuneX.Contracts.Services;
using RyTuneX.Views;

namespace RyTuneX.Services;

public class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = new();

    public PageService()
    {
        Configure<HomePage>();
        Configure<OptimizeSystemPage>();
        Configure<DebloatSystemPage>();
        Configure<PrivacyPage>();
        Configure<FeaturesPage>();
        Configure<NetworkPage>();
        Configure<SystemInfoPage>();
        Configure<SettingsPage>();
    }

    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
            }
        }

        return pageType;
    }

    private void Configure<TPage>()
    {
        lock (_pages)
        {
            var key = typeof(TPage).FullName!;
            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in PageService");
            }

            _pages.Add(key, typeof(TPage));
        }
    }
}
