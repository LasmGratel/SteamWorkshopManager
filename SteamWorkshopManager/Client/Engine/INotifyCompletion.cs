namespace SteamWorkshopManager.Client.Engine;

public interface INotifyCompletion
{
    bool IsCompleted { get; set; }
}