Shader "Custom/NeonGlow"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1, 0, 1, 1)
        _GlowColor ("Glow Color", Color) = (0, 1, 1, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 10)) = 2
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 2
        _RimPower ("Rim Power", Range(0.5, 8)) = 3
        _EmissionStrength ("Emission Strength", Range(0, 5)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        fixed4 _GlowColor;
        float _GlowIntensity;
        float _PulseSpeed;
        float _RimPower;
        float _EmissionStrength;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
            float3 worldNormal;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Base texture
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;

            // Calculate rim lighting
            float rim = 1.0 - saturate(dot(normalize(IN.viewDir), IN.worldNormal));
            rim = pow(rim, _RimPower);

            // Pulsing effect
            float pulse = (sin(_Time.y * _PulseSpeed) + 1) * 0.5;
            pulse = lerp(0.7, 1.0, pulse);

            // Emission with glow
            float3 emission = _GlowColor.rgb * rim * _GlowIntensity * pulse;
            emission += c.rgb * _EmissionStrength;
            
            o.Emission = emission;
            o.Metallic = 0.5;
            o.Smoothness = 0.8;
            o.Alpha = c.a;
        }
        ENDCG
    }

    // Fallback for older hardware
    FallBack "Diffuse"
}
