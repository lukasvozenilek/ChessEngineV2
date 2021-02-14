using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "Board Config", menuName = "NEW BOARD CONFIG", order = 0)]
    public class BoardConfig : ScriptableObject
    {
        [Header("Misc")]
        public Color whiteColor;
        public Color blackColor;

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
    }
}