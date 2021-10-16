using StencilShadowGenerator.Core.ShadowMeshHelpers;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace StencilShadowGenerator.Core.MeshJobs
{
    /// <summary>
    /// DirectionalLightExtrudeJob calculates the mesh extrusion for directional lights
    /// </summary>
    [BurstCompile]
    public struct DirectionalLightExtrudeJob : IJobParallelFor
    {
        public float3 Direction;
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

            if (math.dot(normal, Direction) >= 0)
                point += Direction * ExtrudeDistance;
            else
                point += Direction * Bias;
            Vertices[index] = point;
        }
    }
}