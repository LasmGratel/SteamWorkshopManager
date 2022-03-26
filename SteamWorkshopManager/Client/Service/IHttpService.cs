using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SteamWorkshopManager.Client.Service;

public interface IHttpService
{
    static IHttpService Instance => App.Instance.AppHost.Services.GetRequiredService<IHttpService>();

    Task<T?> SendAsync<T>(
        string? requestUri,
        Func<HttpRequestMessage> requestFactory,
        string? accept,
        //bool enableForward,
        CancellationToken cancellationToken,
        Action<HttpResponseMessage>? handlerResponse = null,
        Action<HttpResponseMessage>? handlerResponseByIsNotSuccessStatusCode = null,
        string? clientName = null) where T : notnull;

    /// <summary>
    /// 通过 Get 请求 API 内容
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="requestUri"></param>
    /// <param name="accept"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<T?> GetAsync<T>(string requestUri,
        string accept = "application/json",
        CancellationToken cancellationToken = default, string? cookie = null) where T : notnull;
}