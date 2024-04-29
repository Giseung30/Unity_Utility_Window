#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class UtilityWindow : EditorWindow
{
    [MenuItem("Window/Utility")]
    private static void Init()
    {
        UtilityWindow editorWindow = (UtilityWindow)GetWindow(typeof(UtilityWindow), false, "Utility");
        editorWindow.Show();
    }
    private void OnGUI()
    {
        // Get Vertices Center
        EditorGUILayout.LabelField("* Get Vertices Center");
        if (GUILayout.Button("Create Center")) GetVerticesCenter();
    }

    //__________________________________________________________________________ Function
    public static void GetVerticesCenter()
    {
        Transform selection = Selection.activeTransform;
        if (selection == null)
        {
            Debug.Log($"{nameof(GetVerticesCenter)} | 선택된 오브젝트가 없습니다.");
            return;
        }

        MeshFilter[] filter = selection.GetComponentsInChildren<MeshFilter>();
        Vector3 center = Vector3.zero;

        if (filter.Length != 0)
        {
            for (int i = 0, l = filter.Length; i < l; ++i)
            {
                Mesh mesh = filter[i].sharedMesh;
                Vector3[] vertices = mesh.vertices;

                if (vertices.Length != 0)
                {
                    Vector3 vertCenter = Vector3.zero;
                    for (int j = 0, l2 = vertices.Length; j < l2; j++)
                        vertCenter += filter[i].transform.TransformPoint(vertices[j]);
                    vertCenter /= vertices.Length;

                    center += vertCenter;
                }
            }
            center /= filter.Length;
        }

        Transform centerObj = (new GameObject("Center")).transform;
        centerObj.position = center;
        centerObj.SetParent(selection.parent, true);
        centerObj.SetSiblingIndex(selection.GetSiblingIndex() + 1);
        Selection.activeTransform = centerObj;
        Undo.RegisterCreatedObjectUndo(centerObj.gameObject, "Create Center");
    }
}
#endif