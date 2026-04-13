using NUnit.Framework;
using System.Diagnostics;

public class Sequence {
    [Test]
    public void Test1() {
        string output = "";
        var s = new LambdaSequence(() => output += "A")
           .Then(new LambdaSequence(() => output += "B"))
           .Then(new LambdaSequence(() => output += "C"))
           .Then(new LambdaSequence(() => output += "D"))
           .Then(new LambdaSequence(() => output += "E"))
           .Then(new LambdaSequence(() => output += "F"))
           .Then(new LambdaSequence(() => output += "G"));


        while (true) {
            s.Next();       // call next before checking done
            if (s.Done()) { // Allow at least 1 guarenteed call to Next 
                break;    // Further calls to Next() may break sequence
            }
        }
        s.OnComplete();     // Clears all data, possibly returns sequences to object pools
        s = null;           // Sequence may be reused elsewhere, cl
        Assert.IsTrue(output == "ABCDEFG");
    }
}
