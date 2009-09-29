// =============================================================
// Sea Rendering Shader
// =============================================================

// -------------------------------------------------------------
// Semantics
// -------------------------------------------------------------
float4x4 World;
float4x4 View;
float4x4 Proj;
float4x4 ReflectionView;
float4 AddedColor;
float4 MultiColor;
float RefractionFactor;

//float xWaveLength;
float xWaveHeight;

//float3 xCamPos;
//float3 xWindDirection;
//float xTime;
//float xWindForce;


texture ReflectionTexture;
sampler ReflectionSampler: register(s0) = sampler_state
{
	Texture = <ReflectionTexture>;
	AddressU = CLAMP;
	AddressV = CLAMP;
	MagFilter = Linear;
};

texture BumpTexture;
sampler BumpSampler: register(s1) = sampler_state
{
	Texture = <BumpTexture>;
	AddressU = mirror;
	AddressV = mirror;

	MipFilter = Linear;
	MagFilter = Linear;
};

// -------------------------------------------------------------
// Input/Output channels
// -------------------------------------------------------------
struct VS_OUTPUT
{
	float4 Position : POSITION;
	float4 TexCoord : TEXCOORD0;
	float4 ReflectionMapSamplingPos: TEXCOORD1;
//	float2 BumpMapSamplingPos: TEXCOORD2;
	
//	float4 Position3D: TEXCOORD4;
};

struct PS_OUTPUT
{
	float4 RGBColor : COLOR0;
};

// -------------------------------------------------------------
// Vertex Shader function
// -------------------------------------------------------------
VS_OUTPUT VS(float4 vPosition : POSITION, float2 texCoord : TEXCOORD)
{
	VS_OUTPUT Output;
	
	float4x4 preViewProjection = mul (View, Proj);
	float4x4 preWorldViewProjection = mul (World, preViewProjection);
	float4x4 preReflectionViewProjection = mul (ReflectionView, Proj);
	float4x4 preWorldReflectionViewProjection = mul (World, preReflectionViewProjection);
     
	Output.Position = mul(vPosition, preWorldViewProjection);
	Output.TexCoord = Output.Position;
	Output.ReflectionMapSamplingPos = mul(vPosition, preWorldReflectionViewProjection);	
//	Output.BumpMapSamplingPos = texCoord / xWaveLength;
//	Output.Position3D = mul(vPosition, World);  
	
/*	
	float3 windDir = normalize(xWindDirection);    
	float3 perpDir = cross(xWindDirection, float3(0,1,0));
	float ydot = dot(texCoord, xWindDirection.xz);
	float xdot = dot(texCoord, perpDir.xz);
	float2 moveVector = float2(xdot, ydot);
	moveVector.y += xTime * xWindForce;    
*/	
//	Output.BumpMapSamplingPos = moveVector / xWaveLength;   	
	
	return Output;
}

// -------------------------------------------------------------
// Pixel Shader function
// -------------------------------------------------------------
PS_OUTPUT PS(VS_OUTPUT In)
{
	PS_OUTPUT Output;
	
	float4 projCoord = In.TexCoord / In.TexCoord.w;
	projCoord += float4(1.0, 1.0, 1.0, 1.0);
	projCoord *= 0.5;
	projCoord = clamp(projCoord, 0.001, 0.999);

/*
	float2 ProjectedTexCoords;
	ProjectedTexCoords.x = (In.ReflectionMapSamplingPos.x / In.ReflectionMapSamplingPos.w) / 2.0f + 0.5f;
	ProjectedTexCoords.y = (-In.ReflectionMapSamplingPos.y / In.ReflectionMapSamplingPos.w) / 2.0f + 0.5f;

	float4 bumpColor = tex2D(BumpSampler, In.BumpMapSamplingPos);

	
	float2 perturbation = xWaveHeight * (bumpColor.rg - 0.5f) * 2.0f;
	float2 perturbatedTexCoords = ProjectedTexCoords + perturbation;
	Output.RGBColor = tex2D(ReflectionSampler, perturbatedTexCoords);	
*/

	float4 bumpColor = tex2D(BumpSampler, projCoord);
	float2 perturbation = xWaveHeight * (bumpColor.rg - 0.5f) * 2.0f;

	float2 coords = projCoord + perturbation;
	float4 refTex = tex2D(ReflectionSampler, coords);
	refTex = (refTex + AddedColor) * MultiColor;

	
/*	090209
	float3 eyeVector = normalize(xCamPos - In.Position3D);
	float3 normalVector = float3(0,1,0);
	float fresnelTerm = dot(eyeVector, normalVector);    
	float4 combinedColor = lerp(refTex, refTex, fresnelTerm);
	float4 dullColor = float4(0.3f, 0.3f, 0.5f, 1.0f);
	Output.RGBColor = lerp(combinedColor, dullColor, 0.2f);	
*/
	
	
	
	Output.RGBColor = refTex;
	Output.RGBColor.a = RefractionFactor;
	
	return Output;
}

// -------------------------------------------------------------
// Technique
// -------------------------------------------------------------
technique TShader
{
    pass P0
    {
        // Compile Shaders
        VertexShader = compile vs_2_0 VS();
        PixelShader  = compile ps_2_0 PS();
    }
}
