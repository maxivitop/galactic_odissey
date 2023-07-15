using System.Collections.Generic;

public class SingleEvent<T>
{
    public delegate void Listener(T value);

    private readonly List<Listener> listeners = new();

    public void AddListener(Listener listener)
    {
        listeners.Add(listener);
    }
    
    public void RemoveListener(Listener listener)
    {
        listeners.Remove(listener);
    }

    public void Invoke(T value)
    {
        foreach (var listener in listeners)
        {
            listener(value);
        }
    }

}
