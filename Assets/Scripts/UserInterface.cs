using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class UserInterface : MonoBehaviour
    {
        public ChessBoard chessBoardRef;

        [Header("PERFT")] public TMP_Dropdown perftConfigDropdown;
        public Button runPERFTButton;
        public TMP_InputField perftDepthInput;

        [Header("New Game")] public Button newGameButton;
        public TMP_Dropdown player1dropdown;
        public TMP_Dropdown player2dropdown;
        public TMP_InputField FENInput;
        private PERFT perft;


        public GameObject canvasComponent;
        public void Start()
        {
            canvasComponent.SetActive(true);
            runPERFTButton.onClick.AddListener(RunPERFT);
            newGameButton.onClick.AddListener(StartNewGame);
         
            perft = new PERFT();
        }

        public void StartNewGame()
        {
            GameConfiguration config = new GameConfiguration((PlayerType)player1dropdown.value, (PlayerType)player2dropdown.value);
            config.startingFEN = String.IsNullOrEmpty(FENInput.text)? Constants.startingFEN : FENInput.text ;
            chessBoardRef.StartNewGame(config);
        }

        public void RunPERFT()
        {
            PERFTConfig config1 = new PERFTConfig();
            config1.FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            config1.requirements = new List<long> {1, 20, 400, 8902, 197281, 4865609};
            config1.depth = int.Parse(perftDepthInput.text);

            PERFTConfig config2 = new PERFTConfig();
            config2.FEN = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
            config2.requirements = new List<long> {1, 48, 2039, 97862, 4085603, 193690690};
            config2.depth = int.Parse(perftDepthInput.text);

            PERFTConfig config3 = new PERFTConfig();
            config3.FEN = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -";
            config3.requirements = new List<long> {1, 14, 191, 2812, 43238, 674624};
            config3.depth = int.Parse(perftDepthInput.text);

            PERFTConfig[] configs = new[]
            {
                config1,
                config2,
                config3
            };

            perft.RUN_PERFT(configs[perftConfigDropdown.value]);
        }
    }
}