using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.VisualBasic;
using SteamWorkshopManager.Util;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.Network;

namespace SteamWorkshopManager.Client.Service;

public sealed class HttpProxyService : IHttpProxyService
{

    readonly ProxyServer proxyServer = new();

    DnsAnalysisService DnsAnalysis { get; }

    public bool IsCertificate => proxyServer.CertificateManager == null || proxyServer.CertificateManager.RootCertificate == null;

    public IReadOnlyCollection<AccelerateProjectDTO>? ProxyDomains { get; set; }

    public bool IsEnableScript { get; set; }

    public bool IsOnlyWorkSteamBrowser { get; set; }

    public CertificateEngine CertificateEngine { get; set; } = CertificateEngine.BouncyCastle;

    public int ProxyPort { get; set; } = 26502;

    public IPAddress ProxyIp { get; set; } = IPAddress.Any;

    public bool IsSystemProxy { get; set; }

    public bool IsProxyGOG { get; set; }

    public bool OnlyEnableProxyScript { get; set; }

    public bool Socks5ProxyEnable { get; set; }

    public int Socks5ProxyPortId { get; set; }

    public bool TwoLevelAgentEnable { get; set; }

    public ExternalProxyType TwoLevelAgentProxyType { get; set; }
        = IHttpProxyService.DefaultTwoLevelAgentProxyType;

    public string? TwoLevelAgentIp { get; set; }

    public int TwoLevelAgentPortId { get; set; }

    public string? TwoLevelAgentUserName { get; set; }

    public string? TwoLevelAgentPassword { get; set; }

    public IPAddress? ProxyDNS { get; set; }

    public bool ProxyRunning => proxyServer.ProxyRunning;

    public static IList<HttpHeader> JsHeader => new List<HttpHeader>() { new HttpHeader("Content-Type", "text/javascript;charset=UTF-8") };

    private static bool IsIpv6Support = false;
    bool disposedValue;

    public HttpProxyService(DnsAnalysisService dnsAnalysis)
    {
        DnsAnalysis = dnsAnalysis;
        //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        //    proxyServer.CertificateManager.CertificateEngine = CertificateEngine.DefaultWindows;
        //else
        proxyServer.ExceptionFunc = exception =>
        {
            Console.Error.WriteLine(exception);
        };
        proxyServer.EnableHttp2 = false;
        proxyServer.EnableConnectionPool = true;
        proxyServer.CheckCertificateRevocation = X509RevocationMode.NoCheck;
        // 可选地设置证书引擎
        proxyServer.CertificateManager.CertificateEngine = CertificateEngine;
        //proxyServer.CertificateManager.PfxPassword = $"{CertificateName}";
        //proxyServer.ThreadPoolWorkerThread = Environment.ProcessorCount * 8;
        proxyServer.CertificateManager.PfxFilePath = ((IHttpProxyService)this).PfxFilePath;
        proxyServer.CertificateManager.RootCertificateIssuerName = IHttpProxyService.RootCertificateIssuerName;
        proxyServer.CertificateManager.RootCertificateName = IHttpProxyService.RootCertificateName;
        //mac和ios的证书信任时间不能超过300天
        proxyServer.CertificateManager.CertificateValidDays = 300;
        //proxyServer.CertificateManager.SaveFakeCertificates = true;

        proxyServer.CertificateManager.RootCertificate = proxyServer.CertificateManager.LoadRootCertificate();
    }

    private static async Task HttpRequest(SessionEventArgs e)
    {
        //IHttpService.Instance.SendAsync<object>();
        var url = HttpUtility.UrlDecode(e.HttpClient.Request.RequestUri.Query.Replace("?request=", ""));
        var cookie = e.HttpClient.Request.Headers.GetFirstHeader("cookie-steamTool")?.Value ??
            e.HttpClient.Request.Headers.GetFirstHeader("Cookie")?.Value;
        var headrs = new List<HttpHeader>() {
                new HttpHeader("Access-Control-Allow-Origin", e.HttpClient.Request.Headers.GetFirstHeader("Origin")?.Value ?? "*"),
                new HttpHeader("Access-Control-Allow-Headers", "*"), new HttpHeader("Access-Control-Allow-Methods", "*"),
                new HttpHeader("Access-Control-Allow-Credentials", "true")
            };
        //if (cookie != null)
        //    headrs.Add(new HttpHeader("Cookie", cookie));
        if (e.HttpClient.Request.ContentType != null)
            headrs.Add(new HttpHeader("Content-Type", e.HttpClient.Request.ContentType));
        switch (e.HttpClient.Request.Method.ToUpperInvariant())
        {
            case "GET":
                var body = await IHttpService.Instance.GetAsync<string>(url, cookie: cookie);
                e.Ok(body ?? "500", headrs);
                return;
            case "POST":
                try
                {
                    if (e.HttpClient.Request.ContentLength > 0)
                    {
                        var conext = await IHttpService.Instance.SendAsync<string>(url, () =>
                        {
                            using var sw = new MemoryStream().GetWriter(leaveOpen: true);
                            sw.Write(e.HttpClient.Request.BodyString);
                            var req = new HttpRequestMessage
                            {
                                Method = HttpMethod.Post,
                                Content = new StreamContent(sw.BaseStream),
                            };
                            req.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(e.HttpClient.Request.ContentType);
                            req.Content.Headers.ContentLength = e.HttpClient.Request.BodyString.Length;
                            return req;
                        }, null/*, false*/, default);
                        e.Ok(conext ?? "500", headrs);
                    }
                    else
                    {
                        e.Ok("500", headrs);
                    }
                }
                catch (Exception error)
                {
                    e.Ok(error.Message ?? "500", headrs);
                }
                return;
        }
        //e.Ok(respone, new List<HttpHeader>() { new HttpHeader("Access-Control-Allow-Origin", e.HttpClient.Request.Headers.GetFirstHeader("Origin")?.Value ?? "*") });
    }

    private async Task<IPAddress?> GetReverseProxyIp(string url, IPAddress? proxyDns, bool isDomain = false)
    {
        if (isDomain || !IPAddress.TryParse(url, out var ip))
        {
            if (url == "partner.steamgames.com") return IPAddress.Parse("184.28.13.50");
            if (proxyDns != null)
            {
                ip = (await DnsAnalysis.AnalysisDomainIpByCustomDns(url, new[] { proxyDns }, IsIpv6Support))?.First();
            }
            else
            {
                ip = (await DnsAnalysis.AnalysisDomainIp(url, IsIpv6Support))?.First();
            }
        }
        return ip;
    }

    private async Task OnRequest(object sender, SessionEventArgs e)
    {
        if (e.HttpClient.Request.Host == null) return;

        if (e.HttpClient.Request.Host.Contains(IHttpProxyService.LocalDomain, StringComparison.OrdinalIgnoreCase))
        {
            if (e.HttpClient.Request.Method.ToUpperInvariant() == "OPTIONS")
            {
                e.Ok("", new List<HttpHeader>() {
                        new HttpHeader("Access-Control-Allow-Origin", e.HttpClient.Request.Headers.GetFirstHeader("Origin")?.Value ?? "*"),
                        new HttpHeader("Access-Control-Allow-Headers", "*"),
                        new HttpHeader("Access-Control-Allow-Methods", "*"),
                        new HttpHeader("Access-Control-Allow-Credentials", "true") });
                return;
            }
            var type = e.HttpClient.Request.Headers.GetFirstHeader("requestType")?.Value;
            await HttpRequest(e);
        }

        //host模式下不启用加速会出现无限循环问题
        if (ProxyDomains is null || TwoLevelAgentEnable || (OnlyEnableProxyScript && IsSystemProxy)) return;


        //var item = ProxyDomains.FirstOrDefault(f => f.DomainNamesArray.Any(h => e.HttpClient.Request.RequestUri.AbsoluteUri.Contains(h, StringComparison.OrdinalIgnoreCase)));

        //if (item != null)
        //{

        //e.HttpClient.Request.Headers.AddHeader("User-Agent", "Steam++ Proxy/" + ThisAssembly.Version);

        foreach (var item in ProxyDomains)
        {
            foreach (var host in item.DomainNamesArray)
            {
                if (e.HttpClient.Request.RequestUri.AbsoluteUri.IsDomainPattern(host, RegexOptions.IgnoreCase))
                {
                    if (!e.HttpClient.IsHttps)
                    {
                        e.HttpClient.Request.RequestUri = new Uri(e.HttpClient.Request.RequestUri.AbsoluteUri.Remove(0, 4).Insert(0, "https"));
                        //e.Redirect(e.HttpClient.Request.RequestUri.AbsoluteUri.Remove(0, 4).Insert(0, "https"));
                        //return;
                    }

                    if (item.Redirect)
                    {
                        var url = item.ForwardDomainName.Replace("{path}", e.HttpClient.Request.RequestUri.AbsolutePath);
                        url = url.Replace("{args}", e.HttpClient.Request.RequestUri.Query);
                        //url = url.Replace("{url}", e.HttpClient.Request.RequestUri.AbsoluteUri);
                        if (url.IsHttpUrl())
                        {
                            e.HttpClient.Request.RequestUri = new Uri(e.HttpClient.Request.RequestUri.AbsoluteUri.Replace(e.HttpClient.Request.RequestUri.Scheme + "://" + e.HttpClient.Request.RequestUri.Host, url));
                            //e.Redirect(e.HttpClient.Request.RequestUri.AbsoluteUri.Replace(e.HttpClient.Request.RequestUri.Scheme + "://" + e.HttpClient.Request.RequestUri.Host, url));
                            return;
                        }
                        e.HttpClient.Request.RequestUri = new Uri(e.HttpClient.Request.RequestUri.AbsoluteUri.Replace(e.HttpClient.Request.RequestUri.Host, url));
                        //e.Redirect(e.HttpClient.Request.RequestUri.AbsoluteUri.Replace(e.HttpClient.Request.RequestUri.Host, url));
                        return;
                    }

                    if (e.HttpClient.UpStreamEndPoint == null)
                    {
                        var addres = item.ForwardDomainIsNameOrIP ? item.ForwardDomainName : item.ForwardDomainIP;
                        var ip = await GetReverseProxyIp(addres, ProxyDNS, item.ForwardDomainIsNameOrIP);
                        if (ip == null || IPAddress.IsLoopback(ip) || ip.Equals(IPAddress.Any))
                            goto exit;
                        e.HttpClient.UpStreamEndPoint = new IPEndPoint(ip, item.PortId);
                    }

                    if (!string.IsNullOrEmpty(item.UserAgent))
                    {
                        var oldua = e.HttpClient.Request.Headers.GetFirstHeader("User-Agent")?.Value;
                        e.HttpClient.Request.Headers.RemoveHeader("User-Agent");
                        var newUA = item.UserAgent.Replace("${origin}", oldua);
                        e.HttpClient.Request.Headers.AddHeader("User-Agent", newUA);
                    }

                    if (e.HttpClient.ConnectRequest?.ClientHelloInfo?.Extensions != null)
                    {
                        if (!string.IsNullOrEmpty(item.ServerName))
                        {
                            var sni = e.HttpClient.ConnectRequest.ClientHelloInfo.Extensions["server_name"];
                            e.HttpClient.ConnectRequest.ClientHelloInfo.Extensions["server_name"] =
                                new Titanium.Web.Proxy.StreamExtended.Models.SslExtension(sni.Value, sni.Name, item.ServerName, sni.Position);
                        }
                        else
                        {
                            e.HttpClient.ConnectRequest.ClientHelloInfo.Extensions.Remove("server_name");
                        }
                    }
                    return;
                }
            }
        }
    //}

    exit:
        //部分运营商将奇怪的域名解析到127.0.0.1 再此排除这些不支持的代理域名
        if (IPAddress.IsLoopback(e.ClientRemoteEndPoint.Address))
        {
            var ip = (await DnsAnalysis.AnalysisDomainIpByAliDns(e.HttpClient.Request.Host))?.First();
            if (ip == null || IPAddress.IsLoopback(ip))
            {
                e.TerminateSession();
            }
            else
            {
                e.HttpClient.UpStreamEndPoint = new IPEndPoint(ip, e.ClientRemoteEndPoint.Port);
            }
        }
        return;
    }

    private async Task OnResponse(object sender, SessionEventArgs e)
    {
    }

    // 允许重写默认的证书验证逻辑
    private static Task OnCertificateValidation(object sender, CertificateValidationEventArgs e)
    {
        // 根据证书错误，设置IsValid为真/假
        //if (e.SslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
        e.IsValid = true;
        return Task.CompletedTask;
    }

    // 允许在相互身份验证期间重写默认客户端证书选择逻辑
    private static Task OnCertificateSelection(object sender, CertificateSelectionEventArgs e)
    {
        // set e.clientCertificate to override
        return Task.CompletedTask;
    }

    string? IHttpProxyService.GetCerFilePathGeneratedWhenNoFileExists() => GetCerFilePathGeneratedWhenNoFileExists();

    /// <inheritdoc cref="IHttpProxyService.GetCerFilePathGeneratedWhenNoFileExists"/>
    public string? GetCerFilePathGeneratedWhenNoFileExists(string? filePath = null)
    {
        filePath ??= ((IHttpProxyService)this).CerFilePath;
        if (!File.Exists(filePath))
        {
            if (!GenerateCertificate(filePath)) return null;
        }
        return filePath;
    }

    public static void SaveCerCertificateFile(X509Certificate2 @this, string pathOrName)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("-----BEGIN CERTIFICATE-----");
        builder.AppendLine(
            Convert.ToBase64String(@this.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks));
        builder.AppendLine("-----END CERTIFICATE-----");
        File.WriteAllText(pathOrName, builder.ToString(), IOHelper.Utf8NoBom);
    }

    public bool GenerateCertificate(string? filePath = null)
    {
        var result = proxyServer.CertificateManager.CreateRootCertificate(true);
        if (!result || proxyServer.CertificateManager.RootCertificate == null)
        {
            //Toast.Show(AppResources.CreateCertificateFaild);
            return false;
        }

        filePath ??= ((IHttpProxyService)this).CerFilePath;

        SaveCerCertificateFile(proxyServer.CertificateManager.RootCertificate, filePath);

        return true;
    }

    public bool SetupCertificate()
    {
        // 此代理使用的本地信任根证书
        //proxyServer.CertificateManager.TrustRootCertificate(true);
        //proxyServer.CertificateManager
        //    .CreateServerCertificate($"{Assembly.GetCallingAssembly().GetName().Name} Certificate")
        //    .ContinueWith(c => proxyServer.CertificateManager.RootCertificate = c.Result);

        if (!GenerateCertificate()) return false;

        try
        {
            proxyServer.CertificateManager.TrustRootCertificate();
        }
#if DEBUG
        catch (Exception e)
        {
            //TODO e.LogAndShowT(TAG, msg: "TrustRootCertificate Error");
        }
#else
            catch { }
#endif
        try
        {
            proxyServer.CertificateManager.EnsureRootCertificate();
        }

#if DEBUG
        catch (Exception e)
        {
            //TODO e.LogAndShowT(TAG, msg: "EnsureRootCertificate Error");
        }
#else
            catch { }
#endif
        return IsCertificateInstalled(proxyServer.CertificateManager.RootCertificate);
    }
    public bool DeleteCertificate()
    {
        if (ProxyRunning)
            return false;
        if (proxyServer.CertificateManager.RootCertificate == null)
            return true;
        try
        {
            //using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
            //{
            //    store.Open(OpenFlags.MaxAllowed);
            //    var test = store.Certificates.Find(X509FindType.FindByIssuerName, CertificateName, true);
            //    foreach (var item in test)
            //    {
            //        store.Remove(item);
            //    }
            //}
            //proxyServer.CertificateManager.ClearRootCertificate();
            proxyServer.CertificateManager.RemoveTrustedRootCertificate();
            if (IsCertificateInstalled(proxyServer.CertificateManager.RootCertificate) == false)
            {
                proxyServer.CertificateManager.RootCertificate = null;
                if (File.Exists(proxyServer.CertificateManager.PfxFilePath))
                    File.Delete(proxyServer.CertificateManager.PfxFilePath);
            }
            //proxyServer.CertificateManager.RemoveTrustedRootCertificateAsAdmin();
            //proxyServer.CertificateManager.CertificateStorage.Clear();
        }
        catch (CryptographicException)
        {
            //取消删除证书
        }
        catch (Exception)
        {
            throw;
        }
        return true;
    }


    /// <summary>
    /// 获取一个随机的未使用的端口
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public static int GetRandomUnusedPort(IPAddress address)
    {
        var listener = new TcpListener(address, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    /// <summary>
    /// 检查指定的端口是否被占用
    /// </summary>
    /// <param name="address"></param>
    /// <param name="port"></param>
    /// <returns></returns>
    public static bool IsUsePort(IPAddress address, int port)
    {
        try
        {
            var listener = new TcpListener(address, port);
            listener.Start();
            listener.Stop();
            return false;
        }
        catch
        {
            return true;
        }
    }

    /// <inheritdoc cref="IsUsePort(IPAddress, int)"/>
    public static bool IsUsePort(int port)
    {
        try
        {
            return IPGlobalProperties.GetIPGlobalProperties()
                .GetActiveTcpListeners()
                .Any(x => x.Port == port);
        }
        catch
        {
            return IsUsePort(IPAddress.Loopback, port);
        }
    }

    public int GetRandomUnusedPort() => GetRandomUnusedPort(ProxyIp);

    public bool PortInUse(int port) => IsUsePort(ProxyIp, port);

    public async Task<bool> StartProxy()
    {
        var isCertificateInstalled = IsCertificateInstalled(proxyServer.CertificateManager.RootCertificate);
        if (!isCertificateInstalled)
        {
            DeleteCertificate();
            var isOk = SetupCertificate();
            if (!isOk)
            {
                return false;
            }
        }

        #region 启动代理

        proxyServer.EnableHttp2 = false;
        proxyServer.BeforeRequest += OnRequest;
        proxyServer.BeforeResponse += OnResponse;
        proxyServer.ServerCertificateValidationCallback += OnCertificateValidation;
        //proxyServer.ClientCertificateSelectionCallback += OnCertificateSelection;

        try
        {
            if (PortInUse(ProxyPort)) ProxyPort = GetRandomUnusedPort();
            var explicitProxyEndPoint = new ExplicitProxyEndPoint(ProxyIp, ProxyPort, true)
            {
                // 通过不启用为每个http的域创建证书来优化性能
                //GenericCertificate = proxyServer.CertificateManager.RootCertificate
            };
            explicitProxyEndPoint.BeforeTunnelConnectRequest += ExplicitProxyEndPoint_BeforeTunnelConnectRequest;
            
            proxyServer.AddEndPoint(explicitProxyEndPoint);

            if (Socks5ProxyEnable)
            {
                proxyServer.AddEndPoint(new SocksProxyEndPoint(ProxyIp, Socks5ProxyPortId, true));
            }

            if (TwoLevelAgentEnable && TwoLevelAgentIp != null)
            {
                proxyServer.UpStreamHttpsProxy = new ExternalProxy(TwoLevelAgentIp, TwoLevelAgentPortId)
                {
                    ProxyDnsRequests = true,
                    BypassLocalhost = true,
                    ProxyType = TwoLevelAgentProxyType,
                    UserName = TwoLevelAgentUserName,
                    Password = TwoLevelAgentPassword,
                };
                proxyServer.ForwardToUpstreamGateway = true;
            }

            IsIpv6Support = await DnsAnalysis.GetIsIpv6Support();

            proxyServer.Start();


            proxyServer.SetAsSystemHttpProxy(explicitProxyEndPoint);
            proxyServer.SetAsSystemHttpsProxy(explicitProxyEndPoint);
        }
        catch (Exception ex)
        {
            return false;
        }

        #endregion
#if DEBUG
        foreach (var endPoint in proxyServer.ProxyEndPoints)
            Debug.WriteLine("Listening on '{0}' endpoint at Ip {1} and port: {2} ",
                endPoint.GetType().Name, endPoint.IpAddress, endPoint.Port);
#endif
        return true;
    }

    private Task TransparentProxyEndPoint_BeforeSslAuthenticate(object sender, BeforeSslAuthenticateEventArgs e)
    {
        e.DecryptSsl = false;
        if (e.SniHostName.Contains(IHttpProxyService.LocalDomain, StringComparison.OrdinalIgnoreCase))
        {
            e.DecryptSsl = true;
            return Task.CompletedTask;
        }
        if (ProxyDomains is null)
        {
            return Task.CompletedTask;
        }
        foreach (var item in ProxyDomains)
        {
            foreach (var host in item.DomainNamesArray)
            {
                if (Uri.TryCreate(host, UriKind.RelativeOrAbsolute, out var u))
                {
                    string h;
                    if (u.IsAbsoluteUri)
                        h = u.Host;
                    else
                        h = u.OriginalString;

                    if (e.SniHostName.Contains(h, StringComparison.OrdinalIgnoreCase))
                    {
                        e.ForwardHttpsHostName = item.ServerName;
                        e.ForwardHttpsPort = item.PortId;
                        e.DecryptSsl = true;
                        return Task.CompletedTask;
                    }
                }
            }
        }
        //var ip = Dns.GetHostAddresses(e.SniHostName).FirstOrDefault();
        //if (IPAddress.IsLoopback(ip))
        //{
        //    e.TerminateSession();
        //    return Task.CompletedTask;
        //}
        return Task.CompletedTask;
    }

    private async Task ExplicitProxyEndPoint_BeforeTunnelConnectRequest(object sender, TunnelConnectSessionEventArgs e)
    {
        e.DecryptSsl = false;
        if (ProxyDomains is null || e.HttpClient?.Request?.Host == null)
        {
            return;
        }
        if (e.HttpClient.Request.Host.Contains(IHttpProxyService.LocalDomain, StringComparison.OrdinalIgnoreCase))
        {
            e.DecryptSsl = true;
            return;
        }
        foreach (var item in ProxyDomains)
        {
            foreach (var host in item.DomainNamesArray)
            {
                if (e.HttpClient.Request.Url.Contains(host, StringComparison.OrdinalIgnoreCase))
                {
                    e.DecryptSsl = true;
                    if (item.ProxyType == ProxyType.Local ||
                        item.ProxyType == ProxyType.ServerAccelerate)
                    {
                        var addres = item.ForwardDomainIsNameOrIP ? item.ForwardDomainName : item.ForwardDomainIP;
                        var ip = await GetReverseProxyIp(addres, ProxyDNS, item.ForwardDomainIsNameOrIP);
                        if (ip != null && !IPAddress.IsLoopback(ip) && !ip.Equals(IPAddress.Any))
                        {
                            e.HttpClient.UpStreamEndPoint = new IPEndPoint(ip, item.PortId);
                        }
                    }
                    return;
                }
            }
        }
        //var ip = Dns.GetHostAddresses(e.HttpClient.Request.Host).FirstOrDefault();
        //if (IPAddress.IsLoopback(ip))
        //{
        //    e.TerminateSession();
        //    return Task.CompletedTask;
        //}
        return;
    }

    public void StopProxy()
    {
        try
        {
            if (proxyServer.ProxyRunning)
            {
                proxyServer.BeforeRequest -= OnRequest;
                proxyServer.BeforeResponse -= OnResponse;
                proxyServer.ServerCertificateValidationCallback -= OnCertificateValidation;
                proxyServer.ClientCertificateSelectionCallback -= OnCertificateSelection;
                proxyServer.Stop();
            }
        }
        catch (Exception ex)
        {
            //Todo ex.LogAndShowT(TAG);
        }
    }
    public static string GetPublicPemCertificateString(X509Certificate2 @this)
    {
        StringBuilder builder = new();
        //builder.AppendLine(Constants.CERTIFICATE_TAG);
        builder.AppendLine("-----BEGIN CERTIFICATE-----");
        builder.AppendLine(
            Convert.ToBase64String(@this.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks));
        builder.AppendLine("-----END CERTIFICATE-----");
        //builder.AppendLine(Constants.CERTIFICATE_TAG);
        return builder.ToString();
    }

    public bool WirtePemCertificateToGoGSteamPlugins()
    {
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var gogPlugins = Path.Combine(local, "GOG.com", "Galaxy", "plugins", "installed");
        if (Directory.Exists(gogPlugins))
        {
            foreach (var dir in Directory.GetDirectories(gogPlugins))
            {
                if (dir.Contains("steam"))
                {
                    var pem = 
                        GetPublicPemCertificateString(proxyServer.CertificateManager.RootCertificate!);
                    var certifi = Path.Combine(local, dir, "certifi", "cacert.pem");
                    if (File.Exists(certifi))
                    {
                        var file = File.ReadAllText(certifi);
                        var s = file.Substring(AppContext.CERTIFICATE_TAG, AppContext.CERTIFICATE_TAG, true);
                        if (string.IsNullOrEmpty(s))
                        {
                            File.AppendAllText(certifi, Environment.NewLine + pem);
                        }
                        else if (s.Trim() != pem.Trim())
                        {
                            var index = file.IndexOf(AppContext.CERTIFICATE_TAG);
                            File.WriteAllText(certifi, file.Remove(index, s.Length) + pem);
                        }
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public bool IsCurrentCertificateInstalled
    {
        get
        {
            if (proxyServer.CertificateManager.RootCertificate == null)
                if (GetCerFilePathGeneratedWhenNoFileExists() == null) return false;
            return IsCertificateInstalled(proxyServer.CertificateManager.RootCertificate, usePlatformCheck: true);
        }
    }

    public bool IsCertificateInstalled(X509Certificate2? certificate2) => IsCertificateInstalled(certificate2, false);

    public bool IsCertificateInstalled(X509Certificate2? certificate2, bool usePlatformCheck)
    {
        if (certificate2 == null)
            return false;
        if (certificate2.NotAfter <= DateTime.Now)
            return false;
        

        bool result;
        using var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);
        result = store.Certificates.Contains(certificate2);
        return result;
    }

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: 释放托管状态(托管对象)
                if (proxyServer.ProxyRunning)
                {
                    StopProxy();
                }
                proxyServer.Dispose();
            }

            // TODO: 释放未托管的资源(未托管的对象)并重写终结器
            // TODO: 将大型字段设置为 null
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}