using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using Highlighters;

namespace Highlighters_URP
{
    public class ObjectsPass : ScriptableRenderPass
    {
        private readonly string profilingName;

        private List<HighlighterRenderer> renderersToDraw;
        private List<Material> materialsToDraw;
        private List<int> materialsPassIndexes;
        private HighlighterSettings highlighterSettings;

        private bool useSceneDepth = false;

        // New API
        public RTHandle objectsInfo;
        private RTHandle sceneDepthMaskHandle;
        //private string objectsInfoName;

        /// <summary>
        /// Vector4(MinX, MinY, MaxX, MaxY) in viewport space
        /// </summary>
        private Vector4 renderingBounds;

        public ObjectsPass(RenderPassEvent renderPassEvent, HighlighterSettings highlighterSettings, int ID, string profilingName, List<HighlighterRenderer> renderers)
        {
            this.renderPassEvent = renderPassEvent;
            this.profilingName = profilingName;
            this.highlighterSettings = highlighterSettings;

            //objectsInfo.Init("_HighlightedObjects_" + ID);
            //objectsInfoName = "_HighlightedObjects_" + ID;
            objectsInfo = RTHandles.Alloc("_HighlightedObjects_" + ID, name: "_HighlightedObjects_" + ID);


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
            materialsPassIndexes = new List<int>();

            bool useDepth = true;
            if (highlighterSettings.DepthMask == DepthMask.Disable) useDepth = false;

            foreach (var item in renderersToDraw)
            {
                if (item.useCutout)
                {
                    var materialCutout = CoreUtils.CreateEngineMaterial(Shader.Find("HighlightersURP/ObjectsInfo"));
                    materialCutout.SetTexture("_MainTex", item.GetClipTexture());
                    materialCutout.SetFloat("_Cutoff", item.clippingThreshold);
                    materialCutout.SetInt("useDepth", useDepth ? 1 : 0);
                    materialsToDraw.Add(materialCutout);
                    materialsPassIndexes.Add(((int)item.cullMode));

                }
                else
                {
                    var material = CoreUtils.CreateEngineMaterial(Shader.Find("HighlightersURP/ObjectsInfo"));
                    material.SetInt("useDepth", useDepth ? 1 : 0);
                    materialsToDraw.Add(material);
                    //materialsPassIndexes.Add(((int)item.cullMode));
                    materialsPassIndexes.Add(((int)item.cullMode));
                }
            }
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RenderTextureDescriptor textureDescriptor = cameraTextureDescriptor;
            // TODO: change texture formats to lower values like 8 bits to improve performance
            textureDescriptor.colorFormat = RenderTextureFormat.ARGBFloat; // could be: RGB565 
            textureDescriptor.msaaSamples = 1;
            textureDescriptor.width = Mathf.FloorToInt(textureDescriptor.width * highlighterSettings.InfoRenderScale);
            textureDescriptor.height = Mathf.FloorToInt(textureDescriptor.height * highlighterSettings.InfoRenderScale);

            // this breaks depth testing
            //textureDescriptor.depthBufferBits = 0;

            // see DepthMaskPass.cs for information on why we do not use that
            // RenderingUtils.ReAllocateIfNeeded(ref objectsInfo, textureDescriptor, FilterMode.Point, TextureWrapMode.Clamp, name: objectsInfoName);

            cmd.GetTemporaryRT(Shader.PropertyToID(objectsInfo.name), textureDescriptor, FilterMode.Point);

            ConfigureTarget(objectsInfo);
            ConfigureClear(ClearFlag.All, new Color(0, 0, 0, 0));
            
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler(
                profilingName)))
            {
                if (useSceneDepth) cmd.SetGlobalTexture("_SceneDepthMask", sceneDepthMaskHandle);

                if (renderersToDraw.Count == materialsToDraw.Count)
                {
                    renderingBounds = new Vector4(10, 10, -10, -10);
                    Vector3 rendererCenter = Vector3.zero;

                    var camera = renderingData.cameraData.camera;
                    if (camera != null)
                    {
                        for (int i = 0; i < renderersToDraw.Count; i++)
                        {
                            var item = renderersToDraw[i];
                            if (item.renderer == null || item.renderer.enabled == false) continue;

                            for (int submeshIndex = 0; submeshIndex < item.submeshIndexes.Count; submeshIndex++)
                            {
                                cmd.DrawRenderer(item.renderer, materialsToDraw[i], item.submeshIndexes[submeshIndex], materialsPassIndexes[i]);
                            }

                            
                            var bounds = item.renderer.bounds;
                            var center = bounds.center;
                            var extents = bounds.extents;
                            rendererCenter = center;

                            if (highlighterSettings.RenderingBoundsDistanceFix)
                            {
                                if (RenderingBounds.CloseEnughToRenderFullScreen(camera, center, highlighterSettings.RenderingBoundsMaxDistanceFix, highlighterSettings.RenderingBoundsMinDistanceFix))
                                {
                                    renderingBounds = new Vector4(0, 0, 1, 1);
                                    highlighterSettings.SetRenderBoundsValues(renderingBounds);
                                    continue;
                                }
                            }

                            renderingBounds = RenderingBounds.CalculateBounds(camera, extents, center, renderingBounds, highlighterSettings.RenderingBoundsSizeIncrease);
                        }
                    }

                    float cameraObjectDist = (rendererCenter - camera.transform.position).magnitude;
                    highlighterSettings.CameraObjectDistace = cameraObjectDist;

                    highlighterSettings.SetRenderBoundsValues(renderingBounds);
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
            cmd.ReleaseTemporaryRT(Shader.PropertyToID(objectsInfo.name));
        }
    }
}