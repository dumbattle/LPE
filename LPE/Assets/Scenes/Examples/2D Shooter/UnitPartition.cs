using UnityEngine;
using LPE.SpacePartition;
using System.Collections.Generic;

namespace Example.Shooter2D {
    public class UnitPartition{
        Partition2D<Unit> partition;

        public UnitPartition() {
            partition = new Grid2D<Unit>(
                -new Vector2(Params.Width, Params.Height) / 2, 
                new Vector2(Params.Width, Params.Height) / 2, 
                new Vector2Int(Params.Width, Params.Height));
        }

        public void AddUnit(Unit u) {
            partition.Add(u, u.AABB());
        }
        public void UpdateUnit(Unit u) {

            partition.UpdateItem(u, u.AABB());
        }
        public void RemoveUnit(Unit u) {
            partition.Remove(u);
        }
   
        public void Query(Vector2 min, Vector2 max, List<Unit> results) {
            partition.QueryItems(min, max, results);
        }
    
    }

}