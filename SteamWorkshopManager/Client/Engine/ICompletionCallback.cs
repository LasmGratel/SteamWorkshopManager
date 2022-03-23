namespace SteamWorkshopManager.Client.Engine;

public interface ICompletionCallback<in T>
{
    void OnCompletion(T param);
}