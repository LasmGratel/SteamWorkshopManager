using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SteamWorkshopManager.Core;

namespace SteamWorkshopManager.Client.Engine;

public abstract class AbstractFetchEngineAsyncEnumerator<TEntity, TFetchEngine> : IAsyncEnumerator<TEntity>
    where TEntity : class?
    where TFetchEngine : FetchEngine<TEntity>
{
    protected readonly SteamWorkshopClient Client;
    protected readonly TFetchEngine FetchEngine;

    /// <summary>
    ///     The result entries of the current page
    /// </summary>
    protected IEnumerator<TEntity>? CurrentEntityEnumerator;

    protected AbstractFetchEngineAsyncEnumerator(TFetchEngine fetchEngine, SteamWorkshopClient client)
    {
        FetchEngine = fetchEngine;
        Client = client;
    }

    /// <summary>
    ///     Indicates if the current operation has been cancelled
    /// </summary>
    protected bool IsCancellationRequested => FetchEngine.EngineHandle.IsCancelled;

    /// <summary>
    ///     The current result entry of <see cref="CurrentEntityEnumerator" />
    /// </summary>
    public TEntity? Current => CurrentEntityEnumerator?.Current;

    /// <summary>
    ///     Moves the <see cref="MoveNextAsync" /> one step ahead, if fails, it will tries to
    ///     fetch a new page
    /// </summary>
    /// <returns></returns>
    public abstract ValueTask<bool> MoveNextAsync();

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return default;
    }

    public async Task<Result<string>> GetResponseAsync(string url)
    {
        try
        {
            var response = await Client.SendRequest(url).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return Result<string>.OfFailure(new HttpRequestException("HTTP " + response.StatusCode));
            return Result<string>.OfSuccess(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }
        catch (HttpRequestException e)
        {
            return Result<string>.OfFailure(e);
        }
    }
}