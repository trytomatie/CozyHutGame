Shader "Universal Render Pipeline/Unlit/Clouds"
{
    Properties
    {
        _Color("Tint", Color) = (1,1,1,1)
        _Noise("Noise (RGB)", 2D) = "gray" {}
        _TColor("Cloud Top Color", Color) = (1,0.6,1,1)
        _CloudColor("Cloud Base Color", Color) = (0.6,1,1,1)
        _RimColor("Rim Color", Color) = (0.6,1,1,1)
        _RimPower("Rim Power", Range(0,40)) = 20
        _Scale("World Scale", Range(0,0.1)) = 0.004
        _AnimSpeedX("Animation Speed X", Range(-2,2)) = 1
        _AnimSpeedY("Animation Speed Y", Range(-2,2)) = 1
        _AnimSpeedZ("Animation Speed Z", Range(-2,2)) = 1
        _Height("Noise Height", Range(0,2)) = 0.8
        _Strength("Noise Emission Strength", Range(0,2)) = 0.3
    }

        SubShader
        {
            Tags{ "Queue" = "Overlay" }
            LOD 100

            CGPROGRAM
            #pragma surface surf Standard

            sampler2D _Noise;
            float4 _Color, _CloudColor, _TColor, _RimColor;
            float _Scale, _Strength, _RimPower, _Height;
            float _AnimSpeedX, _AnimSpeedY, _AnimSpeedZ;

            struct Input
            {
                float3 viewDir;
                float4 noiseComb;
                float4 col;
            };

            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                float3 worldSpaceNormal = normalize(mul((float3x3)unity_ObjectToWorld, IN.worldNormal));
                float3 worldNormalS = saturate(pow(worldSpaceNormal * 1.4, 4));

                float movementSpeedX = _Time.x * _AnimSpeedX;
                float movementSpeedY = _Time.x * _AnimSpeedY;
                float movementSpeedZ = _Time.x * _AnimSpeedZ;

                float4 xy = float4((IN.worldPos.x * _Scale) - movementSpeedX, (IN.worldPos.y * _Scale) - movementSpeedY, 0, 0);
                float4 xz = float4((IN.worldPos.x * _Scale) - movementSpeedX, (IN.worldPos.z * _Scale) - movementSpeedZ, 0, 0);
                float4 yz = float4((IN.worldPos.y * _Scale) - movementSpeedY, (IN.worldPos.z * _Scale) - movementSpeedZ, 0, 0);

                float4 noiseXY = tex2D(_Noise, xy);
                float4 noiseXZ = tex2D(_Noise, xz);
                float4 noiseYZ = tex2D(_Noise, yz);

                IN.noiseComb = noiseXY;
                IN.noiseComb = lerp(IN.noiseComb, noiseXZ, worldNormalS.y);
                IN.noiseComb = lerp(IN.noiseComb, noiseYZ, worldNormalS.x);

                IN.worldPos.xyz += (IN.worldNormal * (IN.noiseComb * _Height));

                o.EmissionColor = _RimColor.rgb * pow(1.0 - saturate(dot(normalize(IN.viewDir), normalize(IN.worldNormal) * (IN.noiseComb * _Strength))), _RimPower);
                o.BaseColor = lerp(_CloudColor, _TColor, IN.worldPos.y) * _Color;
            }
            ENDCG
        }

            Fallback "Diffuse"
}