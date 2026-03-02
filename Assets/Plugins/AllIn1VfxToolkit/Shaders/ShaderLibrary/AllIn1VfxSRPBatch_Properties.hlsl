#ifndef ALLIN1VFXSRPBATCH_PROPERTIES
#define ALLIN1VFXSRPBATCH_PROPERTIES

CBUFFER_START(UnityPerMaterial)
	half4 _Color;
	half _Alpha;

	float4 globalCustomTime;
	
	half4 _MainTex_ST, _ShapeColor;
	half _ShapeXSpeed, _ShapeYSpeed, _ShapeColorWeight, _ShapeAlphaWeight;
	
	//SHAPE1CONTRAST_ON
	half _ShapeContrast, _ShapeBrightness;
	
	half4 _ShapeDistortTex_ST;
	half _ShapeDistortAmount, _ShapeDistortXSpeed, _ShapeDistortYSpeed;

	//SHAPE1ROTATE_ON
	half _ShapeRotationOffset, _ShapeRotationSpeed;

	//OFFSETSTREAM_ON
	half _OffsetSh1;
	
	half _Sh1BlendOffset;
	
	half4 _Shape2Tex_ST, _Shape2Color;
	half _Shape2XSpeed, _Shape2YSpeed, _Shape2ColorWeight, _Shape2AlphaWeight;
		
	half _Shape2Contrast, _Shape2Brightness;
		
	half4 _Shape2DistortTex_ST;
	half _Shape2DistortAmount, _Shape2DistortXSpeed, _Shape2DistortYSpeed;
		
	half _Shape2RotationOffset, _Shape2RotationSpeed;
		
	half _OffsetSh2;
		
	half _Sh2BlendOffset;
		
	half4 _Shape3Tex_ST, _Shape3Color;
	half _Shape3XSpeed, _Shape3YSpeed, _Shape3ColorWeight, _Shape3AlphaWeight;
		
	half _Shape3Contrast, _Shape3Brightness;
		
	half4 _Shape3DistortTex_ST;
	half _Shape3DistortAmount, _Shape3DistortXSpeed, _Shape3DistortYSpeed;
		
	half _Shape3RotationOffset, _Shape3RotationSpeed;
		
	half _OffsetSh3;
		
	half _Sh3BlendOffset;
	
	half4 _GlowColor;
	half _Glow, _GlowGlobal;

	half4 _GlowTex_ST;
	
	half _SoftFactor;
	
	half _DepthGlowDist, _DepthGlowPow, _DepthGlow, _DepthGlowGlobal;
	half4 _DepthGlowColor;
	
	half4 _MaskTex_ST;
	half _MaskPow;
		
	half _ColorRampLuminosity, _ColorRampBlend;

	half _AlphaCutoffValue;

	half _AlphaStepMin, _AlphaStepMax;

	half _AlphaFadeAmount, _AlphaFadeSmooth, _AlphaFadePow;

	half _HsvShift, _HsvSaturation, _HsvBright;

	half _PosterizeNumColors;
	
	half _PixelateSize;
	
	half4 _DistortTex_ST;
	half _DistortTexXSpeed, _DistortTexYSpeed, _DistortAmount;
	
	half _TextureScrollXSpeed, _TextureScrollYSpeed;
	
	half _ShakeUvSpeed, _ShakeUvX, _ShakeUvY;
	
	half _WaveAmount, _WaveSpeed, _WaveStrength, _WaveX, _WaveY;

	half _RoundWaveStrength, _RoundWaveSpeed;
	
	half _TwistUvAmount, _TwistUvPosX, _TwistUvPosY, _TwistUvRadius;
	
	half _HandDrawnAmount, _HandDrawnSpeed;
	
	half4 _DistNormalMap_ST, _GrabTexture_ST;
	half _DistortionPower, _DistortionBlend, _DistortionScrollXSpeed, _DistortionScrollYSpeed;
	
	half4 _MainTex_TexelSize;
	
	half4 _VertOffsetTex_ST;
	half _VertOffsetAmount, _VertOffsetPower, _VertOffsetTexXSpeed, _VertOffsetTexYSpeed;
	
	half4 _FadeTex_ST;
	half _FadeAmount, _FadeTransition, _FadePower, _FadeScrollXSpeed, _FadeScrollYSpeed;
	
	half4 _FadeBurnColor, _FadeBurnTex_ST;
	half _FadeBurnWidth, _FadeBurnGlow;
	
	half3 _ColorGradingLight, _ColorGradingMiddle, _ColorGradingDark;
	half _ColorGradingMidPoint;
	
	half _CamDistFadeStepMin, _CamDistFadeStepMax, _CamDistProximityFade;
	
	half _ScreenUvShDistScale, _ScreenUvSh2DistScale, _ScreenUvSh3DistScale;

	half _RimBias, _RimScale, _RimPower, _RimIntensity, _RimAddAmount, _RimErodesAlpha;
	half4 _RimColor;
	
	half4 _BackFaceTint, _FrontFaceTint;
	
	half _DebugShape;
	
	half4 _Shape1MaskTex_ST;
	half _Shape1MaskPow;
	
	half _TrailWidthPower;
	
	half3 _All1VfxLightDir;
	half _ShadowAmount, _ShadowStepMin, _ShadowStepMax, _LightAmount;
	half4 _LightColor;
	
	half _RandomSh1Mult, _RandomSh2Mult, _RandomSh3Mult;
	
	half _TimingSeed;
	
CBUFFER_END
	
sampler2D _MainTex;
#if SHAPE1DISTORT_ON
	sampler2D _ShapeDistortTex;
#endif

#if SHAPE2_ON
	sampler2D _Shape2Tex;
	#if SHAPE2DISTORT_ON
		sampler2D _Shape2DistortTex;
	#endif
#endif

#if SHAPE3_ON
	sampler2D _Shape3Tex;
	#if SHAPE3DISTORT_ON
		sampler2D _Shape3DistortTex;
	#endif
#endif

#if GLOW_ON
	#if GLOWTEX_ON
		sampler2D _GlowTex;
	#endif
#endif
	
#if MASK_ON
	sampler2D _MaskTex;
#endif

#if COLORRAMP_ON
	sampler2D _ColorRampTex;
#endif

#if COLORRAMPGRAD_ON
	sampler2D _ColorRampTexGradient;
#endif
	
#if DISTORT_ON
	sampler2D _DistortTex;
#endif

#if SCREENDISTORTION_ON
	sampler2D _DistNormalMap, _GrabTexture;
#endif
	
#if VERTOFFSET_ON
	sampler2D _VertOffsetTex;
#endif

#if FADE_ON
	sampler2D _FadeTex;
	#if FADEBURN_ON
		sampler2D _FadeBurnTex;
	#endif
#endif
	
#if SHAPE1MASK_ON
	sampler2D _Shape1MaskTex;
#endif

#if TRAILWIDTH_ON
	sampler2D _TrailWidthGradient;
#endif

#endif