// 只写模板缓冲、不输出颜色，配合 CurveFillMask 把面特效裁成曲线形状
Shader "VFX/CurveStencilMask"
{
    Properties
    {
        _Stencil("Stencil Bit", Float) = 64
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            ColorMask 0
            ZWrite Off
            ZTest Always
            Cull Off
            Stencil
            {
                Ref [_Stencil]
                WriteMask [_Stencil]
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 vert(float4 positionOS : POSITION) : SV_POSITION
            {
                return TransformObjectToHClip(positionOS.xyz);
            }

            half4 frag() : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
