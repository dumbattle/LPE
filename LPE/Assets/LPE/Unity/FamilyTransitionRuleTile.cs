using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LPE.Unity {
    [CreateAssetMenu(menuName = "LPE/Tiles/Transition Rule Tile")]
    public class FamilyTransitionRuleTile : RuleTile, IFamilyTile {
        public string family;
        string IFamilyTile.family => family;
        public Sprite[] baseSprites;


        public List<TransitionData> transitions = new List<TransitionData>(); 
        public int rngSeed;


        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData) {
            string nF = (tilemap.GetTile(position + Vector3Int.up) as IFamilyTile)?.family;
            string eF = (tilemap.GetTile(position + Vector3Int.right) as IFamilyTile)?.family;
            string sF = (tilemap.GetTile(position + Vector3Int.down) as IFamilyTile)?.family;
            string wF = (tilemap.GetTile(position + Vector3Int.left) as IFamilyTile)?.family;

            foreach (var transition in transitions) {
                int mask = 0;
                if (nF == transition.targetFamily) mask |= 1;
                if (eF == transition.targetFamily) mask |= 2;
                if (sF == transition.targetFamily) mask |= 4;
                if (wF == transition.targetFamily) mask |= 8;

                if (mask > 0) {
                    Sprite[] sprites = transition.bitmaskRules[mask];
                    if (sprites != null && sprites.Length > 0) {
                        SetRandomSprite(position, sprites, ref tileData);
                        return;
                    }
                }
            }

            SetRandomSprite(position, baseSprites, ref tileData);
            tileData.sprite ??= m_DefaultSprite;
        }

        void SetRandomSprite(Vector3Int position, Sprite[] sprites, ref TileData tileData) {
            if (sprites == null || sprites.Length == 0) return;

            long hash = rngSeed;
            hash = hash * 397 ^ position.x;
            hash = hash * 397 ^ position.y;
            hash = hash * 397 ^ position.z;
            int index = (int)((hash & 0x7FFFFFFF) % sprites.Length);
            tileData.sprite = sprites[index];
        }

        [Serializable]
        public struct TransitionData {
            public string targetFamily;
            public BitmaskSpriteGroup bitmaskRules;
        }

        [Serializable]
        public struct BitmaskSpriteGroup {
            // The Indexer: Maps bitmask (0-15) to the specific sprite field
            public Sprite[] this[int mask] {
                get {
                    return mask switch {
                        1 => _U,
                        2 => _R,
                        4 => _D,
                        8 => _L,
                        5 => _UD,    // 1 + 4
                        10 => _RL,    // 2 + 8
                        3 => _UR,    // 1 + 2
                        6 => _RD,    // 2 + 4
                        12 => _DL,    // 4 + 8
                        9 => _UL,    // 1 + 8
                        14 => _RDL,   // 2 + 4 + 8
                        13 => _UDL,   // 1 + 4 + 8
                        11 => _URL,   // 1 + 2 + 8
                        7 => _URD,    // 1 + 2 + 4
                        15 => _URDL,  // 1 + 2 + 4 + 8
                        _ => null    // Mask 0 or undefined
                    };
                }
            }

            public int bitmask; // 4-neighbor (1=U, 2=R, 4=D, 8=W)
            [SerializeField] Sprite[] _U;  // 1 neighbor north of current tile
            [SerializeField] Sprite[] _R;
            [SerializeField] Sprite[] _D;
            [SerializeField] Sprite[] _L;

            [SerializeField] Sprite[] _UD;
            [SerializeField] Sprite[] _RL;

            [SerializeField] Sprite[] _UR;
            [SerializeField] Sprite[] _RD;
            [SerializeField] Sprite[] _DL;
            [SerializeField] Sprite[] _UL;

            [SerializeField] Sprite[] _RDL;
            [SerializeField] Sprite[] _UDL;
            [SerializeField] Sprite[] _URL;
            [SerializeField] Sprite[] _URD;

            [SerializeField] Sprite[] _URDL;
        }

    }
}
