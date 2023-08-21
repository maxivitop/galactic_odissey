using System.Collections.Generic;

public class SingleEvent<T>
{
    public delegate void Listener(T value);

    private readonly List<Listener> listeners = new();
    private readonly List<Listener> pendingAdds = new();
    private readonly List<Listener> pendingRemoves = new();
    private volatile bool isIterating;

    public void AddListener(Listener listener)
    {
        if (isIterating)
        {
           pendingAdds.Add(listener); 
        }
        else
        {
            listeners.Add(listener);
        }
    }
    
    public void RemoveListener(Listener listener)
    {
        if (isIterating)
        {
            pendingRemoves.Add(listener); 
        }
        else
        {
            listeners.Remove(listener);
        }    
    }

    public void Invoke(T value)
    {
        isIterating = true;
        foreach (var listener in listeners)
        {
            listener(value);
        }
        listeners.AddRange(pendingAdds);
        pendingAdds.Clear();
        foreach (var pendingRemove in pendingRemoves)
        {
            listeners.Remove(pendingRemove);
        }
        pendingRemoves.Clear();
        isIterating = false;
    }
}
