
using UnityEngine;

public class TranspositionTable
{
    public struct TranspositionEntry
    {
        public ulong key;
        public int depth;
        public int flags;
        public float value;
        public Move best;

        public TranspositionEntry(ulong key, int depth, int flags, int value, Move best)
        {
            this.key = key;
            this.depth = depth;
            this.flags = flags;
            this.value = value;
            this.best = best;
        }
    }

    public const int FLAG_EXACT = 0;
    public const int FLAG_ALPHA = 1;
    public const int FLAG_BETA = 2;
    public const int NORESULT = -9999;

    public Board board;
    public ulong size;

    public TranspositionEntry[] table;

    public TranspositionTable(Board board, ulong size)
    {
        this.board = board;
        this.size = size;
        table = new TranspositionEntry[size];
    }

    public void StorePosition(ulong key, float eval, int flag, int depth)
    {
        TranspositionEntry entry = new TranspositionEntry();
        entry.key = key;
        entry.value = eval;
        entry.flags = flag;
        entry.depth = depth;
        table[key % size] = entry;
    }

    public float GetEvaluation(ulong key)
    {
        TranspositionEntry entry = table[key % size];
        if (entry.key == key)
        {
            return entry.value;
        }
        return NORESULT;
    }
}
