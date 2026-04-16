using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Human-controlled chess player. Waits for the user to click squares on the board.
/// First click selects a piece; second click on a legal target submits the move.
/// Clicking the same piece again, or clicking an empty square, deselects.
/// </summary>
public class HumanChessPlayer : ChessPlayer {

    private bool _cancelled;
    private int  _selectedSquare = -1;

    public override IEnumerator TakeTurn(ChessBoard board, Action<ChessMove> submitMove) {
        _cancelled      = false;
        _selectedSquare = -1;
        List<ChessMove> legalMoves = null;

        while (!_cancelled) {
            if (Input.GetMouseButtonDown(0)) {
                int clicked = Game.GetSquareAtMouse();

                if (_selectedSquare >= 0) {
                    // ── Something is already selected ──────────────────────
                    ChessMove? move = PickMove(legalMoves, clicked);

                    if (move.HasValue) {
                        // Valid move
                        _selectedSquare = -1;
                        Game.ClearHighlights();
                        submitMove(move.Value);
                        yield break;
                    }

                    if (clicked >= 0 && Piece.IsColor(board.GetPiece(clicked), Color)) {
                        // Clicked a different friendly piece – re-select
                        Select(clicked, board);
                        legalMoves = board.GetLegalMovesFrom(clicked);
                        Game.ShowSelection(_selectedSquare, legalMoves);
                    } else {
                        // Clicked empty or enemy square without a legal move – deselect
                        Deselect();
                    }
                } else {
                    // ── Nothing selected ────────────────────────────────────
                    if (clicked >= 0 && Piece.IsColor(board.GetPiece(clicked), Color)) {
                        Select(clicked, board);
                        legalMoves = board.GetLegalMovesFrom(clicked);

                        if (legalMoves.Count > 0)
                            Game.ShowSelection(_selectedSquare, legalMoves);
                        else
                            Deselect(); // piece has no legal moves – don't select
                    }
                }
            }

            yield return null;
        }
    }

    public override void Cancel() {
        _cancelled      = true;
        _selectedSquare = -1;
        if (Game != null) Game.ClearHighlights();
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private void Select(int square, ChessBoard board) {
        _selectedSquare = square;
    }

    private void Deselect() {
        _selectedSquare = -1;
        Game.ClearHighlights();
    }

    /// <summary>
    /// Finds a legal move from the pre-generated list that ends on <paramref name="toSquare"/>.
    /// For promotions, auto-promotes to queen.
    /// </summary>
    private static ChessMove? PickMove(List<ChessMove> moves, int toSquare) {
        if (moves == null || toSquare < 0) return null;

        ChessMove? queenPromotion = null;
        foreach (var m in moves) {
            if (m.To != toSquare) continue;
            if (!m.IsPromotion)   return m;                              // Normal move
            if (m.PromotionPieceType == Piece.Queen) queenPromotion = m; // Prefer queen
        }
        return queenPromotion; // null if toSquare had no legal destination
    }
}
