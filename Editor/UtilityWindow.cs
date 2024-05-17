#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

public class UtilityWindow : EditorWindow
{
    private Dictionary<string, DataArrayCollection> _datas;

    public class DataArrayCollection
    {
        public bool[] bools;
        public int[] ints;
        public float[] floats;
        public string[] strings;
        public Vector2[] vector2s;
        public Vector3[] vector3s;
        public Quaternion[] quaternions;
        public object[] objects;
    }

    [MenuItem("Window/Utility")]
    private static void Init()
    {
        UtilityWindow editorWindow = (UtilityWindow)GetWindow(typeof(UtilityWindow), false, "Utility");
        editorWindow.Show();
    }
    private void OnGUI()
    {
        initializeData();
        DataArrayCollection data;

        // Get Vertices Center
        GUILayout.Label("* Get Vertices Center", EditorStyles.boldLabel);
        GUILayout.Label("   ※ Select one object.");
        if (GUILayout.Button("Create Center")) GetVerticesCenter();
        EditorGUILayout.Space();

        // Reverse From Pivot
        GUILayout.Label("* Reverse From Pivot", EditorStyles.boldLabel);
        GUILayout.Label("   ※ Set the pivot and select the objects.");

        data = _datas[nameof(ReverseFromPivot)];
        data.objects[0] = (Transform)EditorGUILayout.ObjectField("Pivot", (Transform)data.objects[0], typeof(Transform), true);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Position");
        data.bools[0] = GUILayout.Toggle(data.bools[0], "X");
        data.bools[1] = GUILayout.Toggle(data.bools[1], "Y");
        data.bools[2] = GUILayout.Toggle(data.bools[2], "Z");
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Reverse")) ReverseFromPivot((Transform)data.objects[0], data.bools[0], data.bools[1], data.bools[2]);
        EditorGUILayout.Space();

        // Replace Material By Name
        GUILayout.Label("* Replace Material By Name", EditorStyles.boldLabel);
        GUILayout.Label("   ※ Set the Material and select the objects.");

        data = _datas[nameof(ReplaceMaterialByName)];
        data.objects[0] = (Material)EditorGUILayout.ObjectField("Base Material", (Material)data.objects[0], typeof(Material), true);
        if (GUILayout.Button("Replace")) ReplaceMaterialByName((Material)data.objects[0]);
    }
    private void initializeData()
    {
        if (_datas == null) _datas = new Dictionary<string, DataArrayCollection>();

        // Reverse From Pivot
        if (!_datas.ContainsKey(nameof(ReverseFromPivot)))
        {
            DataArrayCollection data = new();
            _datas.Add(nameof(ReverseFromPivot), data);
            data.objects = new Transform[1];
            data.bools = new bool[] { true, true, true };
        }

        // Replace Material By Name
        if (!_datas.ContainsKey(nameof(ReplaceMaterialByName)))
        {
            DataArrayCollection data = new();
            _datas.Add(nameof(ReplaceMaterialByName), data);
            data.objects = new Material[1];
        }
    }

    //__________________________________________________________________________ Function
    public static void GetVerticesCenter()
    {
        Transform selection = Selection.activeTransform;
        if (selection == null)
        {
            Debug.Log($"{nameof(GetVerticesCenter)} | No object selected.");
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
        Undo.RegisterCreatedObjectUndo(centerObj.gameObject, nameof(GetVerticesCenter));
    }
    public static void ReverseFromPivot(Transform pivot, bool x = true, bool y = true, bool z = true)
    {
        if (pivot == null)
        {
            Debug.Log($"{nameof(ReverseFromPivot)} | No pivot.");
            return;
        }

        GameObject[] selections = Selection.gameObjects;
        if (selections == null || selections.Length == 0)
        {
            Debug.Log($"{nameof(ReverseFromPivot)} | No objects are selected.");
            return;
        }

        Transform[] transforms = Array.ConvertAll(selections, (s) => s.transform);
        Undo.RecordObjects(transforms, nameof(ReverseFromPivot));
        for (int i = 0, l = transforms.Length; i < l; ++i)
        {
            Vector3 dir = (pivot.position - transforms[i].position) * 2f;
            if (x) transforms[i].position += new Vector3(dir.x, 0f, 0f);
            if (y) transforms[i].position += new Vector3(0f, dir.y, 0f);
            if (z) transforms[i].position += new Vector3(0f, 0f, dir.z);
        }
    }
    public static void ReplaceMaterialByName(Material baseMaterial)
    {
        if (baseMaterial == null)
        {
            Debug.Log($"{nameof(ReplaceMaterialByName)} | No Base Material.");
            return;
        }

        GameObject[] selections = Selection.gameObjects;
        if (selections == null || selections.Length == 0)
        {
            Debug.Log($"{nameof(ReplaceMaterialByName)} | No objects are selected.");
            return;
        }

        for (int i = 0, l = selections.Length; i < l; ++i)
        {
            MeshRenderer[] renderers = selections[i].GetComponentsInChildren<MeshRenderer>(true);
            for (int j = 0, l2 = renderers.Length; j < l2; ++j)
            {
                for (int k = 0, l3 = renderers[j].sharedMaterials.Length; k < l3; ++k)
                {
                    if (renderers[j].sharedMaterials[k].name == baseMaterial.name)
                    {
                        Material[] materials = renderers[j].sharedMaterials;
                        materials[k] = baseMaterial;
                        renderers[j].sharedMaterials = materials;
                        Debug.Log($"{nameof(ReplaceMaterialByName)} | [{renderers[j].name}]'s [{baseMaterial.name}] is Replaced.");
                    }
                }
            }
        }
    }
}
#endif