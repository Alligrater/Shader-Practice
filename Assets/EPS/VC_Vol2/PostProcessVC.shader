Shader "Hidden/PostProcessing/PostProcessVC"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        CGINCLUDE
            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float3 _VBoxMin;
            float3 _VBoxMax;
            float _DistanceStep;
            float _DensityMultiplier;
            float2 trace_vbox_planes(float3 cameraPos, float3 oneOverCameraVector){
                float3 hitT0 = (_VBoxMin - cameraPos) * oneOverCameraVector;
                float3 hitT1 = (_VBoxMax - cameraPos) * oneOverCameraVector;

                float3 minT = min(hitT0, hitT1);
                float3 maxT = max(hitT0, hitT1);

                float dstA = max(max(minT.x, minT.y), minT.z);
                float dstB = min(min(maxT.x, maxT.y), maxT.z);

                float dstToBox = max(0, dstA);//if you are inside the box, this returns 0
                float dstInBox = max(0, dstB - dstToBox);
                return float2(dstToBox, dstInBox);
            }


        ENDCG

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 viewVector : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
                o.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
                return o;
            }





            fixed4 frag (v2f i) : SV_Target
            {
                fixed3 viewVector = i.viewVector;
                fixed3 normalizedVector = normalize(viewVector);
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                float linearDepth = LinearEyeDepth(depth);
                /*
                float3 worldPos = reconstruct_worldpos(i.uv).xyz;
                float3 cameraVector = normalize(worldPos - _WorldSpaceCameraPos);*/
                //float3 worldPos = _WorldSpaceCameraPos + i.viewVector * linearDepth;
                //now, test for whether you hit the box or not.
                float2 vboxHitInfo = trace_vbox_planes(_WorldSpaceCameraPos, 1 / viewVector);
                float dstToBox = vboxHitInfo.x;
                float dstInBox = vboxHitInfo.y;
                //take the union with the scene depth:
                //return dstToBox;
                //float dstToFront = 
                float dstToBoxBack = min(dstInBox + dstToBox, linearDepth);
                float isRayHittingBox = sign(dstInBox);

                float distanceStep = max(dstToBoxBack - dstToBox, 0) / 32;

                float dstTravelled = 0.0f;
                float transmission = 1.0f;
                float3 headPos = _WorldSpaceCameraPos + dstToBox * viewVector;
                for(int step = 0; step < 32; step++){
                    if(dstTravelled > dstToBoxBack){
                        break;
                    }
                    if(transmission < 0.01){
                        break; //too occluded to do anything
                    }
                    transmission *= exp(-_DensityMultiplier * distanceStep);
                    headPos += distanceStep * normalizedVector;
                    dstTravelled += distanceStep;
                }


                fixed4 col = tex2D(_MainTex, i.uv);
                return lerp(col, float4(1.0, 1.0, 1.0, 1.0), (1 - transmission) * isRayHittingBox);

                // just invert the colors
                //col.rgb = 1 - col.rgb;
                return col;
            }
            ENDCG
        }
    }
}
