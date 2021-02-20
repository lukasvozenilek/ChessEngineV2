using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Players
{
    public class Stockfish : Player
    {
        public Stockfish(Board board) : base(board)
        {
            
        }

        private Process stockFishProcess;
        public override void PlayMove()
        {
            Debug.Log("Starting StockFish thread...");
            stockFishProcess = new Process();
            
            stockFishProcess.StartInfo.FileName = Application.streamingAssetsPath + "/stockfish_20090216_x64_bmi2.exe";
            stockFishProcess.StartInfo.UseShellExecute = false;
            stockFishProcess.StartInfo.RedirectStandardInput = true;
            stockFishProcess.StartInfo.RedirectStandardOutput = true;
            stockFishProcess.StartInfo.CreateNoWindow = true;
            stockFishProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            stockFishProcess.EnableRaisingEvents = true;
        
            stockFishProcess.Start();
            string FEN = board.GenerateFEN();
            Debug.Log(FEN);
            stockFishProcess.StandardInput.WriteLine("position fen "+ FEN);
        
            // Process for 5 seconds
            string processString = "go movetime 2000";
    
            // Process 20 deep
            // string processString = "go depth 20";

            stockFishProcess.StandardInput.WriteLine(processString);
            Task.Factory.StartNew(() => ReadResult(), TaskCreationOptions.LongRunning);
        }

        private void ReadResult()
        {
            Debug.Log("Wait thread working");
            Thread.Sleep(2100);
            
            Debug.Log("Wait complete");
            string moveLine = "";
            while (true)
            {
                string readLine = stockFishProcess.StandardOutput.ReadLine();
                if (readLine.Contains("bestmove"))
                {
                    moveLine = readLine;
                    break;
                }
            }
            string movestring = moveLine.Split(' ')[1];
            
            stockFishProcess.Close();
            InvokeMoveComplete(board.MakeMove(Constants.StringToMove(movestring)));
        }
    }
}