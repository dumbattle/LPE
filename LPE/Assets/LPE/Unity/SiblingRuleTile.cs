using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LPE.Unity {


    [CreateAssetMenu(fileName = "Sibling Rule Tile", menuName = "LPE/Tiles/Sibling Rule Tile")]
    public class SiblingRuleTile : RuleTile, IFamilyTile {
        string IFamilyTile.family => siblingID;
        public TileBase defaultRules;
        public string siblingID;
        public string[] siblingGroups;
         
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData) {
            var iden = Matrix4x4.identity;

            tileData.sprite = m_DefaultSprite;
            tileData.gameObject = m_DefaultGameObject;
            tileData.colliderType = m_DefaultColliderType;
            tileData.flags = TileFlags.LockTransform;
            tileData.transform = iden;

            Matrix4x4 transform = iden;
            foreach (TilingRule rule in m_TilingRules) {
                if (RuleMatches(rule, position, tilemap, ref transform)) {
                    switch (rule.m_Output) {
                        case TilingRuleOutput.OutputSprite.Single:
                        case TilingRuleOutput.OutputSprite.Animation:
                            tileData.sprite = rule.m_Sprites[0];
                            break;
                        case TilingRuleOutput.OutputSprite.Random:
                            int index = Mathf.Clamp(Mathf.FloorToInt(GetPerlinValue(position, rule.m_PerlinScale, 100000f) * rule.m_Sprites.Length), 0, rule.m_Sprites.Length - 1);
                            tileData.sprite = rule.m_Sprites[index];
                            if (rule.m_RandomTransform != TilingRuleOutput.Transform.Fixed)
                                transform = ApplyRandomTransform(rule.m_RandomTransform, transform, rule.m_PerlinScale, position);
                            break;
                    }
                    tileData.transform = transform;
                    tileData.gameObject = rule.m_GameObject;
                    tileData.colliderType = rule.m_ColliderType;
                    return;
                }
            }

            if (defaultRules != null) {
                defaultRules.GetTileData(position, tilemap, ref tileData);
            }
        }


        public override bool RuleMatch(int neighbor, TileBase other) {
            if (other is RuleOverrideTile)
                other = (other as RuleOverrideTile).m_InstanceTile;

            bool validSibling = other == this; ;


            foreach (var s in siblingGroups??Array.Empty<string>()) {
                if (validSibling) {
                    break;
                }
                validSibling = other is IFamilyTile t && t.family == s && s != "";
            }
            switch (neighbor) {
                case TilingRule.Neighbor.This: {
                    return validSibling;
                }
                case TilingRule.Neighbor.NotThis: {
                    return !validSibling;
                }
            }

            return base.RuleMatch(neighbor, other);
        }


    }
}
