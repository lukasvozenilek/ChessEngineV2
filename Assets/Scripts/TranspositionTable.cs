
using UnityEngine;

public class TranspositionTable
{
    public struct TranspositionEntry
    {
        public ulong key;
        public byte depth;
        public byte flags;
        public float value;

        public TranspositionEntry(ulong key, int depth, int flags, float value)
        {
            this.key = key;
            this.depth = (byte)depth;
            this.flags = (byte)flags;
            this.value = value;
        }
    }

    public const int FLAG_EXACT = 0;
    public const int FLAG_ALPHA = 1;
    public const int FLAG_BETA = 2;
    public const int NORESULT = int.MinValue;

    public Board board;
    public ulong size;

    public TranspositionEntry[] table;

    public TranspositionTable(Board board, ulong size)
    {
        this.board = board;
        this.size = size;
        table = new TranspositionEntry[size];
    }

    public void Clear()
    {
        table = new TranspositionEntry[size];
    }

    public void StorePosition(ulong key, float eval, int flag, int depth)
    {
        TranspositionEntry entry = new TranspositionEntry(key, depth, flag, eval);
        table[key % size] = entry;
    }

    public bool ContainsPosition(ulong key)
    {
        TranspositionEntry entry = table[key % size];
        return entry.key == key;
    }

    public float GetEvaluation(ulong key, int depth, float alpha, float beta)
    {
        TranspositionEntry entry = table[key % size];
        if (entry.key == key && depth >= entry.depth)
        {
            switch (entry.flags)
            {
                //If we found an exact position, return the evaluation.
                case FLAG_EXACT:
                    return entry.value;
                
                //If this was an alpha evaluation, use the current alpha.
                case FLAG_ALPHA:
                    if (entry.value <= alpha)
                        return alpha;
                    break;

                //If this was an beta evaluation, use the current beta.
                case FLAG_BETA:
                    if (entry.value >= beta)
                        return beta;
                    break;
            }
            return entry.value;
        }
        return NORESULT;
    }
}
