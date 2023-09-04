using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;

using Highlighters;

namespace Highlighters_URP
{
    public class OverlayPass : ScriptableRenderPass
    {
        private readonly Material material;
        private RTHandle cameraColorTarget;

        private string profilingName;
        private RenderTargetIdentifier objectsInfoIdentifier;
        private RenderTargetIdentifier meshOutlineIdentifier;
        private bool useMeshOutline = false;

        public OverlayPass(RenderPassEvent renderPassEvent, HighlighterSettings highlighterSettings,string profilingName)
        {
            this.renderPassEvent = renderPassEvent;
            this.profilingName = profilingName;

            //material = CoreUtils.CreateEngineMaterial(Shader.Find("ColorBlit"));
            material = CoreUtils.CreateEngineMaterial(Shader.Find("HighlightersURP/Overlay"));
            highlighterSettings.SetOverlayMaterialProperties(material);
        }

        public void SetupMeshOutlineTarget(RenderTargetIdentifier meshOutlineIdentifier)
        {
            useMeshOutline = true;
            this.meshOutlineIdentifier = meshOutlineIdentifier;
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
            // this breaks shader blending
            // DO NOT USE!!
            //ConfigureTarget(cameraColorTarget);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!material) return;

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler(
                profilingName)))
            {
                cmd.SetGlobalTexture("_ObjectsInfo", objectsInfoIdentifier);
                if (useMeshOutline) cmd.SetGlobalTexture("_MeshOutlineObjects", meshOutlineIdentifier);

                Blitter.BlitCameraTexture(cmd, cameraColorTarget, cameraColorTarget, material, 0);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}