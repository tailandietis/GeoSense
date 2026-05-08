using System.Collections.Generic;

namespace GeoDa.BlazorWebApp.Services.Observers;

public class Observable : IObservable
{
    private readonly List<IObserverClient> _observers;
    private readonly object _locker;

    public Observable()
    {
        _locker = new object();
        _observers = new List<IObserverClient>();
    }

    // IObservable
    public bool IsRegistered(IObserverClient observer) =>
        _observers.Contains(observer);

    public void RegisterObserver(IObserverClient observer)
    {
        lock (_locker)
        {
            _observers.Add(observer);
        }
    }

    public void RemoveObserver(IObserverClient observer)
    {
        lock (_locker)
        {
            _observers.Remove(observer);
        }
    }

    public void NotifyObservers(object data)
    {
        lock (_locker)
        {
            _observers.ForEach(v => v.Update(data));
        }
    }
}
