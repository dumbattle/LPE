using LPE.AI.LPE;

/// <summary>
/// Thalamic chess AI.  Implements the three LensedPerceptionEngine phases
/// for chess using AttackPieceSpec, ProtectPieceSpec, and ProtectSquareSpec.
///
/// ┌──────────────┬─────────────────────────────────────────────────────────┐
/// │ Phase        │ What it does                                            │
/// ├──────────────┼─────────────────────────────────────────────────────────┤
/// │ Attention    │ Sets ALL own pieces, opponent pieces, and legal moves   │
/// │              │ to AttentionLevel.focus (basic AI sees everything).     │
/// ├──────────────┼─────────────────────────────────────────────────────────┤
/// │ EmitDesires  │ • AttackPieceSpec  for every opponent piece             │
/// │              │   (primary=queen, secondary=rook/minor, incidental=pawn)│
/// │              │ • ProtectPieceSpec for every own non-king piece         │
/// │              │   (same tiers; bumped up if currently under attack)     │
/// │              │ • ProtectSquareSpec for center squares (secondary) and  │
/// │              │   extended-center squares (incidental)                  │
/// ├──────────────┼─────────────────────────────────────────────────────────┤
/// │ ProposeActions│ One proposal per legal move. Reports:                  │
/// │              │   AttackPieceSpec  – if it captures                     │
/// │              │   ProtectPieceSpec – for each own piece it covers       │
/// │              │   ProtectSquareSpec– for each square it attacks         │
/// └──────────────┴─────────────────────────────────────────────────────────┘
/// </summary>
public class ChessAI : LensedPerceptionEngine<ChessAIView> {

    // ── Square tables ─────────────────────────────────────────────────────────

    // d4=27, e4=28, d5=35, e5=36
    private static readonly int[] CenterSquares = { 27, 28, 35, 36 };

    // c3-f3, c4/f4, c5/f5, c6-f6
    private static readonly int[] ExtendedCenter = {
        18, 19, 20, 21,
        26,         29,
        34,         37,
        42, 43, 44, 45,
    };

    // ── Attention ─────────────────────────────────────────────────────────────

    protected override void Attention(ChessAIView view, AttentionContext ctx) {
        // Basic AI: all pieces and all moves are in full focus.
        foreach (var (elem, _) in ctx.EnumerateElements(view.OwnPieces))
            ctx.SetAttentionLevel(elem, AttentionLevel.focus);

        foreach (var (elem, _) in ctx.EnumerateElements(view.OpponentPieces))
            ctx.SetAttentionLevel(elem, AttentionLevel.focus);

        foreach (var (elem, _) in ctx.EnumerateElements(view.PossibleMoves))
            ctx.SetAttentionLevel(elem, AttentionLevel.focus);
    }

    // ── EmitDesires ───────────────────────────────────────────────────────────

    protected override void EmitDesires(ChessAIView view, DesireContext ctx) {

        // ── 1. Attack opponent pieces ─────────────────────────────────────
        foreach (var p in ctx.EnumerateData(view.OpponentPieces)) {
            ctx.EmitDesire<AttackPieceSpec, int>(
                p.square,
                AttackStrength(p.materialValue),
                $"Attack {Piece.TypeName(p.piece)} on {SqName(p.square)}");
        }

        // ── 2. Protect own pieces (king excluded — handled by move legality) ──
        foreach (var p in ctx.EnumerateData(view.OwnPieces)) {
            if (Piece.Type(p.piece) == Piece.King) continue;

            ctx.EmitDesire<ProtectPieceSpec, int>(
                p.square,
                ProtectStrength(p.materialValue, p.isUnderAttack),
                $"Protect {Piece.TypeName(p.piece)} on {SqName(p.square)}{(p.isUnderAttack ? " [ATTACKED]" : "")}");
        }

        // ── 3. Control center squares ─────────────────────────────────────
        foreach (int sq in CenterSquares)
            ctx.EmitDesire<ProtectSquareSpec, int>(sq, DesireStrength.secondary,
                $"Control center {SqName(sq)}");

        foreach (int sq in ExtendedCenter)
            ctx.EmitDesire<ProtectSquareSpec, int>(sq, DesireStrength.incidental,
                $"Control ext-center {SqName(sq)}");
    }

    // ── ProposeActions ────────────────────────────────────────────────────────

    protected override void ProposeActions(ChessAIView view, ActionProposalContext ctx) {
        foreach (var m in ctx.EnumerateData(view.PossibleMoves)) {
            var b = ctx.BeginProposal(m.move);

            // ── Attacks a piece ───────────────────────────────────────────
            if (m.capturedPiece != Piece.None)
                b = b.ReportSatisfies<AttackPieceSpec, int>(m.capturedSquare);

            // ── Defends own pieces from its destination ───────────────────
            foreach (int sq in m.protectedOwnSquares)
                b = b.ReportSatisfies<ProtectPieceSpec, int>(sq);

            // ── Controls squares from its destination ─────────────────────
            foreach (int sq in m.controlledSquares)
                b = b.ReportSatisfies<ProtectSquareSpec, int>(sq);
        }
    }

    // ── Desire-strength helpers ───────────────────────────────────────────────

    private static DesireStrength AttackStrength(int value) => value switch {
        >= 9 => DesireStrength.primary,    // queen
        >= 5 => DesireStrength.secondary,  // rook
        >= 3 => DesireStrength.secondary,  // bishop / knight
        _    => DesireStrength.incidental, // pawn
    };

    private static DesireStrength ProtectStrength(int value, bool underAttack) {
        // Base tier by material value
        DesireStrength s = value switch {
            >= 9 => DesireStrength.primary,    // queen
            >= 5 => DesireStrength.secondary,  // rook
            >= 3 => DesireStrength.secondary,  // bishop / knight
            _    => DesireStrength.incidental, // pawn
        };
        // Downgrade one tier if the piece is not currently under threat
        return underAttack ? s : Downgrade(s);
    }

    private static DesireStrength Downgrade(DesireStrength s) => s switch {
        DesireStrength.primary   => DesireStrength.secondary,
        DesireStrength.secondary => DesireStrength.incidental,
        _                        => DesireStrength.incidental,
    };

    private static string SqName(int sq) => $"{(char)('a' + sq % 8)}{sq / 8 + 1}";
}
