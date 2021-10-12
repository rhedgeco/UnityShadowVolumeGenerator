using UnityEngine;

namespace StencilShadowGenerator.Core.ShadowMeshHelpers
{
    /// <summary>
    /// Vertex structure stores position and normal information for a given vertex
    /// </summary>
    public struct Vertex
    {
        public Vector3 Point;
        public Vector3 Normal;

        public Vertex(Vector3 point, Vector3 normal)
        {
            Point = point;
            Normal = normal;
        }
    }
}