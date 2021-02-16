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
            
            Board board = new Board("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -");
            chessBoardRef.InitializeBoard(board);
        }

        public void RunPERFT()
        {
            PERFTConfig config1 = new PERFTConfig();
            config1.FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            config1.requirements = new List<long>{1, 20, 400, 8902, 197281, 4865609};
            config1.depth = int.Parse(PERFTDepth.text);

            PERFTConfig config2 = new PERFTConfig();
            config2.FEN = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
            config2.requirements = new List<long>{1, 48, 2039, 97862, 4085603, 193690690};
            config2.depth = int.Parse(PERFTDepth.text);
            
            PERFTConfig config3 = new PERFTConfig();
            config3.FEN = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -";
            config3.requirements = new List<long>{1, 14, 191, 2812, 43238, 674624};
            config3.depth = int.Parse(PERFTDepth.text);
            
            
            PERFT.RUN_PERFT(config3);
        }
    }
    
    
}