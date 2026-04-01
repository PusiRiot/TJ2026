using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public abstract class Subject<TEvent> : MonoBehaviour
{
    // Avoid duplicated observers 
    protected readonly HashSet<IObserver<TEvent>> _observers = new HashSet<IObserver<TEvent>>();

    protected void AddObserversOnScene()
    {
        List<IObserver<TEvent>> observers = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(mb =>
                mb.GetType()
                  .GetInterfaces()
                  .Any(i =>
                      i.IsGenericType &&
                      i.GetGenericTypeDefinition() == typeof(IObserver<>) &&
                      i.GetGenericArguments()[0] == typeof(TEvent)))
            .Select(mb => (IObserver<TEvent>)mb)
            .ToList();

        foreach (var observer in observers)
            _observers.Add(observer);
    }

    public void AddObserver(IObserver<TEvent> observer)
    {
        if (observer != null) _observers.Add(observer);
    }
    public void RemoveObserver(IObserver<TEvent> observer)
    {
        if (observer != null) _observers.Remove(observer);
    }
    protected void Clear()
    {
        _observers.Clear();
    }
    protected void Notify(TEvent evt, object data = null)
    {
        // Create a snapshot to be safe if observers change during notification.
        var snapshot = _observers.Count > 0 ? new IObserver<TEvent>[_observers.Count] : null;

        if (snapshot == null) 
            return;

        _observers.CopyTo(snapshot);

        for (int i = 0; i < snapshot.Length; i++)
        {
            var obs = snapshot[i];

            obs?.OnNotify(evt, data);
        }
    }
}
