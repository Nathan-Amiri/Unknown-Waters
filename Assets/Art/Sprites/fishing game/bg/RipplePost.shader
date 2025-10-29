Shader "Hidden/RipplePost"
{
    Properties{
        _NoiseTex ("Noise", 2D) = "gray" {}
        _Strength ("Strength", Float) = 0.02
        _Speed    ("Speed", Float) = 0.5
        _Tiling   ("Tiling", Float) = 2.0
    }
    SubShader{
        Tags{ "Queue"="Overlay" }
        Cull Off ZWrite Off ZTest Always
        Pass{
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float _Strength, _Speed, _Tiling;

            struct v2f{ float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };
            v2f vert(appdata_img v){ v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.texcoord; return o; }

            fixed4 frag(v2f i):SV_Target{
                float2 nUV = i.uv * _Tiling + float2(_Time.y*_Speed, _Time.y*_Speed*0.7);
                float2 n = (tex2D(_NoiseTex, nUV).rg - 0.5) * 2.0; // -1..1
                float2 uv = i.uv + n * _Strength;
                return tex2D(_MainTex, uv);
            }
            ENDHLSL
        }
    }
}
