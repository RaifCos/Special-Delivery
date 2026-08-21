Shader "UI/Opening"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0,0,0,1)
        _Center ("Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius", Range(0,1.5)) = 0
        _Feather ("Feather", Range(0.0001,0.2)) = 0.01
        _AspectRatio ("Screen Aspect (W/H)", Float) = 1.78
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t { float4 vertex:POSITION; float2 texcoord:TEXCOORD0; };
            struct v2f { float4 vertex:SV_POSITION; float2 texcoord:TEXCOORD0; };

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                return OUT;
            }

            fixed4 _Color;
            float2 _Center;
            float _Radius;
            float _Feather;
            float _AspectRatio;

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord - _Center;
                uv.x *= _AspectRatio; // correct for non-square panel so circle stays round

                float dist = length(uv);
                float alpha = smoothstep(_Radius, _Radius + _Feather, dist);

                return fixed4(_Color.rgb, _Color.a * alpha);
            }
            ENDCG
        }
    }
}