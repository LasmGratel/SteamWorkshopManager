using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.Extensions.DependencyInjection;

namespace SteamWorkshopManager.Client.Service;

public class DnsAnalysisService
{
    static DnsAnalysisService Instance => App.Instance.AppHost.Services.GetRequiredService<DnsAnalysisService>();

    private static readonly LookupClient lookupClient = new();

    #region DNS常量
    protected const string PrimaryDNS_IPV6_Ali = "2400:3200::1";

    public const string PrimaryDNS_Ali = "223.5.5.5";
    public const string SecondaryDNS_Ali = "223.6.6.6";

    public const string PrimaryDNS_Dnspod = "119.29.29.29";
    public const string SecondaryDNS_Dnspod = "182.254.116.116";

    public const string PrimaryDNS_114 = "114.114.114.114";
    public const string SecondaryDNS_114 = "114.114.115.115";

    public const string PrimaryDNS_Google = "8.8.8.8";
    public const string SecondaryDNS_Google = "8.8.4.4";

    public const string PrimaryDNS_Cloudflare = "1.1.1.1";
    public const string SecondaryDNS_Cloudflare = "1.0.0.1";

    public const string PrimaryDNS_Baidu = "180.76.76.76";


    protected static readonly IPAddress[] DNS_Alis = new[] { IPAddress.Parse(PrimaryDNS_Ali), IPAddress.Parse(SecondaryDNS_Ali) };
    protected static readonly IPAddress[] DNS_Dnspods = new[] { IPAddress.Parse(PrimaryDNS_Dnspod), IPAddress.Parse(SecondaryDNS_Dnspod) };
    protected static readonly IPAddress[] DNS_114s = new[] { IPAddress.Parse(PrimaryDNS_114), IPAddress.Parse(SecondaryDNS_114) };
    protected static readonly IPAddress[] DNS_Googles = new[] { NameServer.GooglePublicDns.Address, NameServer.GooglePublicDns2.Address };
    protected static readonly IPAddress[] DNS_Cloudflares = new[] { NameServer.Cloudflare.Address, NameServer.Cloudflare2.Address };

    private static class NameServer
    {
        public static readonly IPEndPoint GooglePublicDns = new(IPAddress.Parse(SecondaryDNS_Google), 53);

        public static readonly IPEndPoint GooglePublicDns2 = new(IPAddress.Parse(PrimaryDNS_Google), 53);

        public static readonly IPEndPoint Cloudflare = new(IPAddress.Parse("1.1.1.1"), 53);

        public static readonly IPEndPoint Cloudflare2 = new(IPAddress.Parse("1.0.0.1"), 53);
    }
    #endregion

    protected const string IPV6_TESTDOMAIN = "ipv6.rmbgame.net";
    protected const string IPV6_TESTDOMAIN_SUCCESS = PrimaryDNS_IPV6_Ali;

    public async Task<long> PingHostname(string url)
    {
        var pin = new Ping();
        var r = await pin.SendPingAsync(url, 30);
        if (r.Status != IPStatus.Success)
        {
            return 0;
        }
        return r.RoundtripTime;
    }

    public Task<IPAddress[]?> AnalysisDomainIp(string url, bool isIPv6 = false)
    {
        return AnalysisDomainIpByCustomDns(url, null, isIPv6);
    }

    public Task<IPAddress[]?> AnalysisDomainIpByGoogleDns(string url, bool isIPv6 = false)
    {
        return AnalysisDomainIpByCustomDns(url, DNS_Googles, isIPv6);
    }

    public Task<IPAddress[]?> AnalysisDomainIpByCloudflare(string url, bool isIPv6 = false)
    {
        return AnalysisDomainIpByCustomDns(url, DNS_Cloudflares, isIPv6);
    }

    public Task<IPAddress[]?> AnalysisDomainIpByDnspod(string url, bool isIPv6 = false)
    {
        return AnalysisDomainIpByCustomDns(url, DNS_Dnspods, isIPv6);
    }

    public Task<IPAddress[]?> AnalysisDomainIpByAliDns(string url, bool isIPv6 = false)
    {
        return AnalysisDomainIpByCustomDns(url, DNS_Alis, isIPv6);
    }

    public Task<IPAddress[]?> AnalysisDomainIpBy114Dns(string url, bool isIPv6 = false)
    {
        return AnalysisDomainIpByCustomDns(url, DNS_114s, isIPv6);
    }

    public async Task<IPAddress[]?> AnalysisDomainIpByCustomDns(string url, IPAddress[]? dnsServers = null, bool isIPv6 = false)
    {
        if (!string.IsNullOrEmpty(url))
        {
            var client = lookupClient;
            DnsQuestion question;
            DnsQueryAndServerOptions? options = null;
            IDnsQueryResponse response;

            if (dnsServers != null)
                options = new DnsQueryAndServerOptions(dnsServers);

            if (isIPv6)
            {
                question = new DnsQuestion(url, QueryType.AAAA);

                if (options != null)
                    response = await client.QueryAsync(question, options);
                else
                    response = await client.QueryAsync(question);

                if (!response.HasError && response.Answers.AaaaRecords().Any())
                {
                    var aaaaRecord = response.Answers.AaaaRecords().Select(s => s.Address).ToArray();
                    if (aaaaRecord.Any()) return aaaaRecord;
                }
            }

            question = new DnsQuestion(url, QueryType.A);

            if (options != null)
                response = await client.QueryAsync(question, options);
            else
                response = await client.QueryAsync(question);

            if (!response.HasError && response.Answers.ARecords().Any())
            {
                var aRecord = response.Answers.ARecords().Select(s => s.Address).ToArray();
                if (aRecord.Any()) return aRecord;
            }
        }
        return null;
    }

    public async Task<string?> GetHostByIPAddress(IPAddress ip)
    {
        //var hostName = Dns.GetHostEntry(IPAddress2.Parse(ip)).HostName;
        var client = lookupClient;
        var result = await client.QueryReverseAsync(ip);
        var hostName = result.Answers.PtrRecords().FirstOrDefault()?.PtrDomainName.Value;
        return hostName;
    }

    public Task<bool> GetIsIpv6Support() => Task.FromResult(false);
}