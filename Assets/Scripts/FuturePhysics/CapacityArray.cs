public class CapacityArray<T>
{
    public T[] array;
    public int size;
    
    public CapacityArray(int capacity, int size = 0)
    {
        array = new T[capacity];
        this.size = size;
    }
}