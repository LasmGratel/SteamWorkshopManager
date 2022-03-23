using SteamWorkshopManager.Core;

namespace SteamWorkshopManager.Client;

public class WorkshopManagerClient
{
    public SteamWorkshopClient Client { get; set; }

    public WorkshopManagerClient(SteamWorkshopClient client)
    {
        Client = client;
    }
}