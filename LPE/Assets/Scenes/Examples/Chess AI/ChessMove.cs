public readonly struct ChessMove {
    public readonly int  From;
    public readonly int  To;
    public readonly int  PromotionPieceType; // Piece type constant, 0 = none
    public readonly Flag MoveFlag;

    public enum Flag : byte {
        None            = 0,
        EnPassant       = 1,
        CastleKingside  = 2,
        CastleQueenside = 3,
        Promotion       = 4,
    }

    public bool IsPromotion => MoveFlag == Flag.Promotion;
    public bool IsEnPassant => MoveFlag == Flag.EnPassant;
    public bool IsCastle    => MoveFlag == Flag.CastleKingside || MoveFlag == Flag.CastleQueenside;
    public bool IsValid     => From >= 0 && To >= 0;

    public ChessMove(int from, int to, Flag flag = Flag.None, int promotionPieceType = 0) {
        From               = from;
        To                 = to;
        MoveFlag           = flag;
        PromotionPieceType = promotionPieceType;
    }

    public static readonly ChessMove Invalid = new ChessMove(-1, -1);

    public override string ToString() {
        if (!IsValid) return "invalid";
        const string files = "abcdefgh";
        string s = $"{files[From % 8]}{From / 8 + 1}{files[To % 8]}{To / 8 + 1}";
        if (IsPromotion) s += Piece.TypeName(PromotionPieceType)[0].ToString().ToLower();
        return s;
    }
}
