#ifndef ALLIN1VFXSRPBATCH_FRAGMENTPASS
#define ALLIN1VFXSRPBATCH_FRAGMENTPASS

half4 frag(v2f i) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID(i);
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
	float seed = i.uvSeed.z + UNITY_ACCESS_INSTANCED_PROP(Seeds, _TimingSeed);
#if TIMEISCUSTOM_ON
                 const float4 shaderTime = globalCustomTime;
#else
	const float4 shaderTime = _Time;
#endif
	float time = shaderTime.y + seed;

#if SHAPE1SCREENUV_ON || SHAPE2SCREENUV_ON || SHAPE3SCREENUV_ON
			 	half2 originalUvs = i.uvSeed.xy;
#endif

#if PIXELATE_ON
			 	half aspectRatio = _MainTex_TexelSize.x / _MainTex_TexelSize.y;
			 	half2 pixelSize = float2(_PixelateSize, _PixelateSize * aspectRatio);
			 	i.uvSeed.xy = floor(i.uvSeed.xy * pixelSize) / pixelSize;
#endif

#if TWISTUV_ON
			 	half2 tempUv = i.uvSeed.xy - half2(_TwistUvPosX * _MainTex_ST.x, _TwistUvPosY * _MainTex_ST.y);
			 	_TwistUvRadius *= (_MainTex_ST.x + _MainTex_ST.y) / 2;
			 	half percent = (_TwistUvRadius - length(tempUv)) / _TwistUvRadius;
			 	half theta = percent * percent * (2.0 * sin(_TwistUvAmount)) * 8.0;
			 	half s = sin(theta);
			 	half c = cos(theta);
			 	half beta = max(sign(_TwistUvRadius - length(tempUv)), 0.0);
			 	tempUv = half2(dot(tempUv, half2(c, -s)), dot(tempUv, half2(s, c))) * beta +	tempUv * (1 - beta);
			 	tempUv += half2(_TwistUvPosX * _MainTex_ST.x, _TwistUvPosY * _MainTex_ST.y);
			 	i.uvSeed.xy = tempUv;
#endif

#if DOODLE_ON
			 	half2 uvCopy = i.uvSeed.xy;
			 	_HandDrawnSpeed = (floor((shaderTime.x + seed) * 20 * _HandDrawnSpeed) / _HandDrawnSpeed) * _HandDrawnSpeed;
			 	uvCopy.x = sin((uvCopy.x * _HandDrawnAmount + _HandDrawnSpeed) * 4);
			 	uvCopy.y = cos((uvCopy.y * _HandDrawnAmount + _HandDrawnSpeed) * 4);
			 	i.uvSeed.xy = lerp(i.uvSeed.xy, i.uvSeed.xy + uvCopy, 0.0005 * _HandDrawnAmount);
#endif

#if SHAKEUV_ON
			 	half xShake = sin((shaderTime.x + seed) * _ShakeUvSpeed * 50) * _ShakeUvX;
			 	half yShake = cos((shaderTime.x + seed) * _ShakeUvSpeed * 50) * _ShakeUvY;
			 	i.uvSeed.xy += half2(xShake * 0.012, yShake * 0.01);
#endif

#if WAVEUV_ON
			 	half2 uvWave = half2(_WaveX * _MainTex_ST.x, _WaveY * _MainTex_ST.y) - i.uvSeed.xy;
#if ATLAS_ON
			 	uvWave = half2(_WaveX, _WaveY) - uvRect;
#endif
			 	uvWave.x *= _ScreenParams.x / _ScreenParams.y;
			 	half angWave = (sqrt(dot(uvWave, uvWave)) * _WaveAmount) - ((time *  _WaveSpeed) % 360.0);
			 	i.uvSeed.xy = i.uvSeed.xy + normalize(uvWave) * sin(angWave) * (_WaveStrength / 1000.0);
#endif

#if ROUNDWAVEUV_ON
			 	half xWave = ((0.5 * _MainTex_ST.x) - i.uvSeed.x);
			 	half yWave = ((0.5 * _MainTex_ST.y) - i.uvSeed.y) * (_MainTex_TexelSize.w / _MainTex_TexelSize.z);
			 	half ripple = -sqrt(xWave*xWave + yWave* yWave);
             	i.uvSeed.xy += (sin((ripple + time * (_RoundWaveSpeed/10.0)) / 0.015) * (_RoundWaveStrength/10.0)) % 1;
#endif

#if POLARUV_ON
             	half2 prePolarUvs = i.uvSeed.xy;
             	i.uvSeed.xy = i.uvSeed.xy - half2(0.5, 0.5);
			 	i.uvSeed.xy = half2(atan2(i.uvSeed.y, i.uvSeed.x) / (1.0 * 6.28318530718), length(i.uvSeed.xy) * 2.0);
             	i.uvSeed.xy *= _MainTex_ST.xy;
#endif

#if DISTORT_ON
#if POLARUVDISTORT_ON
             	half2 distortUvs = TRANSFORM_TEX(i.uvSeed.xy, _DistortTex);
#else
             	half2 distortUvs = i.uvDistTex.xy;
#endif
			 	distortUvs.x += ((shaderTime.x + seed) * _DistortTexXSpeed) % 1;
			 	distortUvs.y += ((shaderTime.x + seed) * _DistortTexYSpeed) % 1;
#if ATLAS_ON
			 	i.uvDistTex = half2((i.uvDistTex.x - _MinXUV) / (_MaxXUV - _MinXUV), (i.uvDistTex.y - _MinYUV) / (_MaxYUV - _MinYUV));
#endif
			 	half distortAmnt = (tex2D(_DistortTex, distortUvs).r - 0.5) * 0.2 * _DistortAmount;
			 	i.uvSeed.x += distortAmnt;
			 	i.uvSeed.y += distortAmnt;
#endif

#if TEXTURESCROLL_ON
			 	i.uvSeed.x += (time * _TextureScrollXSpeed) % 1;
			 	i.uvSeed.y += (time * _TextureScrollYSpeed) % 1;
#endif

#if TRAILWIDTH_ON
             	half width = pow(tex2D(_TrailWidthGradient, i.uvSeed).r, _TrailWidthPower);
             	i.uvSeed.y = (i.uvSeed.y * 2 - 1) / width * 0.5 + 0.5;
             	clip(i.uvSeed.y);
             	clip(1 - i.uvSeed.y);
#endif

	float2 shape1Uv = i.uvSeed.xy;
#if SHAPE2_ON
             	float2 shape2Uv = shape1Uv;
#endif
#if SHAPE3_ON
             	float2 shape3Uv = shape1Uv;
#endif

#if CAMDISTFADE_ON || SHAPE1SCREENUV_ON || SHAPE2SCREENUV_ON || SHAPE3SCREENUV_ON
             	half camDistance = distance(i.worldPos, _WorldSpaceCameraPos);
#endif

#if SHAPE1SCREENUV_ON || SHAPE2SCREENUV_ON || SHAPE3SCREENUV_ON
             	half2 uvOffsetPostFx = i.uvSeed.xy - originalUvs;
			 	i.uvSeed.xy = i.screenCoord.xy / i.screenCoord.w;
			 	i.uvSeed.x = i.uvSeed.x * (_ScreenParams.x / _ScreenParams.y);
			 	i.uvSeed.x -= 0.5;
             	i.uvSeed.xy -= uvOffsetPostFx;
             	originalUvs += uvOffsetPostFx;
             	half distanceZoom = camDistance * 0.1;
             	half2 scaleWithDistUvs = i.uvSeed.xy * distanceZoom + ((-distanceZoom * 0.5) + 0.5);
#if SHAPE1SCREENUV_ON
             	shape1Uv = lerp(i.uvSeed.xy, scaleWithDistUvs, _ScreenUvShDistScale);
#else
             	shape1Uv = originalUvs;
#endif
#if SHAPE2SCREENUV_ON && SHAPE2_ON
             	shape2Uv = lerp(i.uvSeed.xy, scaleWithDistUvs, _ScreenUvSh2DistScale);
#else
#if SHAPE2_ON
             	shape2Uv = originalUvs;
#endif
#endif
#if SHAPE3SCREENUV_ON && SHAPE3_ON
             	shape3Uv = lerp(i.uvSeed.xy, scaleWithDistUvs, _ScreenUvSh3DistScale);
#else
#if SHAPE3_ON
             	shape3Uv = originalUvs;
#endif
#endif
#endif

	shape1Uv = TRANSFORM_TEX(shape1Uv, _MainTex);
#if OFFSETSTREAM_ON
			 	shape1Uv.x += i.offsetCustomData.x * _OffsetSh1;
                 shape1Uv.y += i.offsetCustomData.y * _OffsetSh1;
#endif
#if SHAPETEXOFFSET_ON
			 	shape1Uv += seed * _RandomSh1Mult;
#endif
#if SHAPE1DISTORT_ON
#if POLARUVDISTORT_ON
             	half2 sh1DistortUvs = TRANSFORM_TEX(i.uvSeed.xy, _ShapeDistortTex);
#else
             	half2 sh1DistortUvs = i.uvSh1DistTex.xy;
#endif
                 sh1DistortUvs.x += ((time + seed) * _ShapeDistortXSpeed) % 1;
                 sh1DistortUvs.y += ((time + seed) * _ShapeDistortYSpeed) % 1;
                 half distortAmount = (tex2D(_ShapeDistortTex, sh1DistortUvs).r - 0.5) * 0.2 * _ShapeDistortAmount;
                 shape1Uv.x += distortAmount;
                 shape1Uv.y += distortAmount;
#endif
#if SHAPE1ROTATE_ON
             	shape1Uv = RotateUvs(shape1Uv, _ShapeRotationOffset + ((_ShapeRotationSpeed * time) % 6.28318530718), _MainTex_ST);
#endif
	half4 shape1 = SampleTextureWithScroll(_MainTex, shape1Uv, _ShapeXSpeed, _ShapeYSpeed, time);
#if SHAPE1SHAPECOLOR_ON
                 shape1.a = shape1.r;
                 shape1.rgb = _ShapeColor.rgb;
#else
	shape1 *= _ShapeColor;
#endif
#if SHAPE1CONTRAST_ON
#if SHAPE1SHAPECOLOR_ON
                 shape1.a = saturate((shape1.a - 0.5) * _ShapeContrast + 0.5 + _ShapeBrightness);
#else
                 shape1.rgb = max(0, (shape1.rgb - half3(0.5, 0.5, 0.5)) * _ShapeContrast + half3(0.5, 0.5, 0.5) + _ShapeBrightness);
#endif
#endif

#if SHAPE2_ON
             	shape2Uv = TRANSFORM_TEX(shape2Uv, _Shape2Tex);
#if OFFSETSTREAM_ON
			 	shape2Uv.x += i.offsetCustomData.x * _OffsetSh2;
                 shape2Uv.y += i.offsetCustomData.y * _OffsetSh2;
#endif
#if SHAPETEXOFFSET_ON
			 	shape2Uv += seed * _RandomSh2Mult;
#endif
#if SHAPE2DISTORT_ON
#if POLARUVDISTORT_ON
             	half2 sh2DistortUvs = TRANSFORM_TEX(i.uvSeed.xy, _Shape2DistortTex);
#else
             	half2 sh2DistortUvs = i.uvSh2DistTex.xy;
#endif
                 sh2DistortUvs.x += ((time + seed) * _Shape2DistortXSpeed) % 1;
                 sh2DistortUvs.y += ((time + seed) * _Shape2DistortYSpeed) % 1;
                 half distortAmnt2 = (tex2D(_Shape2DistortTex, sh2DistortUvs).r - 0.5) * 0.2 * _Shape2DistortAmount;
                 shape2Uv.x += distortAmnt2;
                 shape2Uv.y += distortAmnt2;
#endif
#if SHAPE2ROTATE_ON
             	shape2Uv = RotateUvs(shape2Uv, _Shape2RotationOffset + ((_Shape2RotationSpeed * time) % 6.28318530718), _Shape2Tex_ST);
#endif
                 half4 shape2 = SampleTextureWithScroll(_Shape2Tex, shape2Uv, _Shape2XSpeed, _Shape2YSpeed, time);
#if SHAPE2SHAPECOLOR_ON
                 shape2.a = shape2.r;
                 shape2.rgb = _Shape2Color.rgb;
#else
                 shape2 *= _Shape2Color;
#endif
#if SHAPE2CONTRAST_ON
#if SHAPE2SHAPECOLOR_ON
                 shape2.a = max(0, (shape2.a - 0.5) * _Shape2Contrast + 0.5 + _Shape2Brightness);
#else
                 shape2.rgb = max(0, (shape2.rgb - half3(0.5, 0.5, 0.5)) * _Shape2Contrast + half3(0.5, 0.5, 0.5) + _Shape2Brightness);
#endif
#endif
#endif

#if SHAPE3_ON
                 shape3Uv = TRANSFORM_TEX(shape3Uv, _Shape3Tex);
#if OFFSETSTREAM_ON
			 	shape3Uv.x += i.offsetCustomData.x * _OffsetSh3;
                 shape3Uv.y += i.offsetCustomData.y * _OffsetSh3;
#endif
#if SHAPETEXOFFSET_ON
			 	shape3Uv += seed * _RandomSh3Mult;
#endif
#if SHAPE3DISTORT_ON
#if POLARUVDISTORT_ON
             	half2 sh3DistortUvs = TRANSFORM_TEX(i.uvSeed.xy, _Shape3DistortTex);
#else
             	half2 sh3DistortUvs = i.uvSh3DistTex.xy;
#endif
                 sh3DistortUvs.x += ((time + seed) * _Shape3DistortXSpeed) % 1;
                 sh3DistortUvs.y += ((time + seed) * _Shape3DistortYSpeed) % 1;
                 half distortAmnt3 = (tex2D(_Shape3DistortTex, sh3DistortUvs).r - 0.5) * 0.3 * _Shape3DistortAmount;
                 shape3Uv.x += distortAmnt3;
                 shape3Uv.y += distortAmnt3;
#endif
#if SHAPE3ROTATE_ON
             	shape3Uv = RotateUvs(shape3Uv, _Shape3RotationOffset + ((_Shape3RotationSpeed * time) % 6.28318530718), _Shape3Tex_ST);
#endif
                 half4 shape3 = SampleTextureWithScroll(_Shape3Tex, shape3Uv, _Shape3XSpeed, _Shape3YSpeed, time);
#if SHAPE3SHAPECOLOR_ON
                 shape3.a = shape3.r;
                 shape3.rgb = _Shape3Color.rgb;
#else
                 shape3 *= _Shape3Color;
#endif
#if SHAPE3CONTRAST_ON
#if SHAPE3SHAPECOLOR_ON
                 shape3.a = max(0, (shape3.a - 0.5) * _Shape3Contrast + 0.5 + _Shape3Brightness);
#else
                 shape3.rgb = max(0, (shape3.rgb - half3(0.5, 0.5, 0.5)) * _Shape3Contrast + half3(0.5, 0.5, 0.5) + _Shape3Brightness);
#endif
#endif
#endif

                 //ALWAYS UNCOMMENT THE FOLLOWING CODE BLOCK-------------------------------------------
#if SHAPEDEBUG_ON
                 if (_DebugShape < 1.5) return shape1;
#if SHAPE2_ON
                 else if (_DebugShape < 2.5) return shape2;
#endif
#if SHAPE3_ON
                 else return shape3;
#endif
#endif

	half4 col = shape1;
                 //Mix all shapes pre: change weights if custom vertex effect active
#if SHAPEWEIGHTS_ON
             	half shapeWeightOffset;
#if SHAPE2_ON
             	shapeWeightOffset = i.offsetCustomData.z * _Sh1BlendOffset;
             	_ShapeColorWeight = max(0, _ShapeColorWeight + shapeWeightOffset);
             	_ShapeAlphaWeight = max(0, _ShapeAlphaWeight + shapeWeightOffset);
			 	shapeWeightOffset = i.offsetCustomData.z * _Sh2BlendOffset;
             	_Shape2ColorWeight = max(0, _Shape2ColorWeight + shapeWeightOffset);
             	_Shape2AlphaWeight = max(0, _Shape2AlphaWeight + shapeWeightOffset);
#endif
#if SHAPE3_ON
			 	shapeWeightOffset = i.offsetCustomData.z * _Sh3BlendOffset;
             	_Shape3ColorWeight = max(0, _Shape3ColorWeight + shapeWeightOffset);
             	_Shape3AlphaWeight = max(0, _Shape3AlphaWeight + shapeWeightOffset);
#endif
#endif
             	//Mix all shapes
#if SHAPE2_ON
#if !SPLITRGBA_ON
			 	_ShapeAlphaWeight = _ShapeColorWeight;
			 	_Shape2AlphaWeight = _Shape2ColorWeight;
#endif
#if SHAPE3_ON //Shape3 On
#if !SPLITRGBA_ON
                 _Shape3AlphaWeight = _Shape3ColorWeight;
#endif
#if SHAPEADD_ON
			 	col.rgb = ((shape1.rgb * _ShapeColorWeight) + (shape2.rgb * _Shape2ColorWeight)) + (shape3.rgb * _Shape3ColorWeight);
                 col.a = saturate(max(shape3.a * _Shape3AlphaWeight, max(shape1.a * _ShapeAlphaWeight, shape2.a * _Shape2AlphaWeight)));
#else
             	col.rgb = ((shape1.rgb * _ShapeColorWeight) * (shape2.rgb * _Shape2ColorWeight)) * (shape3.rgb * _Shape3ColorWeight);
                 col.a = saturate(((shape1.a * _ShapeAlphaWeight) * (shape2.a * _Shape2AlphaWeight)) * (shape3.a * _Shape3AlphaWeight));
#endif
#else //Shape3 Off
#if SHAPEADD_ON
			 	col.rgb = (shape1.rgb * _ShapeColorWeight) + (shape2.rgb * _Shape2ColorWeight);
			     col.a = saturate(max(shape1.a * _ShapeAlphaWeight, shape2.a * _Shape2AlphaWeight));
#else
             	col.rgb = (shape1.rgb * _ShapeColorWeight) * (shape2.rgb * _Shape2ColorWeight);
			     col.a = saturate((shape1.a * _ShapeAlphaWeight) * (shape2.a * _Shape2AlphaWeight));
#endif
#endif
#endif

#if SHAPE1MASK_ON
             	col = lerp(col, shape1, pow(tex2D(_Shape1MaskTex, TRANSFORM_TEX(i.uvSeed.xy, _Shape1MaskTex)).r, _Shape1MaskPow));
#endif

#if PREMULTIPLYCOLOR_ON
             	half luminance = 0;
                 luminance = 0.3 * col.r + 0.59 * col.g + 0.11 * col.b;
                 luminance *= col.a;
                 col.a = min(luminance, col.a);
#endif

	col.rgb *= _Color.rgb * i.color.rgb;
#if PREMULTIPLYALPHA_ON
                 col.rgb *= col.a;
#endif
                 //col.rgb = saturate(col.rgb); //Removed to allow for HDR shape color. Contrast and shape combinations can now overexpose the result

#if !PREMULTIPLYCOLOR_ON && (COLORRAMP_ON || ALPHAFADE_ON || COLORGRADING_ON || FADE_ON || (ADDITIVECONFIG_ON && (GLOW_ON || DEPTHGLOW_ON)))
                 half luminance = 0;
                 luminance = 0.3 * col.r + 0.59 * col.g + 0.11 * col.b;
                 luminance *= col.a;
#endif

#if (FADE_ON || ALPHAFADE_ON) && ALPHAFADEINPUTSTREAM_ON
             	col.a *= i.color.a;
			 	i.color.a = i.uvSeed.w;
#endif

#if FADE_ON
             	half preFadeAlpha = col.a;
             	_FadeAmount = saturate(_FadeAmount + (1 - i.color.a));
             	_FadeTransition = max(0.01, _FadeTransition * EaseOutQuint(saturate(_FadeAmount)));
             	half2 fadeUv;
             	fadeUv = i.uvSeed.xy + seed;
             	fadeUv.x += (time * _FadeScrollXSpeed) % 1;
                 fadeUv.y += (time * _FadeScrollYSpeed) % 1;
             	half2 tiledUvFade1 = TRANSFORM_TEX(fadeUv, _FadeTex);
#if ADDITIVECONFIG_ON && !PREMULTIPLYCOLOR_ON
                 preFadeAlpha *= luminance;
#endif
             	_FadeAmount = saturate(pow(_FadeAmount, _FadePower));
#if FADEBURN_ON
			 	half2 tiledUvFade2 = TRANSFORM_TEX(fadeUv, _FadeBurnTex);
			 	half fadeSample = tex2D(_FadeTex, tiledUvFade1).r;
			 	half fadeNaturalEdge = saturate(smoothstep(0.0 , _FadeTransition, RemapFloat(1.0 - _FadeAmount, 0.0, 1.0, -1.0, 1.0) + fadeSample));
             	col.a *= fadeNaturalEdge;
             	half fadeBurn = saturate(smoothstep(0.0 , _FadeTransition + _FadeBurnWidth, RemapFloat(1.0 - _FadeAmount, 0.0, 1.0, -1.0, 1.0) + fadeSample));
             	fadeBurn = fadeNaturalEdge - fadeBurn;
			 	_FadeBurnColor.rgb *= _FadeBurnGlow;
			 	col.rgb += fadeBurn * tex2D(_FadeBurnTex, tiledUvFade2).rgb * _FadeBurnColor.rgb * preFadeAlpha;
#else
			 	half fadeSample = tex2D(_FadeTex, tiledUvFade1).r;
			 	float fade = saturate(smoothstep(0.0 , _FadeTransition, RemapFloat(1.0 - _FadeAmount, 0.0, 1.0, -1.0, 1.0) + fadeSample));
			 	col.a *= fade;
#endif
#if ALPHAFADETRANSPARENCYTOO_ON
                 col.a *= 1 - _FadeAmount;
#endif
#endif

#if ALPHAFADE_ON
             	half alphaFadeLuminance;
             	_AlphaFadeAmount = saturate(_AlphaFadeAmount + (1 - i.color.a));
             	_AlphaFadeAmount = saturate(pow(_AlphaFadeAmount, _AlphaFadePow));
             	_AlphaFadeSmooth = max(0.01, _AlphaFadeSmooth * EaseOutQuint(saturate(_AlphaFadeAmount)));
#if ALPHAFADEUSESHAPE1_ON
                 alphaFadeLuminance = shape1.r;
#else
                 alphaFadeLuminance = luminance;
#endif
                 alphaFadeLuminance = saturate(alphaFadeLuminance - 0.001);
#if ALPHAFADEUSEREDCHANNEL_ON
                 col.a *= col.r;
#endif
                 col.a = saturate(col.a);
			 	float alphaFade = saturate(smoothstep(0.0 , _AlphaFadeSmooth, RemapFloat(1.0 - _AlphaFadeAmount, 0.0, 1.0, -1.0, 1.0) + alphaFadeLuminance));
			 	col.a *= alphaFade;
#if ALPHAFADETRANSPARENCYTOO_ON
                 col.a *= 1 - _AlphaFadeAmount;
#endif
#endif

#if BACKFACETINT_ON
             	col.rgb = lerp(col.rgb * _BackFaceTint, col.rgb * _FrontFaceTint, step(0, dot(i.normal, i.viewDir)));
#endif

#if LIGHTANDSHADOW_ON
                 half NdL = saturate(dot(i.normal, -_All1VfxLightDir));
                 col.rgb += _LightColor * _LightAmount * NdL;
                 NdL = max(_ShadowAmount, NdL);
             	NdL = smoothstep(_ShadowStepMin, _ShadowStepMax, NdL);
             	col.rgb *= NdL;
#endif

#if COLORGRADING_ON
			 	col.rgb *= lerp(lerp(_ColorGradingDark, _ColorGradingMiddle, luminance/_ColorGradingMidPoint),
			 		lerp(_ColorGradingMiddle, _ColorGradingLight, (luminance - _ColorGradingMidPoint)/(1.0 - _ColorGradingMidPoint)), step(_ColorGradingMidPoint, luminance));
#endif

#if COLORRAMP_ON
                 half colorRampLuminance = saturate(luminance + _ColorRampLuminosity);
#if COLORRAMPGRAD_ON
                 half4 colorRampRes = tex2D(_ColorRampTexGradient, half2(colorRampLuminance, 0));
#else
             	half4 colorRampRes = tex2D(_ColorRampTex, half2(colorRampLuminance, 0));
#endif
             	col.rgb = saturate(lerp(saturate(col.rgb), saturate(colorRampRes.rgb), _ColorRampBlend)); //Saturate to avoid SRP volume glow scene view fullscreen bug
                 col.a = saturate(lerp(col.a, saturate(col.a * colorRampRes.a), _ColorRampBlend));
#endif

#if POSTERIZE_ON && !POSTERIZEOUTLINE_ON
             	col.rgb = floor(col.rgb / (1.0 / _PosterizeNumColors)) * (1.0 / _PosterizeNumColors);
#endif

#if SOFTPART_ON || DEPTHGLOW_ON
             	float4 depthScreenPos = i.screenCoord;
			 	float4 depthScreenPosNorm = depthScreenPos / depthScreenPos.w;
			 	//depthScreenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? depthScreenPosNorm.z : depthScreenPosNorm.z * 0.5 + 0.5;
			 	half sceneDepthDiff = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(depthScreenPosNorm.xy),_ZBufferParams) - ComputeGrabScreenPos(depthScreenPos).a;
#endif

#if SOFTPART_ON
                 half sceneZMult = saturate(_SoftFactor * sceneDepthDiff);
                 col.a *= sceneZMult;
#endif

#if RIM_ON
			 	half NdV = 1 - abs(dot(i.normal, i.viewDir));
             	half rimFactor = saturate(_RimBias + _RimScale * pow(NdV, _RimPower));
				half4 rimCol = _RimColor * rimFactor;
				rimCol.rgb *= _RimIntensity;
             	col.rgb = lerp(col.rgb * (rimCol.rgb + half3(1,1,1)), col.rgb + rimCol.rgb, _RimAddAmount);
             	col.a = saturate(col.a * (1 - rimFactor * _RimErodesAlpha));
#endif

#if DEPTHGLOW_ON
				half depthGlowMask = saturate(_DepthGlowDist * pow((1 - sceneDepthDiff), _DepthGlowPow));
				col.rgb = lerp(col.rgb, _DepthGlowGlobal * col.rgb, depthGlowMask);
             	half depthGlowMult = 1;
#if ADDITIVECONFIG_ON
             	depthGlowMult = luminance;
#endif
                 col.rgb += _DepthGlowColor.rgb * _DepthGlow * depthGlowMask * col.a * depthGlowMult;
#endif

#if GLOW_ON
                 half glowMask = 1;
#if GLOWTEX_ON
                 glowMask = tex2D(_GlowTex, TRANSFORM_TEX(i.uvSeed.xy, _GlowTex));
#endif
                 col.rgb *= _GlowGlobal * glowMask;
			 	half glowMult = 1;
#if ADDITIVECONFIG_ON
             	glowMult = luminance;
#endif
                 col.rgb += _GlowColor.rgb * _Glow * glowMask * col.a * glowMult;
#endif

#if HSV_ON
			 	half3 resultHsv = half3(col.rgb);
			 	half cosHsv = _HsvBright * _HsvSaturation * cos(_HsvShift * 3.14159265 / 180);
			 	half sinHsv = _HsvBright * _HsvSaturation * sin(_HsvShift * 3.14159265 / 180);
			 	resultHsv.x = (.299 * _HsvBright + .701 * cosHsv + .168 * sinHsv) * col.x
			 		+ (.587 * _HsvBright - .587 * cosHsv + .330 * sinHsv) * col.y
			 		+ (.114 * _HsvBright - .114 * cosHsv - .497 * sinHsv) * col.z;
			 	resultHsv.y = (.299 * _HsvBright - .299 * cosHsv - .328 * sinHsv) *col.x
			 		+ (.587 * _HsvBright + .413 * cosHsv + .035 * sinHsv) * col.y
			 		+ (.114 * _HsvBright - .114 * cosHsv + .292 * sinHsv) * col.z;
			 	resultHsv.z = (.299 * _HsvBright - .3 * cosHsv + 1.25 * sinHsv) * col.x
			 		+ (.587 * _HsvBright - .588 * cosHsv - 1.05 * sinHsv) * col.y
			 		+ (.114 * _HsvBright + .886 * cosHsv - .203 * sinHsv) * col.z;
			 	col.rgb = resultHsv;
#endif

#if CAMDISTFADE_ON
             	col.a *= 1 - saturate(smoothstep(_CamDistFadeStepMin, _CamDistFadeStepMax, camDistance));
             	col.a *= smoothstep(0.0, _CamDistProximityFade, camDistance);
#endif

#if MASK_ON
             	half2 maskUv = i.uvSeed.xy;
#if POLARUV_ON
			 	maskUv = prePolarUvs;
#endif
             	half4 maskSample = tex2D(_MaskTex, TRANSFORM_TEX(maskUv, _MaskTex));
                 half mask = pow(min(maskSample.r, maskSample.a), _MaskPow);
                 col.a *= mask;
#endif

#if ALPHASMOOTHSTEP_ON
                 col.a = smoothstep(_AlphaStepMin, _AlphaStepMax, col.a);
#endif
                
#if ALPHACUTOFF_ON
                 clip((1 - _AlphaCutoffValue) - (1 - col.a) - 0.01);
#endif

#if FOG_ON
                 col.rgb = MixFog(col.rgb, i.fogFactor);
#endif

                 //Don't use a starting i.color.a lower than 1 unless using vertex stream dissolve when using a FADE effect
#if !FADE_ON && !ALPHAFADE_ON
	col.a *= _Alpha * i.color.a;
#endif
#if FADE_ON || ALPHAFADE_ON
                 col.a *= _Alpha;
#endif
#if ADDITIVECONFIG_ON
                 col.rgb *= col.a;
#endif

#if SCREENDISTORTION_ON
#if DISTORTUSECOL_ON
             	half4 normalMap = col;
             	normalMap.b = 1;
             	normalMap *= col.r;
#else
             	half2 distortionUv = TRANSFORM_TEX(i.uvSeed.xy, _DistNormalMap);
             	distortionUv.x += (time * _DistortionScrollXSpeed) % 1;
                 distortionUv.y += (time * _DistortionScrollYSpeed) % 1;
             	half4 normalMap = tex2D(_DistNormalMap, distortionUv);
#endif
#if DISTORTONLYBACK_ON
			 	half4 originalScreenCoord = i.screenCoord;
#endif
             	half3 usableNormals = UnpackNormal(normalMap);
             	i.screenCoord.xy /= i.screenCoord.w;
             	half2 distortUvOffset = usableNormals.rg * _DistortionPower * i.color.a * i.screenCoord.z * normalMap.a;
			 	i.screenCoord.xy = distortUvOffset + i.screenCoord.xy;
             	half3 distortCol = CustomSampleSceneColor(i.screenCoord.xy);
#if DISTORTONLYBACK_ON
             	float4 backDistortScreenPos = i.screenCoord;
			 	float4 backDistortScreenPosNorm = originalScreenCoord / originalScreenCoord.w;
			 	backDistortScreenPosNorm.xy += distortUvOffset;
			 	half frontMask = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(backDistortScreenPosNorm.xy),_ZBufferParams) - ComputeGrabScreenPos(backDistortScreenPos).a;
             	frontMask = 1 - saturate(frontMask);
             	col.rgb = lerp(lerp(col.rgb, saturate(distortCol.rgb), _DistortionBlend), lerp(col.rgb, CustomSampleSceneColor((originalScreenCoord / originalScreenCoord.w).xy).rgb, _DistortionBlend), frontMask);
#else
             	col.rgb = lerp(col.rgb, saturate(distortCol.rgb), _DistortionBlend);
#endif
#endif
	
	return col;
}

#endif