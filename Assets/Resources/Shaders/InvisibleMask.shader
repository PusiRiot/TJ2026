Shader "Custom/InvisibleMask"
{
    SubShader
    {
        // Render AFTER the opaque world (Floor, walls), but BEFORE transparents
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Queue"="AlphaTest" }

        Pass
        {
            ColorMask 0
            ZWrite On
            ZTest LEqual
        }
    }
}
