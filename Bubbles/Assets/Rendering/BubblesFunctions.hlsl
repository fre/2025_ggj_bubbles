#ifndef BUBBLES_FUNCTIONS_INCLUDED
#define BUBBLES_FUNCTIONS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

void FindClosestBubbles_float(
    float2 WorldPos,  // Now taking world position directly
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
        float4 bubbleData = SAMPLE_TEXTURE2D_LOD(BubbleData, BubbleDataSampler, float2((float)i / MaxBubbleCount, 0), 0);
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
    // Basic distance curve (0 at center, 1 at edge)
    float t = distanceFromCenter;  // Remove saturation to allow overflow
    
    // Apply smoothing as a power curve near center (allows overflow)
    float smoothPower = lerp(1, 0.5, smoothing);  // Smoothing now affects curve steepness
    float smoothT = pow(abs(t), smoothPower) * sign(t);
    
    // Apply main falloff curve
    float curveT = pow(abs(smoothT), falloff) * sign(smoothT);
    
    // Allow overflow in center while maintaining edge behavior
    float alpha = lerp(coreOpacity, edgeOpacity, saturate(curveT));
    
    // Boost center alpha based on smoothing
    float centerBoost = 1 + smoothing * 2;  // More smoothing = more center boost
    alpha *= lerp(centerBoost, 1, saturate(curveT));  // Only boost center
    
    return alpha;
}

void CalculateBubbleColor_float(
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
    out float3 Color,
    out float Alpha)
{
    // Initialize with fully transparent background
    Color = BackgroundColor.rgb;
    Alpha = 0;
    
    // Early out if no bubble
    if (ClosestDist >= 1)
        return;
        
    // Convert hue to RGB color
    float3 bubbleColor = HsvToRgb(float3(ClosestBubbleData.w, 0.7, 0.6));
    float radius = ClosestBubbleData.z;
    
    // Calculate outline and interface
    bool isOutline = (ClosestDist * radius + OutlineThickness) > radius;
    float distanceBetweenCenters = length(ClosestBubbleData.xy - SecondClosestBubbleData.xy);
    float actualDistanceAtPixel = ClosestDist * radius + SecondClosestDist * SecondClosestBubbleData.z;
    float thicknessMultiplier = distanceBetweenCenters / actualDistanceAtPixel;
    bool isInterface = SecondClosestDist < 1 && 
        abs(SecondClosestDist - ClosestDist) * radius < OutlineThickness * thicknessMultiplier;
    
    // Set color based on whether we're in outline/interface
    Color = (isOutline || isInterface) ? OutlineColor.rgb : bubbleColor;
    
    // Calculate base alpha from distance with both falloff parameters
    Alpha = GetBubbleAlpha(ClosestDist, CoreOpacity, EdgeOpacity, OpacityFalloff, OpacitySmoothing);
    
    // Use outline alpha for outlines/interfaces
    if (isOutline || isInterface)
    {
        Alpha = OutlineColor.a;
    }
    
    // Ensure we're fully transparent outside the bubble
    Alpha *= (ClosestDist < 1);
}

#endif 