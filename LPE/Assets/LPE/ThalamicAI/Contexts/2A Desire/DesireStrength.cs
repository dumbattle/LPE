using System.Collections.Generic;

namespace LPE.AI.LPE {
    public enum DesireStrength {
        none = 0,
        idle = 1,  // special, for idle actions only
        incidental = 2,
        secondary = 3,
        primary = 4,
    }
    public static class DesireStrengthUtility {
        public static IReadOnlyList<DesireStrength> DesiresDescending = new DesireStrength[] {
            DesireStrength.primary,
            DesireStrength.secondary,
            DesireStrength.incidental,
            DesireStrength.idle,
            DesireStrength.none,
        };

    }
}
