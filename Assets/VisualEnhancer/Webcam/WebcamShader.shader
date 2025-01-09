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

            float4 daltonize(fixed4 input, int mode) {
                if (mode == 0) {
                    return input;
                }
                // RGB to LMS matrix conversion
                float3 L = (17.8824f * input.r) + (43.5161f * input.g) + (4.11935f * input.b);
                float3 M = (3.45565f * input.r) + (27.1554f * input.g) + (3.86714f * input.b);
                float3 S = (0.0299566f * input.r) + (0.184309f * input.g) + (1.46709f * input.b);
                
                // Simulate color blindness
                
                float l, m, s;
                if ( mode == 1) {
                    // Protanope - reds are greatly reduced (1% men)
                    l = 0.0f * L + 2.02344f * M + -2.52581f * S;
                    m = 0.0f * L + 1.0f * M + 0.0f * S;
                    s = 0.0f * L + 0.0f * M + 1.0f * S;
                } else if (mode == 2) {
                    // Deuteranope - greens are greatly reduced (1% men)
                    l = 1.0f * L + 0.0f * M + 0.0f * S;
                    m = 0.494207f * L + 0.0f * M + 1.24827f * S;
                    s = 0.0f * L + 0.0f * M + 1.0f * S;
                } else if (mode == 3) {
                    // Tritanope - blues are greatly reduced (0.003% population)
                    l = 1.0f * L + 0.0f * M + 0.0f * S;
                    m = 0.0f * L + 1.0f * M + 0.0f * S;
                    s = -0.395913f * L + 0.801109f * M + 0.0f * S;
                }
                                
                // LMS to RGB matrix conversion
                float4 error;
                error.r = (0.0809444479f * l) + (-0.130504409f * m) + (0.116721066f * s);
                error.g = (-0.0102485335f * l) + (0.0540193266f * m) + (-0.113614708f * s);
                error.b = (-0.000365296938f * l) + (-0.00412161469f * m) + (0.693511405f * s);
                error.a = 1;

                // return error.rgba;
                
                // Isolate invisible colors to color vision deficiency (calculate error matrix)
                error = (input - error);
                
                // Shift colors towards visible spectrum (apply error modifications)
                float4 correction;
                correction.r = 0; // (error.r * 0.0) + (error.g * 0.0) + (error.b * 0.0);
                correction.g = (error.r * 0.7) + (error.g * 1.0); // + (error.b * 0.0);
                correction.b = (error.r * 0.7) + (error.b * 1.0); // + (error.g * 0.0);
                
                // Add compensation to original values
                correction = input + correction;
                correction.a = input.a;
                
                return correction.rgba;
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 zoom_col = tex2D(_MainTex, i.uv * float2(1.0 - _Zoom, 1.0 - _Zoom) + _Offset + _Zoom * float2(0.5, 0.5));
                // float4 col = tex2D(_MainTex, i.uv);

                float4 corr = daltonize(zoom_col, _Mode);
                return corr;
                // return _Mode == 0 ? zoom_col : corr;
            }
            ENDCG
        }
    }
}
