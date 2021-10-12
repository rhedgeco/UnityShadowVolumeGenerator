using System.Collections.Generic;
using UnityEngine;

namespace StencilShadowGenerator.Core.ShadowMeshHelpers
{
    /// <summary>
    /// Edge structure for relating faces in a mesh to one another
    /// </summary>
    public struct Edge
    {
        // this is totally arbitrary lol
        private const float MergeDistance = 0.00001f;
        
        public int Vertex1 { get; }
        public int Vertex2 { get; }

        /// <summary>
        /// Creates an Edge object
        /// </summary>
        /// <param name="vertex1">index of vertex1</param>
        /// <param name="vertex2">index of vertex2</param>
        public Edge(int vertex1, int vertex2)
        {
            Vertex1 = vertex1;
            Vertex2 = vertex2;
        }

        /// <summary>
        /// Compares this edge to another edge
        /// </summary>
        /// <param name="edge">Edge to compare to</param>
        /// <param name="vertices">Reference list of vertices to pull vertex indices from</param>
        /// <returns>true if edges match</returns>
        public bool CompareEdge(Edge edge, List<Vector3> vertices)
        {
            Vector3 v1 = vertices[Vertex1];
            Vector3 v2 = vertices[Vertex2];
            Vector3 edgeV1 = vertices[edge.Vertex1];
            Vector3 edgeV2 = vertices[edge.Vertex2];

            // we only check this orientation, because when the items are extracted,
            // the vertices will only have matching face normals with this patten
            if (Vector3.Distance(v1, edgeV2) > MergeDistance) return false;
            if (Vector3.Distance(v2, edgeV1) > MergeDistance) return false;
            return true;
        }
    }
}