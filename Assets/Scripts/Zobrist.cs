
using System;
using System.Collections.Generic;

public class Zobrist
{
    public ulong[][] ZobristTable = new ulong[64][];
    private Random random;
    
    public Zobrist()
    {
        random = new Random();
        for (int i = 0; i < 64; i++)
        {
            ZobristTable[i] = new ulong[12];
            for (int j = 0; j < 12; j++)
            {
                ZobristTable[i][j] = RandomLong();
            }
        }
    }

    public ulong HashPosition(Board board)
    {
        ulong hash = 0;
        foreach (KeyValuePair<int,int> piece in board.whitePieces)
        {
            hash ^= HashPiece(piece.Key, piece.Value);
        }
        foreach (KeyValuePair<int,int> piece in board.blackPieces)
        {
            hash ^= HashPiece(piece.Key, piece.Value);
        }
        return hash;
    }

    public ulong HashPiece(int square, int piece)
    {
        int pieceType = Piece.GetType(piece);
        if (pieceType == 0) return 0;
        return ZobristTable[square][Piece.GetPieceColor(piece) ? 5 + pieceType : pieceType-1];
    }
    
    ulong RandomLong() {
        byte[] buf = new byte[8];
        random.NextBytes(buf);
        ulong longRand = BitConverter.ToUInt64(buf, 0);

        return longRand;
    }
}