using UnityEngine;
using UnityEditor;

namespace Game.Editor
{
    [CustomEditor(typeof(GridBaseGenerator))]
    public class GridBaseGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GridBaseGenerator gridBaseGenerator = (GridBaseGenerator)target;

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate Grid Base"))
            {
                gridBaseGenerator.GenerateGridBase();
            }

            if (GUILayout.Button("Clear Grid Base"))
            {
                gridBaseGenerator.ClearGridBase();
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Update Start Position"))
            {
                gridBaseGenerator.SetCurrentPositionToStartPosition();
            }
        }
    }
}
