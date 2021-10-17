using System;
using StencilShadowGenerator.Core.Extensions;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace StencilShadowGenerator.Core
{
    /// <summary>
    /// VolumeData
    /// Holds and creates relevant information for extruding shadow volumes
    /// </summary>
    public class VolumeData : IDisposable
    {
        public Mesh Mesh { get; }
        public NativeArray<Vector3> AdjustedVertices { get; }

        private GameObject _object;

        public VolumeData(ShadowVolume volume, Transform parent, Material volumeMaterial)
        {
            Mesh = volume.CreateMeshCopy();
            _object = new GameObject($"{volume.name}_VOLUME");
            _object.transform.parent = parent;

            MeshFilter filter = _object.AddComponent<MeshFilter>();
            MeshRenderer renderer = _object.AddComponent<MeshRenderer>();
            filter.mesh = Mesh;
            renderer.material = volumeMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            AdjustedVertices = new NativeArray<Vector3>(Mesh.vertexCount, Allocator.Persistent);
        }

        /// <summary>
        /// Resets the transforms on the object associated with the mesh
        /// </summary>
        public void ResetTransform()
        {
            _object.transform.Reset();
        }
        
        public void Dispose()
        {
            Object.Destroy(_object);
            if (AdjustedVertices.IsCreated) AdjustedVertices.Dispose();
        }
    }
}