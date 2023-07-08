using System.Collections.Generic;

public class Event<T>
{
    public delegate void Listener(T value);

    private List<Listener> listeners = new();

    public void AddListener(Listener listener)
    {
        listeners.Add(listener);
    }

    public void Invoke(T value)
    {
        foreach (var listener in listeners)
        {
            listener(value);
        }
    }

}
