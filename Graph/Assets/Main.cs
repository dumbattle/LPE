using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using LPE.Graph;


public class Main : MonoBehaviour {
    void Start() {
        Graph<string> harmonyGraph = new Graph<string>();
        var root = harmonyGraph.CreateNode("C");
        var fourth = harmonyGraph.CreateNode("F");
        var fifth = harmonyGraph.CreateNode("G");
        var sixth = harmonyGraph.CreateNode("a");

        harmonyGraph.ConnectNodes(root, fourth, true);
        harmonyGraph.ConnectNodes(root, fifth, true);

        harmonyGraph.ConnectNodes(fourth, root, true);
        harmonyGraph.ConnectNodes(fourth, fifth, true);

        harmonyGraph.ConnectNodes(fifth, root, true);
        harmonyGraph.ConnectNodes(fifth, sixth, true);
        harmonyGraph.ConnectNodes(sixth, fourth, true);

        foreach (var n in harmonyGraph.GetNodes()) {
            print($"{n.value}");
            foreach (var nn in n.GetAccessableNodes()) {
                print($"\t{nn.value}");
            }
        }
    }

}