using System.Collections.Generic;
using StencilShadowGenerator.Core.Attributes.DisplayConditional;
using StencilShadowGenerator.Core.ShadowMeshHelpers;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

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

        // Events for when a volume is added or removed
        public static readonly UnityEvent<ShadowVolume> VolumeAdded = new UnityEvent<ShadowVolume>();
        public static readonly UnityEvent<ShadowVolume> VolumeRemoved = new UnityEvent<ShadowVolume>();

        #endregion

        #region Serialized Fields

        [SerializeField] private bool preGenerateMesh;

        [SerializeField] [DisplayIf(nameof(preGenerateMesh))]
        private Mesh preGeneratedMesh;

        #endregion

        #region Private Members

        private Mesh _mesh;
        private TransformData _transformData;
        private NativeArray<Vertex> _originalVertices;

        #endregion

        #region Properties
        
        public NativeArray<Vertex> OriginalVertices => _originalVertices;

        /// <summary>
        /// Gets the relevant transform data in a struct for later use in jobs
        /// </summary>
        public TransformData TransformData
        {
            get
            {
                Transform t = transform;
                _transformData.Position = t.position;
                _transformData.Rotation = t.rotation;
                _transformData.Scale = t.lossyScale;
                return _transformData;
            }
        }

        #endregion

        #region Unity Event Methods

        private void Awake()
        {
            if (!preGenerateMesh) _mesh = GenerateMesh();
            else if (preGeneratedMesh) _mesh = preGeneratedMesh;
            else
            {
                _mesh = new Mesh();
                Debug.LogWarning($"No pre generated mesh is assigned for [{name}].\n" +
                                 $"Either assign a pre generated mesh, or turn off pre generation.");
            }

            CreateNativeVertexArray();
        }

        private void Start()
        {
            VolumeAdded.Invoke(this);
        }

        private void OnDestroy()
        {
            VolumeRemoved.Invoke(this);
            
            // be sure to dispose of array to avoid leaks
            if (_originalVertices.IsCreated) _originalVertices.Dispose();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies the internally created mesh for shadow casting
        /// </summary>
        /// <returns>Copy of the internal volume mesh</returns>
        public Mesh CreateMeshCopy()
        {
            Mesh mesh = new Mesh();
            mesh.MarkDynamic();
            mesh.SetVertices(_mesh.vertices);
            mesh.SetNormals(_mesh.normals);
            mesh.SetTriangles(_mesh.triangles, 0);
            return mesh;
        }
        
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
        /// Creates a native vertex array for later use in jobs
        /// </summary>
        private void CreateNativeVertexArray()
        {
            Vector3[] verts = _mesh.vertices;
            Vector3[] norms = _mesh.normals;
            _originalVertices = new NativeArray<Vertex>(_mesh.vertexCount, Allocator.Persistent);
            for (int i = 0; i < _mesh.vertexCount; i++)
            {
                _originalVertices[i] = new Vertex(verts[i], norms[i]);
            }
        }

        #endregion
    }
}