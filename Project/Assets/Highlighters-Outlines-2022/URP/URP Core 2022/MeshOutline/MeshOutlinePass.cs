using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using Highlighters;

namespace Highlighters_URP
{
    public class MeshOutlinePass : ScriptableRenderPass
    {
        private readonly string profilingName;

        private List<HighlighterRenderer> renderersToDraw;
        private List<Material> materialsToDraw;
        private HighlighterSettings highlighterSettings;

        private bool useSceneDepth = false;

        // New API
        private RTHandle sceneDepthMaskHandle;
        public readonly RTHandle meshOutlineObjects;

        public MeshOutlinePass(RenderPassEvent renderPassEvent, HighlighterSettings highlighterSettings, int ID , string profilingName, List<HighlighterRenderer> renderers)
        {
            this.renderPassEvent = renderPassEvent;
            this.profilingName = profilingName;
            this.highlighterSettings = highlighterSettings;

            meshOutlineObjects = RTHandles.Alloc("_MeshOutlineObjects_" + ID, name: "_MeshOutlineObjects_" + ID);


            renderersToDraw = renderers;
            UpdateMaterialsToDraw();

        }

        public void SetupSceneDepthTarget(RTHandle sceneDepthMaskHandle)
        {
            useSceneDepth = true;
            this.sceneDepthMaskHandle = sceneDepthMaskHandle;
        }

        private void UpdateMaterialsToDraw()
        {
            materialsToDraw = new List<Material>();

            for (int i = 0; i < renderersToDraw.Count; i++)
            {
                var material = CoreUtils.CreateEngineMaterial(Shader.Find("HighlightersURP/MeshOutlineObjects"));
                highlighterSettings.SetMeshOutlineMaterialProperties(material);
                materialsToDraw.Add(material);
            }
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RenderTextureDescriptor textureDescriptor = cameraTextureDescriptor;
            textureDescriptor.colorFormat = RenderTextureFormat.ARGBFloat;
            textureDescriptor.msaaSamples = 1;
            textureDescriptor.width = Mathf.FloorToInt(textureDescriptor.width * highlighterSettings.InfoRenderScale);
            textureDescriptor.height = Mathf.FloorToInt(textureDescriptor.height * highlighterSettings.InfoRenderScale);

            cmd.GetTemporaryRT(Shader.PropertyToID(meshOutlineObjects.name), textureDescriptor, FilterMode.Bilinear);

            ConfigureTarget(meshOutlineObjects);
            ConfigureClear(ClearFlag.All, new Color(0,0,0,0));
        }


        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler(
                profilingName)))
            {
                if(useSceneDepth) cmd.SetGlobalTexture("_SceneDepthMask", sceneDepthMaskHandle);

                if (renderersToDraw.Count == materialsToDraw.Count)
                {
                    for (int i = 0; i < renderersToDraw.Count; i++)
                    {
                        var item = renderersToDraw[i];
                        
                        if (item.renderer == null || item.renderer.enabled == false) continue;

                        for (int submeshIndex = 0; submeshIndex < item.submeshIndexes.Count; submeshIndex++)
                        {
                            int ShaderPass;
                            if (highlighterSettings.DepthMask == DepthMask.BehindOnly) ShaderPass = 1;
                            else if (highlighterSettings.DepthMask == DepthMask.FrontOnly) ShaderPass = 0;
                            else ShaderPass = 2;

                            cmd.DrawRenderer(item.renderer, materialsToDraw[i], item.submeshIndexes[submeshIndex], ShaderPass);
                        }
                    }
                }
                else
                {
                    UpdateMaterialsToDraw();
                }
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(Shader.PropertyToID(meshOutlineObjects.name));
        }
    }
}