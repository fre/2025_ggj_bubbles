#ifndef BUBBLES_FUNCTIONS_INCLUDED
#define BUBBLES_FUNCTIONS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

// Helper function to get exact texel coordinates
float2 GetTexelCoords(int column, int row, int maxRows)
{
    // Use integer division and explicit conversion to avoid precision loss
    return float2(
        (column + 0.5) / 4.0,           // x coordinate (4 columns)
        (row + 0.5) / float(maxRows)    // y coordinate
    );
}

float4 GetBubbleData(UnityTexture2D BubbleData, UnitySamplerState BubbleDataSampler, float bubbleIndex, int column, int MaxBubbleCount)
{
    // Convert bubbleIndex to integer to ensure exact texel lookup
    int row = (int)bubbleIndex;
    float2 uv = GetTexelCoords(column, row, MaxBubbleCount);
    return SAMPLE_TEXTURE2D_LOD(BubbleData, BubbleDataSampler, uv, 0);
}

void FindClosestBubbles_float(
    float2 WorldPos,
    UnityTexture2D BubbleData,
    UnitySamplerState BubbleDataSampler,
    float BubbleCount,
    float MaxBubbleCount,
    float SmallRadiusPreservationFactor,
    out float ClosestDist,
    out float SecondClosestDist,
    out float4 ClosestBubbleData,
    out float4 SecondClosestBubbleData)
{
    float minDist = 1000;
    float secondMinDist = 1000;
    float4 closestData = float4(0,0,0,0);
    float4 secondData = float4(0,0,0,0);
    
    int bubbleCountInt = (int)BubbleCount;
    int maxBubbleCountInt = (int)MaxBubbleCount;
    
    for(int i = 0; i < bubbleCountInt; i++)
    {
        // Use integer-based lookup for position and wave data
        float2 posUV = GetTexelCoords(0, i, maxBubbleCountInt); // Column 0 for position
        float2 waveUV = GetTexelCoords(3, i, maxBubbleCountInt); // Column 3 for wave
        
        float4 bubbleData = SAMPLE_TEXTURE2D_LOD(BubbleData, BubbleDataSampler, posUV, 0);
        float4 waveData = SAMPLE_TEXTURE2D_LOD(BubbleData, BubbleDataSampler, waveUV, 0);
        
        float2 center = bubbleData.xy;
        float radius = bubbleData.z;
        
        float waveAmplitude = waveData.r;
        float waveCount = waveData.g;
        float waveRotation = waveData.b;
        
        // Calculate angle to current pixel
        float2 toPixel = WorldPos - center;
        float angle = atan2(toPixel.y, toPixel.x);
        
        // Calculate wave offset
        float wavePhase = angle + waveRotation;
        float waveOffset = sin(wavePhase * waveCount) * waveAmplitude;
        float adjustedRadius = radius * (1 + waveOffset);
        
        // Calculate distance with radius preservation
        float rawDist = length(toPixel);
        float radiusPreservation = lerp(1, 1 / max(adjustedRadius, 0.001), SmallRadiusPreservationFactor);
        float d = rawDist / (adjustedRadius * radiusPreservation);
        
        if (d < 1 && d < minDist)
        {
            secondMinDist = minDist;
            secondData = closestData;
            minDist = d;
            closestData = float4(center, adjustedRadius, bubbleData.w); // Store adjusted radius
        }
        else if (d > minDist && d < 1 && d < secondMinDist)
        {
            secondMinDist = d;
            secondData = float4(center, adjustedRadius, bubbleData.w); // Store adjusted radius
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
    float OutlineSmoothRadius,
    float SmallRadiusPreservationFactor,
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
    float radius = ClosestBubbleData.z; // This is now the pre-adjusted radius
    float bubbleIndex = ClosestBubbleData.w;
    
    float4 bubbleData1 = GetBubbleData(BubbleData, BubbleDataSampler, bubbleIndex, 1, MaxBubbleCount);
    float4 bubbleData2 = GetBubbleData(BubbleData, BubbleDataSampler, bubbleIndex, 2, MaxBubbleCount);
    float hoverT = bubbleData1.g;

    // Get full HSV color data
    float hue = bubbleData2.r;
    float saturation = bubbleData2.g;
    float value = bubbleData2.b;

    // Convert HSV to RGB color
    float3 bubbleColor = HsvToRgb(float3(hue, saturation, value));

    // Calculate outline thickness and color based on hover state
    float currentOutlineThickness = lerp(OutlineThickness, HoverOutlineThickness, hoverT);
    float4 currentOutlineColor = lerp(OutlineColor, HoverOutlineColor, hoverT);
    
    // Calculate radius preservation factor (same as in FindClosestBubbles_float)
    float radiusPreservation = lerp(1, 1 / max(radius, 0.001), SmallRadiusPreservationFactor);
    
    // Calculate smooth outline transition - compensate for radius preservation
    float outlineDistance = (ClosestDist * radius + currentOutlineThickness / radiusPreservation) - radius;
    float outlineFactor = smoothstep(-OutlineSmoothRadius, OutlineSmoothRadius, outlineDistance);
    
    // Calculate smooth interface transition
    float distanceBetweenCenters = length(ClosestBubbleData.xy - SecondClosestBubbleData.xy);
    float actualDistanceAtPixel = ClosestDist * radius + SecondClosestDist * SecondClosestBubbleData.z;
    float thicknessMultiplier = pow(distanceBetweenCenters / actualDistanceAtPixel, 0.5);
    thicknessMultiplier = thicknessMultiplier * 0.6; // Thinner interface
    
    // Compensate interface thickness based on average radius preservation
    float avgRadiusPreservation = lerp(1, 1 / max((radius + SecondClosestBubbleData.z) * 0.5, 0.001), SmallRadiusPreservationFactor);
    avgRadiusPreservation = pow(avgRadiusPreservation, 1) * 1;
    float interfaceDistance = abs(SecondClosestDist - ClosestDist) * radius - (currentOutlineThickness * thicknessMultiplier / avgRadiusPreservation);
    float interfaceFactor = SecondClosestDist < 1 ? 
        smoothstep(OutlineSmoothRadius, -OutlineSmoothRadius, interfaceDistance) : 0;
    
    // Combine outline and interface factors
    float outlineStrength = max(outlineFactor, interfaceFactor);
    
    // Calculate base alpha from distance with both falloff parameters
    float baseAlpha = GetBubbleAlpha(ClosestDist, CoreOpacity, EdgeOpacity, OpacityFalloff, OpacitySmoothing);
    
    // Blend between bubble color and outline color
    Color = lerp(bubbleColor, currentOutlineColor.rgb, outlineStrength);
    Alpha = lerp(baseAlpha, currentOutlineColor.a, outlineStrength);
    
    // Ensure we're fully transparent outside the bubble
    Alpha *= (ClosestDist < 1);
}

#endif 