﻿using System.Collections.Generic;
using UnityEngine;


namespace Example.Shooter2D {
    public class UnitManager {

        public Player player;

        UnitPartition partition;
        List<Unit> units = new List<Unit>();

        public UnitManager(GameObject player) {
            partition = new UnitPartition();
            this.player = new Player(player, .45f);
            AddUnit(this.player);
        }

        public void Update() {
            for (int i = 0; i < units.Count; i++) {
                Unit u = units[i];
                u.Update();
            }
            if (Input.GetKeyDown(KeyCode.Space)) {
                Debug.Log($"Num Creep: {units.Count}");

            }
        }
        public void AddUnit(Unit u) {
            units.Add(u);
            partition.AddUnit(u);
        }
        public void UpdateUnit(Unit u) {
            partition.UpdateUnit(u);
        }
        public void Remove(Unit u) {
            units.Remove(u);
            partition.RemoveUnit(u);
        }
        public void Query(Vector2 min, Vector2 max, List<Unit> results) {
            partition.Query(min, max, results);
        }
        public void Query((Vector2 min, Vector2 max) AABB, List<Unit> results) {
            partition.Query(AABB.min, AABB.max, results);
        }

        public void DrawGizmos() {

            foreach (var u in units) {
                u.DrawGizmos();
            }
        }
    }

}