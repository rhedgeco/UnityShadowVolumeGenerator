using System;
using StencilShadowGenerator.Core.ShadowMeshHelpers;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace StencilShadowGenerator.Core.MeshJobs
{
    /// <summary>
    /// LightJobManager holds relevant methods for getting light extrude jobs
    /// </summary>
    public static class LightJobManager
    {
        /// <summary>
        /// Creates a light extrusion job
        /// </summary>
        /// <param name="light">Light to get info from</param>
        /// <param name="extrudeDistance">Distance to extrude mesh</param>
        /// <param name="transformData">Transform information for the volume source</param>
        /// <param name="vertexData">Original vertex data from volume source</param>
        /// <param name="vertices">array of vertices to place modifications</param>
        /// <returns>A nullable job handle with scheduled job</returns>
        public static JobHandle? CreateLightJob(Light light, float extrudeDistance,
            TransformData transformData, NativeArray<Vertex> vertexData, NativeArray<Vector3> vertices)
        {
            JobHandle? handle = null;
            if (light.type == LightType.Directional)
            {
                DirectionalLightExtrudeJob job = new DirectionalLightExtrudeJob
                {
                    Direction = light.transform.forward,
                    ExtrudeDistance = extrudeDistance,
                    TransformData = transformData,
                    VertexData = vertexData,
                    Vertices = vertices
                };
                handle = job.Schedule(vertices.Length, 64);
            }
            else if (light.type == LightType.Point)
            {
                PointLightExtrudeJob job = new PointLightExtrudeJob
                {
                    Point = light.transform.position,
                    ExtrudeDistance = extrudeDistance,
                    TransformData = transformData,
                    VertexData = vertexData,
                    Vertices = vertices
                };
                handle = job.Schedule(vertices.Length, 64);
            }

            return handle;
        }
    }
}