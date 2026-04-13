using System;
using System.Collections.Generic;
using UnityEngine;

namespace LPE.SpacePartition {
    public class ChunkedGrid2D<T> : Partition2D<T> {
        //***********************************************************************************************
        // Fields
        //***********************************************************************************************

        readonly Vector2 _chunkSize;
        readonly Vector2Int _chunkResolution; 

        readonly Dictionary<Vector2Int, Grid2D<T>> chunk2grid = new();
        readonly Dictionary<T, (float minX, float minY, float maxX, float maxY)> item2aabb = new();

        //***********************************************************************************************
        // Constructor
        //***********************************************************************************************

        /// <param name="chunkSize">World-space width and height of one chunk.</param>
        /// <param name="chunkResolution">Grid cells inside each chunk.</param>
        public ChunkedGrid2D(Vector2 chunkSize, Vector2Int chunkResolution) {
            _chunkSize = chunkSize;
            _chunkResolution = chunkResolution;
        }


        public ChunkedGrid2D(float chunkSize) {
            _chunkSize = new Vector2(chunkSize, chunkSize);
            _chunkResolution = new Vector2Int(Mathf.CeilToInt(chunkSize), Mathf.CeilToInt(chunkSize));
        }

        //***********************************************************************************************
        // Partition2D
        //***********************************************************************************************

        public override void Add(T item, Vector2 min, Vector2 max) {
            if (item2aabb.ContainsKey(item)) {
                throw new ArgumentException("Grid already contains item");
            }

            // store AABB 
            item2aabb.Add(item, (min.x, min.y, max.x, max.y));

            // add to grids
            var keys = ObjectPool.GetList<Vector2Int>();
            foreach (var key in OverlappingChunkKeys(min, max, keys)) {
                GetOrCreateChunk(key).Add(item, min, max);
            }
            ObjectPool.ReturnList(keys);
        }

        public override void Remove(T item) {
            if (!item2aabb.ContainsKey(item)) {
                throw new ArgumentException("Grid does not contain item");
            }
            // remove AABB
            var (ax, ay, bx, by) = item2aabb[item];
            item2aabb.Remove(item);


            // Remove from grids
            var keys = ObjectPool.GetList<Vector2Int>();
            foreach (var key in OverlappingChunkKeys(new Vector2(ax, ay), new Vector2(bx, by), keys)) {
                if (chunk2grid.TryGetValue(key, out var grid)) {
                    grid.Remove(item);
                }
            }
            ObjectPool.ReturnList(keys);
        }

        public override void UpdateItem(T item, Vector2 min, Vector2 max) {
            if (!item2aabb.ContainsKey(item)) {
                throw new ArgumentException("Grid does not contain item");
            }
            var (oax, oay, obx, oby) = item2aabb[item];

            var nax = min.x;
            var nay = min.y;
            var nbx = max.x;
            var nby = max.y;

            // no change to cells
            bool sameCells =
                WorldToChunkX(oax) == WorldToChunkX(nax) &&
                WorldToChunkY(oay) == WorldToChunkY(nay) &&
                WorldToChunkX(obx) == WorldToChunkX(nbx) &&
                WorldToChunkY(oby) == WorldToChunkY(nby);

            if (sameCells) {
                item2aabb[item] = (nax, nay, nbx, nby);

                // item may still change in internal grids
                var keys = ObjectPool.GetList<Vector2Int>();
                foreach (var key in OverlappingChunkKeys(new Vector2(nax, nay), new Vector2(nbx, nby), keys)) {
                    if (chunk2grid.TryGetValue(key, out var grid)) {
                        grid.UpdateItem(item, min, max);
                    }
                }
                ObjectPool.ReturnList(keys);

                return;
            }

            // TODO - Could compute full difference, but items realistically only occupy 1 chunk at a time, 4 at most if at corners
            // Difference calculation would not be worth the gains

            // Update
            Remove(item);
            Add(item, min, max);
        }

        public override void QueryItems(Vector2 min, Vector2 max, HashSet<T> result) {
            var keys = ObjectPool.GetList<Vector2Int>();
            foreach (var key in OverlappingChunkKeys(min, max, keys)) {
                if (chunk2grid.TryGetValue(key, out var grid)){
                    grid.QueryItems(min, max, result);
                }
            }
            ObjectPool.ReturnList(keys);
        }
      
        //***********************************************************************************************
        // Helpers
        //***********************************************************************************************

        List<Vector2Int> OverlappingChunkKeys(Vector2 min, Vector2 max, List<Vector2Int> results) {
            int x0 = WorldToChunkX(min.x);
            int y0 = WorldToChunkY(min.y);
            int x1 = WorldToChunkX(max.x);
            int y1 = WorldToChunkY(max.y);

            for (int x = x0; x <= x1; x++) {
                for (int y = y0; y <= y1; y++) {
                    results.Add(new Vector2Int(x, y));
                }
            }
            return results;
        }

        int WorldToChunkX(float wx) {
            return Mathf.FloorToInt(wx / _chunkSize.x); 
        }

        int WorldToChunkY(float wy) { 
            return Mathf.FloorToInt(wy / _chunkSize.y);
        }

        Grid2D<T> GetOrCreateChunk(Vector2Int key) {
            if (chunk2grid.TryGetValue(key, out var existing)) {
                return existing;
            }

            var chunkMin = new Vector2(
                key.x * _chunkSize.x,
                key.y * _chunkSize.y
            );

            var chunkMax = new Vector2(
                chunkMin.x + _chunkSize.x,
                chunkMin.y + _chunkSize.y
            );

            var grid = new Grid2D<T>(chunkMin, chunkMax, _chunkResolution);
            chunk2grid[key] = grid;
            return grid;
        }
    }
}