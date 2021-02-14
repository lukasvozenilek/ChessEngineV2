using System;
using UnityEngine;

public static class GameState
{
    public static Camera MainCamera;
    public static bool BlackPerspective;
    public static event Action UpdateBoardEvent;

    public static void UpdateBoard()
    {
        UpdateBoardEvent?.Invoke();
    }
}