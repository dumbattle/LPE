using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Abstract base for any chess player (human or AI).
/// Subclasses implement <see cref="TakeTurn"/> as a coroutine that eventually
/// calls <paramref name="submitMove"/> with the chosen move and yields break.
/// </summary>
public abstract class ChessPlayer : MonoBehaviour {

    /// <summary>Reference back to the game controller. Set by ChessMain.Initialize.</summary>
    public ChessMain Game { get; private set; }

    /// <summary>The color this player controls (Piece.White or Piece.Black). Set by ChessMain.</summary>
    public int Color { get; private set; }

    internal void Initialize(ChessMain game, int color) {
        Game  = game;
        Color = color;
        OnInitialized();
    }

    /// <summary>Called once after <see cref="Game"/> and <see cref="Color"/> are set.</summary>
    protected virtual void OnInitialized() { }

    /// <summary>
    /// Coroutine that runs for one turn. Must call <paramref name="submitMove"/> exactly once
    /// (or not at all if <see cref="Cancel"/> is called first) and then yield break.
    /// </summary>
    public abstract IEnumerator TakeTurn(ChessBoard board, Action<ChessMove> submitMove);

    /// <summary>
    /// Called by ChessMain when the turn is interrupted (undo, new game, etc.).
    /// Override to clean up any pending state (e.g. hide selection highlights).
    /// </summary>
    public virtual void Cancel() { }
}
