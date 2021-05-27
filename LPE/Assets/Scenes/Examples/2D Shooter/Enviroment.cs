using System.Collections.Generic;
using UnityEngine;
using LPE.Triangulation;
using LPE.SpacePartition;
using Unity.Profiling;


namespace Example.Shooter2D {
    public class Enviroment {
        Delaunay delaunay;
        List<DelaunayTriangle> path = new List<DelaunayTriangle>();
        Grid2D<RectangleShape> walls;
        int[,] grid;

        List<RectangleShape> queryResults = new List<RectangleShape>();

        ProfilerMarker pm1 = new ProfilerMarker("Pathfind");
        ProfilerMarker pm2 = new ProfilerMarker("AStar");
        ProfilerMarker pm3 = new ProfilerMarker("Funnel");
        public Enviroment(GameObject wallSprite, GameObject bgSprite) {
            walls = new Grid2D<RectangleShape>(
                new Vector2(-Params.Width / 2 - 1, -Params.Height / 2 - 1),
                new Vector2(Params.Width / 2 + 1, Params.Height / 2 + 1),
                new Vector2Int(Params.Width, Params.Height));
            GenerateWalls();
            //Smooth();
            //Smooth();
            //Smooth();
            BuildDelaunay();
            BuildWallObjects(wallSprite);
            bgSprite.transform.localScale = new Vector3(Params.Width, Params.Height, 1);
            wallSprite.SetActive(false);
            return;




        }


        public List<Vector2> GetFirstInPath(Vector2 start, Vector2 end, float r, List<Vector2> result) {
            pm1.Begin();

            path.Clear();

            pm2.Begin();
            delaunay.AStar(start, end, path, r);
            pm2.End();

            if (path.Count <= 1) {
                pm1.End();
                result.Add(start);
                result.Add(end);

                return result;
            }

            pm3.Begin();
            result.Clear();
            DelaunayAlgorithms.Funnel(path, start, end, result, r);
            pm3.End();
            pm1.End();
            return result;
        }

        CircleShape cir = new CircleShape(0);
        public Vector2 GetCorrection(Vector2 pos, float r) {
            Vector2 result = new Vector2(0, 0);
            cir.radius = r;
            cir.position = pos;
            queryResults.Clear();
            walls.QueryItems(LPE.Math.Geometry.CircleAABB(pos, r), queryResults);
            foreach (var w in queryResults) {
                var cv = cir.CheckCollisionWithCorrection(w);
                result += cv;

            }
            return result;

        }

        public bool IsOverlapping(Vector2 pos, float r) {
            cir.radius = r;
            cir.position = pos;
            queryResults.Clear();
            walls.QueryItems(LPE.Math.Geometry.CircleAABB(pos, r), queryResults);

            foreach (var w in queryResults) {
                if (w.CheckCollision(cir)) {
                    return true;
                }


            }
            return false;
        }
        
        public void DrawGizmos() {
            Gizmos.color = Color.green;
            delaunay.DrawGizmos();

            if (grid == null) {
                return;
            }

            int offX = Params.Width / 2;
            int offY = Params.Height / 2;
            for (int x = 0; x < grid.GetLength(0); x++) {
                for (int y = 0; y < grid.GetLength(1); y++) {
                    if (grid[x, y] > 0) {
                        Gizmos.DrawCube(new Vector2(x - offX, y - offY), Vector3.one);
                    }
                }
            }
        }


        void GenerateWalls() {
            grid = new int[Params.Width, Params.Height];
            int offX = Params.Width / 2;
            int offY = Params.Height / 2;

            for (int i = 0; i < 100; i++) {
                int ID = i + 1;
                List<(int x, int y)> l1 = new List<(int x, int y)>();
                List<(int x, int y)> l2 = new List<(int x, int y)>();
                Vector2Int s = new Vector2Int();

                bool valid = false;
                for (int j = 0; j < 5; j++) {
                    s = new Vector2Int(Random.Range(0, Params.Width), Random.Range(0, Params.Height));
                    if (ValidTile(s.x, s.y, ID)) {
                        valid = true;
                        break;
                    }
                }
                if (!valid) {
                    continue;
                }
                int count = 0;

                l1.Add((s.x, s.y));
                float size = Random.Range(100, 300);
                while (count < size && l1.Count > 0) {
                    foreach (var t in l1) {
                        if (Random.value < (count) / size) {
                            continue;
                        }
                        count++;
                        grid[t.x, t.y] = ID;

                        if (ValidTile(t.x, t.y + 1, ID)) {
                            l2.Add((t.x, t.y + 1));
                        }
                        if (ValidTile(t.x + 1, t.y, ID)) {
                            l2.Add((t.x + 1, t.y));
                        }
                        if (ValidTile(t.x - 1, t.y, ID)) {
                            l2.Add((t.x - 1, t.y));
                        }
                        if (ValidTile(t.x, t.y - 1, ID)) {
                            l2.Add((t.x, t.y - 1));
                        }
                    }

                    var l3 = l1;
                    l1 = l2;
                    l2 = l3;
                    l2.Clear();
                }
            }


            bool ValidTile(int x, int y, int id) {
                int cr = 5;
                // In center area
                if (x >= offX - cr && x <= offX + cr && y >= offY - cr && y <= offY + cr) {
                    return false;
                }

                // out of bounds
                if (x < Params.SpawnBuffer || x >= Params.Width - Params.SpawnBuffer || y < Params.SpawnBuffer || y >= Params.Height - Params.SpawnBuffer) {
                    return false;
                }

                // already wall
                if (grid[x, y] > 0) {
                    return false;
                }

                // surrounding another wall
                if (
                    (InRange(x, y + 1) && grid[x, y + 1] != 0 && grid[x, y + 1] != id) ||
                    (InRange(x + 1, y + 1) && grid[x + 1, y + 1] != 0 && grid[x + 1, y + 1] != id) ||
                    (InRange(x + 1, y) && grid[x + 1, y] != 0 && grid[x + 1, y] != id) ||
                    (InRange(x + 1, y - 1) && grid[x + 1, y - 1] != 0 && grid[x + 1, y - 1] != id) ||
                    (InRange(x, y - 1) && grid[x, y - 1] != 0 && grid[x, y - 1] != id) ||
                    (InRange(x - 1, y - 1) && grid[x - 1, y - 1] != 0 && grid[x - 1, y - 1] != id) ||
                    (InRange(x - 1, y) && grid[x - 1, y] != 0 && grid[x - 1, y] != id) ||
                    (InRange(x - 1, y + 1) && grid[x - 1, y + 1] != 0 && grid[x - 1, y + 1] != id)
                    ) {
                    return false;
                }
                return true;

                bool InRange(int x, int y) {

                    if (x < 0 || x >= Params.Width || y < 0 || y >= Params.Height) {
                        return false;
                    }
                    return true;
                }
            }
        }

        void Smooth() {
            for (int x = 1; x < grid.GetLength(0) - 1; x++) {
                for (int y = 1; y < grid.GetLength(1) - 1; y++) {
                    if (grid[x, y] > 0) {
                        int count = 0;

                        if (grid[x, y + 1] == 0) {
                            count++;
                        }
                        if (grid[x, y - 1] == 0) {
                            count++;
                        }
                        if (grid[x + 1, y] == 0) {
                            count++;
                        }
                        if (grid[x - 1, y] == 0) {
                            count++;
                        }

                        if (count >= 3) {
                            grid[x, y] = 0;
                        }
                    }
                    else {

                        int count = 0;

                        if (grid[x, y + 1] > 0) {
                            count++;
                        }
                        if (grid[x, y - 1] > 0) {
                            count++;
                        }
                        if (grid[x + 1, y] > 0) {
                            count++;
                        }
                        if (grid[x - 1, y] > 0) {
                            count++;
                        }

                        if (count >= 3) {
                            grid[x, y] = 1;
                        }
                    }
                }
            }
        }

        void BuildDelaunay() {
            List<Vector2> constraints = new List<Vector2>();
            var offset = -new Vector2(Params.Width, Params.Height) / 2;

            // horizontal
            for (int y = 1; y < grid.GetLength(1) - 1; y++) {
                bool foundFirst = false;
                int x1 = 0;
                int y1 = 0;
                bool o = false;
                for (int x = 1; x < grid.GetLength(0) - 1; x++) {
                    bool wall =
                        grid[x, y] == 0 && grid[x, y + 1] > 0 ||
                        grid[x, y] > 0 && grid[x, y + 1] == 0;
                    var o2 = grid[x, y] == 0;
                    if (wall && !foundFirst) {
                        foundFirst = true;
                        x1 = x;
                        y1 = y;
                        o = o2;
                    }
                    if (!wall && foundFirst) {
                        if (!wall) {
                            constraints.Add(new Vector2(x1, y1) + new Vector2(-.5f, .5f) + offset);
                            constraints.Add(new Vector2(x, y) + new Vector2(-.5f, .5f) + offset);
                            foundFirst = false;
                        }
                        else if (o2 != o) {
                            constraints.Add(new Vector2(x1, y1) + new Vector2(-.5f, .5f) + offset);
                            constraints.Add(new Vector2(x, y) + new Vector2(-.5f, .5f) + offset);
                            x1 = x;
                            y1 = y;
                        }
                    }
                }
            }
            // vertical
            for (int x = 1; x < grid.GetLength(0) - 1; x++) {
                bool foundFirst = false;
                int x1 = 0;
                int y1 = 0;
                bool o = false;

                for (int y = 1; y < grid.GetLength(1) - 1; y++) {
                    bool wall =
                        grid[x, y] == 0 && grid[x + 1, y] > 0 ||
                        grid[x, y] > 0 && grid[x + 1, y] == 0;

                    var o2 = grid[x, y] == 0;
                    if (wall && !foundFirst) {
                        foundFirst = true;
                        x1 = x;
                        y1 = y;
                        o = o2;
                    }
                    if (foundFirst) {
                        if (!wall) {
                            constraints.Add(new Vector2(x1, y1) + new Vector2(.5f, -.5f) + offset);
                            constraints.Add(new Vector2(x, y) + new Vector2(.5f, -.5f) + offset);
                            foundFirst = false;
                        }
                        else if (o2 != o) {
                            constraints.Add(new Vector2(x1, y1) + new Vector2(.5f, -.5f) + offset);
                            constraints.Add(new Vector2(x, y) + new Vector2(.5f, -.5f) + offset);
                            x1 = x;
                            y1 = y;
                        }
                    }
                }
            }


            //for (int x = 1; x < grid.GetLength(0) - 1; x++) {
            //    for (int y = 1; y < grid.GetLength(1) - 1; y++) {
            //        if (grid[x, y] > 0) {
            //            var pos = new Vector2(x, y) + offset;
            //            // top
            //            if (grid[x, y + 1] == 0) {
            //                constraints.Add(pos + new Vector2(-.5f, .5f));
            //                constraints.Add(pos + new Vector2(.5f, .5f));
            //            }
            //            // bottom
            //            if (grid[x, y - 1] == 0) {
            //                constraints.Add(pos + new Vector2(-.5f, -.5f));
            //                constraints.Add(pos + new Vector2(.5f, -.5f));
            //            }
            //            // right
            //            if (grid[x + 1, y] == 0) {
            //                constraints.Add(pos + new Vector2(.5f, -.5f));
            //                constraints.Add(pos + new Vector2(.5f, .5f));
            //            }
            //            // left
            //            if (grid[x - 1, y] == 0) {
            //                constraints.Add(pos + new Vector2(-.5f, -.5f));
            //                constraints.Add(pos + new Vector2(-.5f, .5f));
            //            }
            //        }
            //    }
            //}





            delaunay = new Delaunay();
            // bounds   
            delaunay.AddPoints(
                new Vector2(0, 0) + offset,
                new Vector2(Params.Width, 0) + offset,
                new Vector2(0, Params.Height) + offset,
                new Vector2(Params.Width, Params.Height) + offset);

            constraints.AddRange(new[] {
                new Vector2(0, 0) + offset,
                new Vector2(Params.Width, 0) + offset,

                new Vector2(Params.Width, 0) + offset,
                new Vector2(Params.Width, Params.Height) + offset,

                new Vector2(Params.Width, Params.Height) + offset,
                new Vector2(0, Params.Height) + offset,

                new Vector2(0, Params.Height) + offset,
                new Vector2(0, 0) + offset
            });

            delaunay.AddConstraints(constraints);
        }

        void BuildWallObjects(GameObject src) {
            GameObject obj;
            // top
            obj = GameObject.Instantiate(src);
            obj.transform.position = new Vector3(0, Params.Height / 2 + .5f, 0);
            obj.transform.localScale = new Vector3(Params.Width + 2, 1, 1);
            var r1 = new RectangleShape(obj.transform.localScale.x, obj.transform.localScale.y);
            r1.position = obj.transform.position;
            r1.UpdateShape();
            walls.Add(r1, r1.AABB());
            // bottom
            obj = GameObject.Instantiate(src);
            obj.transform.position = new Vector3(0, -Params.Height / 2 - .5f, 0);
            obj.transform.localScale = new Vector3(Params.Width + 2, 1, 1);
            r1 = new RectangleShape(obj.transform.localScale.x, obj.transform.localScale.y);
            r1.position = obj.transform.position;
            r1.UpdateShape();
            walls.Add(r1, r1.AABB());

            // right
            obj = GameObject.Instantiate(src);
            obj.transform.position = new Vector3(Params.Width / 2 + .5f, 0, 0);
            obj.transform.localScale = new Vector3(1, Params.Height + 2, 1);
            r1 = new RectangleShape(obj.transform.localScale.x, obj.transform.localScale.y);
            r1.position = obj.transform.position;
            r1.UpdateShape();
            walls.Add(r1, r1.AABB());

            // left
            obj = GameObject.Instantiate(src);
            obj.transform.position = new Vector3(-Params.Width / 2 - .5f, 0, 0);
            obj.transform.localScale = new Vector3(1, Params.Height + 2, 1);
            r1 = new RectangleShape(obj.transform.localScale.x, obj.transform.localScale.y);
            r1.position = obj.transform.position;
            r1.UpdateShape();
            walls.Add(r1, r1.AABB());



            var offset = -new Vector2(Params.Width, Params.Height) / 2;


            for (int x = 1; x < grid.GetLength(0) - 1; x++) {
                for (int y = 1; y < grid.GetLength(1) - 1; y++) {
                    if (grid[x, y] > 0) {

                        obj = GameObject.Instantiate(src);
                        obj.transform.position = new Vector2(x, y) + offset;
                        obj.transform.localScale = new Vector3(1, 1, 1);

                        r1 = new RectangleShape(obj.transform.localScale.x, obj.transform.localScale.y);
                        r1.position = obj.transform.position;
                        r1.UpdateShape();
                        walls.Add(r1, r1.AABB());
                    }
                }
            }
        }


    }
}
