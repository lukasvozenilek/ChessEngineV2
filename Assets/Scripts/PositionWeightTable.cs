using UnityEngine;

[CreateAssetMenu(fileName = "Knight Position Table", menuName = "Create Position Table", order = 0)]
public class PositionWeightTable : ScriptableObject
{
    public float[] table = new float[64];
}
