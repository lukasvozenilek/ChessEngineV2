using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "Board Config", menuName = "NEW BOARD CONFIG", order = 0)]
    public class BoardConfig : ScriptableObject
    {
        [Header("Colors")]
        public Color whiteColor;
        public Color blackColor;

        [Header("Audio")] 
        public AudioClip captureSound;
        public AudioClip moveSound;
        public AudioClip checkSound;
        public AudioClip castleSound;

        [Header("White Pieces")]
        public Sprite whiteKing;
        public Sprite whiteQueen;
        public Sprite whiteBishop;
        public Sprite whiteKnight;
        public Sprite whitePawn;
        public Sprite whiteRook;
        
        [Header("Black Pieces")]
        public Sprite blackKing;
        public Sprite blackQueen;
        public Sprite blackBishop;
        public Sprite blackKnight;
        public Sprite blackPawn;
        public Sprite blackRook;


        [Header("Position Tables")] 
        public PositionWeightTable pawnTable;
        public PositionWeightTable rookTable;
        public PositionWeightTable kingTable;
        public PositionWeightTable queenTable;
        public PositionWeightTable knightTable;
        public PositionWeightTable bishopTable;
    }
}