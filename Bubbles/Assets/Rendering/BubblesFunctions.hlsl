#ifndef BUBBLES_FUNCTIONS_INCLUDED
#define BUBBLES_FUNCTIONS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

float4 GetBubbleData(UnityTexture2D BubbleData, UnitySamplerState BubbleDataSampler, float bubbleIndex, int column, int MaxBubbleCount)
{
    // Convert raw index to normalized UV coordinate
    float2 uv = float2((float)column / 4, bubbleIndex / MaxBubbleCount);
    return SAMPLE_TEXTURE2D_LOD(BubbleData, BubbleDataSampler, uv, 0);
}

void FindClosestBubbles_float(
    float2 WorldPos,
    UnityTexture2D BubbleData,
    UnitySamplerState BubbleDataSampler,
    float BubbleCount,
    float MaxBubbleCount,
    out float ClosestDist,
    out float SecondClosestDist,
    out float4 ClosestBubbleData,
    out float4 SecondClosestBubbleData)
{
    float minDist = 1000;
    float secondMinDist = 1000;
    float4 closestData = float4(0,0,0,0);
    float4 secondData = float4(0,0,0,0);
    
    for(int i = 0; i < (int)BubbleCount; i++)
    {
        // Read from column 0 (position data)
        float2 uv = float2(0, (float)i / MaxBubbleCount); // Normalize index for UV
        float4 bubbleData = SAMPLE_TEXTURE2D_LOD(BubbleData, BubbleDataSampler, uv, 0);
        float2 center = bubbleData.xy;
        float radius = bubbleData.z;
        float d = length(WorldPos - center) / radius;
        
        if (d < 1 && d < minDist)
        {
            secondMinDist = minDist;
            secondData = closestData;
            minDist = d;
            closestData = bubbleData;
        }
        else if (d > minDist && d < 1 && d < secondMinDist)
        {
            secondMinDist = d;
            secondData = bubbleData;
        }
    }
    
    ClosestDist = minDist;
    SecondClosestDist = secondMinDist;
    ClosestBubbleData = closestData;
    SecondClosestBubbleData = secondData;
}

float GetBubbleAlpha(float distanceFromCenter, float coreOpacity, float edgeOpacity, float falloff, float smoothing)
{
    // Remap distance to create a smooth transition centered around the falloff point
    float t = smoothstep(falloff - smoothing * 0.5, falloff + smoothing * 0.5, distanceFromCenter);
    
    // Lerp between core and edge opacity using the smoothed transition
    return lerp(coreOpacity, edgeOpacity, t);
}

void CalculateBubbleColor_float(
    UnityTexture2D BubbleData,
    UnitySamplerState BubbleDataSampler,
    float MaxBubbleCount,
    float ClosestDist,
    float SecondClosestDist,
    float4 ClosestBubbleData,
    float4 SecondClosestBubbleData,
    float4 BackgroundColor,
    float4 OutlineColor,
    float OutlineThickness,
    float CoreOpacity,
    float EdgeOpacity,
    float OpacityFalloff,
    float OpacitySmoothing,
    float4 HoverOutlineColor,
    float HoverOutlineThickness,
    out float3 Color,
    out float Alpha)
{
    // Initialize with fully transparent background
    Color = BackgroundColor.rgb;
    Alpha = BackgroundColor.a;
    
    // Early out if no bubble
    if (ClosestDist >= 1)
        return;

    // Get raw bubble index from column 0
    float2 center = ClosestBubbleData.xy;
    float radius = ClosestBubbleData.z;
    float bubbleIndex = ClosestBubbleData.w;
    
    float4 bubbleData1 = GetBubbleData(BubbleData, BubbleDataSampler, bubbleIndex, 1, MaxBubbleCount);
    float hue = bubbleData1.r;
    float hoverT = bubbleData1.g;

    // Convert hue to RGB color
    float3 bubbleColor = HsvToRgb(float3(hue, 0.7, 0.6));

    // Calculate outline and interface
    bool isOutline = (ClosestDist * radius + OutlineThickness) > radius;
    float distanceBetweenCenters = length(ClosestBubbleData.xy - SecondClosestBubbleData.xy);
    float actualDistanceAtPixel = ClosestDist * radius + SecondClosestDist * SecondClosestBubbleData.z;
    float thicknessMultiplier = distanceBetweenCenters / actualDistanceAtPixel;
    bool isInterface = SecondClosestDist < 1 && 
        abs(SecondClosestDist - ClosestDist) * radius < OutlineThickness * thicknessMultiplier;
    
    // Calculate base alpha from distance with both falloff parameters
    float baseAlpha = GetBubbleAlpha(ClosestDist, CoreOpacity, EdgeOpacity, OpacityFalloff, OpacitySmoothing);
    
    // Set color and alpha based on region
    if (isOutline || isInterface)
    {
        Color = OutlineColor.rgb;
        Alpha = OutlineColor.a;
    }
    else
    {
        Color = bubbleColor;
        Alpha = baseAlpha;
    }
    
    // Ensure we're fully transparent outside the bubble
    Alpha *= (ClosestDist < 1);
}

#endif 