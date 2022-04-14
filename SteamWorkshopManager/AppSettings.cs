using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using SteamWorkshopManager.Client.Service;

namespace SteamWorkshopManager;

public partial class AppSettings : ObservableObject
{
    [ObservableProperty]
    private string? _cookie;

    [ObservableProperty]
    private string? _userLink;

    [ObservableProperty]
    private string? _proxy;

    public AppSettings(ApplicationDataContainer sessionContainer)
    {
        _cookie = (string?)sessionContainer.Values["Cookie"];
        _userLink = (string?)sessionContainer.Values["UserLink"];
        _proxy = (string?)sessionContainer.Values["Proxy"];
        PropertyChanged += (sender, args) =>
        {
            var name = args.PropertyName!;
            var info = typeof(AppSettings).GetProperty(name)!;
            sessionContainer.Values[name] = info.GetValue(sender);
        };
    }
}