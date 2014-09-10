texture ScreenTexture;

uniform float scale;

sampler TextureSampler = sampler_state
{
	Texture = <ScreenTexture>;
};

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(TextureSampler, texCoord);
	float smoothing = 0.15 / scale;

	float dist = color.a;
	color.a = smoothstep(0.5 - smoothing, 0.5 + smoothing, dist);
	color.rgb = float3(1, 1, 1);

	return color;
}

technique DistanceFontTechnique
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}