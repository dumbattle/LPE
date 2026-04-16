using LPE.AI.LPE;
using UnityEngine;

/// <summary>
/// Chess player driven by <see cref="ChessAI"/> (a <see cref="LensedPerceptionEngine{TView}"/>).
/// Drop this component onto a GameObject in the scene and assign it to
/// <see cref="ChessMain.whitePlayer"/> or <see cref="ChessMain.blackPlayer"/>.
///
/// Falls back to a random legal move (inherited from <see cref="AIChessPlayer"/>)
/// if the engine returns no valid proposal.
///
/// ── Debug mode ─────────────────────────────────────────────────────────────
/// Enable <see cref="debugRun"/> in the Inspector to print a full breakdown of
/// desires, proposals, and the selection decision to the Unity Console each turn.
/// The engine is run twice when debug is on (once to log, once to get the move).
/// </summary>
public class ThalamicAIChessPlayer : AIChessPlayer {

    [Header("Thalamic AI")]
    [Tooltip("Print desires / proposals / selection to the Console each turn.")]
    [SerializeField] private bool debugRun = false;

    private ChessAI     _engine;
    private ChessAIView _view;

    // Called by ChessPlayer.Initialize after Color and Game are set.
    protected override void OnInitialized() {
        _engine = new ChessAI();
        _view   = new ChessAIView();
    }

    protected override ChessMove ChooseMove(ChessBoard board) {
        // Populate the view from the current board state.
        _view.Capture(board);

        // Optional: dump desire / proposal / selection breakdown to console.
        if (debugRun) _engine.DebugRun(_view);

        // Run the engine and extract the winning move.
        ActionProposal proposal = _engine.Run(_view);

        if (proposal != null && proposal.TryGetRepresentative<ChessMove>(out ChessMove move))
            return move;

        // No valid proposal (shouldn't happen in a normal game).
        Debug.LogWarning("[ThalamicAI] Engine returned no proposal — using random fallback.");
        return base.ChooseMove(board);
    }
}
