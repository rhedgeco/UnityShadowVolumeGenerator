using System.Collections.Generic;
using StencilShadowGenerator.Core.MeshJobs;
using Unity.Jobs;
using UnityEngine;

namespace StencilShadowGenerator.Core
{
    /// <summary>
    /// ShadowVolumeCaster component
    /// Casts a volume from all ShadowVolumes when this is attached to a light
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class ShadowVolumeCaster : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] [Range(0, 1)] private float bias = 0.001f;
        [SerializeField] private float extrudeDistance = 10f;
        [SerializeField] private Material shadowVolumeMaterial;

        #endregion

        #region Private Members

        private Light _light;

        private Dictionary<ShadowVolume, VolumeData> _volumes =
            new Dictionary<ShadowVolume, VolumeData>();

        private List<(JobHandle, VolumeData)> _lightHandles =
            new List<(JobHandle, VolumeData)>();

        #endregion

        #region Unity Event Methods

        private void Awake()
        {
            _light = GetComponent<Light>();
            ShadowVolume.VolumeAdded.AddListener(VolumeAdded);
            ShadowVolume.VolumeRemoved.AddListener(VolumeRemoved);
        }

        private void OnDestroy()
        {
            ShadowVolume.VolumeAdded.RemoveListener(VolumeAdded);
            ShadowVolume.VolumeRemoved.RemoveListener(VolumeRemoved);
            foreach (VolumeData data in _volumes.Values)
            {
                // dispose of all volume data to avoid memory leaks
                data.Dispose();
            }
        }

        private void LateUpdate()
        {
            // create all job handles
            _lightHandles.Clear();
            foreach (KeyValuePair<ShadowVolume, VolumeData> pair in _volumes)
            {
                ShadowVolume volume = pair.Key;
                VolumeData data = pair.Value;
                data.ResetTransform();
                Transform t = volume.transform;
                JobHandle? handle = LightJobManager.CreateLightJob(_light, extrudeDistance, bias,
                    volume.TransformData, volume.OriginalVertices, data.AdjustedVertices);
                if (!handle.HasValue) Debug.LogWarning($"Cannot create shadow volume with light type {_light.type}");
                else _lightHandles.Add((handle.Value, data));
            }

            // complete jobs and apply data
            foreach ((JobHandle, VolumeData) handle in _lightHandles)
            {
                // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
                handle.Item1.Complete();
                VolumeData data = handle.Item2;
                data.Mesh.SetVertices(data.AdjustedVertices);
                data.Mesh.RecalculateBounds();
                data.Mesh.MarkModified();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Starts tracking a ShadowVolume
        /// </summary>
        /// <param name="volume">ShadowVolume to track</param>
        private void VolumeAdded(ShadowVolume volume)
        {
            if (_volumes.ContainsKey(volume)) return;
            _volumes.Add(volume, new VolumeData(volume, transform, shadowVolumeMaterial));
        }

        /// <summary>
        /// Stops tracking a ShadowVolume
        /// </summary>
        /// <param name="volume">ShadowVolume to stop tracking</param>
        private void VolumeRemoved(ShadowVolume volume)
        {
            if (!_volumes.TryGetValue(volume, out VolumeData data)) return;
            data.Dispose();
            _volumes.Remove(volume);
        }

        #endregion
    }
}