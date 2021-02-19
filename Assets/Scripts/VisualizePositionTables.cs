using System.Collections.Generic;
using UnityEngine;

public class VisualizePositionTables : MonoBehaviour
{
    public PositionWeightTable positionTable;
    public Grid gridComponent;
    public GameObject overlayPrefab;
    
    [Header("Colors")]
    public float upperRange;
    public Color upperColor;
    public float lowerRange;
    public Color lowerColor;
    
    [HideInInspector]
    public List<GameObject> allObjects = new List<GameObject>();
}
