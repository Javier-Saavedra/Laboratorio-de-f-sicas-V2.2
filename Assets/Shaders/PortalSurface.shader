Shader "Portal/PortalSurface"
{
    Properties
    {
        [HDR] _RimColor ("Color del borde", Color) = (0.1, 0.55, 1.0, 1.0)
        _PortalTex ("Vista del otro lado", 2D) = "black" {}
        _InnerTint ("Color interior sin vista", Color) = (0.02, 0.03, 0.05, 1.0)
        _Open ("Apertura", Range(0, 1)) = 1
        _HasView ("Hay vista", Range(0, 1)) = 0
        _InnerRadius ("Radio interior", Range(0.5, 0.95)) = 0.80
        _RimWidth ("Grosor del borde", Range(0.005, 0.2)) = 0.045
        _GlowSize ("Tamano del resplandor", Range(0.01, 0.3)) = 0.10
        _RimIntensity ("Intensidad del borde", Range(0, 6)) = 2.5
        _NoiseSpeed ("Velocidad de ondulacion", Float) = 1.6
        _NoiseAmount ("Ondulacion del borde", Range(0, 0.05)) = 0.014
        _FlipY ("Invertir Y de la vista", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "PortalSurface"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_PortalTex);
            SAMPLER(sampler_PortalTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _RimColor;
                half4 _InnerTint;
                half _Open;
                half _HasView;
                half _InnerRadius;
                half _RimWidth;
                half _GlowSize;
                half _RimIntensity;
                half _NoiseSpeed;
                half _NoiseAmount;
                half _FlipY;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vi = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = vi.positionCS;
                OUT.screenPos = vi.positionNDC;
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN, FRONT_FACE_TYPE face : FRONT_FACE_SEMANTIC) : SV_Target
            {
                bool frontal = IS_FRONT_VFACE(face, true, false);

                // Coordenadas centradas: el borde del quad es 1 en cada eje, asi el borde sale ovalado
                float2 p = (IN.uv - 0.5) * 2.0;
                float open = max(_Open, 0.001);
                float ang = atan2(p.y, p.x);
                float t = _Time.y * _NoiseSpeed;

                // Ondulacion suave del borde (senos con frecuencia entera para que no haya costura)
                float n = sin(ang * 5.0 + t) * 0.5 + sin(ang * 9.0 - t * 1.37) * 0.3 + sin(ang * 14.0 + t * 0.83) * 0.2;

                float rr = length(p) / open + n * _NoiseAmount;

                float R = _InnerRadius;
                float W = _RimWidth;
                float G = _GlowSize;

                float s1 = smoothstep(R - 0.012, R + 0.006, rr);
                float s2 = smoothstep(R + W, R + W + 0.03, rr);
                float inside = 1.0 - s1;
                float band = s1 * (1.0 - s2);
                float outside = s2;
                float glowT = saturate((rr - (R + W + 0.03)) / G);
                float glow = outside * (1.0 - glowT) * (1.0 - glowT);

                // Vista del otro lado (coordenadas de pantalla)
                float2 suv = IN.screenPos.xy / max(IN.screenPos.w, 1e-5);
                suv.y = lerp(suv.y, 1.0 - suv.y, _FlipY);
                half3 view = SAMPLE_TEXTURE2D(_PortalTex, sampler_PortalTex, suv).rgb;

                half hasView = frontal ? _HasView : 0.0h;
                half3 inner = lerp(_InnerTint.rgb, view, hasView);

                // El borde tine un poco el interior, como un halo hacia adentro
                float innerGlow = smoothstep(R - 0.22, R, rr);
                inner = lerp(inner, _RimColor.rgb * 1.2, innerGlow * 0.40);

                float pulse = 0.85 + 0.15 * sin(t * 2.3 + ang * 3.0);
                half3 ringCol = _RimColor.rgb * _RimIntensity * (0.9 + 0.2 * n) * pulse;
                half3 glowCol = _RimColor.rgb * (_RimIntensity * 0.6);

                if (!frontal)
                {
                    ringCol *= 0.45h;
                    glowCol *= 0.45h;
                }

                float glowAlpha = glow * 0.75;
                float alpha = saturate(inside + band + glowAlpha);
                half3 col = (inner * inside + ringCol * band + glowCol * glowAlpha) / max(alpha, 1e-4);

                return half4(col, alpha);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
