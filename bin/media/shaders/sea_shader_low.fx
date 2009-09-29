// -------------------------------------------------------------
// Vertex Shader function
// -------------------------------------------------------------
float4 VS(float4 position : POSITION) : POSITION
{
	return position;
}

// -------------------------------------------------------------
// Pixel Shader function
// -------------------------------------------------------------
float4 PS() : COLOR
{
	return float4(1, 1, 1, 1);
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
