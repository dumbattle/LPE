using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main chess game controller. Manages board rendering, game loop, and features
/// like undo and flip-board. Attach to a GameObject in the scene; assign player
/// components and piece sprites in the Inspector.
///
/// Keyboard shortcuts (in Play mode):
///   U      – Undo last full move pair
///   F      – Flip board
///   R      – Restart game
/// </summary>
public class ChessMain : MonoBehaviour {

    // ── Piece Sprites ────────────────────────────────────────────────────────

    [Header("White Pieces")]
    public Sprite whitePawn;
    public Sprite whiteKnight;
    public Sprite whiteBishop;
    public Sprite whiteRook;
    public Sprite whiteQueen;
    public Sprite whiteKing;

    [Header("Black Pieces")]
    public Sprite blackPawn;
    public Sprite blackKnight;
    public Sprite blackBishop;
    public Sprite blackRook;
    public Sprite blackQueen;
    public Sprite blackKing;

    [Header("Board Sprites")]
    [Tooltip("Optional sprite for light squares. Leave null to use solid colour.")]
    public Sprite lightSquareSprite;
    [Tooltip("Optional sprite for dark squares. Leave null to use solid colour.")]
    public Sprite darkSquareSprite;

    [Header("Colours (used when sprites are null)")]
    public Color lightSquareColor  = new Color(0.93f, 0.85f, 0.72f);
    public Color darkSquareColor   = new Color(0.71f, 0.53f, 0.39f);
    public Color highlightColor    = new Color(1f,  1f,  0f,  0.45f);
    public Color selectionColor    = new Color(0f,  1f,  0f,  0.55f);
    public Color legalMoveColor    = new Color(0f,  0f,  0f,  0.25f);
    public Color lastMoveColor     = new Color(1f,  0.84f, 0f, 0.40f);
    public Color checkColor        = new Color(1f,  0f,  0f,  0.55f);

    // ── Players ──────────────────────────────────────────────────────────────

    [Header("Players")]
    [Tooltip("Assign a HumanChessPlayer or AIChessPlayer component.")]
    public ChessPlayer whitePlayer;
    [Tooltip("Assign a HumanChessPlayer or AIChessPlayer component.")]
    public ChessPlayer blackPlayer;

    // ── Settings ─────────────────────────────────────────────────────────────

    [Header("Settings")]
    public float squareSize = 1f;

    // ── Internal State ───────────────────────────────────────────────────────

    private ChessBoard  _board;
    private bool        _boardFlipped;
    private Coroutine   _gameCoroutine;
    private bool        _gameOver;
    private string      _statusMessage = "";

    // Visual objects
    private SpriteRenderer[,] _squareRenderers  = new SpriteRenderer[8, 8];
    private SpriteRenderer[,] _highlightOverlay = new SpriteRenderer[8, 8];
    private SpriteRenderer[,] _dotOverlay       = new SpriteRenderer[8, 8];
    private SpriteRenderer[,] _pieceRenderers   = new SpriteRenderer[8, 8];

    // ── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Start() {
        _board = new ChessBoard();
        _board.SetupStartPosition();
        BuildBoardVisuals();
        InitializePlayers();
        StartNewGame();
    }

    private void Update() {
        HandleKeyboard();
    }

    // ── Initialization ───────────────────────────────────────────────────────

    private void InitializePlayers() {
        if (whitePlayer != null) whitePlayer.Initialize(this, Piece.White);
        if (blackPlayer != null) blackPlayer.Initialize(this, Piece.Black);
    }

    private void BuildBoardVisuals() {
        var squaresRoot    = new GameObject("Squares");
        var highlightsRoot = new GameObject("Highlights");
        var dotsRoot       = new GameObject("Dots");
        var piecesRoot     = new GameObject("Pieces");

        squaresRoot.transform.SetParent(transform, false);
        highlightsRoot.transform.SetParent(transform, false);
        dotsRoot.transform.SetParent(transform, false);
        piecesRoot.transform.SetParent(transform, false);

        for (int rank = 0; rank < 8; rank++) {
            for (int file = 0; file < 8; file++) {
                bool isLight = (file + rank) % 2 == 0;

                // ── Square ──────────────────────────────────────────────────
                var sqGO = new GameObject($"Sq_{(char)('a'+file)}{rank+1}");
                sqGO.transform.SetParent(squaresRoot.transform, false);
                var sqSR = sqGO.AddComponent<SpriteRenderer>();
                sqSR.sortingOrder = 0;

                if (isLight && lightSquareSprite != null) sqSR.sprite = lightSquareSprite;
                else if (!isLight && darkSquareSprite != null) sqSR.sprite = darkSquareSprite;
                else sqSR.sprite = CreateSolidSprite();

                sqSR.color = isLight ? lightSquareColor : darkSquareColor;
                sqSR.size  = new Vector2(squareSize, squareSize);
                if (sqSR.drawMode == SpriteDrawMode.Simple) sqSR.transform.localScale = Vector3.one * squareSize;

                _squareRenderers[file, rank] = sqSR;

                // ── Highlight overlay ────────────────────────────────────────
                var hlGO = new GameObject("HL");
                hlGO.transform.SetParent(highlightsRoot.transform, false);
                var hlSR = hlGO.AddComponent<SpriteRenderer>();
                hlSR.sprite       = CreateSolidSprite();
                hlSR.sortingOrder = 1;
                hlSR.color        = Color.clear;
                hlSR.transform.localScale = Vector3.one * squareSize;
                _highlightOverlay[file, rank] = hlSR;

                // ── Dot overlay (legal move indicator) ───────────────────────
                var dotGO = new GameObject("Dot");
                dotGO.transform.SetParent(dotsRoot.transform, false);
                var dotSR = dotGO.AddComponent<SpriteRenderer>();
                dotSR.sprite       = CreateCircleSprite();
                dotSR.sortingOrder = 2;
                dotSR.color        = Color.clear;
                dotSR.transform.localScale = Vector3.one * squareSize * 0.35f;
                _dotOverlay[file, rank] = dotSR;

                // ── Piece ────────────────────────────────────────────────────
                var pieceGO = new GameObject("Piece");
                pieceGO.transform.SetParent(piecesRoot.transform, false);
                var pieceSR = pieceGO.AddComponent<SpriteRenderer>();
                pieceSR.sortingOrder = 3;
                _pieceRenderers[file, rank] = pieceSR;
            }
        }

        RefreshPositions();
        RefreshPieces();
    }

    // ── Game Loop ────────────────────────────────────────────────────────────

    private void StartNewGame() {
        _gameOver = false;
        StopGameCoroutine();
        _gameCoroutine = StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop() {
        while (true) {
            RefreshPieces();
            RefreshHighlights();

            var result = _board.GetGameResult();
            if (result != ChessBoard.GameResult.Ongoing) {
                _gameOver = true;
                _statusMessage = result switch {
                    ChessBoard.GameResult.WhiteWins => "Checkmate – White wins!",
                    ChessBoard.GameResult.BlackWins => "Checkmate – Black wins!",
                    ChessBoard.GameResult.Draw      => "Draw!",
                    _                               => ""
                };
                yield break;
            }

            _statusMessage = (_board.IsWhiteToMove ? "White" : "Black") + " to move"
                           + (_board.IsInCheck() ? " (Check!)" : "");

            ChessPlayer current = _board.IsWhiteToMove ? whitePlayer : blackPlayer;

            if (current == null) {
                Debug.LogWarning($"No player assigned for {(_board.IsWhiteToMove ? "White" : "Black")}");
                yield break;
            }

            ChessMove chosenMove = ChessMove.Invalid;
            bool      moveReady  = false;

            yield return current.TakeTurn(_board, m => { chosenMove = m; moveReady = true; });

            if (moveReady && chosenMove.IsValid) {
                _board.MakeMove(chosenMove);
            }
        }
    }

    private void StopGameCoroutine() {
        if (_gameCoroutine == null) return;
        StopCoroutine(_gameCoroutine);
        _gameCoroutine = null;
        whitePlayer?.Cancel();
        blackPlayer?.Cancel();
    }

    // ── Keyboard Shortcuts ───────────────────────────────────────────────────

    private void HandleKeyboard() {
        if (Input.GetKeyDown(KeyCode.U)) RequestUndo();
        if (Input.GetKeyDown(KeyCode.F)) FlipBoard();
        if (Input.GetKeyDown(KeyCode.R)) Restart();
    }

    // ── Public Actions ────────────────────────────────────────────────────────

    /// <summary>Undoes the last move pair (AI + Human) and restarts the turn.</summary>
    public void RequestUndo() {
        StopGameCoroutine();

        // Undo until it's a human player's turn, or up to 2 moves
        int undone = 0;
        while (_board.CanUndo() && undone < 2) {
            _board.UndoMove();
            undone++;
            // Stop if the player for the resulting turn is human
            ChessPlayer next = _board.IsWhiteToMove ? whitePlayer : blackPlayer;
            if (next is HumanChessPlayer) break;
        }

        ClearHighlights();
        RefreshPieces();
        RefreshHighlights();
        _gameCoroutine = StartCoroutine(GameLoop());
    }

    /// <summary>Flips the board so black is at the bottom.</summary>
    public void FlipBoard() {
        _boardFlipped = !_boardFlipped;
        RefreshPositions();
        RefreshPieces();
        RefreshHighlights();
    }

    /// <summary>Resets to the start position and begins a new game.</summary>
    public void Restart() {
        _board.SetupStartPosition();
        _boardFlipped  = false;
        _statusMessage = "";
        ClearHighlights();
        StartNewGame();
    }

    // ── Visual API (called by players) ───────────────────────────────────────

    /// <summary>Shows which square is selected and highlights legal move targets.</summary>
    public void ShowSelection(int selectedSquare, List<ChessMove> legalMoves) {
        ClearHighlights();

        SetOverlay(_highlightOverlay, selectedSquare, selectionColor);

        // Last move ghost
        if (_board.LastMove.IsValid) {
            SetOverlay(_highlightOverlay, _board.LastMove.From, lastMoveColor);
            SetOverlay(_highlightOverlay, _board.LastMove.To,   lastMoveColor);
        }

        foreach (var m in legalMoves) {
            bool isCapture = _board.GetPiece(m.To) != Piece.None || m.IsEnPassant;
            if (isCapture)
                SetOverlay(_highlightOverlay, m.To, legalMoveColor * new Color(1, 1, 1, 1.8f));
            else
                SetOverlay(_dotOverlay, m.To, legalMoveColor);
        }
    }

    /// <summary>Clears all highlight and dot overlays.</summary>
    public void ClearHighlights() {
        for (int r = 0; r < 8; r++)
            for (int f = 0; f < 8; f++) {
                _highlightOverlay[f, r].color = Color.clear;
                _dotOverlay[f, r].color       = Color.clear;
            }
    }

    /// <summary>Returns the board square (0–63) under the mouse cursor, or -1 if outside the board.</summary>
    public int GetSquareAtMouse() {
        if (Camera.main == null) return -1;
        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0;
        return WorldToSquare(world);
    }

    // ── Rendering Helpers ─────────────────────────────────────────────────────

    private void RefreshPositions() {
        for (int rank = 0; rank < 8; rank++) {
            for (int file = 0; file < 8; file++) {
                Vector3 pos = SquareToWorld(rank * 8 + file);
                _squareRenderers [file, rank].transform.position = pos;
                _highlightOverlay[file, rank].transform.position = pos;
                _dotOverlay      [file, rank].transform.position = pos;
                _pieceRenderers  [file, rank].transform.position = pos;
            }
        }
    }

    private void RefreshPieces() {
        for (int rank = 0; rank < 8; rank++) {
            for (int file = 0; file < 8; file++) {
                int piece = _board.GetPiece(file, rank);
                var sr    = _pieceRenderers[file, rank];
                sr.sprite = GetPieceSprite(piece);
                sr.color  = Color.white;

                // Auto-scale piece to fill the square based on sprite size
                if (sr.sprite != null) {
                    float spriteUnits = sr.sprite.bounds.size.y; // assumes square sprite
                    if (spriteUnits > 0f)
                        sr.transform.localScale = Vector3.one * (squareSize / spriteUnits) * 0.7f;
                } else {
                    sr.transform.localScale = Vector3.one;
                }
            }
        }
    }

    private void RefreshHighlights() {
        ClearHighlights();
        if (!_board.LastMove.IsValid) return;

        SetOverlay(_highlightOverlay, _board.LastMove.From, lastMoveColor);
        SetOverlay(_highlightOverlay, _board.LastMove.To,   lastMoveColor);

        if (_board.IsInCheck()) {
            int kingSq = -1;
            int kingColor = _board.IsWhiteToMove ? Piece.White : Piece.Black;
            for (int sq = 0; sq < 64; sq++)
                if (_board.GetPiece(sq) == (kingColor | Piece.King)) { kingSq = sq; break; }
            if (kingSq >= 0) SetOverlay(_highlightOverlay, kingSq, checkColor);
        }
    }

    private void SetOverlay(SpriteRenderer[,] grid, int square, Color color) {
        if (square < 0 || square >= 64) return;
        int f = square % 8;
        int r = square / 8;
        grid[f, r].color = color;
    }

    // ── Coordinate Conversion ─────────────────────────────────────────────────

    private Vector3 SquareToWorld(int square) {
        int file = square % 8;
        int rank = square / 8;
        if (_boardFlipped) { file = 7 - file; rank = 7 - rank; }
        return transform.position + new Vector3(
            (file - 3.5f) * squareSize,
            (rank - 3.5f) * squareSize,
            0f);
    }

    private int WorldToSquare(Vector3 worldPos) {
        Vector3 local = worldPos - transform.position;
        int file = Mathf.FloorToInt(local.x / squareSize + 4f);
        int rank = Mathf.FloorToInt(local.y / squareSize + 4f);
        if (_boardFlipped) { file = 7 - file; rank = 7 - rank; }
        if (file < 0 || file > 7 || rank < 0 || rank > 7) return -1;
        return rank * 8 + file;
    }

    // ── Sprite Lookup ─────────────────────────────────────────────────────────

    private Sprite GetPieceSprite(int piece) {
        if (piece == Piece.None) return null;
        bool white = Piece.IsWhite(piece);
        return Piece.Type(piece) switch {
            Piece.Pawn   => white ? whitePawn   : blackPawn,
            Piece.Knight => white ? whiteKnight : blackKnight,
            Piece.Bishop => white ? whiteBishop : blackBishop,
            Piece.Rook   => white ? whiteRook   : blackRook,
            Piece.Queen  => white ? whiteQueen  : blackQueen,
            Piece.King   => white ? whiteKing   : blackKing,
            _            => null
        };
    }

    // ── Sprite Factories ─────────────────────────────────────────────────────

    private static Sprite _solidSprite;
    private static Sprite CreateSolidSprite() {
        if (_solidSprite != null) return _solidSprite;
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        _solidSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return _solidSprite;
    }

    private static Sprite _circleSprite;
    private static Sprite CreateCircleSprite() {
        if (_circleSprite != null) return _circleSprite;
        int   size = 64;
        var   tex  = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float half = size / 2f;
        float r2   = (half - 1) * (half - 1);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++) {
                float dx = x - half + 0.5f;
                float dy = y - half + 0.5f;
                tex.SetPixel(x, y, dx * dx + dy * dy <= r2 ? Color.white : Color.clear);
            }
        tex.Apply();
        _circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return _circleSprite;
    }

    // ── OnGUI Status Overlay ──────────────────────────────────────────────────

    private void OnGUI() {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label(_statusMessage);
        if (!_gameOver) {
            if (GUILayout.Button("Undo (U)"))    RequestUndo();
            if (GUILayout.Button("Flip (F)"))    FlipBoard();
            if (GUILayout.Button("Restart (R)")) Restart();
        } else {
            if (GUILayout.Button("New Game (R)")) Restart();
        }
        GUILayout.EndArea();
    }
}
