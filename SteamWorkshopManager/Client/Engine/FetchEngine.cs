using System;
using System.Collections.Generic;
using System.Threading;
using SteamWorkshopManager.Core;

namespace SteamWorkshopManager.Client.Engine;

public abstract class FetchEngine<T> : IAsyncEnumerable<T>
{
    /// <summary>
    ///     How many pages have been fetched
    /// </summary>
    public int RequestedPages { get; set; }

    public EngineHandle EngineHandle { get; }

    public SteamWorkshopClient Client { get; }

    public FetchEngine(SteamWorkshopClient client, EngineHandle? engineHandle = null)
    {
        Client = client;
        EngineHandle = engineHandle ?? new EngineHandle(Guid.NewGuid());
    }


    public abstract IAsyncEnumerator<T> GetAsyncEnumerator(
        CancellationToken cancellationToken = new CancellationToken());

    public void Cancel()
    {
        EngineHandle.Cancel();
    }
}