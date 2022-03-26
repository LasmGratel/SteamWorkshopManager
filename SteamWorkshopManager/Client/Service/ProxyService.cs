using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace SteamWorkshopManager.Client.Service;

public partial class ProxyService : ObservableObject, IDisposable
{
    private static ProxyService? _current;

    public static ProxyService Current => _current ??= new ProxyService();

    readonly IHttpProxyService httpProxyService = IHttpProxyService.Instance;

    public void Dispose()
    {
    }
}