using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace StencilShadowGenerator.Core.RenderFeature
{
    class ShadowVolumeRenderPass : ScriptableRenderPass
    {
        private static readonly int FlipYCoord = Shader.PropertyToID("_FlipYCoord");

        private readonly Material _occluderMaterial;
        private readonly Material _shadowMaterial;
        private readonly Material _blitMaterial;
        private readonly ShaderTagId _volumeShader;
        private readonly List<ShaderTagId> _occluderShaders;
        
        private RenderTargetIdentifier _shadowMap;
        private RenderTargetHandle _tempTarget;
        private ShadowVolumeRenderingSettings _settings;
        private FilteringSettings _filteringSettings;

        public ShadowVolumeRenderPass(ShadowVolumeRenderingSettings settings)
        {
            _settings = settings;
            
            // set up render textures
            _tempTarget = RenderTargetHandle.CameraTarget;
            _shadowMap = new RenderTargetIdentifier(Shader.PropertyToID("_ScreenSpaceShadowmapTexture"));
            
            // set up materials
            _occluderMaterial = new Material(Shader.Find("Hidden/ShadowVolumes/White"));
            _shadowMaterial = new Material(Shader.Find("Hidden/ShadowVolumes/ShadowRender"));
            _blitMaterial = new Material(Shader.Find("Hidden/ShadowVolumes/BlitFlip"));

            // set up shader tags
            _volumeShader = new ShaderTagId("ShadowVolume");
            _occluderShaders = new List<ShaderTagId>
            {
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("LightweightForward"),
                new ShaderTagId("SRPDefaultUnlit")
            };

            // set up render pass and filter settings
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
            _filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor cameraTextureDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            cameraTextureDescriptor.depthBufferBits = 0;
            cmd.GetTemporaryRT(_tempTarget.id, cameraTextureDescriptor, FilterMode.Point);
            ConfigureTarget(_tempTarget.Identifier());
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            ConfigureClear(ClearFlag.All, Color.white);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!_shadowMaterial) return;
            
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("Shadow Volume Rendering")))
            {
                // prepare and clear buffer
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                
                // set matrices
                Camera camera = renderingData.cameraData.camera;
                cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
                
                // draw occluders
                DrawingSettings occluderSettings = CreateDrawingSettings(_occluderShaders, 
                    ref renderingData, SortingCriteria.CommonOpaque);
                occluderSettings.overrideMaterial = _occluderMaterial;
                context.DrawRenderers(renderingData.cullResults, ref occluderSettings, ref _filteringSettings);

                // draw shadow volume stencil
                DrawingSettings volumeSettings = CreateDrawingSettings(_volumeShader, 
                    ref renderingData, SortingCriteria.CommonOpaque);
                context.DrawRenderers(renderingData.cullResults, ref volumeSettings, ref _filteringSettings);

                // draw shadow material using fullscreen quad
                cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _shadowMaterial);
                cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
                
                // blit to shadow texture
                bool flipY = camera.cameraType == CameraType.SceneView;
                _blitMaterial.SetInt(FlipYCoord, flipY ? 0 : 1);
                Blit(cmd, _tempTarget.Identifier(), _shadowMap, _blitMaterial);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_tempTarget.id);
        }
    }
}