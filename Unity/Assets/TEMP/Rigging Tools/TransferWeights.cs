using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class WeightTransferWindow : EditorWindow
{
    [MenuItem("Ubiq/Weight Transfer")]
    public static void ShowExample()
    {
        WeightTransferWindow wnd = GetWindow<WeightTransferWindow>();
        wnd.titleContent = new GUIContent("WeightTransferWindow");
    }

    ObjectField from;
    ObjectField to;

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy

        from = new ObjectField();
        root.Add(from);

        to = new ObjectField();
        root.Add(to);

        // Create button
        Button button = new Button();
        button.name = "Run";
        button.text = "Run";
        root.Add(button);

        button.clicked += Button_clicked;
    }

    private void Button_clicked()
    {
        TransferWeights(from.value as GameObject, to.value as GameObject);
    }

    private void TransferWeights(GameObject from, GameObject to)
    {
        // Start by making a single mesh referencing all those from the source.

        List<Vector3> positions = new List<Vector3>();
        List<int> triangles = new List<int>();

        foreach (var renderer in to.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            var mesh = new Mesh();
            renderer.BakeMesh(mesh, false);

            var startingIndex = positions.Count;

            //positions.AddRange(mesh.vertices.Select(v => renderer.transform.TransformPoint(v)));
         //   triangles.AddRange(mesh.triangles.Select(index => index + startingIndex));
        }

        foreach (var renderer in to.GetComponentsInChildren<MeshFilter>())
        {
            var mesh = renderer.sharedMesh;

            var startingIndex = positions.Count;

            positions.AddRange(mesh.vertices.Select(v => renderer.transform.TransformPoint(v)));
            triangles.AddRange(mesh.triangles.Select(index => index + startingIndex));
        }

        var m = new Mesh();

        m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        m.vertices = positions.ToArray();
        m.triangles = triangles.ToArray();

        m.RecalculateBounds();
        m.RecalculateNormals();

        AssetDatabase.CreateAsset(m,  "Assets/Generic Avatar/avatarMesh.asset");
    }
}
