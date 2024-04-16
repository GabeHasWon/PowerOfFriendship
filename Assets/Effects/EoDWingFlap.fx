sampler2D tex;
float topWing;
float bottomWing;
texture depthMap;
float depthFactor;

sampler2D depthSampler = sampler_state
{
    Texture = (depthMap);
    AddressU = wrap;
    AddressV = wrap;
};

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float wingLoc = lerp(topWing, bottomWing, uv.y);
    float2 location = float2(lerp(uv.x, 0.5, wingLoc), uv.y);
    float4 color = tex2D(tex, location);
    float4 depth = tex2D(depthSampler, location);
    color = lerp(color, float4(0, 0, 0, 1), (1 - depth.r) * depthFactor) * color.a;
    return color;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile ps_2_0 main();
    }
};
