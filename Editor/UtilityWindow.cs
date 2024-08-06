#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
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
        /// <see cref="GetVerticesCenter"/>
        GUILayout.Label("* Get Vertices Center", EditorStyles.boldLabel);
        GUILayout.Label("   『 Select one object.");
        if (GUILayout.Button("Create Center")) GetVerticesCenter();
        EditorGUILayout.Space();

        /// <see cref="ReverseFromPivot"/>
        GUILayout.Label("* Reverse from Pivot", EditorStyles.boldLabel);
        GUILayout.Label("   『 Set the pivot and select the objects.");

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

        /// <see cref="ReplaceMaterialByName"/>
        GUILayout.Label("* Replace Material by Name", EditorStyles.boldLabel);
        GUILayout.Label("   『 Set the Material and select the objects.");

        data = _datas[nameof(ReplaceMaterialByName)];
        data.objects[0] = (Material)EditorGUILayout.ObjectField("Base Material", (Material)data.objects[0], typeof(Material), true);
        if (GUILayout.Button("Replace")) ReplaceMaterialByName((Material)data.objects[0]);
        EditorGUILayout.Space();

        /// <see cref="SetLineRendererPositions"/>
        GUILayout.Label("* Set Line Renderer Positions", EditorStyles.boldLabel);
        GUILayout.Label("   『 Set up the LineRenderer and positions.");

        data = _datas[nameof(SetLineRendererPositions)];
        data.objects[0] = (LineRenderer)EditorGUILayout.ObjectField("Line Renderer", (LineRenderer)data.objects[0], typeof(LineRenderer), true);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+"))
            Array.Resize(ref data.objects, data.objects.Length + 1);
        if (GUILayout.Button("-") && data.objects.Length > 1)
            Array.Resize(ref data.objects, data.objects.Length - 1);
        EditorGUILayout.EndHorizontal();

        for (int i = 1, l = data.objects.Length; i < l; ++i)
            data.objects[i] = (Transform)EditorGUILayout.ObjectField($"Position {i}", (Transform)data.objects[i], typeof(Transform), true);

        if (GUILayout.Button("Set Positions"))
            SetLineRendererPositions(data.objects[0] as LineRenderer, data.objects.Skip(1).Select(obj => obj != null ? (Transform)obj : null).ToArray());
    }
    private void initializeData()
    {
        if (_datas == null) _datas = new Dictionary<string, DataArrayCollection>();

        /// <see cref="ReverseFromPivot"/>
        if (!_datas.ContainsKey(nameof(ReverseFromPivot)))
        {
            DataArrayCollection data = new();
            _datas.Add(nameof(ReverseFromPivot), data);
            data.objects = new Transform[1];
            data.bools = new bool[] { true, true, true };
        }

        /// <see cref="ReplaceMaterialByName"/>
        if (!_datas.ContainsKey(nameof(ReplaceMaterialByName)))
        {
            DataArrayCollection data = new();
            _datas.Add(nameof(ReplaceMaterialByName), data);
            data.objects = new Material[1];
        }

        /// <see cref="SetLineRendererPositions"/>
        if (!_datas.ContainsKey(nameof(SetLineRendererPositions)))
        {
            DataArrayCollection data = new();
            _datas.Add(nameof(SetLineRendererPositions), data);
            data.objects = new LineRenderer[1];
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
    public static void SetLineRendererPositions(LineRenderer renderer, Transform[] transforms)
    {
        if (renderer == null)
        {
            Debug.Log($"{nameof(SetLineRendererPositions)} | No LineRenderer.");
            return;
        }
        if (transforms == null)
        {
            Debug.Log($"{nameof(SetLineRendererPositions)} | No Transforms.");
            return;
        }
        if (transforms.Contains(null))
        {
            Debug.Log($"{nameof(SetLineRendererPositions)} | There is null.");
            return;
        }
        renderer.SetPositions(transforms.Select(tr => renderer.useWorldSpace ? tr.position : renderer.transform.InverseTransformPoint(tr.position)).ToArray());
    }
}
#endif