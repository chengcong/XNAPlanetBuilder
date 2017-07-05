#include "DeferredHeader.fxh"

Texture surfaceTexture;
samplerCUBE TextureSampler = sampler_state 
{ 
    texture = <surfaceTexture> ; 
    magfilter = LINEAR; 
    minfilter = LINEAR; 
    mipfilter = LINEAR; 
    AddressU = Mirror;
    AddressV = Mirror;
};

float4x4 World : World;
float4x4 View : View;
float4x4 Projection : Projection;

float3 EyePosition : CameraPosition;

float alpha = 1;

struct VS_INPUT 
{
    float4 Position    : POSITION0;
    float3 Normal : NORMAL0;    
};

struct VS_OUTPUT 
{
    float4 Position    : POSITION0;
    float4 SPos : TEXCOORD3;
    float3 ViewDirection : TEXCOORD2;
        
};

float4 CubeMapLookup(float3 CubeTexcoord)
{    
    return texCUBE(TextureSampler, CubeTexcoord);
}

VS_OUTPUT Transform(VS_INPUT Input)
{
    float4x4 WorldViewProjection = mul(mul(World, View), Projection);
    float3 ObjectPosition = mul(Input.Position, World);
    
    VS_OUTPUT Output;
    Output.Position = mul(Input.Position, WorldViewProjection);
    Output.ViewDirection = ObjectPosition - EyePosition;    
    Output.SPos = Output.Position;
    return Output;
}

struct PS_INPUT 
{    
    float3 ViewDirection : TEXCOORD2;
    float4 SPos : TEXCOORD3;
};

PixelShaderOutput BasicShader(PS_INPUT Input) : COLOR0
{    
	PixelShaderOutput output = (PixelShaderOutput)0;
	
    float3 ViewDirection = normalize(Input.ViewDirection);    
    ViewDirection.x *= -1;
    
    output.Color = CubeMapLookup(ViewDirection) * alpha;
    
    output.Tangent = -1;
    output.SGR = float4(0,1,0,0);
    output.Depth.r = 1-(Input.SPos.z/Input.SPos.w);
	output.Depth.a = 1;
    
	return output;
}

technique Deferred 
{
    pass P0
    {
        VertexShader = compile vs_2_0 Transform();
        PixelShader  = compile ps_2_0 BasicShader();
    }
}
