// *****************************************************
// Vertex shader
// *****************************************************
float4x4	View;
float4x4	WorldViewProj;
float4x4	WorldReflectionViewProj;
float		WaveLength;
float		Time;
float		WindForce;
float2		WindDirection;

struct VertexInput
{
	float4 position  : POSITION;
	float4 color     : COLOR0;
	float2 texCoord0 : TEXCOORD0;
};

struct VertexOutput
{
	float4 position              : POSITION;
	float4 position3D            : TEXCOORD0;
	float2 bumpMapTexCoord       : TEXCOORD2;
	float4 refractionMapTexCoord : TEXCOORD3;
	float4 reflectionMapTexCoord : TEXCOORD4;
};

VertexOutput VertexShaderFunction(VertexInput input)
{
	VertexOutput output;

	output.position = mul(input.position, WorldViewProj);
	output.position3D = mul(input.position, WorldViewProj);
	output.bumpMapTexCoord = (input.texCoord0 / WaveLength) + (Time * WindForce * WindDirection);	
	output.refractionMapTexCoord = mul(input.position, WorldViewProj);
	output.reflectionMapTexCoord = mul(input.position, WorldReflectionViewProj);	
	
	return output;
};

// *****************************************************
// Pixel shader
// *****************************************************
float3		CameraPosition;
float       WaveHeight = 0.1;
float4      WaterColor = float4(1, 0, 0.5, 0.5);
float       ColorBlendFactor = 0.1;
float4      SpecularColor = float4(1, 0.8, 1, 1);
float       Specular = 256;
sampler2D	WaterBump;
sampler2D	RefractionMap;
sampler2D	ReflectionMap;

struct PixcelOutput
{
	float4 color : COLOR0;
};

PixcelOutput PixelShaderFunction(VertexOutput input)
{
	PixcelOutput output;
	
	float4 bumpColor = tex2D(WaterBump, input.bumpMapTexCoord);
	float2 perturbation = WaveHeight * (bumpColor.rg - 0.5) * 2;
	
	float2 ProjectedRefractionTexCoords;
	ProjectedRefractionTexCoords.x = (input.refractionMapTexCoord.x / input.refractionMapTexCoord.w) / 2 + 0.5;
	ProjectedRefractionTexCoords.y = -(input.refractionMapTexCoord.y / input.refractionMapTexCoord.w) / 2 + 0.5;
	
	float4 refractiveColor = tex2D(RefractionMap, ProjectedRefractionTexCoords + perturbation);
	
	float2 ProjectedReflectionTexCoords;
	ProjectedReflectionTexCoords.x = (input.reflectionMapTexCoord.x / input.reflectionMapTexCoord.w) / 2 + 0.5;
	ProjectedReflectionTexCoords.y = -(input.reflectionMapTexCoord.y / input.reflectionMapTexCoord.w) / 2 + 0.5;
	
	float4 reflectiveColor = tex2D(ReflectionMap, ProjectedReflectionTexCoords + perturbation);

	float3 eyeVector = normalize(CameraPosition - input.position3D);
	float3 normalVector = normalize((bumpColor.rbg - 0.5) * 2);
	
	float fresnelTerm = dot(normalVector, eyeVector) + 0.5;
	fresnelTerm = clamp(fresnelTerm, 0, 1);
	
	output.color = lerp(reflectiveColor, refractiveColor, fresnelTerm);
	
	float3 lightVector = normalize(float3(0, 1024, -256) - input.position3D);
	float3 reflectionVector = reflect(lightVector, normalVector);
	float specular = dot(normalize(reflectionVector), eyeVector);
	specular = pow(specular, Specular);        
	output.color = output.color + SpecularColor * specular;
	output.color = (WaterColor * ColorBlendFactor) + (output.color * (1 - ColorBlendFactor));
    
	return output;
};

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
