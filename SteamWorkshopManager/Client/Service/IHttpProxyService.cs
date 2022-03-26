using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.Network;

namespace SteamWorkshopManager.Client.Service;

public interface IHttpProxyService
{
    /// <summary>
    /// 证书名称，硬编码不可改动，确保兼容性
    /// </summary>
    const string CertificateName = "SteamWorkshopManager";
    const string RootCertificateName = $"{CertificateName} Certificate";
    const string RootCertificateIssuerName = $"{CertificateName} Certificate Authority";
    const string LocalDomain = "local.steampp.net";
    protected const string TAG = "HttpProxyS";

    static IHttpProxyService Instance => App.Instance.AppHost.Services.GetRequiredService<IHttpProxyService>();
    
    

    IReadOnlyCollection<AccelerateProjectDTO>? ProxyDomains { get; set; }
    

    bool IsOnlyWorkSteamBrowser { get; set; }

    CertificateEngine CertificateEngine { get; set; }

    int ProxyPort { get; set; }

    IPAddress ProxyIp { get; set; }

    bool IsSystemProxy { get; set; }
    

    bool OnlyEnableProxyScript { get; set; }

    bool Socks5ProxyEnable { get; set; }

    int Socks5ProxyPortId { get; set; }

    bool TwoLevelAgentEnable { get; set; }

    ExternalProxyType TwoLevelAgentProxyType { get; set; }


    const ExternalProxyType DefaultTwoLevelAgentProxyType = ExternalProxyType.Socks5;

    string? TwoLevelAgentIp { get; set; }

    int TwoLevelAgentPortId { get; set; }

    string? TwoLevelAgentUserName { get; set; }

    string? TwoLevelAgentPassword { get; set; }

    IPAddress? ProxyDNS { get; set; }

    bool ProxyRunning { get; }

    bool SetupCertificate();

    bool DeleteCertificate();

    bool PortInUse(int port);

    Task<bool> StartProxy();

    void StopProxy();

    bool WirtePemCertificateToGoGSteamPlugins();

    bool IsCertificateInstalled(X509Certificate2? certificate2);

    const string PfxFileName = $"{CertificateName}.Certificate.pfx";

    const string CerFileName = $"{CertificateName}.Certificate.cer";

    static string CerExportFileName
    {
        get
        {
            var now = DateTime.Now;
            const string f = $"SteamWorkshopManager  Certificate {{0}}.cer";
            return string.Format(f, now.ToString("yyyy-MM-dd HHmmssfffffff"));
        }
    }

    string PfxFilePath => AppContext.ResolveLocalFilePath(PfxFileName);

    string CerFilePath => AppContext.ResolveLocalFilePath(CerFileName);

    /// <summary>
    /// 获取 Cer 证书路径，当不存在时生成文件后返回路径
    /// </summary>
    /// <returns></returns>
    string? GetCerFilePathGeneratedWhenNoFileExists();

    bool IsCurrentCertificateInstalled { get; }
}