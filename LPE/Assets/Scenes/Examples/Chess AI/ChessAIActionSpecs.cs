using LPE.AI.LPE;

// ── Chess Action Specifications ───────────────────────────────────────────────
// All use int (square index 0-63) as the parameter so the desire system can
// match "I want to attack the piece on square X" against "this move lands on X".

/// <summary>
/// Desire to capture an opponent piece currently standing on a specific square.
/// Emitted for every visible opponent piece, weighted by material value.
/// Proposals report this spec with the square of whatever piece they capture.
/// </summary>
public class AttackPieceSpec : ActionSpec<int> { }

/// <summary>
/// Desire to have a friendly piece cover / defend a specific square
/// so any enemy capture there can be met with a recapture.
/// Emitted for every own piece (excluding king), weighted by material value and
/// bumped up a tier when the piece is currently under attack.
/// Proposals report this spec for each own-piece square the moving piece
/// will attack from its destination.
/// </summary>
public class ProtectPieceSpec : ActionSpec<int> { }

/// <summary>
/// Desire to exert influence over a specific square (space / outpost control).
/// Emitted for center squares (secondary) and extended-center squares (incidental).
/// Proposals report this spec for every square the moving piece attacks from
/// its destination (static geometry, no ray-blocking check).
/// </summary>
public class ProtectSquareSpec : ActionSpec<int> { }
