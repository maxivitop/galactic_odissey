public class CapacityArray<T>
{
    public T[] array;
    public int size;
    
    public CapacityArray(int capacity, int size = 0)
    {
        array = new T[capacity];
        this.size = size;
    }

    public void CopyIntoSelf(CapacityArray<T> value)
    {
        for (var i = 0; i < value.size; i++)
        {
            array[i] = value.array[i];
        }
        size = value.size;
    }

    public T GetOrElse(int index, T value)
    {
        if (index < 0 || index > size) return value;
        return array[index];
    }
}