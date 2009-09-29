// -------------------------------------------------------------
// Parameters
// -------------------------------------------------------------
float4x4 WVPMatrix;
float3 SunVector;
float3 ZenithColor;
float SunTheta;
float InvGammaCorrection;
float InvNegMaxLum;
float InvPowLumFactor;
float NightDarkness;
float BlendingRate;
float CloudAlpha;
float _xDistribCoeffs[5];
float _yDistribCoeffs[5];
float _YDistribCoeffs[5];

sampler CloudTextureSampler : register(s0);
sampler StarTextureSampler : register(s1);

// -------------------------------------------------------------
// Constants
// -------------------------------------------------------------
// -- XYZ to RGB conversion matrix (rec.709 HDTV XYZ to RGB, D65 white point)
const float3x3 XYZtoRGBconv = { {  3.24079f,  -1.53715f, -0.49853f},
								{-0.969256f,  1.875991f,  0.041556f},
								{ 0.055648f, -0.204043f,  1.057311f} };
								
// -------------------------------------------------------------
// Input/Output channels
// -------------------------------------------------------------
struct VertexInput
{
	float4 position : POSITION;
	float2 texcoord : TEXCOORD0;
};

struct VertexOutput
{
	float4 position    : POSITION;
	float2 texcoord    : TEXCOORD0;
	float3 vertexColor : TEXCOORD1;
};

#define PixcelInput VertexOutput


// -------------------------------------------------------------
// Calc function.
// -------------------------------------------------------------
float AngleBetween(float3 point1, float3 point2)
{
	return acos(dot(point1, point2));
}

float PerezFunction(float A, float B, float C, float D, float E, float theta, float gamma)
{
	float cosGamma = cos(gamma);
	return (1.0f + A * exp(B / cos(theta))) * (1.0f + C * exp(D * gamma) + E * cosGamma * cosGamma);
}

float Distribution(float coeffs[5], float theta, float gamma, float zenith)
{
	float A = coeffs[0], B = coeffs[1], C = coeffs[2], D = coeffs[3], E = coeffs[4];
	return (zenith * PerezFunction(A, B, C, D, E, theta, gamma) / PerezFunction(A, B, C, D, E, 0.0f, SunTheta));
}

float3 xyYtoRGB(float3 xyY)
{
	float Yony = xyY.z / xyY.y;
	float3 XYZ = float3(xyY.x * Yony, xyY.z,  (1.0f - xyY.x - xyY.y) * Yony);
						
	return mul(XYZtoRGBconv, XYZ);
}

float3 CalculateColor(float3 vertexVector)
{
	float gamma = AngleBetween(vertexVector, SunVector);
	float theta = AngleBetween(float3(0.0f, 1.0f, 0.0f), vertexVector);

	float3 skyColor;
	
	// Sky color distribution (using the Perez Function)
	skyColor[0] = Distribution(_xDistribCoeffs, theta, gamma, ZenithColor[0]);
	skyColor[1] = Distribution(_yDistribCoeffs, theta, gamma, ZenithColor[1]);
	skyColor[2] = Distribution(_YDistribCoeffs, theta, gamma, ZenithColor[2]);

	// Expononentially scale the luminosity
	skyColor[2] = pow(1.0f - exp(InvNegMaxLum * skyColor[2]), InvPowLumFactor);

	// Convert to RGB and return
	return xyYtoRGB(skyColor);
}

// -------------------------------------------------------------
// Vertex Shader function
// -------------------------------------------------------------
VertexOutput VS(VertexInput input)
{
	VertexOutput output;
    
    output.position = mul(input.position, WVPMatrix);
    output.texcoord = input.texcoord;
     
	output.vertexColor = CalculateColor(normalize(input.position));
	output.vertexColor = pow(output.vertexColor, InvGammaCorrection);
	output.vertexColor *= NightDarkness;
	
	return output;
}

// -------------------------------------------------------------
// Pixel Shader function
// -------------------------------------------------------------
float4 PS(PixcelInput input) : COLOR0
{
	// Base color.
	float4 colorBase = float4(input.vertexColor.rgb, 1);

	// Cloud color.
	float4 colorAdd1 = tex2D(CloudTextureSampler, input.texcoord) * CloudAlpha;

	// Star color.
	float4 colorAdd2 = tex2D(StarTextureSampler, input.texcoord);
	
	// Blend cloud and star.
	float4 blendColor = lerp(colorAdd1, colorAdd2, BlendingRate);

	return colorBase + blendColor - (colorBase * blendColor);
}

// -------------------------------------------------------------
// Technique
// -------------------------------------------------------------
technique TShader
{
    pass P0
    {
        VertexShader = compile vs_2_0 VS();
        PixelShader  = compile ps_2_0 PS();
    }
}
