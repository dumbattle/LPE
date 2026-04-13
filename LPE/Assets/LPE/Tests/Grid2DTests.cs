using LPE.SpacePartition;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Grid2DTests {
    struct Item {
        public Vector2 position;
        public float radius;

        public Item(Vector2 position, float radius) {
            this.position = position;
            this.radius = radius;
        }
    }
    void AssertQuery(Grid2D<int> grid, List<Item> items, Vector2 queryPoint, float queryRadius) {
        HashSet<int> queryResults = new HashSet<int>();

        List<int> expected = new List<int>();


        for (int i = 0; i < items.Count; i++) {
            Item item = items[i];
            grid.Add(i, LPE.Math.Geometry.CircleAABB(item.position, item.radius));

            var distSqr = (queryPoint - item.position).sqrMagnitude;
            if (distSqr < queryRadius * queryRadius) {
                expected.Add(i);
            }
        }


        grid.QueryItems(LPE.Math.Geometry.CircleAABB(queryPoint, queryRadius), queryResults);

        // queryResults may contain other items
        // we ignore those
        foreach (var item in expected) {
            Assert.IsTrue(queryResults.Contains(item), $"Point: {items[item].position}\nRadius: {items[item].radius}\nQuery Point: {queryPoint}\nQuery Radius: {queryRadius}\n");
        }
    }


    [Test]
    public void BasicQueries() {
        const int SIZE = 10;
        const int NUM_TRIALS = 20;
        for (int t = 0; t < NUM_TRIALS; t++) {
            var g = new Grid2D<int>(new Vector2(0, 0), new Vector2(SIZE, SIZE), new Vector2Int(SIZE + 1, SIZE + 1));

            // each item is the index
            // list stores radius
            var points = new List<Item>();

            for (int i = 0; i < 100; i++) {
                points.Add(new Item(new Vector2(Random.Range(0f, SIZE + 1), Random.Range(0f, SIZE + 1)), Random.Range(0.5f, 3f)));
            }

            var targetPoint = new Vector2(Random.Range(0f, SIZE + 1), Random.Range(0f, SIZE + 1));

            AssertQuery(g, points, targetPoint, 5);
        }
    }


    [Test]
    public void RandomOperations() {
        const int SIZE = 50;
        const int MAX_ITEMS = 200;
        const int STEPS = 50000;
        const int SEED = 12345;

        Random.InitState(SEED);

        var grid = new Grid2D<int>(
            new Vector2(0, 0),
            new Vector2(SIZE, SIZE),
            new Vector2Int(SIZE + 1, SIZE + 1)
        );


        List<int> expected = new List<int>();
        HashSet<int> queryResults = new HashSet<int>();
        Dictionary<int, Item> items = new Dictionary<int, Item>();
        int nextId = 0;

        // intitial items
        for (int i = 0; i < 20; i++) {
            int id = nextId++;
            var item = RandomItem(SIZE);
            items[id] = item;
            grid.Add(i, LPE.Math.Geometry.CircleAABB(item.position, item.radius));
        }


        for (int step = 0; step < STEPS; step++) {
            int action = Random.Range(0, 4);

            switch (action) {
                // ADD
                case 0: {
                    if (items.Count >= MAX_ITEMS)
                        break;

                    int id = nextId++;
                    var item = RandomItem(SIZE);

                    items[id] = item;
                    grid.Add(id, LPE.Math.Geometry.CircleAABB(item.position, item.radius));
                    break;
                }

                // REMOVE
                case 1: {
                    if (items.Count == 0)
                        break;

                    int id = RandomKey(items);
                    items.Remove(id);
                    grid.Remove(id);
                    break;
                }

                // UPDATE
                case 2: {
                    if (items.Count == 0)
                        break;

                    int id = RandomKey(items);
                    var item = RandomItem(SIZE);
                    items[id] = item;
                    grid.UpdateItem(id, LPE.Math.Geometry.CircleAABB(item.position, item.radius));
                    break;
                }

                // QUERY
                case 3: {
                    var queryPoint = new Vector2(
                        Random.Range(0f, SIZE),
                        Random.Range(0f, SIZE)
                    );

                    float queryRadius = Random.Range(0.5f, 10f);
                    queryResults.Clear();
                    expected.Clear();

                    foreach (var kv in items) {
                        var item = kv.Value;
                        var distSqr = (queryPoint - item.position).sqrMagnitude;
                        if (distSqr < queryRadius * queryRadius) {
                            expected.Add(kv.Key);
                        }
                    }

                    grid.QueryItems(LPE.Math.Geometry.CircleAABB(queryPoint, queryRadius), queryResults);

                    // queryResults may contain other items
                    // we ignore those
                    foreach (var item in expected) {
                        Assert.IsTrue(queryResults.Contains(item), $"Point: {items[item].position}\nRadius: {items[item].radius}\nQuery Point: {queryPoint}\nQuery Radius: {queryRadius}\n");
                    }
                    break;
                }
            }
        }




        static Item RandomItem(float size) {
            return new Item(
                new Vector2(
                    Random.Range(0f, size),
                    Random.Range(0f, size)
                ),
                Random.Range(0.2f, 5f)
            );
        }
        static int RandomKey(Dictionary<int, Item> dict) {
            int index = Random.Range(0, dict.Count);
            foreach (var kv in dict) {
                if (index-- == 0) {
                    return kv.Key;
                }
            }
            throw new System.Exception("Should not happen");
        }

    }

}
