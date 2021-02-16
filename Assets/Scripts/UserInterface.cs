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
            
        }
    }
    
    
}