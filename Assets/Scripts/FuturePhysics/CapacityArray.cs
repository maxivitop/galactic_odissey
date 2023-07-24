using System;
using UnityEngine;

public class CapacityArray<T>
{
    public T[] array;
    private T[] tmpArray;
    public int start;
    public int size;
    private int capacity;
    
    public CapacityArray(int capacity, int size = 0)
    {
        array = new T[capacity];
        tmpArray = new T[capacity];
        this.size = size;
        this.capacity = capacity;
    }

    public void CopyIntoSelf(CapacityArray<T> value)
    {
        size = value.size;
        start = value.start;
        for (var i = 0; i < value.size; i++)
        {
            this[i] = value[i];
        }
    }

    public T GetOrElse(int index, T value)
    {
        if (index < 0 || index >= size) return value;
        return this[index];
    }

    public void MoveStart(int amount)
    {
        size -= amount;
        if (size < 0)
        {
            size = 0;
        }
        start += amount;
    }
    
    public T this[int i]
    {
        get => array[(start + i) % capacity];
        set => array[(start + i) % capacity] = value;
    }

    public void Add(T element)
    {
        array[(start + size)%capacity] = element;
        size++;
    }

    public void Normalize()
    {
        if (start == 0)
        {
            return;
        }
        var endSliceSize = Mathf.Min(size, capacity-start);
        Array.Copy(
            array,
            start,
            tmpArray, 
            0, 
            endSliceSize
            );
        if (endSliceSize < size)
        {
            Array.Copy(
                array,
                0,
                tmpArray,
                endSliceSize,
                size - endSliceSize
            );
        }
        (tmpArray, array) = (array, tmpArray);
        start = 0;
    }
}