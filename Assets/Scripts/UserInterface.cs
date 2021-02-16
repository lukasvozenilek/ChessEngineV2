using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class UserInterface : MonoBehaviour
    {
        public ChessBoard chessBoardRef;
        public Button runPERFT;
        public TMP_InputField PERFTDepth;

        public void Start()
        {
            runPERFT.onClick.AddListener(RunPERFT);
            
            Board board = new Board();
            chessBoardRef.InitializeBoard(board);
        }

        public void RunPERFT()
        {
            PERFTConfig config1 = new PERFTConfig();
            config1.FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            config1.requirements = new List<long>{1, 20, 400, 8902, 197281, 4865609};
            config1.depth = int.Parse(PERFTDepth.text);
            PERFT.RUN_PERFT(config1);
        }
    }
    
    
}