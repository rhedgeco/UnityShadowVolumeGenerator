using StencilShadowGenerator.Core.ShadowMeshHelpers;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace StencilShadowGenerator.Core.MeshJobs
{
    /// <summary>
    /// PointLightExtrudeJob calculates the mesh extrusion for point lights
    /// </summary>
    [BurstCompile]
    public struct PointLightExtrudeJob : IJobParallelFor
    {
        public float3 Point;
        public float Bias;
        public float ExtrudeDistance;
        public TransformData TransformData;
        public NativeArray<Vertex> VertexData;
        public NativeArray<Vector3> Vertices;

        public void Execute(int index)
        {
            float3 point = VertexData[index].Point;
            float3 normal = VertexData[index].Normal;

            point *= TransformData.Scale;
            point = TransformData.Rotation * point;
            normal = TransformData.Rotation * normal;
            point += TransformData.Position;

            float3 direction = math.normalize(point - Point);
            if (math.dot(normal, direction) >= 0)
                point += direction * ExtrudeDistance;
            
            Vertices[index] = point;
        }
    }
}