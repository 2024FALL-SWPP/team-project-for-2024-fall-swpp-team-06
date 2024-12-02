using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ThumbnailCreator))]
public class ThumbnailCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ThumbnailCreator creator = (ThumbnailCreator)target;
        if (GUILayout.Button("Generate Entity Icons"))
        {
            creator.generateEntityIcons();
        }
    }
}
