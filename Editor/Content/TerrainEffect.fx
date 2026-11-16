
#if OPENGL
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix View;
matrix Projection;

float3 LightDirection;
float TextureTiling = 15.0f;
bool Tint = false;

texture BaseTexture;

sampler BaseTextureSampler = sampler_state
{
    Texture = <BaseTexture>;
    AddressU = Wrap;
    AddressV = Wrap;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 Normal : TEXCOORD1;
    float2 UV : TEXCOORD0;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    float4 viewPosition = mul(input.Position, View);
    output.Position = mul(viewPosition, Projection);

    output.Normal = input.Normal;
    output.UV = input.UV * TextureTiling;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float3 normal = normalize(input.Normal);
    float3 light = normalize(LightDirection);

    float diffuse = saturate(dot(normal, light));

    float lighting = 0.3f + diffuse * 0.7f;

    float4 textureColor = tex2D(BaseTextureSampler, input.UV);

    float3 finalColor = textureColor.rgb * lighting;

    // Highlight the terrain when selected.
    if (Tint)
    {
        finalColor = lerp(finalColor, float3(1.0f, 0.85f, 0.3f), 0.4f);
    }

    return float4(finalColor, textureColor.a);
}

technique TerrainDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
