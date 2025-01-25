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

void CalculateBubbleColor_float(
    float ClosestDist,
    float SecondClosestDist,
    float4 ClosestBubbleData,
    float4 SecondClosestBubbleData,
    float3 BackgroundColor,
    float3 OutlineColor,
    float OutlineThickness,
    out float3 Color,
    out float Alpha)
{
    float3 finalColor = BackgroundColor;
    float alpha = 1;
    float radius = ClosestBubbleData.z;
    float secondRadius = SecondClosestBubbleData.z;
    
    if (ClosestDist < 1)
    {
        float3 bubbleColor = HsvToRgb(float3(ClosestBubbleData.w, 0.7, 0.6));
        bool isOutline = (ClosestDist * radius + OutlineThickness) > radius;

        // Distance between the two bubbles
        float distanceBetweenCenters = length(ClosestBubbleData.xy - SecondClosestBubbleData.xy);
        float actualDistanceAtPixel = ClosestDist * radius + SecondClosestDist * secondRadius;
        float thicknessMultiplier = distanceBetweenCenters / actualDistanceAtPixel;
        bool isInterface = SecondClosestDist < 1 && abs(SecondClosestDist - ClosestDist)
            * radius < OutlineThickness * thicknessMultiplier ;
        
        finalColor = (isOutline || isInterface) ? OutlineColor : bubbleColor;
        alpha = 1.0;
    }
    
    Color = finalColor;
    Alpha = alpha;
}

#endif 