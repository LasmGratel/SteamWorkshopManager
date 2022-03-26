using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SteamWorkshopManager.Util;

namespace SteamWorkshopManager.Client.Service;

public sealed class HttpService : IHttpService
{

    /// <summary>
    /// 用于 <see cref="IHttpClientFactory.CreateClient(string)"/> 中传递的 name
    /// <para>如果为 <see langword="null"/> 则调用 <see cref="HttpClientFactoryExtensions.CreateClient(IHttpClientFactory)"/></para>
    /// <para>默认值为 <see langword="null"/></para>
    /// </summary>
    protected string? DefaultClientName { get; }

    protected HttpClient CreateClient(string? clientName = null)
    {
        var client = CreateClientCore(clientName);
        client.Timeout = TimeSpan.FromSeconds(30);
        return client;
    }

    HttpClient CreateClientCore(string? clientName)
    {
        clientName ??= DefaultClientName;
#if DEBUG
        //logger.LogDebug("CreateClient, clientName: {0}", clientName);
#endif
        if (clientName == null)
        {
            return CreateClient();
        }
        else
        {
            return CreateClient(clientName);
        }
    }

    async Task<T?> SendAsync<T>(
        bool isCheckHttpUrl,
        string? requestUri,
        Func<HttpRequestMessage> requestFactory,
        string? accept,
        //bool enableForward,
        CancellationToken cancellationToken,
        Action<HttpResponseMessage>? handlerResponse = null,
        Action<HttpResponseMessage>? handlerResponseByIsNotSuccessStatusCode = null,
        string? clientName = null) where T : notnull
    {
        HttpRequestMessage? request = null;
        bool requestIsSend = false;
        HttpResponseMessage? response = null;
        bool notDisposeResponse = false;
        try
        {
            request = requestFactory();
            requestUri ??= request.RequestUri.ToString();

            if (!isCheckHttpUrl && !requestUri.IsHttpUrl()) return default;

            //if (enableForward && IsAllowUrl(requestUri))
            //{
            //    try
            //    {
            //        requestIsSend = true;
            //        response = await Csc.Forward(request,
            //            HttpCompletionOption.ResponseHeadersRead,
            //            cancellationToken);
            //    }
            //    catch (Exception e)
            //    {
            //        logger.LogWarning(e, "CloudService Forward Fail, requestUri: {0}", requestUri);
            //        response = null;
            //    }
            //}

            if (response == null)
            {
                var client = CreateClient(clientName);
                if (requestIsSend)
                {
                    request.Dispose();
                    request = requestFactory();
                }

                response = await client.SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken).ConfigureAwait(false);
            }

            handlerResponse?.Invoke(response);

            if (response.IsSuccessStatusCode)
            {
                if (response.Content != null)
                {
                    var rspContentClrType = typeof(T);
                    if (rspContentClrType == typeof(string))
                    {
                        return (T)(object)await response.Content.ReadAsStringAsync();
                    }
                    else if (rspContentClrType == typeof(byte[]))
                    {
                        return (T)(object)await response.Content.ReadAsByteArrayAsync();
                    }
                    else if (rspContentClrType == typeof(Stream))
                    {
                        notDisposeResponse = true;
                        return (T)(object)await response.Content.ReadAsStreamAsync();
                    }
                }
            }
            else
            {
                handlerResponseByIsNotSuccessStatusCode?.Invoke(response);
            }
        }
        catch (Exception e)
        {
        }
        finally
        {
            request?.Dispose();
            if (!notDisposeResponse)
            {
                response?.Dispose();
            }
        }

        return default;
    }

    public Task<T?> SendAsync<T>(
        string? requestUri,
        Func<HttpRequestMessage> requestFactory,
        string? accept,
        //bool enableForward,
        CancellationToken cancellationToken,
        Action<HttpResponseMessage>? handlerResponse = null,
        Action<HttpResponseMessage>? handlerResponseByIsNotSuccessStatusCode = null,
        string? clientName = null) where T : notnull
    {
        return SendAsync<T>(
            isCheckHttpUrl: false,
            requestUri,
            requestFactory,
            accept,
            //enableForward,
            cancellationToken,
            handlerResponse,
            handlerResponseByIsNotSuccessStatusCode,
            clientName);
    }

    public Task<T?> GetAsync<T>(
        string requestUri,
        string accept,
        CancellationToken cancellationToken,
        string? cookie = null) where T : notnull
    {
        if (!requestUri.IsHttpUrl()) return Task.FromResult(default(T?));
        return SendAsync<T>(isCheckHttpUrl: true, requestUri, () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            if (cookie != null)
            {
                request.Headers.Add("Cookie", cookie);
            }

            request.Headers.Accept.ParseAdd(accept);
            //request.Headers.UserAgent.ParseAdd(http_helper.UserAgent);
            return request;
        }, accept /*, true*/, cancellationToken);
    }

}