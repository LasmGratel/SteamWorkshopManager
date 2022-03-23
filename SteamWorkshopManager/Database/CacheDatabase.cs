using System.Collections.Generic;
using System.Threading.Tasks;
using LiteDB;
using SteamWorkshopManager.Core;

namespace SteamWorkshopManager.Database
{
    public class CacheDatabase
    {
        private readonly ILiteDatabase _db;
        private readonly SteamWorkshopClient _client;

        private readonly ILiteCollection<Workshop> _appsCollection;

        public CacheDatabase(ILiteDatabase db, SteamWorkshopClient client)
        {
            _db = db;
            _client = client;

            _appsCollection = _db.GetCollection<Workshop>("Apps");
        }

        public async Task RefreshApps()
        {
            _appsCollection.DeleteAll();
            _appsCollection.InsertBulk(await _client.GetAllWorkshops());
        }

        public IEnumerable<Workshop> GetApps()
        {
            return _appsCollection.FindAll();
        }
    }
}
