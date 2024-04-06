// Converts a 180° FOV fisheye image into a 180° panorama projection.
// Assumes there are two fisheyes on the image, left to right.
// The fisheye may have black borders on the side, as long as each eye is
// properly centered. The output will not have these borders.

Shader "Hidden/Fish2Panorama" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _AspectInv ("Inverse Aspect ratio (h/w)", Float) = 1
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

            #define PI 3.14159265

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
            // I have *no* idea why, but calculating the aspect ratio
            // with this *somewhy* results in values that are.. off.
            // Like, by 10%, very noticable.
            //float4 _MainTex_TexelSize;
            float _AspectInv;

            // With uvs that assume [0,1]^2 is exactly the square containing the
            // fish-eye, returns similar [0,1]^2 where to sample the fisheye texture.
            float2 calculate_sample_pos(float2 uv) {
                // From https://paulbourke.net/dome/fish2/
                // (Minor modifications for fewer ops and
                //  wikipedia-consistency.)
                
                // Orthographically project onto a halfsphere
                float2 angles = PI * uv;
                float2 sins, coss;
                sincos(angles, sins, coss);

                float3 psph;
                psph.x = sins.y * coss.x;
                psph.y = sins.y * sins.x;
                psph.z = coss.y;

                // Somewhat reminiscent to mercator shit I did once
                // (Less cylindrically and more spherical though)
                float theta = atan2(psph.z, psph.x);
                float phi = atan2(length(psph.xz), psph.y);
                float r = phi / PI;
                
                float s,c;
                sincos(theta, s, c);
                return 0.5 - r * float2(c,s);
            }

            fixed4 frag (v2f i) : SV_Target {
                // Convert each half to a [0,1]^2 representing the fisheye part.
                // For this we need the aspect ratio of each half.
                float2 fish_uv = frac(i.uv * float2(2, 1));
                float2 target_uv = calculate_sample_pos(fish_uv);
                target_uv.x *= _AspectInv;
                target_uv.x += (1 - _AspectInv) * 0.5;

                // This target uv is wrt its own half. Go back to the full image and sample.
                target_uv.x += i.uv.x >= 0.5;
                target_uv.x *= 0.5;
               
                float4 col = tex2D(_MainTex, target_uv);
                // if (any(fish_uv < 0 | fish_uv > 1))
                //     col = 0;
                return col;
            }
            ENDCG
        }
    }
}
