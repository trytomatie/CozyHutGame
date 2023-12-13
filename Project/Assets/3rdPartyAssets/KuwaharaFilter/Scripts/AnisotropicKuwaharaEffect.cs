using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KuwaharaFilter
{
    public class ConstantBufferVariable
    {
        public int GaussRadius;
        public int KuwaharaRadius;
        public int KuwaharaQ;
        public float KuwaharaAlpha;
        public static void Apply(ComputeShader shader, ConstantBufferVariable buffer)
        {
            shader.SetInt("GaussRadius", buffer.GaussRadius);
            shader.SetInt("KuwaharaRadius", buffer.KuwaharaRadius);
            shader.SetInt("KuwaharaQ", buffer.KuwaharaQ);
            shader.SetFloat("KuwaharaAlpha", buffer.KuwaharaAlpha);
        }
    }

    [RequireComponent(typeof(Camera)), ExecuteInEditMode, ImageEffectAllowedInSceneView]
    public class AnisotropicKuwaharaEffect : MonoBehaviour
    {
        [Range(0, 10), Tooltip("Warning: A value has aт impact on performance")]
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

        private ComputeShader m_GaussComputer;
        private ComputeShader m_SSTComputer;
        private ComputeShader m_TFMComputer;
        private ComputeShader m_LICComputer;
        private ComputeShader m_KuwaharaComputer;

        private RenderTexture m_TextureColor;
        private RenderTexture m_TextureSST;
        private RenderTexture m_TextureGaussHS;
        private RenderTexture m_TextureGaussVS;
        private RenderTexture m_TextureTFM;
        private RenderTexture m_TextureLIC;
        private RenderTexture m_TextureKuwahara;

        private Camera m_Camera;
        private ComputeBuffer m_KernelGaussFilter;
        private ConstantBufferVariable m_ConstantBuffer = new ConstantBufferVariable();

        private void Start()
        {
            m_Camera = GetComponent<Camera>();
            m_GaussComputer = Resources.Load<ComputeShader>("Shaders/ComputerGauss");
            m_SSTComputer = Resources.Load<ComputeShader>("Shaders/ComputerStructureTensor");
            m_TFMComputer = Resources.Load<ComputeShader>("Shaders/ComputerVectorField");
            m_LICComputer = Resources.Load<ComputeShader>("Shaders/ComputerLineIntegralConvolution");
            m_KuwaharaComputer = Resources.Load<ComputeShader>("Shaders/ComputerAnisotropicKuwahara");
        }

        private List<float> GenerateGaussKernel(int radius, float sigma)
        {
            var data = Enumerable.Range(0, 2 * radius + 1).Select(x => Mathf.Exp(-Mathf.Pow(x - radius, 2) / (2.0f * sigma * sigma))).ToList();
            var sum = data.Sum();
            return data.Select(x => x / sum).ToList();
        }

        private void InitializeRenderTexture(int width, int height)
        {
            if (m_TextureColor == null || m_TextureColor.width != width || m_TextureColor.height != height)
            {
                m_TextureColor?.Release();
                m_TextureSST?.Release();
                m_TextureGaussHS?.Release();
                m_TextureGaussVS?.Release();
                m_TextureTFM?.Release();
                m_TextureLIC?.Release();
                m_TextureKuwahara?.Release();

                m_TextureColor = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                m_TextureColor.Create();

                m_TextureSST = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
                m_TextureSST.enableRandomWrite = true;
                m_TextureSST.Create();

                m_TextureGaussHS = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
                m_TextureGaussHS.enableRandomWrite = true;
                m_TextureGaussHS.Create();

                m_TextureGaussVS = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
                m_TextureGaussVS.enableRandomWrite = true;
                m_TextureGaussVS.Create();

                m_TextureTFM = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
                m_TextureTFM.enableRandomWrite = true;
                m_TextureTFM.Create();

                m_TextureLIC = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                m_TextureLIC.enableRandomWrite = true;
                m_TextureLIC.Create();

                m_TextureKuwahara = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                m_TextureKuwahara.enableRandomWrite = true;
                m_TextureKuwahara.Create();
            }
        }

        private void InitializeComputeBuffer()
        {
            var kernel = GenerateGaussKernel(GaussRadius, GaussSigma);
            m_KernelGaussFilter.SetData(kernel);
        }

        private void Update()
        {
            m_ConstantBuffer.GaussRadius = GaussRadius;
            m_ConstantBuffer.KuwaharaRadius = KuwaharaRadius;
            m_ConstantBuffer.KuwaharaAlpha = KuwaharaAlpha;
            m_ConstantBuffer.KuwaharaQ = KuwaharaQ;
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            var width = Mathf.CeilToInt(ResolutionScale * m_Camera.pixelWidth);
            var height = Mathf.CeilToInt(ResolutionScale * m_Camera.pixelHeight);

            InitializeRenderTexture(width, height);
            InitializeComputeBuffer();
            Graphics.Blit(src, m_TextureColor);

            ConstantBufferVariable.Apply(m_SSTComputer, m_ConstantBuffer);
            ConstantBufferVariable.Apply(m_GaussComputer, m_ConstantBuffer);
            ConstantBufferVariable.Apply(m_TFMComputer, m_ConstantBuffer);
            ConstantBufferVariable.Apply(m_LICComputer, m_ConstantBuffer);
            ConstantBufferVariable.Apply(m_KuwaharaComputer, m_ConstantBuffer);

            var kernelStructureTensor = m_SSTComputer.FindKernel("StructureTensor");
            m_SSTComputer.SetTexture(kernelStructureTensor, "TextureColorSRV", m_TextureColor);
            m_SSTComputer.SetTexture(kernelStructureTensor, "TextureColorUAV", m_TextureSST);
            m_SSTComputer.Dispatch(kernelStructureTensor, Mathf.CeilToInt(width / 8.0f), Mathf.CeilToInt(height / 8.0f), 1);

            var kernelGaussHS = m_GaussComputer.FindKernel("GaussHS");
            m_GaussComputer.SetTexture(kernelGaussHS, "TextureColorSRV", m_TextureSST);
            m_GaussComputer.SetTexture(kernelGaussHS, "TextureColorUAV", m_TextureGaussHS);
            m_GaussComputer.SetBuffer(kernelGaussHS, "BufferGaussKernel", m_KernelGaussFilter);
            m_GaussComputer.Dispatch(kernelGaussHS, Mathf.CeilToInt(width / 8.0f), Mathf.CeilToInt(height / 8.0f), 1);

            var kernelGaussVS = m_GaussComputer.FindKernel("GaussVS");
            m_GaussComputer.SetTexture(kernelGaussVS, "TextureColorSRV", m_TextureGaussHS);
            m_GaussComputer.SetTexture(kernelGaussVS, "TextureColorUAV", m_TextureGaussVS);
            m_GaussComputer.SetBuffer(kernelGaussVS, "BufferGaussKernel", m_KernelGaussFilter);
            m_GaussComputer.Dispatch(kernelGaussVS, Mathf.CeilToInt(width / 8.0f), Mathf.CeilToInt(height / 8.0f), 1);

            var kernelVectorField = m_TFMComputer.FindKernel("VectorField");
            m_TFMComputer.SetTexture(kernelVectorField, "TextureColorSRV", m_TextureGaussVS);
            m_TFMComputer.SetTexture(kernelVectorField, "TextureColorUAV", m_TextureTFM);
            m_TFMComputer.Dispatch(kernelVectorField, Mathf.CeilToInt(width / 8.0f), Mathf.CeilToInt(height / 8.0f), 1);

            var kernelLineIntegration = m_LICComputer.FindKernel("LineIntegralConvolution");
            m_LICComputer.SetTexture(kernelLineIntegration, "TextureTFMSRV", m_TextureTFM);
            m_LICComputer.SetTexture(kernelLineIntegration, "TextureColorSRV", src);
            m_LICComputer.SetTexture(kernelLineIntegration, "TextureColorUAV", m_TextureLIC);
            m_LICComputer.SetBuffer(kernelLineIntegration, "BufferGaussKernel", m_KernelGaussFilter);
            m_LICComputer.Dispatch(kernelLineIntegration, Mathf.CeilToInt(width / 8.0f), Mathf.CeilToInt(height / 8.0f), 1);

            var kerneKuwahara = m_KuwaharaComputer.FindKernel("AnisotropicKuwahara");
            m_KuwaharaComputer.SetTexture(kerneKuwahara, "TextureTFMSRV", m_TextureTFM);
            m_KuwaharaComputer.SetTexture(kerneKuwahara, "TextureColorSRV", m_TextureLIC);
            m_KuwaharaComputer.SetTexture(kerneKuwahara, "TextureColorUAV", m_TextureKuwahara);
            m_KuwaharaComputer.Dispatch(kerneKuwahara, Mathf.CeilToInt(width / 8.0f), Mathf.CeilToInt(height / 8.0f), 1);
            Graphics.Blit(m_TextureKuwahara, dst);
        }

        public void OnEnable()
        {
            m_KernelGaussFilter = new ComputeBuffer(64, sizeof(float), ComputeBufferType.Default);
        }

        public void OnDisable()
        {
            if (m_KernelGaussFilter != null)
            {
                m_KernelGaussFilter.Dispose();
                m_KernelGaussFilter = null;
            }
        }
    }
}
