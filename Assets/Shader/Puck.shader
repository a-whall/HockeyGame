// This shader is based on an answer to this thread:
// https://answers.unity.com/questions/26486/display-complex-object-when-it-is-behind-the-wall.html

Shader "Custom/Puck"
{
    Properties
    {
        _Regular_Color ("Regular Color", Color) = (0,0,0,1)
        _Occluded_Color ("Occluded Color", Color) = (1,1,0,1)
    }
    Category
    {
        SubShader
        {
            Tags { "Queue"="Overlay+1" }

            Pass
            {
                ZWrite Off
                ZTest Greater
                Lighting Off
                Color [_Occluded_Color]
            }
            Pass
            {
                ZTest Less
                Color [_Color]
            }
        }
    }
    FallBack "Diffuse"
}