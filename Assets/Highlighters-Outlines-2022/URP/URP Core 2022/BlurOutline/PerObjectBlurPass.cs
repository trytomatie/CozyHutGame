using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;

using Highlighters;

namespace Highlighters_URP
{
    public class PerObjectBlurPass : ScriptableRenderPass
    {
        private readonly Material blurMaterial;
        private RTHandle cameraColorTarget;

        private RTHandle blurredObjectsBuffer;
        private string profilingName;
        private HighlighterSettings highlighterSettings;

        // Accurate Alpha Blending for two sides depth mask properties
        private RTHandle blurredObjectsBothBuffer;
        private readonly Material alphaBlitMaterial;

        private RenderTargetIdentifier objectsInfoIdentifier;

        // ID holder for reallocating RTHandles
        private int ID;

        #region GaussianBlur

        private int MaxWidth = 50;
        private float[] gaussSamples;

        private float[] GetGaussSamples(int width, float[] samples)
        {
            var stdDev = width * 0.5f;

            if (samples is null)
            {
                samples = new float[MaxWidth];
            }

            for (var i = 0; i < width; i++)
            {
                samples[i] = Gauss(i, stdDev);
            }

            return samples;
        }
        private float Gauss(float x, float stdDev)
        {
            var stdDev2 = stdDev * stdDev * 2;
            var a = 1 / Mathf.Sqrt(Mathf.PI * stdDev2);
            var gauss = a * Mathf.Pow((float)Math.E, -x * x / stdDev2);

            return gauss;
        }
        #endregion

        public PerObjectBlurPass(RenderPassEvent renderPassEvent, HighlighterSettings blurOutlineSettings, int ID, string profilingName)
        {
            this.renderPassEvent = renderPassEvent;
            this.profilingName = profilingName;
            this.highlighterSettings = blurOutlineSettings;
            this.ID = ID;

            blurMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("HighlightersURP/Blur"));
            blurOutlineSettings.SetBlurMaterialProperties(blurMaterial);
            blurMaterial.EnableKeyword("_Variation_" + ID.ToString());

            gaussSamples = GetGaussSamples(50, gaussSamples);
            blurMaterial.SetFloatArray("_GaussSamples", gaussSamples);

            alphaBlitMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("HighlightersURP/AlphaBlit"));
            alphaBlitMaterial.EnableKeyword("_Variation_" + ID.ToString());
            blurOutlineSettings.SetAlphaBlitMaterialProperties(alphaBlitMaterial);
        }

        /// <summary>
        /// Make sure render textures are released when no longer used.
        /// </summary>
        public void Dispose()
        {
            blurredObjectsBuffer?.Release();
            blurredObjectsBothBuffer?.Release();
        }

        public void SetupObjectsTarget(RenderTargetIdentifier objectsInfoIdentifier)
        {
            this.objectsInfoIdentifier = objectsInfoIdentifier;
        }

        public void SetCameraTarget(RTHandle cameraColorTarget)
        {
            this.cameraColorTarget = cameraColorTarget;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor textureDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            textureDescriptor.colorFormat = RenderTextureFormat.RGFloat; // was: RG16  ARGBFloat
            textureDescriptor.msaaSamples = 1;
            textureDescriptor.width = Mathf.FloorToInt(textureDescriptor.width * highlighterSettings.BlurRenderScale);
            textureDescriptor.height = Mathf.FloorToInt(textureDescriptor.height * highlighterSettings.BlurRenderScale);

            textureDescriptor.depthBufferBits = 0;

            // Proper way to use RTHandles / rt needs to be released 
            RenderingUtils.ReAllocateIfNeeded(ref blurredObjectsBuffer, textureDescriptor,
                FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_BluerredObjects_" + ID);
            
            RenderingUtils.ReAllocateIfNeeded(ref blurredObjectsBothBuffer, textureDescriptor,
                FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_BlurredObjectsBoth_" + ID);
            
            // this thorws errors while blitting
            //cmd.GetTemporaryRT(Shader.PropertyToID(blurredObjectsBuffer.name), textureDescriptor, FilterMode.Bilinear);
            //cmd.GetTemporaryRT(Shader.PropertyToID(blurredObjectsBothBuffer.name), textureDescriptor, FilterMode.Bilinear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!blurMaterial) return;

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler(
                profilingName)))
            {
                cmd.SetGlobalTexture("_ObjectsInfo", objectsInfoIdentifier);
                cmd.SetGlobalTexture("_BlurredObjects", blurredObjectsBuffer);
                cmd.SetGlobalTexture("_BlurredObjectsBoth", blurredObjectsBothBuffer);

                // Horizontal blur
                Blitter.BlitCameraTexture(cmd, cameraColorTarget, blurredObjectsBuffer, blurMaterial, 0);

                if (highlighterSettings.BlendingType == HighlighterSettings.BlendType.Alpha)
                {
                    // Use additional buffer for right alpha blending
                    if(highlighterSettings.DepthMask == DepthMask.Both && !highlighterSettings.UseSingleOuterGlow)
                    {
                        // Use VPassAlphaBoth shader Pass
                        //Blit(cmd, blurredObjectsBuffer, blurredObjectsBothBuffer, blurMaterial, 3);
                        Blitter.BlitCameraTexture(cmd, blurredObjectsBuffer, blurredObjectsBothBuffer, blurMaterial, 3);

                        Blitter.BlitCameraTexture(cmd, blurredObjectsBothBuffer, cameraColorTarget, alphaBlitMaterial, 0);
                        Blitter.BlitCameraTexture(cmd, blurredObjectsBothBuffer, cameraColorTarget, alphaBlitMaterial, 1);

                        //Blit(cmd,blurredObjectsBothBuffer, cameraColorTarget, alphaBlitMaterial, 0); // Front Blit
                        //Blit(cmd,blurredObjectsBothBuffer, cameraColorTarget, alphaBlitMaterial, 1); // Back Blit
                    }
                    else
                    {
                        Blitter.BlitCameraTexture(cmd, blurredObjectsBuffer, cameraColorTarget, blurMaterial, 1);
                        //Blit(cmd, blurredObjectsBuffer, cameraColorTarget, blurMaterial, 1);
                    }
                }
                else // Don't do anything fancy when BlendType is additive 
                {
                    Blitter.BlitCameraTexture(cmd, blurredObjectsBuffer, cameraColorTarget, blurMaterial, 2);
                }

            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}