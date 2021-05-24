using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using LPE;
using System.Linq;

public class FreeListTests {

    FreeList<int> list;

    List<int> data;


    [Test, Order(0)]
    public void FreeList_Init() {
        list = new FreeList<int>();

        data = new List<int>() { 0, 1, 2, 3, 4 };

    }
    [Test, Order(1)]
    public void FreeList_Add() {
        var a = list.Add(data[0]);
        var b = list.Add(data[1]);
        var c = list.Add(data[2]);
        var d = list.Add(data[3]);
        var e = list.Add(data[4]);

        Assert.AreEqual(list[a], data[0]);
        Assert.AreEqual(list[b], data[1]);
        Assert.AreEqual(list[c], data[2]);
        Assert.AreEqual(list[d], data[3]);
        Assert.AreEqual(list[e], data[4]);

    }
    [Test, Order(2)]
    public void FreeList_Remove() {
        list.RemoveAt(3);
        var a = list.Add(4);
        Assert.AreEqual(3, a);
        Assert.AreEqual(list[a], 4);

        list.RemoveAt(1);
        list.RemoveAt(4);
        var b = list.Add(8);
        var c = list.Add(9);

        Assert.AreEqual(4, b);
        Assert.AreEqual(1, c);
        Assert.AreEqual(list[b], 8);
        Assert.AreEqual(list[c], 9);

    }

    void UninitializedCheck() {
        Assert.IsNotNull(data, "Cannot run test independently");
    }

}
