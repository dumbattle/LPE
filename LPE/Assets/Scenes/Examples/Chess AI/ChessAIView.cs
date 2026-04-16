using System;
using System.Collections.Generic;
using LPE.AI.LPE;

// ── Data structs ──────────────────────────────────────────────────────────────

/// <summary>Snapshot of a single piece on the board.</summary>
public struct ChessPieceData {
    /// <summary>Square index 0-63.</summary>
    public int  square;
    /// <summary>Piece encoding (Piece.White | Piece.Rook, etc.).</summary>
    public int  piece;
    /// <summary>True if an opponent piece can capture this piece right now.</summary>
    public bool isUnderAttack;
    /// <summary>Conventional material value (pawn=1 … queen=9, king=100).</summary>
    public int  materialValue;
}

/// <summary>
/// A legal move together with precomputed metadata needed by the AI phases.
/// </summary>
public struct ChessMoveData {
    /// <summary>The legal chess move.</summary>
    public ChessMove move;
    /// <summary>Piece encoding of the piece being moved.</summary>
    public int movingPiece;
    /// <summary>Piece encoding of the captured piece, or <see cref="Piece.None"/>.</summary>
    public int capturedPiece;
    /// <summary>Square of the captured piece (-1 if no capture).</summary>
    public int capturedSquare;
    /// <summary>
    /// Squares of own pieces (other than the moving piece itself) that the
    /// moving piece will defend / cover from its destination.
    /// Computed using static (ray-blocking-free) attack geometry.
    /// </summary>
    public int[] protectedOwnSquares;
    /// <summary>
    /// All squares the moving piece attacks from its destination square.
    /// Computed using static geometry (no ray-blocking).
    /// Used for ProtectSquareSpec reporting.
    /// </summary>
    public int[] controlledSquares;
}

// ── View ──────────────────────────────────────────────────────────────────────

/// <summary>
/// World snapshot for the chess AI.
/// Call <see cref="LPEView{TWorld}.Capture"/> once per turn before running
/// the engine; all three collections are repopulated from the given board state.
/// </summary>
public class ChessAIView : LPEView<ChessBoard> {

    // ── Collections (read by all AI phases) ──────────────────────────────────

    /// <summary>All pieces belonging to the AI player.</summary>
    public readonly ViewCollection<ChessPieceData> OwnPieces = new();
    /// <summary>All pieces belonging to the opponent.</summary>
    public readonly ViewCollection<ChessPieceData> OpponentPieces = new();
    /// <summary>Every legal move available to the AI this turn.</summary>
    public readonly ViewCollection<ChessMoveData>  PossibleMoves = new();

    // ── Board context (set during Capture; read-only afterwards) ─────────────

    /// <summary>The board that was captured. Do not mutate during AI phases.</summary>
    public ChessBoard Board   { get; private set; }
    /// <summary>Piece.White or Piece.Black — the colour the AI is playing.</summary>
    public int        AIColor { get; private set; }

    // ── LPEView implementation ────────────────────────────────────────────────

    protected override void Capture(SnapshotContext ctx, ChessBoard board) {
        Board   = board;
        AIColor = board.IsWhiteToMove ? Piece.White : Piece.Black;
        int opponentColor = Piece.Opponent(AIColor);

        // ── Build piece lists ─────────────────────────────────────────────
        var ownList    = new List<ChessPieceData>(16);
        var oppList    = new List<ChessPieceData>(16);
        var ownSquares = new HashSet<int>(16);

        for (int sq = 0; sq < 64; sq++) {
            int p = board.GetPiece(sq);
            if (p == Piece.None) continue;

            var data = new ChessPieceData {
                square        = sq,
                piece         = p,
                isUnderAttack = board.IsAttacked(sq, opponentColor),
                materialValue = MaterialValue(Piece.Type(p)),
            };

            if (Piece.IsColor(p, AIColor)) {
                ownList.Add(data);
                ownSquares.Add(sq);
            } else {
                oppList.Add(data);
            }
        }

        ctx.SetData(OwnPieces,      ownList);
        ctx.SetData(OpponentPieces, oppList);

        // ── Build move list with precomputed metadata ─────────────────────
        var legalMoves = board.GetLegalMoves();
        var moveList   = new List<ChessMoveData>(legalMoves.Count);

        foreach (var move in legalMoves) {
            int movingPiece    = board.GetPiece(move.From);
            int capturedPiece  = Piece.None;
            int capturedSquare = -1;

            if (move.IsEnPassant) {
                // Captured pawn sits on the same rank as the moving pawn,
                // same file as the destination square.
                int capRank    = move.From / 8;
                int capFile    = move.To   % 8;
                capturedSquare = capRank * 8 + capFile;
                capturedPiece  = board.GetPiece(capturedSquare);
            } else {
                int target = board.GetPiece(move.To);
                if (target != Piece.None) {
                    capturedPiece  = target;
                    capturedSquare = move.To;
                }
            }

            // Squares the piece attacks from its destination (static geometry)
            int[] controlled = StaticAttackSquares(movingPiece, move.To);

            // Own pieces the move will defend (exclude the moving piece's origin)
            var prot = new List<int>(4);
            foreach (int s in controlled) {
                if (s != move.From && ownSquares.Contains(s))
                    prot.Add(s);
            }

            moveList.Add(new ChessMoveData {
                move                = move,
                movingPiece         = movingPiece,
                capturedPiece       = capturedPiece,
                capturedSquare      = capturedSquare,
                protectedOwnSquares = prot.ToArray(),
                controlledSquares   = controlled,
            });
        }

        ctx.SetData(PossibleMoves, moveList);
    }

    // ── Static helpers ────────────────────────────────────────────────────────

    /// <summary>Standard material values used for desire-strength weighting.</summary>
    public static int MaterialValue(int pieceType) => pieceType switch {
        Piece.Pawn   => 1,
        Piece.Knight => 3,
        Piece.Bishop => 3,
        Piece.Rook   => 5,
        Piece.Queen  => 9,
        Piece.King   => 100,
        _            => 0,
    };

    /// <summary>
    /// Returns all squares that <paramref name="piece"/> attacks from
    /// <paramref name="fromSq"/> using static geometry (rays are not stopped by
    /// intervening pieces — intentional approximation for the basic AI).
    /// </summary>
    private static int[] StaticAttackSquares(int piece, int fromSq) {
        var result = new List<int>(27);
        int file   = fromSq % 8;
        int rank   = fromSq / 8;
        int color  = Piece.Color(piece);

        switch (Piece.Type(piece)) {

            case Piece.Pawn: {
                int dir = color == Piece.White ? 1 : -1;
                foreach (int df in new[] { -1, 1 }) {
                    int f = file + df, r = rank + dir;
                    if (f >= 0 && f <= 7 && r >= 0 && r <= 7)
                        result.Add(r * 8 + f);
                }
                break;
            }

            case Piece.Knight: {
                foreach (int off in new[] { -17, -15, -10, -6, 6, 10, 15, 17 }) {
                    int t = fromSq + off;
                    if (t >= 0 && t < 64 && Math.Abs(file - t % 8) <= 2)
                        result.Add(t);
                }
                break;
            }

            case Piece.Bishop: StaticRays(fromSq, new[] { -9, -7,  7, 9 },            result); break;
            case Piece.Rook:   StaticRays(fromSq, new[] { -8, -1,  1, 8 },            result); break;
            case Piece.Queen:  StaticRays(fromSq, new[] { -9,-8,-7,-1, 1, 7, 8, 9 },  result); break;

            case Piece.King: {
                foreach (int off in new[] { -9, -8, -7, -1, 1, 7, 8, 9 }) {
                    int t = fromSq + off;
                    if (t >= 0 && t < 64 && Math.Abs(file - t % 8) <= 1)
                        result.Add(t);
                }
                break;
            }
        }

        return result.ToArray();
    }

    /// <summary>Appends squares along each direction until edge/wrap, ignoring pieces.</summary>
    private static void StaticRays(int fromSq, int[] dirs, List<int> result) {
        foreach (int dir in dirs) {
            int t = fromSq;
            for (int i = 0; i < 7; i++) {
                int prevFile = t % 8;
                t += dir;
                if (t < 0 || t >= 64 || Math.Abs(prevFile - t % 8) > 1) break;
                result.Add(t);
            }
        }
    }
}
