Shader "Hidden/GrassPaint/PaintBrushShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always


        Pass
        {
            Name "HeightPainting"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _MouseInfo; //xy - mouse coordinates, z - aspect ratio, w - rotation (not used yet!)
            float4 _BrushSettings; //x - brush size, y - softness, z - strength, w - is reverse
            float4 _BrushColor; // 0 - nothing (no mouse press)
            float4 _ActiveChannel;
            sampler2D _BrushStroke;
            int _UseCustomStroke;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                // just invert the colors

                //finding the sdfs:

                //compute stroke uv:
                float2 hardDistance =  _MouseInfo.xy - i.uv;//circle sdf (reversed)
                hardDistance.x *= _MouseInfo.z;
                fixed distanceFromCenter = length(hardDistance);
                fixed baseSDF = _BrushSettings.x - distanceFromCenter;

                fixed smoothnessBounds = (1.0 - _BrushSettings.y) * _BrushSettings.x;
                fixed alpha = saturate(distanceFromCenter - _BrushSettings.x * _BrushSettings.y);
                alpha /= smoothnessBounds;
                alpha = saturate(1 - alpha);
                alpha *= _BrushSettings.z * _BrushSettings.w;
                //float2 softnessDistance =  _MouseInfo.xy - i.uv;
                //softnessDistance.x *= _MouseInfo.z;
                //fixed softnessSDF = saturate(_BrushSettings.y * _BrushSettings.x - length(softnessDistance));
                //fixed circle = saturate(sign(baseSDF));

                //fixed smoothCircle = lerp(0.0, )

                


                //col.rgb = 1 - col.rgb;
                return lerp(col, 0.0, alpha);//saturate(col + circle * _BrushSettings.w);
            }
            ENDCG
        }
    }
}
