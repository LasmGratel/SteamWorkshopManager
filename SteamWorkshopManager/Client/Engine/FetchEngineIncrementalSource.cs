using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Common.Collections;

namespace SteamWorkshopManager.Client.Engine;

public abstract class FetchEngineIncrementalSource<T, TModel> : IIncrementalSource<TModel>
{
    private readonly ISet<long> _yieldedItems;

    private readonly IAsyncEnumerator<T> _asyncEnumerator;

    private readonly int? _limit;

    private int _yieldedCounter;

    protected abstract long Identifier(T entity);

    protected abstract TModel Select(T entity);

    protected FetchEngineIncrementalSource(IAsyncEnumerable<T> asyncEnumerator, int? limit = null)
    {
        _asyncEnumerator = asyncEnumerator.GetAsyncEnumerator();
        _limit = limit;
        _yieldedItems = new HashSet<long>();
    }

    public async Task<IEnumerable<TModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = new())
    {
        var result = new List<TModel>();
        var i = 0;
        while (i < pageSize)
        {
            if (_limit is { } l && _yieldedCounter > l)
            {
                return result;
            }
            if (await _asyncEnumerator.MoveNextAsync())
            {
                if (_asyncEnumerator.Current is { } obj && !_yieldedItems.Contains(Identifier(obj)))
                {
                    result.Add(Select(obj));
                    _yieldedItems.Add(Identifier(obj));
                    i++;
                    _yieldedCounter++;
                }
            }
            else
            {
                return Enumerable.Empty<TModel>();
            }
        }

        return result;
    }
}