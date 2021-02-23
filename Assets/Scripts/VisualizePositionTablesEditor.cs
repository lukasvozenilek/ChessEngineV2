using DefaultNamespace;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(VisualizePositionTables))]
public class VisualizePositionTablesEditor : Editor
{
    private VisualizePositionTables monoRef;
    
    private SerializedProperty table;
    
    void OnEnable()
    {
        monoRef = target as VisualizePositionTables;
        table = serializedObject.FindProperty("positionTable");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Enable Visualization"))
        {
            DestroyAllObjects();
            for (int i = 0; i < 64; i++)
            {
                GameObject GO = Instantiate(monoRef.overlayPrefab);
                Overlay overlay = GO.GetComponent<Overlay>();
                overlay.labelRef.gameObject.SetActive(true);
                overlay.labelRef.text = i.ToString();
                GO.transform.position = monoRef.gridComponent.CellToWorld(new Vector3Int(i%8, i/8, 1)) + monoRef.gridComponent.cellSize/2;
                GO.transform.position = new Vector3(GO.transform.position.x, GO.transform.position.y, -1);
                monoRef.allObjects.Add(GO);
            }
            
        }

        if (monoRef.allObjects.Count == 64)
        {
            for (int i = 0; i < 64; i++)
            {
                float scaledValue = (monoRef.positionTable.table[i] - monoRef.lowerRange) / (monoRef.upperRange - monoRef.lowerRange);
                SpriteRenderer spriteRef = monoRef.allObjects[i].GetComponent<Overlay>().spriteRef;
                spriteRef.color = Color.Lerp(monoRef.lowerColor, monoRef.upperColor, scaledValue);
            }
        }

        if (GUILayout.Button("Disable Visualization"))
        {
            DestroyAllObjects();
        }
    }

    private void DestroyAllObjects()
    {
        foreach (GameObject GO in monoRef.allObjects)
        {
            DestroyImmediate(GO);
        }
        monoRef.allObjects.Clear();
    }
}
#endif
