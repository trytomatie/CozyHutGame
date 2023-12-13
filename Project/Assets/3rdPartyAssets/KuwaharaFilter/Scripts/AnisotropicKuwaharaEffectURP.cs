using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KuwaharaFilterURP
{
    [Serializable]
    public class KuwaharaFilterSettings
    {
        [Range(0, 10), Tooltip("Warning: A value has an impact on performance")]
        public int GaussRadius = 5;

        [Range(0.1f, 10.0f)]
        public float GaussSigma = 8.0f;

        [Range(0.0f, 10.0f)]
        public float KuwaharaAlpha = 1.0f;

        [Range(0, 5), Tooltip("Warning: A value has aт impact on performance")]
        public int KuwaharaRadius = 2;

        [Range(1, 20)]
        public int KuwaharaQ = 8;

        [Range(0.1f, 1.0f), Tooltip("Warning: A value has aт impact on performance")]
        public float ResolutionScale = 1.0f;

        public RenderPassEvent RenderPassEvent = RenderPassEvent.AfterRendering;

    }

    public class AnisotropicKuwaharaEffectURP : ScriptableRendererFeature
    {
        private KuwaharaFilterRenderPass m_KuwaharaFilterRenderPass;

        public KuwaharaFilterSettings Settings = new KuwaharaFilterSettings();

        private ComputeBuffer m_KernelGaussFilter;
        public override void Create()
        {
            m_KuwaharaFilterRenderPass = new KuwaharaFilterRenderPass(Settings, m_KernelGaussFilter);
            name = "AnisotropicKuwaharaFilter";
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            m_KuwaharaFilterRenderPass.Setup(renderer.cameraColorTarget, renderer.cameraColorTarget);
            renderer.EnqueuePass(m_KuwaharaFilterRenderPass);
        }

        public void OnEnable()
        {
            m_KernelGaussFilter = new ComputeBuffer(64, sizeof(float), ComputeBufferType.Default);
        }

        public void OnDisable()
        {
            m_KernelGaussFilter.Release();
        }
    }

    public class KuwaharaFilterRenderPass : ScriptableRenderPass
    {
        private ComputeShader m_GaussComputer;
        private ComputeShader m_SSTComputer;
        private ComputeShader m_TFMComputer;
        private ComputeShader m_LICComputer;
        private ComputeShader m_KuwaharaComputer;
        private ComputeBuffer m_KernelGaussFilter;

        private KuwaharaFilterSettings m_Settings;

        RenderTargetHandle m_TextureColor = new RenderTargetHandle();
        RenderTargetHandle m_TextureSST = new RenderTargetHandle();
        RenderTargetHandle m_TextureGaussVS = new RenderTargetHandle();
        RenderTargetHandle m_TextureGaussHS = new RenderTargetHandle();
        RenderTargetHandle m_TextureTFM = new RenderTargetHandle();
        RenderTargetHandle m_TextureLIC = new RenderTargetHandle();
        RenderTargetHandle m_TextureKuwahara = new RenderTargetHandle();

        RenderTargetIdentifier m_TextureSrc;
        RenderTargetIdentifier m_TextureDst;

        public KuwaharaFilterRenderPass(KuwaharaFilterSettings variables, ComputeBuffer computeBuffer)
        {
            m_SSTComputer = Resources.Load<ComputeShader>("Shaders/ComputerStructureTensor");
            m_GaussComputer = Resources.Load<ComputeShader>("Shaders/ComputerGauss");
            m_TFMComputer = Resources.Load<ComputeShader>("Shaders/ComputerVectorField");
            m_LICComputer = Resources.Load<ComputeShader>("Shaders/ComputerLineIntegralConvolution");
            m_KuwaharaComputer = Resources.Load<ComputeShader>("Shaders/ComputerAnisotropicKuwahara");
            m_TextureColor.Init("CLR");
            m_TextureSST.Init("SST");
            m_TextureGaussVS.Init("GVS");
            m_TextureGaussHS.Init("GHS");
            m_TextureTFM.Init("TFM");
            m_TextureLIC.Init("LIC");
            m_TextureKuwahara.Init("KWH");

            m_Settings = variables;
            renderPassEvent = m_Settings.RenderPassEvent;
            m_KernelGaussFilter = computeBuffer;
        }

        private List<float> GenerateGaussKernel(int radius, float sigma)
        {
            var data = Enumerable.Range(0, 2 * radius + 1).Select(x => Mathf.Exp(-Mathf.Pow(x - radius, 2) / (2.0f * sigma * sigma))).ToList();
            var sum = data.Sum();
            return data.Select(x => x / sum).ToList();
        }

        private void InitializeComputeBuffer(int radius, float sigma)
        {
            var kernel = GenerateGaussKernel(radius, sigma);
            m_KernelGaussFilter.SetData(kernel);
        }

        public void Setup(RenderTargetIdentifier textureSrc, RenderTargetIdentifier textureDst)
        {
            m_TextureSrc = textureSrc;
            m_TextureDst = textureDst;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor desc)
        {
            InitializeComputeBuffer(m_Settings.GaussRadius, m_Settings.GaussSigma);

            var width = Mathf.CeilToInt(m_Settings.ResolutionScale * desc.width);
            var height = Mathf.CeilToInt(m_Settings.ResolutionScale * desc.height);

            cmd.GetTemporaryRT(m_TextureColor.id, width, height, 0, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, 1, true);
            cmd.GetTemporaryRT(m_TextureSST.id, width, height, 0, FilterMode.Point, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear, 1, true);
            cmd.GetTemporaryRT(m_TextureGaussHS.id, width, height, 0, FilterMode.Point, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear, 1, true);
            cmd.GetTemporaryRT(m_TextureGaussVS.id, width, height, 0, FilterMode.Point, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear, 1, true);
            cmd.GetTemporaryRT(m_TextureTFM.id, width, height, 0, FilterMode.Point, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear, 1, true);
            cmd.GetTemporaryRT(m_TextureLIC.id, width, height, 0, FilterMode.Point, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear, 1, true);
            cmd.GetTemporaryRT(m_TextureKuwahara.id, width, height, 0, FilterMode.Point, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear, 1, true);

            ConfigureTarget(m_TextureColor.id);
            ConfigureTarget(m_TextureSST.id);
            ConfigureTarget(m_TextureGaussHS.id);
            ConfigureTarget(m_TextureGaussVS.id);
            ConfigureTarget(m_TextureTFM.id);
            ConfigureTarget(m_TextureLIC.id);
            ConfigureTarget(m_TextureKuwahara.id);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(m_TextureColor.id);
            cmd.ReleaseTemporaryRT(m_TextureSST.id);
            cmd.ReleaseTemporaryRT(m_TextureGaussHS.id);
            cmd.ReleaseTemporaryRT(m_TextureGaussVS.id);
            cmd.ReleaseTemporaryRT(m_TextureTFM.id);
            cmd.ReleaseTemporaryRT(m_TextureLIC.id);
            cmd.ReleaseTemporaryRT(m_TextureKuwahara.id);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var width = Mathf.CeilToInt(m_Settings.ResolutionScale * renderingData.cameraData.cameraTargetDescriptor.width);
            var height = Mathf.CeilToInt(m_Settings.ResolutionScale * renderingData.cameraData.cameraTargetDescriptor.height);

            InitializeComputeBuffer(m_Settings.GaussRadius, m_Settings.GaussSigma);

            var commandBuffer = CommandBufferPool.Get("Kuwahara Filter");
            Blit(commandBuffer, m_TextureSrc, m_TextureColor.id);
            {
                var kernel = m_SSTComputer.FindKernel("StructureTensor");
                m_SSTComputer.GetKernelThreadGroupSizes(kernel, out var groupSizeX, out var groupSizeY, out var groupSizeZ);

                commandBuffer.SetComputeTextureParam(m_SSTComputer, kernel, "TextureColorSRV", m_TextureColor.id);
                commandBuffer.SetComputeTextureParam(m_SSTComputer, kernel, "TextureColorUAV", m_TextureSST.id);
                commandBuffer.DispatchCompute(m_SSTComputer, kernel, Mathf.CeilToInt(width / (float)groupSizeX), Mathf.CeilToInt(height / (float)groupSizeY), 1);
            }

            {
                var kernel = m_GaussComputer.FindKernel("GaussHS");
                m_GaussComputer.GetKernelThreadGroupSizes(kernel, out var groupSizeX, out var groupSizeY, out var groupSizeZ);

                commandBuffer.SetComputeIntParam(m_GaussComputer, "GaussRadius", m_Settings.GaussRadius);
                commandBuffer.SetComputeTextureParam(m_GaussComputer, kernel, "TextureColorSRV", m_TextureSST.id);
                commandBuffer.SetComputeTextureParam(m_GaussComputer, kernel, "TextureColorUAV", m_TextureGaussHS.id);
                commandBuffer.SetComputeBufferParam(m_GaussComputer, kernel, "BufferGaussKernel", m_KernelGaussFilter);
                commandBuffer.DispatchCompute(m_GaussComputer, kernel, Mathf.CeilToInt(width / (float)groupSizeX), Mathf.CeilToInt(height / (float)groupSizeY), 1);
            }

            {
                var kernel = m_GaussComputer.FindKernel("GaussVS");
                m_GaussComputer.GetKernelThreadGroupSizes(kernel, out var groupSizeX, out var groupSizeY, out var groupSizeZ);

                commandBuffer.SetComputeIntParam(m_GaussComputer, "GaussRadius", m_Settings.GaussRadius);
                commandBuffer.SetComputeTextureParam(m_GaussComputer, kernel, "TextureColorSRV", m_TextureGaussHS.id);
                commandBuffer.SetComputeTextureParam(m_GaussComputer, kernel, "TextureColorUAV", m_TextureGaussVS.id);
                commandBuffer.SetComputeBufferParam(m_GaussComputer, kernel, "BufferGaussKernel", m_KernelGaussFilter);
                commandBuffer.DispatchCompute(m_GaussComputer, kernel, Mathf.CeilToInt(width / (float)groupSizeX), Mathf.CeilToInt(height / (float)groupSizeY), 1);
            }

            {
                var kernel = m_TFMComputer.FindKernel("VectorField");
                m_TFMComputer.GetKernelThreadGroupSizes(kernel, out var groupSizeX, out var groupSizeY, out var groupSizeZ);

                commandBuffer.SetComputeTextureParam(m_TFMComputer, kernel, "TextureColorSRV", m_TextureGaussVS.id);
                commandBuffer.SetComputeTextureParam(m_TFMComputer, kernel, "TextureColorUAV", m_TextureTFM.id);
                commandBuffer.DispatchCompute(m_TFMComputer, kernel, Mathf.CeilToInt(width / (float)groupSizeX), Mathf.CeilToInt(height / (float)groupSizeY), 1);
            }

            {
                var kernel = m_LICComputer.FindKernel("LineIntegralConvolution");
                m_LICComputer.GetKernelThreadGroupSizes(kernel, out var groupSizeX, out var groupSizeY, out var groupSizeZ);

                commandBuffer.SetComputeIntParam(m_LICComputer, "GaussRadius", m_Settings.GaussRadius);
                commandBuffer.SetComputeTextureParam(m_LICComputer, kernel, "TextureTFMSRV", m_TextureTFM.id);
                commandBuffer.SetComputeTextureParam(m_LICComputer, kernel, "TextureColorSRV", m_TextureSrc);
                commandBuffer.SetComputeTextureParam(m_LICComputer, kernel, "TextureColorUAV", m_TextureLIC.id);
                commandBuffer.SetComputeBufferParam(m_LICComputer, kernel, "BufferGaussKernel", m_KernelGaussFilter);
                commandBuffer.DispatchCompute(m_LICComputer, kernel, Mathf.CeilToInt(width / (float)groupSizeX), Mathf.CeilToInt(height / (float)groupSizeY), 1);
            }

            {
                var kernel = m_KuwaharaComputer.FindKernel("AnisotropicKuwahara");
                m_KuwaharaComputer.GetKernelThreadGroupSizes(kernel, out var groupSizeX, out var groupSizeY, out var groupSizeZ);

                commandBuffer.SetComputeIntParam(m_KuwaharaComputer, "KuwaharaRadius", m_Settings.KuwaharaRadius);
                commandBuffer.SetComputeIntParam(m_KuwaharaComputer, "KuwaharaQ", m_Settings.KuwaharaQ);
                commandBuffer.SetComputeFloatParam(m_KuwaharaComputer, "KuwaharaAlpha", m_Settings.KuwaharaAlpha);

                commandBuffer.SetComputeTextureParam(m_KuwaharaComputer, kernel, "TextureTFMSRV", m_TextureTFM.id);
                commandBuffer.SetComputeTextureParam(m_KuwaharaComputer, kernel, "TextureColorSRV", m_TextureLIC.id);
                commandBuffer.SetComputeTextureParam(m_KuwaharaComputer, kernel, "TextureColorUAV", m_TextureKuwahara.id);
                commandBuffer.DispatchCompute(m_KuwaharaComputer, kernel, Mathf.CeilToInt(width / (float)groupSizeX), Mathf.CeilToInt(height / (float)groupSizeY), 1);

            }

            Blit(commandBuffer, m_TextureKuwahara.id, m_TextureDst);
            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }
    }
}
