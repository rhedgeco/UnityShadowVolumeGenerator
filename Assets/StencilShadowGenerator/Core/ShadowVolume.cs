using StencilShadowGenerator.Core.Attributes.DisplayConditional;
using StencilShadowGenerator.Core.Extensions;
using StencilShadowGenerator.Core.ShadowMeshHelpers;
using UnityEngine;

namespace StencilShadowGenerator.Core
{
    /// <summary>
    /// ShadowVolume component.
    /// Creates initial structure for shadow volume casting
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    public class ShadowVolume : MonoBehaviour
    {
        #region Static Fields
        
        // property fields for shadow volume shader
        private static readonly int Direction = Shader.PropertyToID("_Direction");
        private static readonly int Extrude = Shader.PropertyToID("_Extrude");
        private static readonly int Bias = Shader.PropertyToID("_Bias");
        
        #endregion

        #region Serialized Fields
        
        [Tooltip("Faster rendering, slower mesh generation.")]
        [SerializeField] private bool isTwoManifold = false;
        [SerializeField] private float extrudeDistance = 100;
        [SerializeField] [Range(0.01f,1)] private float shadowBias = 0.01f;
        [SerializeField] private bool preGenerateMesh;

        [SerializeField] [DisplayIf(nameof(preGenerateMesh))]
        private Mesh preGeneratedMesh;

        #endregion

        #region Private Members

        private Mesh _mesh;
        private Material _material;
        private GameObject _volume;

        #endregion

        #region Unity Event Methods

        private void Awake()
        {
            // check or create new mesh
            Mesh mesh;
            if (!preGenerateMesh) mesh = GenerateMesh();
            else if (preGeneratedMesh) mesh = preGeneratedMesh;
            else
            {
                mesh = new Mesh();
                Debug.LogWarning($"No pre generated mesh is assigned for [{name}].\n" +
                                  $"Either assign a pre generated mesh, or turn off pre generation.");
            }
            
            // create mesh volume object
            _material = new Material(Shader.Find("Hidden/ShadowVolumes/StencilWriter"));
            _volume = new GameObject($"{name}_ShadowVolume");
            _volume.transform.parent = transform;
            _volume.transform.LocalReset();
            _volume.AddComponent<MeshRenderer>().material = _material;

            // get mesh copy from filter
            MeshFilter filter = _volume.AddComponent<MeshFilter>();
            filter.mesh = mesh;
            _mesh = filter.mesh;
        }

        private void LateUpdate()
        {
            Light sun = RenderSettings.sun;
            _volume.SetActive(false);
            if (!sun) return;
            _volume.SetActive(true);
            
            // update mesh bounds to prevent culling
            _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * extrudeDistance);
            
            // set all material properties
            _material.SetVector(Direction, sun.transform.forward);
            _material.SetFloat(Extrude, extrudeDistance);
            _material.SetFloat(Bias, shadowBias);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a custom volume mesh based on the mesh filter that is attached
        /// </summary>
        /// <returns>A custom volume mesh</returns>
        public Mesh GenerateMesh()
        {
            MeshFilter filter = GetComponent<MeshFilter>();
            if (filter.sharedMesh == null) return new Mesh();

            // if the mesh is 2 manifold, use 
            if (isTwoManifold)
                return filter.sharedMesh.Generate2ManifoldShadowVolume();
            
            return filter.sharedMesh.GenerateShadowVolumeMesh();
        }

        #endregion
    }
}