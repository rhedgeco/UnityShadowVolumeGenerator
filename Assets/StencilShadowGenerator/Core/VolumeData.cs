using System;
using StencilShadowGenerator.Core.Extensions;
using Unity.Collections;
using UnityEngine;
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
            _object.AddComponent<MeshFilter>().mesh = Mesh;
            _object.AddComponent<MeshRenderer>().material = volumeMaterial;
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