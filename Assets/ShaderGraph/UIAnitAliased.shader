Shader "Custom/UIAntiAliased" {
    Properties{
        _MainTex("Texture", 2D) = "white" {}
        _FrameCount("Frame Count", Range(1, 16)) = 1
        _FramesPerSecond("Frames Per Second", Range(1, 60)) = 30
    }

        SubShader{
            Pass {
                Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                sampler2D _MainTex;
                float _FrameCount;
                float _FramesPerSecond;

                struct appdata {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                v2f vert(appdata v) {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                float4 frag(v2f i) : SV_Target {
                    //Calculate the frame index based on the current time
                    float frameIndex = fmod(_Time.y * _FramesPerSecond, _FrameCount);

                    //Calculate the UV coordinates for the current frame
                    float2 frameSize = float2(1.0 / _FrameCount, 1.0);
                    float2 frameOffset = float2(frameIndex / _FrameCount, 0.0);
                    float2 uv = i.uv * frameSize + frameOffset;

                    // Sample the texture with smooth filtering to reduce flickering
                    float4 color = tex2D(_MainTex, uv);
                    color += tex2D(_MainTex, uv + float2(0.003, 0)) * 0.5;
                    color += tex2D(_MainTex, uv + float2(0, 0.003)) * 0.5;
                    color += tex2D(_MainTex, uv - float2(0.003, 0)) * 0.5;
                    color += tex2D(_MainTex, uv - float2(0, 0.003)) * 0.5;
                    return color;
                }
                ENDCG
            }
        }
        FallBack "Diffuse"
}