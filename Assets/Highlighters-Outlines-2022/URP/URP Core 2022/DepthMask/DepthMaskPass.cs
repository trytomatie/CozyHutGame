using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using Highlighters;

namespace Highlighters_URP
{
    public class DepthMaskPass : ScriptableRenderPass
    {
        private readonly string profilingName;

        private readonly Material maskMaterial;
        private readonly List<ShaderTagId> shaderTagIdList;

        private FilteringSettings filteringSettings;

        // Old API
        //public readonly RenderTargetHandle sceneDepthMask;

        public RTHandle sceneDepthMask;


        public DepthMaskPass(RenderPassEvent renderPassEvent, int layerMask, string profilingName) 
        {
            this.profilingName = profilingName;
            this.renderPassEvent = renderPassEvent;

            filteringSettings = new FilteringSettings(RenderQueueRange.all, layerMask);

            maskMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("HighlightersURP/SceneDepthShader"));

            shaderTagIdList = new List<ShaderTagId>()
            {
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("LightweightForward"),
                new ShaderTagId("SRPDefoultUnlit"),
                new ShaderTagId("DepthOnly"),
                new ShaderTagId("UniversalGBuffer"),
                new ShaderTagId("DepthNormalsOnly"),
                new ShaderTagId("Universal2D"),
                new ShaderTagId("SRPDefaultUnlit"),
            };

            // Old API
            //sceneDepthMask.Init("_SceneDepthMask");
            sceneDepthMask = RTHandles.Alloc("_SceneDepthMask", name: "_SceneDepthMask");
        }

        // TODO: There is probably no difference between Configure and OnCameraSetup but investigate that
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor textureDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            textureDescriptor.colorFormat = RenderTextureFormat.RFloat;
            textureDescriptor.msaaSamples = 1;

            // How unity wants me to do
            //RenderingUtils.ReAllocateIfNeeded(ref sceneDepthMask, textureDescriptor, FilterMode.Point, TextureWrapMode.Clamp, name: "_SceneDepthMask");

            // What I do because this works when passing the texture to shader via sceneDepthMask.nameID
            cmd.GetTemporaryRT(Shader.PropertyToID(sceneDepthMask.name), textureDescriptor, FilterMode.Point);
            
            ConfigureTarget(sceneDepthMask);
            ConfigureClear(ClearFlag.All, Color.black);
        }



        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!maskMaterial)
                return;

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler(
                profilingName)))
            {
                DrawingSettings drawingSettings = CreateDrawingSettings(shaderTagIdList, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
                drawingSettings.overrideMaterial = maskMaterial;
                drawingSettings.enableDynamicBatching = true;

                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);//, ref m_RenderStateBlock
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            
            // testing
            //Debug.Log(sceneDepthMask.nameID);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(Shader.PropertyToID(sceneDepthMask.name));
        }
    }
}