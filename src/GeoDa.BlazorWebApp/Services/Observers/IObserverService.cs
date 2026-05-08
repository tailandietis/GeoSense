namespace GeoDa.BlazorWebApp.Services.Observers;

public interface IObserverService
{
    bool IsRegistered(IObserverClient observer);

    void RegisterObserver(IObserverClient observer);

    void RemoveObserver(IObserverClient observer);

    void NotifyObservers(object data);
}
