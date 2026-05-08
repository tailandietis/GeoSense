namespace GeoDa.BlazorWebApp.Services.Observers;

public class ObserverService : IObserverService
{
    private Observable _observable = new Observable();

    private object _locker = new object();


    // IObserverService
    public bool IsRegistered(IObserverClient observer) =>
        _observable.IsRegistered(observer);

    public void RegisterObserver(IObserverClient observer)
    {
        lock (_locker)
        {
            _observable.RegisterObserver(observer);
        }
    }

    public void RemoveObserver(IObserverClient observer)
    {
        lock (_locker)
        {
            _observable.RemoveObserver(observer);
        }
    }

    public void NotifyObservers(object data)
    {
        lock (_locker)
        {
            _observable.NotifyObservers(data);
        }
    }
}
