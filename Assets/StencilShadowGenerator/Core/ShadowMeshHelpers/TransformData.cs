using Unity.Mathematics;
using UnityEngine;

namespace StencilShadowGenerator.Core.ShadowMeshHelpers
{
    /// <summary>
    /// TransformData structure is used when relevant transform data is needed in a job
    /// </summary>
    public struct TransformData
    {
        public float3 Scale { get; set;  }
        public Quaternion Rotation { get; set; }
        public float3 Position { get; set; }

        public TransformData(Vector3 scale, Quaternion rotation, Vector3 position)
        {
            Scale = scale;
            Rotation = rotation;
            Position = position;
        }
    }
}