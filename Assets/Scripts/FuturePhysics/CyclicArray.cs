using Unity.VisualScripting;

public class CyclicArray<T>
{
    private T[] array;
    private int end;
    private int mod;
    public int Count => end;
    public CyclicArray(int capacity, int size = 0)
    {
        array = new T[capacity];
        end = size;
        mod = capacity;
    }

    public T this[int i]
    {
        get => array[i % mod];
        set => array[i % mod] = value;
    }

    public void Add(T element)
    {
        array[end%mod] = element;
        end++;
    }

    public void ReduceSizeTo(int newSize)
    {
        end = newSize;
    }
}