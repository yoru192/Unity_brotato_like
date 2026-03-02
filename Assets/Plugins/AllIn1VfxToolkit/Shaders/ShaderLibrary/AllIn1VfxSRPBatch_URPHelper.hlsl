#ifndef ALLIN1VFXSRPBATCH_URPHELPER
#define ALLIN1VFXSRPBATCH_URPHELPER

float4 CustomComputeScreenPos(float4 clipPos, float sign)
{
	float4 res = ComputeScreenPos(clipPos, sign);
	return res;
}

float3 CustomSampleSceneColor(float2 uv)
{
	float3 res = SampleSceneColor(uv);
	return res;
}

#endif