using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

public class CyclicArrayTest
{
    [Test]
    public void CyclicArrayStoresLastInserted()
    {
        var array = new CyclicArray<int>(3);
        array.Add(1);
        array.Add(2);
        array.Add(3);
        array.Add(4);
        
        Assert.AreEqual(2, array[1]);
        Assert.AreEqual(3, array[2]);
        Assert.AreEqual(4, array[3]);
        Assert.AreEqual(4, array.Count);
    }
    
    [Test]
    public void CyclicArrayResetWorks()
    {
        var array = new CyclicArray<int>(3);
        array.Add(1);
        array.Add(2);
        array.Add(3);
        array.Add(4);
        array.ReduceSizeTo(2);
        
        Assert.AreEqual(2, array[1]);
        Assert.AreEqual(2, array.Count);
    }
}
