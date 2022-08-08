// COMP30019 - Graphics and Interaction
// (c) University of Melbourne, 2022

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class GenerateCone : MonoBehaviour
{
    // Note: The 'Range' attribute shows a slider in the inspector.
    [SerializeField, Range(3, 100)] private int numBaseVertices = 20;
    [SerializeField, Range(1.5f, 100)] private float radius = 1.5f;
    [SerializeField, Range(3.0f, 100)] private float height = 3.0f;
    
    // Are we sharing vertices? See comments below for more details.
    [SerializeField] private bool shareVertices = true;

    // Variables to have references to the components MeshFilter and MeshRender
    private MeshFilter _meshFilter;

    private void Start()
    {
        this._meshFilter = GetComponent<MeshFilter>();
        this._meshFilter.mesh = CreateMesh();
    }

    private void Update()
    {
        // Re-generate the mesh each frame so it dynamically updates. Note: This
        // is quite expensive. Ideally we would recreate the mesh only if one of
        // the properties *changes*, but this is just for demo purposes.
        this._meshFilter.mesh = CreateMesh();
    }
    
    private Mesh CreateMesh()
    {
        // Generate the cone mesh. Note that there is more than one way to do
        // this - here we demonstrate two ways based on whether we wish to share
        // common vertices or not.
        
        var m = new Mesh
        {
            name = "Cone"
        };

        var vertices = new List<Vector3>();
        var colors = new List<Color>();
        var indices = new List<int>();

        // Define the vertices and corresponding colours.
        // Two approaches: They are both correct but which one is more compact?
        for (var i = 0; i < this.numBaseVertices; i++)
        {
            if (this.shareVertices)
            {
                // Shared vertices approach.
                // Each triangle shares the same base and tip vertices.
                // This makes generating the vertices simple (simple circle).
                // On the other hand, the indices are more complex.
                var fraction = (float)i / this.numBaseVertices;
                var angle = fraction * Mathf.PI * 2.0f;
                vertices.Add(
                    new Vector3(Mathf.Sin(angle), 0.0f, Mathf.Cos(angle)) *
                    this.radius);
                colors.Add(Color.HSVToRGB(fraction, 1.0f,
                    1.0f)); // Vary hue around edge to create rainbow gradient
            }
            else
            {
                // Unique vertices approach.
                // Slightly different approach, we calculate the size of the PI
                // slice according to the number of vertices
                // We then calculate the cos/sin and add it to the vertex list
                var angle1 = 2 * Mathf.PI / this.numBaseVertices * i;
                var angle2 = 2 * Mathf.PI / this.numBaseVertices * (i + 1);

                // Make sure it's in clock-wise order
                vertices.Add(new Vector3(Mathf.Sin(angle1) * this.radius, 0.0f,
                    Mathf.Cos(angle1) * this.radius)); // v1
                vertices.Add(new Vector3(0.0f, 0.0f, 0.0f)); // Base
                vertices.Add(new Vector3(Mathf.Sin(angle2) * this.radius, 0.0f,
                    Mathf.Cos(angle2) * this.radius)); // v2
                colors.Add(Color.HSVToRGB((float)i / this.numBaseVertices, 1.0f,
                    1.0f));
                colors.Add(Color.black);
                colors.Add(Color.HSVToRGB(((float)i + 1) / this.numBaseVertices,
                    1.0f, 1.0f));

                // Make sure it's in clock-wise order
                vertices.Add(new Vector3(0.0f, this.height,
                    0.0f)); // Tip
                vertices.Add(new Vector3(Mathf.Sin(angle1) * this.radius, 0.0f,
                    Mathf.Cos(angle1) * this.radius)); // v1
                vertices.Add(new Vector3(Mathf.Sin(angle2) * this.radius, 0.0f,
                    Mathf.Cos(angle2) * this.radius)); // v2
                colors.Add(Color.black);
                colors.Add(Color.HSVToRGB((float)i / this.numBaseVertices, 1.0f,
                    1.0f));
                colors.Add(Color.HSVToRGB(((float)i + 1) / this.numBaseVertices,
                    1.0f, 1.0f));
            }
        }

        // Define the indices (triangles).
        if (this.shareVertices)
        {
            // Base-center and tip vertices
            vertices.Add(new Vector3(0.0f, 0.0f, 0.0f));
            vertices.Add(new Vector3(0.0f, this.height, 0.0f));
            colors.Add(Color.black);
            colors.Add(Color.black);

            // As we are sharing vertices, we need to be mindful of the way we
            // add the vertices to the triangle list. We can't simply set the
            // triangles array to 0,1,2...|V|-1
            var vBaseCenter = vertices.Count - 2;
            var vTip = vertices.Count - 1;
            for (var i = 0; i < this.numBaseVertices; i++)
            {
                var v1 = i;
                var v2 = (i + 1) % this.numBaseVertices;

                // "Base" triangle
                indices.Add(vBaseCenter);
                indices.Add(v2);
                indices.Add(v1);

                // "Side" triangle
                indices.Add(vTip);
                indices.Add(v1);
                indices.Add(v2);
            }
        }
        else
        {
            // If we are not sharing vertices, we can refer to the technique we
            // used in the cube/pyramid generator scripts. We already defined
            // them in triangle-list order earlier!
            indices = Enumerable.Range(0, vertices.Count).ToList();
        }

        m.SetVertices(vertices);
        m.SetColors(colors);
        m.SetIndices(indices, MeshTopology.Triangles, 0);

        return m;
    }
}