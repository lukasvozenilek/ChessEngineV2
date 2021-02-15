using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class UserInterface : MonoBehaviour
    {
        public ChessBoard chessBoardRef;
        public Button startButton;

        public void Start()
        {
            startButton.onClick.AddListener(OnStartButton);
        }

        public void OnStartButton()
        {
            TestLegalMoves();
        }

        public IEnumerator RunRandomGame()
        {
            Board.Restart();
            List<Move> legalMoves;
            while (true)
            {
                legalMoves = Board.GetAllLegalMoves();
                if (legalMoves.Count == 0) break;
                Board.MakeMove(legalMoves[Random.Range(0, legalMoves.Count)]);
                yield return null;
            }
        }

        public void TestLegalMoves()
        {
            Board.LoadPositionFromFEN("8/3k4/3q4/6b1/8/1R2R3/3K4/8 w - - 0 1");
            chessBoardRef.CreateOverlayFromMoves(Board.GetAllLegalMoves());
        }
    }
    
    
}