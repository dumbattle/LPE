public static class Piece {
    public const int None   = 0;
    public const int Pawn   = 1;
    public const int Knight = 2;
    public const int Bishop = 3;
    public const int Rook   = 4;
    public const int Queen  = 5;
    public const int King   = 6;

    public const int White = 8;
    public const int Black = 16;

    public static int Color(int piece)    => piece & 0b11000;
    public static int Type(int piece)     => piece & 0b00111;

    public static bool IsColor(int piece, int color) => piece != None && (piece & 0b11000) == color;
    public static bool IsWhite(int piece) => IsColor(piece, White);
    public static bool IsBlack(int piece) => IsColor(piece, Black);

    public static int Opponent(int color) => color == White ? Black : White;

    public static bool IsSlider(int piece) {
        int t = Type(piece);
        return t == Bishop || t == Rook || t == Queen;
    }

    public static string TypeName(int piece) {
        return Type(piece) switch {
            Pawn   => "Pawn",
            Knight => "Knight",
            Bishop => "Bishop",
            Rook   => "Rook",
            Queen  => "Queen",
            King   => "King",
            _      => "None"
        };
    }
}
