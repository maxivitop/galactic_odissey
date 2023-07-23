using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

public class CapacityArrayTest
{
    [Test]
    public void CapacityArrayStoresLastInserted()
    {
        var array = new CapacityArray<int>(3);
        array.Add(1);
        array.Add(2);
        array.Add(3);
        array.Add(4);
        
        Assert.AreEqual(2, array[1]);
        Assert.AreEqual(3, array[2]);
        Assert.AreEqual(4, array[3]);
        Assert.AreEqual(4, array.size);
    }
    
    [Test]
    public void CapacityArrayResetWorks()
    {
        var array = new CapacityArray<int>(3);
        array.Add(1);
        array.Add(2);
        array.Add(3);
        array.Add(4);
        array.size = 2;
        
        Assert.AreEqual(2, array[1]);
        Assert.AreEqual(2, array.size);
    }
    
    [Test]
    public void CapacityArrayMoveStartWorks()
    {
        var array = new CapacityArray<int>(3);
        array.Add(1);
        array.Add(2);
        array.Add(3);
        array.Add(4);
        
        array.MoveStart(2);
        
        Assert.AreEqual(3, array[0]);
        Assert.AreEqual(4, array[1]);
        Assert.AreEqual(2, array.size);
        
        array.Normalize();
        Assert.AreEqual(3, array.array[0]);
        Assert.AreEqual(4, array.array[1]);
    } 
    
    [Test]
    public void CapacityArrayMoveStartAndAddWorks()
    {
        var array = new CapacityArray<int>(3);
        array.Add(1);
        array.Add(2);
        array.Add(3);
        
        array.MoveStart(1);

        array[0] = -1;
        array.Add(4);
        
        
        Assert.AreEqual(-1, array[0]);
        Assert.AreEqual(3, array[1]);
        Assert.AreEqual(4, array[2]);
        Assert.AreEqual(3, array.size);
        
        
        array.Normalize();
        Assert.AreEqual(-1, array.array[0]);
        Assert.AreEqual(3, array.array[1]);
        Assert.AreEqual(4, array.array[2]);
    }
}
