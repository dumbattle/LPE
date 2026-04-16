using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AI-controlled chess player. Base implementation plays a random legal move.
/// Override <see cref="ChooseMove"/> in a subclass to add stronger evaluation.
/// </summary>
public class AIChessPlayer : ChessPlayer {

    [Tooltip("Simulated thinking delay in seconds (cosmetic).")]
    [SerializeField] private float thinkTime = 0.3f;

    private bool _cancelled;

    public override IEnumerator TakeTurn(ChessBoard board, Action<ChessMove> submitMove) {
        _cancelled = false;

        if (thinkTime > 0f) yield return new WaitForSeconds(thinkTime);

        if (_cancelled) yield break;

        ChessMove move = ChooseMove(board);
        if (move.IsValid) submitMove(move);
    }

    public override void Cancel() => _cancelled = true;

    // ── Override this for stronger AI ────────────────────────────────────────

    /// <summary>
    /// Selects a move given the current board. Default: random legal move.
    /// Return <see cref="ChessMove.Invalid"/> to forfeit (should never happen in a normal game).
    /// </summary>
    protected virtual ChessMove ChooseMove(ChessBoard board) {
        var legal = board.GetLegalMoves();
        if (legal.Count == 0) return ChessMove.Invalid;
        return legal[UnityEngine.Random.Range(0, legal.Count)];
    }
}
