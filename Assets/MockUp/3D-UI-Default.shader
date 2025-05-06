Shader "UI/StereoSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [KeywordEnum(None, Top_Bottom, Left_Right)] Stereo("Stereo Mode", Float) = 0
        [KeywordEnum(None, Left, Right)] ForceEye ("Force Eye Mode", Float) = 0
        [Toggle(STEREO_DEBUG)] _StereoDebug("Stereo Debug Tinting", Float) = 0
        [Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
        
        // Make this shader compatible with UI Masks
        [HideInInspector] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            
            // Stereo rendering keywords
            #pragma multi_compile_local __ MONOSCOPIC STEREO_TOP_BOTTOM STEREO_LEFT_RIGHT
            #pragma multi_compile_local __ FORCEEYE_NONE FORCEEYE_LEFT FORCEEYE_RIGHT
            #pragma multi_compile_local __ APPLY_GAMMA
            #pragma multi_compile_local __ STEREO_DEBUG

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            
            // Functions from AVProVideo.cginc
            inline bool IsStereoEyeLeft()
            {
                #if FORCEEYE_LEFT
                    return true;
                #elif FORCEEYE_RIGHT
                    return false;
                #elif UNITY_SINGLE_PASS_STEREO
                    // Unity 5.4 has this new variable
                    return (unity_StereoEyeIndex == 0);
                #else
                    // In older versions of Unity or if not using Single-Pass Stereo Rendering
                    // we have to use the following hack, reading a built-in shader variable
                    // _unity_StereoEyeIndex
                    #if defined(UNITY_DECLARE_MULTIVIEW)
                        // OVR_multiview extension for Gear VR/Oculus Go SDK
                        return unity_StereoEyeIndex == 0;
                    #elif defined(SHADER_API_PSSL)
                        // PSVR
                        // This is imprecise because there's no direct correlation between eye and frame indices.
                        // _PSVRIsForeground wasn't yet released in Unity at the time of this writing
                        return OvrGlobal_FrameCounter.x % 2 == 0;
                    #else
                        // Getting the state of the current eye in older versions
                        // of Unity used to be a bit of a hassle. This approach seems to work
                        // although for some reason the value seems to be flipped?
                        // If this is causing issues, an alternative to try would be: 
                        // return (UNITY_MATRIX_P._m02 > 0);
                        // For more info, see:
                        // https://Forum.unity.com/threads/opengl-breaking-openvr-display-matrix.365625/#post-2367930
                        // https://Forum.unity.com/threads/is-unity_matrix_p-_m02-a-safe-way-to-detect-eye-for-single-pass-vr-stereo-rendering.452777/
                        return unity_CameraProjection[0][2] > 0.0;
                    #endif
                #endif
            }
            
            inline float4 GetStereoScaleOffset(bool isLeftEye, bool isYFlipped)
            {
                float4 scaleOffset = float4(1.0, 1.0, 0.0, 0.0);
                
                #if STEREO_TOP_BOTTOM
                    scaleOffset = float4(1.0, 0.5, 0.0, 0.0);
                    if (!isLeftEye)
                    {
                        scaleOffset.w = 0.5;
                    }
                    if (isYFlipped)
                    {
                        scaleOffset.y = -scaleOffset.y;
                        scaleOffset.w = 1.0 - scaleOffset.w;
                    }
                #elif STEREO_LEFT_RIGHT
                    scaleOffset = float4(0.5, 1.0, 0.0, 0.0);
                    if (!isLeftEye)
                    {
                        scaleOffset.z = 0.5;
                    }
                #endif
                
                return scaleOffset;
            }
            
            #if STEREO_DEBUG
            inline fixed4 GetStereoDebugTint(bool isLeftEye)
            {
                // Left = Red, Right = Green
                fixed4 tint = fixed4(1.0, 0.0, 0.0, 1.0);
                if (!isLeftEye)
                {
                    tint = fixed4(0.0, 1.0, 0.0, 1.0);
                }
                return tint;
            }
            #endif
            
            inline fixed4 SampleRGBA(sampler2D tex, float2 uv)
            {
                fixed4 col = tex2D(tex, uv);
                #if APPLY_GAMMA
                col.rgb = pow(col.rgb, 2.2);
                #endif
                return col;
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                
                #if STEREO_TOP_BOTTOM || STEREO_LEFT_RIGHT
                    float4 scaleOffset = GetStereoScaleOffset(IsStereoEyeLeft(), _MainTex_ST.y < 0.0);
                    OUT.texcoord.xy *= scaleOffset.xy;
                    OUT.texcoord.xy += scaleOffset.zw;
                #endif
                
                OUT.color = v.color * _Color;
                
                #if STEREO_DEBUG
                    OUT.color *= GetStereoDebugTint(IsStereoEyeLeft());
                #endif
                
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                
                #if APPLY_GAMMA
                    color.rgb = pow(color.rgb, 2.2);
                #endif
                
                #ifdef UNITY_UI_CLIP_RECT
                    color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                    clip(color.a - 0.001);
                #endif
                
                return color;
            }
        ENDCG
        }
    }
    CustomEditor "StereoSpriteShaderGUI"
}