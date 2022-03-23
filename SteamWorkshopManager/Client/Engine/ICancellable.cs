namespace SteamWorkshopManager.Client.Engine;

public interface ICancellable
{
    bool IsCancelled { get; set; }
}