public static class Piece
{
    //Piece ID's
    public const int None = 0;
    public const int King = 1;
    public const int Queen = 2;
    public const int Bishop = 3;
    public const int Knight = 4;
    public const int Rook = 5;
    public const int Pawn = 6;
    
    //Colors
    public const int White = 8;
    public const int Black = 16;

    public static bool IsType(int piece, int pieceType) 
    {
        //Color is: 0b1000, Mask is 0b111
        return (piece & 0b111) == pieceType;
    }

    public static int GetType(int piece)
    {
        return piece & 0b111;
    }
}