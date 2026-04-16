using System;
using System.Collections.Generic;

/// <summary>
/// Pure C# chess board logic. No Unity dependencies.
/// Square index: rank * 8 + file  (rank 0 = white's back rank, file 0 = a-file)
/// </summary>
public class ChessBoard {

    // ───── Public State ─────────────────────────────────────────────────────

    public int[]  Squares       { get; private set; } = new int[64];
    public bool   WhiteToMove   { get; private set; } = true;
    public bool[] CastlingRights { get; private set; } = new bool[4]; // [0]=WK [1]=WQ [2]=BK [3]=BQ
    public int    EnPassantSquare { get; private set; } = -1;
    public int    HalfMoveClock  { get; private set; }
    public int    FullMoveNumber { get; private set; } = 1;
    public ChessMove LastMove    { get; private set; } = ChessMove.Invalid;

    public int HistoryCount => _history.Count;

    // ───── History ──────────────────────────────────────────────────────────

    private struct Snapshot {
        public int[]     squares;
        public bool      whiteToMove;
        public bool[]    castlingRights;
        public int       enPassantSquare;
        public int       halfMoveClock;
        public int       fullMoveNumber;
        public ChessMove lastMove;
    }

    private readonly Stack<Snapshot> _history = new Stack<Snapshot>();

    // ───── Setup ────────────────────────────────────────────────────────────

    public ChessBoard() { }

    public void SetupStartPosition() {
        Array.Clear(Squares, 0, 64);

        // White back rank
        Squares[0] = Piece.White | Piece.Rook;
        Squares[1] = Piece.White | Piece.Knight;
        Squares[2] = Piece.White | Piece.Bishop;
        Squares[3] = Piece.White | Piece.Queen;
        Squares[4] = Piece.White | Piece.King;
        Squares[5] = Piece.White | Piece.Bishop;
        Squares[6] = Piece.White | Piece.Knight;
        Squares[7] = Piece.White | Piece.Rook;
        for (int f = 0; f < 8; f++) Squares[8  + f] = Piece.White | Piece.Pawn;

        // Black back rank
        Squares[56] = Piece.Black | Piece.Rook;
        Squares[57] = Piece.Black | Piece.Knight;
        Squares[58] = Piece.Black | Piece.Bishop;
        Squares[59] = Piece.Black | Piece.Queen;
        Squares[60] = Piece.Black | Piece.King;
        Squares[61] = Piece.Black | Piece.Bishop;
        Squares[62] = Piece.Black | Piece.Knight;
        Squares[63] = Piece.Black | Piece.Rook;
        for (int f = 0; f < 8; f++) Squares[48 + f] = Piece.Black | Piece.Pawn;

        WhiteToMove    = true;
        CastlingRights = new bool[] { true, true, true, true };
        EnPassantSquare = -1;
        HalfMoveClock  = 0;
        FullMoveNumber = 1;
        LastMove       = ChessMove.Invalid;
        _history.Clear();
    }

    // ───── Make / Undo ───────────────────────────────────────────────────────

    public void MakeMove(ChessMove move) {
        // Save full state for undo
        _history.Push(new Snapshot {
            squares        = (int[])Squares.Clone(),
            whiteToMove    = WhiteToMove,
            castlingRights = (bool[])CastlingRights.Clone(),
            enPassantSquare = EnPassantSquare,
            halfMoveClock  = HalfMoveClock,
            fullMoveNumber = FullMoveNumber,
            lastMove       = LastMove,
        });

        int piece  = Squares[move.From];
        int color  = Piece.Color(piece);
        int type   = Piece.Type(piece);
        bool isCapture = Squares[move.To] != Piece.None || move.IsEnPassant;

        // Half-move clock
        HalfMoveClock = (isCapture || type == Piece.Pawn) ? 0 : HalfMoveClock + 1;

        // En passant capture
        if (move.IsEnPassant) {
            int capFile = move.To % 8;
            int capRank = move.From / 8;
            Squares[capRank * 8 + capFile] = Piece.None;
        }

        // Castling – move rook
        if (move.IsCastle) {
            int baseRank = color == Piece.White ? 0 : 7;
            if (move.MoveFlag == ChessMove.Flag.CastleKingside) {
                Squares[baseRank * 8 + 5] = Squares[baseRank * 8 + 7];
                Squares[baseRank * 8 + 7] = Piece.None;
            } else {
                Squares[baseRank * 8 + 3] = Squares[baseRank * 8 + 0];
                Squares[baseRank * 8 + 0] = Piece.None;
            }
        }

        // Move piece (with optional promotion)
        Squares[move.To]   = move.IsPromotion ? (color | move.PromotionPieceType) : piece;
        Squares[move.From] = Piece.None;

        // New en passant target square
        EnPassantSquare = (type == Piece.Pawn && Math.Abs(move.To - move.From) == 16)
            ? (move.From + move.To) / 2
            : -1;

        UpdateCastlingRights(move.From, move.To);

        if (!WhiteToMove) FullMoveNumber++;
        WhiteToMove = !WhiteToMove;
        LastMove = move;
    }

    public bool CanUndo() => _history.Count > 0;

    /// <summary>Reverts the last move. Returns the move that was undone.</summary>
    public ChessMove UndoMove() {
        if (_history.Count == 0) return ChessMove.Invalid;

        var snap         = _history.Pop();
        var undone       = LastMove;
        Squares          = snap.squares;
        WhiteToMove      = snap.whiteToMove;
        CastlingRights   = snap.castlingRights;
        EnPassantSquare  = snap.enPassantSquare;
        HalfMoveClock    = snap.halfMoveClock;
        FullMoveNumber   = snap.fullMoveNumber;
        LastMove         = snap.lastMove;
        return undone;
    }

    private void UpdateCastlingRights(int from, int to) {
        if (from == 4)  { CastlingRights[0] = CastlingRights[1] = false; } // White king
        if (from == 60) { CastlingRights[2] = CastlingRights[3] = false; } // Black king
        if (from == 7  || to == 7)  CastlingRights[0] = false; // White KS rook
        if (from == 0  || to == 0)  CastlingRights[1] = false; // White QS rook
        if (from == 63 || to == 63) CastlingRights[2] = false; // Black KS rook
        if (from == 56 || to == 56) CastlingRights[3] = false; // Black QS rook
    }

    // ───── Move Generation ──────────────────────────────────────────────────

    private static readonly int[] KnightOffsets  = { -17, -15, -10, -6, 6, 10, 15, 17 };
    private static readonly int[] KingOffsets    = { -9, -8, -7, -1, 1, 7, 8, 9 };
    private static readonly int[] DiagDirs       = { -9, -7, 7, 9 };
    private static readonly int[] StraightDirs   = { -8, -1, 1, 8 };
    private static readonly int[] AllDirs        = { -9, -8, -7, -1, 1, 7, 8, 9 };

    public List<ChessMove> GetLegalMoves() {
        int color   = WhiteToMove ? Piece.White : Piece.Black;
        var pseudo  = GeneratePseudoLegal(color);
        var legal   = new List<ChessMove>(pseudo.Count);

        foreach (var move in pseudo) {
            MakeMove(move);
            bool leftInCheck = IsAttacked(FindKing(color), Piece.Opponent(color));
            UndoMove();
            if (!leftInCheck) legal.Add(move);
        }

        return legal;
    }

    public List<ChessMove> GetLegalMovesFrom(int square) {
        var all    = GetLegalMoves();
        var result = new List<ChessMove>();
        foreach (var m in all) if (m.From == square) result.Add(m);
        return result;
    }

    private List<ChessMove> GeneratePseudoLegal(int color) {
        var moves = new List<ChessMove>(64);

        for (int sq = 0; sq < 64; sq++) {
            if (!Piece.IsColor(Squares[sq], color)) continue;
            switch (Piece.Type(Squares[sq])) {
                case Piece.Pawn:   GenPawn(sq, color, moves);                          break;
                case Piece.Knight: GenKnight(sq, color, moves);                        break;
                case Piece.Bishop: GenSlider(sq, color, DiagDirs, moves);              break;
                case Piece.Rook:   GenSlider(sq, color, StraightDirs, moves);          break;
                case Piece.Queen:  GenSlider(sq, color, AllDirs, moves);               break;
                case Piece.King:   GenKing(sq, color, moves);                          break;
            }
        }

        return moves;
    }

    private void GenPawn(int sq, int color, List<ChessMove> moves) {
        int rank         = sq / 8;
        int file         = sq % 8;
        int dir          = color == Piece.White ? 1 : -1;
        int startRank    = color == Piece.White ? 1 : 6;
        int promoteRank  = color == Piece.White ? 7 : 0;

        // Single push
        int push = sq + dir * 8;
        if (push >= 0 && push < 64 && Squares[push] == Piece.None) {
            if (push / 8 == promoteRank) {
                AddPromotions(sq, push, moves);
            } else {
                moves.Add(new ChessMove(sq, push));
                // Double push
                if (rank == startRank) {
                    int dbl = sq + dir * 16;
                    if (Squares[dbl] == Piece.None) moves.Add(new ChessMove(sq, dbl));
                }
            }
        }

        // Captures
        for (int df = -1; df <= 1; df += 2) {
            int cf = file + df;
            if (cf < 0 || cf > 7) continue;
            int cr  = rank + dir;
            if (cr < 0 || cr > 7) continue;
            int cap = cr * 8 + cf;

            if (Piece.IsColor(Squares[cap], Piece.Opponent(color))) {
                if (cr == promoteRank) AddPromotions(sq, cap, moves);
                else                  moves.Add(new ChessMove(sq, cap));
            }
            if (cap == EnPassantSquare)
                moves.Add(new ChessMove(sq, cap, ChessMove.Flag.EnPassant));
        }
    }

    private void AddPromotions(int from, int to, List<ChessMove> moves) {
        moves.Add(new ChessMove(from, to, ChessMove.Flag.Promotion, Piece.Queen));
        moves.Add(new ChessMove(from, to, ChessMove.Flag.Promotion, Piece.Rook));
        moves.Add(new ChessMove(from, to, ChessMove.Flag.Promotion, Piece.Bishop));
        moves.Add(new ChessMove(from, to, ChessMove.Flag.Promotion, Piece.Knight));
    }

    private void GenKnight(int sq, int color, List<ChessMove> moves) {
        int file = sq % 8;
        foreach (int off in KnightOffsets) {
            int t = sq + off;
            if (t < 0 || t >= 64) continue;
            if (Math.Abs(file - t % 8) > 2) continue; // wrap guard
            if (!Piece.IsColor(Squares[t], color)) moves.Add(new ChessMove(sq, t));
        }
    }

    private void GenSlider(int sq, int color, int[] dirs, List<ChessMove> moves) {
        foreach (int dir in dirs) {
            int t = sq;
            for (int i = 0; i < 7; i++) {
                int prev = t % 8;
                t += dir;
                if (t < 0 || t >= 64) break;
                if (Math.Abs(prev - t % 8) > 1) break; // file wrap
                if (Piece.IsColor(Squares[t], color)) break;
                moves.Add(new ChessMove(sq, t));
                if (Squares[t] != Piece.None) break; // blocked after capture
            }
        }
    }

    private void GenKing(int sq, int color, List<ChessMove> moves) {
        int file = sq % 8;
        foreach (int off in KingOffsets) {
            int t = sq + off;
            if (t < 0 || t >= 64) continue;
            if (Math.Abs(file - t % 8) > 1) continue;
            if (!Piece.IsColor(Squares[t], color)) moves.Add(new ChessMove(sq, t));
        }
        GenCastle(sq, color, moves);
    }

    private void GenCastle(int sq, int color, List<ChessMove> moves) {
        int baseRank  = color == Piece.White ? 0 : 7;
        int kingStart = baseRank * 8 + 4;
        if (sq != kingStart) return;

        int enemy = Piece.Opponent(color);
        if (IsAttacked(kingStart, enemy)) return; // can't castle while in check

        // Kingside
        if (CastlingRights[color == Piece.White ? 0 : 2]) {
            if (Squares[kingStart + 1] == Piece.None &&
                Squares[kingStart + 2] == Piece.None &&
                !IsAttacked(kingStart + 1, enemy) &&
                !IsAttacked(kingStart + 2, enemy))
                moves.Add(new ChessMove(sq, kingStart + 2, ChessMove.Flag.CastleKingside));
        }

        // Queenside
        if (CastlingRights[color == Piece.White ? 1 : 3]) {
            if (Squares[kingStart - 1] == Piece.None &&
                Squares[kingStart - 2] == Piece.None &&
                Squares[kingStart - 3] == Piece.None &&
                !IsAttacked(kingStart - 1, enemy) &&
                !IsAttacked(kingStart - 2, enemy))
                moves.Add(new ChessMove(sq, kingStart - 2, ChessMove.Flag.CastleQueenside));
        }
    }

    // ───── Attack Detection ─────────────────────────────────────────────────

    /// <summary>Returns true if <paramref name="square"/> is attacked by any piece of <paramref name="attackerColor"/>.</summary>
    public bool IsAttacked(int square, int attackerColor) {
        if (square < 0) return false;
        int file = square % 8;

        // Knight
        foreach (int off in KnightOffsets) {
            int t = square + off;
            if (t < 0 || t >= 64) continue;
            if (Math.Abs(file - t % 8) > 2) continue;
            if (Squares[t] == (attackerColor | Piece.Knight)) return true;
        }

        // Rook / Queen (straight)
        foreach (int dir in StraightDirs) {
            int t = square;
            for (int i = 0; i < 7; i++) {
                int prev = t % 8;
                t += dir;
                if (t < 0 || t >= 64) break;
                if (Math.Abs(prev - t % 8) > 1) break;
                if (Squares[t] == Piece.None) continue;
                int tp = Piece.Type(Squares[t]);
                if (Piece.IsColor(Squares[t], attackerColor) && (tp == Piece.Rook || tp == Piece.Queen)) return true;
                break;
            }
        }

        // Bishop / Queen (diagonal)
        foreach (int dir in DiagDirs) {
            int t = square;
            for (int i = 0; i < 7; i++) {
                int prev = t % 8;
                t += dir;
                if (t < 0 || t >= 64) break;
                if (Math.Abs(prev - t % 8) != 1) break;
                if (Squares[t] == Piece.None) continue;
                int tp = Piece.Type(Squares[t]);
                if (Piece.IsColor(Squares[t], attackerColor) && (tp == Piece.Bishop || tp == Piece.Queen)) return true;
                break;
            }
        }

        // King
        foreach (int off in KingOffsets) {
            int t = square + off;
            if (t < 0 || t >= 64) continue;
            if (Math.Abs(file - t % 8) > 1) continue;
            if (Squares[t] == (attackerColor | Piece.King)) return true;
        }

        // Pawn  – look back from the target square toward where an attacker pawn would stand
        int pawnBackDir  = attackerColor == Piece.White ? -8 : 8;
        foreach (int df in new[] { -1, 1 }) {
            int pSq = square + pawnBackDir + df;
            if (pSq < 0 || pSq >= 64) continue;
            if (Math.Abs(file - pSq % 8) != 1) continue;
            if (Squares[pSq] == (attackerColor | Piece.Pawn)) return true;
        }

        return false;
    }

    // ───── Game-State Queries ────────────────────────────────────────────────

    public bool IsInCheck() => IsInCheck(WhiteToMove ? Piece.White : Piece.Black);
    public bool IsInCheck(int color) => IsAttacked(FindKing(color), Piece.Opponent(color));

    private int FindKing(int color) {
        for (int sq = 0; sq < 64; sq++)
            if (Squares[sq] == (color | Piece.King)) return sq;
        return -1;
    }

    public enum GameResult { Ongoing, WhiteWins, BlackWins, Draw }

    public GameResult GetGameResult() {
        int active = WhiteToMove ? Piece.White : Piece.Black;
        var legal  = GetLegalMoves();

        if (legal.Count == 0)
            return IsInCheck(active)
                ? (WhiteToMove ? GameResult.BlackWins : GameResult.WhiteWins)
                : GameResult.Draw; // stalemate

        if (HalfMoveClock >= 100 || IsInsufficientMaterial())
            return GameResult.Draw;

        return GameResult.Ongoing;
    }

    private bool IsInsufficientMaterial() {
        int minors = 0;
        for (int sq = 0; sq < 64; sq++) {
            int p = Squares[sq];
            if (p == Piece.None) continue;
            int t = Piece.Type(p);
            if (t == Piece.Pawn || t == Piece.Rook || t == Piece.Queen) return false;
            if (t != Piece.King) minors++;
        }
        return minors <= 1; // K vs K or K+minor vs K
    }

    // ───── Convenience Read API ──────────────────────────────────────────────

    public int GetPiece(int square)           => Squares[square];
    public int GetPiece(int file, int rank)   => Squares[rank * 8 + file];
    public bool IsWhiteToMove                 => WhiteToMove;

    public static int SquareIndex(int file, int rank) => rank * 8 + file;
    public static int FileOf(int square)               => square % 8;
    public static int RankOf(int square)               => square / 8;
}
