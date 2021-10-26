using System.Collections.Generic;
using UnityEngine;

namespace StencilShadowGenerator.Core.ShadowMeshHelpers
{
    public static class MeshGenerator
    {
        public static Mesh Generate2ManifoldShadowVolume(this Mesh mesh)
        {
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
                    break;
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

        public static Mesh GenerateShadowVolumeMesh(this Mesh mesh)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();
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

                int triangleIndex = vertices.Count;
                
                // add normal face vertices
                vertices.Add(v0);
                vertices.Add(v1);
                vertices.Add(v2);
                
                // add flipped face vertices
                vertices.Add(v0);
                vertices.Add(v1);
                vertices.Add(v2);

                // and regular and flipped normals
                normals.Add(normal);
                normals.Add(normal);
                normals.Add(normal);
                normals.Add(-normal);
                normals.Add(-normal);
                normals.Add(-normal);

                // add normal face
                triangles.Add(triangleIndex + 0);
                triangles.Add(triangleIndex + 1);
                triangles.Add(triangleIndex + 2);
                
                // add flipped face
                triangles.Add(triangleIndex + 5);
                triangles.Add(triangleIndex + 4);
                triangles.Add(triangleIndex + 3);
                
                // add quads between each
                triangles.Add(triangleIndex + 3);
                triangles.Add(triangleIndex + 1);
                triangles.Add(triangleIndex + 0);
                triangles.Add(triangleIndex + 3);
                triangles.Add(triangleIndex + 4);
                triangles.Add(triangleIndex + 1);
                
                triangles.Add(triangleIndex + 4);
                triangles.Add(triangleIndex + 2);
                triangles.Add(triangleIndex + 1);
                triangles.Add(triangleIndex + 4);
                triangles.Add(triangleIndex + 5);
                triangles.Add(triangleIndex + 2);
                
                triangles.Add(triangleIndex + 5);
                triangles.Add(triangleIndex + 3);
                triangles.Add(triangleIndex + 0);
                triangles.Add(triangleIndex + 5);
                triangles.Add(triangleIndex + 0);
                triangles.Add(triangleIndex + 2);
            }
            
            Mesh newMesh = new Mesh();
            newMesh.MarkDynamic();
            newMesh.SetVertices(vertices);
            newMesh.SetNormals(normals);
            newMesh.SetTriangles(triangles, 0);
            newMesh.Optimize();
            return newMesh;
        }
    }
}