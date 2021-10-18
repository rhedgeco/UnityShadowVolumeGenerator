using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace StencilShadowGenerator.Core.RenderFeature
{
    class ShadowVolumeRenderPass : ScriptableRenderPass
    {
        private readonly Material _shadowMaterial;
        private readonly Material _blitFlip;
        private RenderTargetIdentifier _shadowMap;
        private RenderTargetHandle _tempTarget;
        private ShadowVolumeRenderingSettings _settings;
        private readonly List<ShaderTagId> _shaderTagIdList = new List<ShaderTagId>();
        private FilteringSettings _filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        public ShadowVolumeRenderPass(ShadowVolumeRenderingSettings settings)
        {
            _settings = settings;
            _tempTarget = RenderTargetHandle.CameraTarget;
            _shadowMap = new RenderTargetIdentifier(Shader.PropertyToID("_ScreenSpaceShadowmapTexture"));
            _blitFlip = new Material(Shader.Find("Hidden/ShadowVolumes/BlitFlip"));
            _shadowMaterial = new Material(Shader.Find("Hidden/ShadowVolumes/ShadowRender"));
            _shadowMaterial.color = Color.black;
            
            _shaderTagIdList.Add(new ShaderTagId("UniversalForward"));
            _shaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
            _shaderTagIdList.Add(new ShaderTagId("LightweightForward"));
            _shaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));

            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor cameraTextureDescriptor =
                renderingData.cameraData.cameraTargetDescriptor;
            cameraTextureDescriptor.depthBufferBits = 0;
            cmd.GetTemporaryRT(_tempTarget.id, cameraTextureDescriptor, FilterMode.Point);
            ConfigureTarget(_tempTarget.Identifier());
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
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

                // we need to draw all the rendederers... and yeah its expensive i guess
                // but its the only way to populate the stencil buffer in a render feature
                // before the whole scene gets rendered to the screen
                DrawingSettings drawingSettings = CreateDrawingSettings(_shaderTagIdList, 
                    ref renderingData, SortingCriteria.CommonOpaque);
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref _filteringSettings);
                
                // blit shadowmap into this texture
                cmd.Blit(_shadowMap, _tempTarget.Identifier());
                
                // draw a fullscreen quad with shader that uses stencil buffer
                Camera camera = renderingData.cameraData.camera;
                cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _shadowMaterial);
                cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
                
                // for some reason blitting into the shadowmap flips the texture.
                // we will just flip it back in the blitflip shader
                cmd.Blit(_tempTarget.Identifier(), _shadowMap, _blitFlip);
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