using System.Collections.Generic;
using NUnit.Framework;
using LPE;
using System.Linq;

public class FreeLinkedListTests {
    FreeLinkedList<int> list = new FreeLinkedList<int>();
    LinkedList<int> data = new LinkedList<int>();

    [Test, Order(0)]
    public void FreeLinkedList_Init() {
        list = new FreeLinkedList<int>();
        data = new LinkedList<int>();
    }


    [Test, Order(1)]
    public void FreeLinkedList_AddFirst() {
        for (int i = 0; i < 10; i++) {
            list.AddFirst(i);
            data.AddFirst(i);
        }

        var z = (from x in Enumerable.Zip<int, int, (int, int)>(list, data, (a, b) => (a, b)) select x);
        foreach (var (a, b) in z) {
            Assert.AreEqual(a, b);
        }
    }
    [Test, Order(2)]
    public void FreeLinkedList_AddLast() {
        for (int i = 0; i < 10; i++) {
            list.AddLast(i);
            data.AddLast(i);
        }

        var z = (from x in Enumerable.Zip<int, int, (int, int)>(list, data, (a, b) => (a, b)) select x);
        foreach (var (a, b) in z) {
            Assert.AreEqual(a, b);
        }
    }



    void UninitializedCheck() {
        Assert.IsNotNull(data, "Cannot run test independently");
    }
}