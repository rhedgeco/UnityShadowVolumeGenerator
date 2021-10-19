using System.Collections.Generic;
using StencilShadowGenerator.Core.Attributes.DisplayConditional;
using StencilShadowGenerator.Core.Extensions;
using StencilShadowGenerator.Core.ShadowMeshHelpers;
using UnityEngine;

namespace StencilShadowGenerator.Core
{
    /// <summary>
    /// ShadowVolume component.
    /// Creates initial structure for shadow volume casting
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    public class ShadowVolume : MonoBehaviour
    {
        #region Static Fields

        // property fields for shadow volume shader
        private static readonly int Direction = Shader.PropertyToID("_Direction");
        private static readonly int Extrude = Shader.PropertyToID("_Extrude");
        private static readonly int Bias = Shader.PropertyToID("_Bias");
        
        #endregion

        #region Serialized Fields

        [SerializeField] private float extrudeDistance = 100;
        [SerializeField] [Range(0,1)] private float shadowBias = 0.01f;
        [SerializeField] private bool preGenerateMesh;

        [SerializeField] [DisplayIf(nameof(preGenerateMesh))]
        private Mesh preGeneratedMesh;

        #endregion

        #region Private Members

        private Mesh _mesh;
        private Material _material;
        private GameObject _volume;

        #endregion

        #region Unity Event Methods

        private void Awake()
        {
            if (!preGenerateMesh) _mesh = GenerateMesh();
            else if (preGeneratedMesh) _mesh = preGeneratedMesh;
            else Debug.LogWarning($"No pre generated mesh is assigned for [{name}].\n" +
                                  $"Either assign a pre generated mesh, or turn off pre generation.");
            
            CreateVolumeMeshObject();
        }

        private void LateUpdate()
        {
            Light sun = RenderSettings.sun;
            _volume.SetActive(false);
            if (!sun) return;
            _volume.SetActive(true);
            
            // update mesh bounds to prevent culling
            _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * extrudeDistance);
            
            // set all material properties
            _material.SetVector(Direction, sun.transform.forward);
            _material.SetFloat(Extrude, extrudeDistance);
            _material.SetFloat(Bias, shadowBias);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a custom volume mesh based on the mesh filter that is attached
        /// </summary>
        /// <returns>A custom volume mesh</returns>
        public Mesh GenerateMesh()
        {
            MeshFilter filter = GetComponent<MeshFilter>();
            if (filter.sharedMesh == null) return new Mesh();

            // First separate every triangle in the mesh, and calculate actual face normal
            // also create edge collection for linking of faces with shadow extents
            Mesh mesh = filter.sharedMesh;
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Edge> edges = new List<Edge>();
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                int i0 = mesh.triangles[i + 0];
                int i1 = mesh.triangles[i + 1];
                int i2 = mesh.triangles[i + 2];

                Vector3 v0 = mesh.vertices[i0];
                Vector3 v1 = mesh.vertices[i1];
                Vector3 v2 = mesh.vertices[i2];
                // create normal using cross product and right hand rule
                Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0);

                vertices.Add(v0);
                vertices.Add(v1);
                vertices.Add(v2);

                normals.Add(normal);
                normals.Add(normal);
                normals.Add(normal);

                triangles.Add(i + 0);
                triangles.Add(i + 1);
                triangles.Add(i + 2);

                edges.Add(new Edge(i + 0, i + 1));
                edges.Add(new Edge(i + 1, i + 2));
                edges.Add(new Edge(i + 2, i + 0));
            }

            // find matching edges and connect them with triangles
            while (edges.Count > 0)
            {
                Edge edge1 = edges[0];
                for (int i = 1; i < edges.Count; i++)
                {
                    Edge edge2 = edges[i];
                    if (!edge1.CompareEdge(edge2, vertices)) continue;

                    // add triangles to mesh
                    triangles.Add(edge1.Vertex2);
                    triangles.Add(edge1.Vertex1);
                    triangles.Add(edge2.Vertex1);

                    triangles.Add(edge2.Vertex2);
                    triangles.Add(edge2.Vertex1);
                    triangles.Add(edge1.Vertex1);

                    edges.RemoveAt(i);
                }

                edges.RemoveAt(0);
            }

            Mesh newMesh = new Mesh();
            newMesh.MarkDynamic();
            newMesh.SetVertices(vertices);
            newMesh.SetNormals(normals);
            newMesh.SetTriangles(triangles, 0);
            newMesh.Optimize();
            return newMesh;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates the shadow volume object
        /// </summary>
        private void CreateVolumeMeshObject()
        {
            if (!_mesh) _mesh = new Mesh();
            
            _material = new Material(Shader.Find("Hidden/ShadowVolumes/StencilWriter"));
            _volume = new GameObject($"{name}_ShadowVolume");
            _volume.transform.parent = transform;
            _volume.transform.LocalReset();
            _volume.AddComponent<MeshFilter>().mesh = _mesh;
            _volume.AddComponent<MeshRenderer>().material = _material;
        }

        #endregion
    }
}