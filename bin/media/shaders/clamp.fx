float4x4 mWorldViewProj;
float WaterPositionY;

texture DiffuseMapTexture;
sampler DiffuseMapSampler: register(s0) = sampler_state
{
	Texture = <DiffuseMapTexture>;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = Linear;
};

struct VS_OUTPUT
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
	
	float4 Diffuse : COLOR0;
};

struct PS_OUTPUT
{
    float4 RGBColor: COLOR0;
};

VS_OUTPUT VS(float4 vPosition: POSITION, float2 texCoord: TEXCOORD0)
{
	VS_OUTPUT Output;
	
	Output.Position = mul(vPosition, mWorldViewProj);
	Output.TexCoord = texCoord;

	Output.Diffuse = vPosition;
	
	return Output;
}

PS_OUTPUT PS(float4 Position: POSITION, float2 TexCoord: TEXCOORD0, float4 Diffuse: COLOR0)
{
    PS_OUTPUT Output;

//    float4 color = tex2D(DiffuseMap, TexCoord) * 2.0f *
//                   tex2D(DetailMap, float2(TexCoord1.x * 20.0f, TexCoord1.y * 20.0f));

	float4 color = tex2D(DiffuseMapSampler, TexCoord);

    if(Diffuse.y <= WaterPositionY)
        color.a = 0.0;
    else
        color.a = 1.0;
        
    Output.RGBColor = color;
    
    return Output;
}

technique Water
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 VS();
		PixelShader = compile ps_2_0 PS();
	}
}