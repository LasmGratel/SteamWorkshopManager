using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace SteamWorkshopManager.Client.Service;

public partial class AccelerateProjectDTO : ObservableObject
{
    /// <summary>
    /// 显示名称
    /// </summary>
    public string? Name { get; set; } = string.Empty;

    /// <summary>
    /// 端口号
    /// </summary>
    public ushort PortId { get; set; }

    /// <summary>
    /// 域名，分号分割多个
    /// </summary>
    public string? DomainNames { get; set; } = string.Empty;

    string? mDomainNames;
    string[]? mDomainNamesArray;
    readonly object mDomainNamesArrayLock = new();

    /// <inheritdoc cref="DomainNames"/>
    public string[] DomainNamesArray
        => HttpProxyConstants.GetSplitValues(mDomainNamesArrayLock, DomainNames, ref mDomainNames, ref mDomainNamesArray);

    /// <summary>
    /// 转发域名
    /// </summary>
    public string? ForwardDomainName { get; set; } = string.Empty;

    /// <summary>
    /// 转发域名IP
    /// </summary>
    public string? ForwardDomainIP { get; set; } = string.Empty;

    /// <summary>
    /// 转发是域名(<see langword="true"/>)还是域名IP(<see langword="false"/>)
    /// </summary>
    public bool ForwardDomainIsNameOrIP => string.IsNullOrEmpty(ForwardDomainIP);

    /// <summary>
    /// 服务器名
    /// </summary>
    public string? ServerName { get; set; } = string.Empty;

    /// <summary>
    /// 代理类型
    /// </summary>
    public ProxyType ProxyType { get; set; }

    /// <summary>
    /// 启用重定向
    /// </summary>
    public bool Redirect
    {
        get => ProxyType == ProxyType.Redirect;
        set => ProxyType = value ? ProxyType.Redirect : default;
    }

    /// <summary>
    /// Host 域名集合
    /// </summary>
    public string? Hosts { get; set; } = string.Empty;

    /// <summary>
    /// 是否默认启用
    /// </summary>
    [ObservableProperty]
    private bool _enabled;

    string? mHosts;
    string[]? mHostsArray;
    readonly object mHostsArrayLock = new();

    /// <inheritdoc cref="Hosts"/>
    public string[] HostsArray
        => HttpProxyConstants.GetSplitValues(mHostsArrayLock, Hosts, ref mHosts, ref mHostsArray);

    public Guid Id { get; set; }

    public int Order { get; set; }

    public string? UserAgent { get; set; }
}