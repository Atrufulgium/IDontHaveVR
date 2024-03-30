// Grabs a portion of _MainTex and puts it into a portion of the destination.
// The rest is filled with _FillColor.

Shader "Hidden/BlitPart" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _UVSrcMinMax ("Source MinUV MaxUV", Vector) = (0,0,1,1)
        _UVDstMinMax ("Dst MinUV MaxUV", Vector) = (0,0,1,1)
        _FillColor ("Fill Col Outside UV", Color) = (0,0,0,1)
    }
    SubShader {
        Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend One OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _UVSrcMinMax;
            float4 _UVDstMinMax;
            float4 _FillColor;

            float4 frag (v2f i) : SV_Target {
                float2 uv = i.uv;
                // Rescale s.t. only _UVDstMinMax is in [0,1]^2
                uv = (uv - _UVDstMinMax.xy) / (_UVDstMinMax.zw - _UVDstMinMax.xy);
                // Use fill color if not inside [0,1]^2.
                bool useFillColor = any(uv < 0 | uv > 1);
                // Now rescale the [0,1]^2 onto _UVSrcMinMax
                uv = uv * (_UVSrcMinMax.zw - _UVSrcMinMax.xy) + _UVSrcMinMax.xy;

                float4 col = tex2D(_MainTex, uv);
                if (useFillColor)
                    col = _FillColor;
                return col;
            }
            ENDCG
        }
    }
}
