using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace SteamWorkshopManager;

public partial class AppSettings : ObservableObject
{
    [ObservableProperty]
    private string? _cookie;

    [ObservableProperty]
    private string? _userLink;

    public AppSettings(ApplicationDataContainer sessionContainer)
    {
        _cookie = (string?)sessionContainer.Values["Cookie"];
        _userLink = (string?)sessionContainer.Values["UserLink"];
        PropertyChanged += (sender, args) =>
        {
            var name = args.PropertyName!;
            var info = typeof(AppSettings).GetProperty(name)!;
            sessionContainer.Values[name] = info.GetValue(sender);
        };
    }
}