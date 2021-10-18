using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace StencilShadowGenerator.Core.RenderFeature
{
    public class ShadowVolumeRenderingFeature : ScriptableRendererFeature
    {
        [SerializeField] private ShadowVolumeRenderingSettings settings = new ShadowVolumeRenderingSettings();
        ShadowVolumeRenderPass _renderPass;

        /// <inheritdoc/>
        public override void Create()
        {
            _renderPass = new ShadowVolumeRenderPass(settings);
            _renderPass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_renderPass);
        }
    }
}