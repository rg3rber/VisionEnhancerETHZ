Shader "Unlit/WebcamShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Zoom ("Zoom", Range(0.0, 1.0)) = 0.0
        _Offset ("Offset", Vector) = (0, 0, 0, 0)
        _Mode ("Mode", Integer) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Zoom;
            float2 _Offset;
            int _Mode;

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 zoom_col = tex2D(_MainTex, i.uv * float2(1.0 - _Zoom, 1.0 - _Zoom) + _Offset + _Zoom * float2(0.5, 0.5));
                fixed4 col = tex2D(_MainTex, i.uv);

                float L = (17.8824 * col.r) + (43.5161 * col.g) + (4.11935 * col.b);
                float M = (3.45565 * col.r) + (27.1554 * col.g) + (3.86714 * col.b);
                float S = (0.0299566 * col.r) + (0.184309 * col.g) + (1.46709 * col.b);

                float l, m, s;
                if (_Mode == 1) //Protanopia
                {
                    l = 0.0 * L + 2.02344 * M + -2.52581 * S;
                    m = 0.0 * L + 1.0 * M + 0.0 * S;
                    s = 0.0 * L + 0.0 * M + 1.0 * S;
                }
                
                if (_Mode == 2) //Deuteranopia
                {
                    l = 1.0 * L + 0.0 * M + 0.0 * S;
                    m = 0.494207 * L + 0.0 * M + 1.24827 * S;
                    s = 0.0 * L + 0.0 * M + 1.0 * S;
                }
                
                if (_Mode == 3) //Tritanopia
                {
                    l = 1.0 * L + 0.0 * M + 0.0 * S;
                    m = 0.0 * L + 1.0 * M + 0.0 * S;
                    s = -0.395913 * L + 0.801109 * M + 0.0 * S;
                }

                fixed4 error;
                error.r = (0.0809444479 * l) + (-0.130504409 * m) + (0.116721066 * s);
                error.g = (-0.0102485335 * l) + (0.0540193266 * m) + (-0.113614708 * s);
                error.b = (-0.000365296938 * l) + (-0.00412161469 * m) + (0.693511405 * s);
                error.a = 1.0;

                fixed4 diff = col - error;
                fixed4 correction;
                correction.r = 0.0;
                correction.g =  (diff.r * 0.7) + (diff.g * 1.0);
                correction.b =  (diff.r * 0.7) + (diff.b * 1.0);
                correction = col + correction * _Zoom;
                correction.a = col.a;

                return _Mode == 0 ? zoom_col : correction;
            }
            ENDCG
        }
    }
}
