#ifndef ADDITIONAL_LIGHTS_INCLUDED
#define ADDITIONAL_LIGHTS_INCLUDED

//------------------------------------------------------------------------------------------------------
// Will Additional Lights
//------------------------------------------------------------------------------------------------------

/*
- Ben Cloward's Phong Specular
*/
float BenSpecular(half3 lightDir, half3 normal, half3 viewDir, half focus, half brightness) {
	half reflection = reflect(lightDir, normal);
	half product = saturate(dot(reflection, -1 * viewDir));
	return pow(product, focus) * brightness;
}

float NedDiffuse(half3 lightDir, half3 normal) {
	return saturate(dot(normal, lightDir));
}

/*
- Handles additional lights (e.g. point, spotlights) with simplified Specular lighting
- For shadows to work in the Unlit Graph, the following keywords must be defined in the blackboard :
	- Boolean Keyword, Global Multi-Compile "_ADDITIONAL_LIGHT_SHADOWS"
	- Boolean Keyword, Global Multi-Compile "_ADDITIONAL_LIGHTS" (required to prevent the one above from being stripped from builds)
- For a PBR/Lit Graph, these keywords are already handled for you.
*/
void AdditionalLightsWill_float(float SpecularFocus, float SpecularBrightness, float3 WorldPosition, float3 WorldNormal, float3 WorldView, half4 Shadowmask,
							float MainDiffuse, float MainSpecular, float3 MainColor,
							out float Diffuse, out float Specular, out float3 Color) {
	float intensity = max(MainColor.r, max(MainColor.g, MainColor.b));
	float diffuse = MainDiffuse * intensity;
	float specular = MainSpecular;
	float3 color = MainColor;
	float highestDiffuse = MainDiffuse;
#ifndef SHADERGRAPH_PREVIEW
	uint pixelLightCount = GetAdditionalLightsCount();
	uint meshRenderingLayers = GetMeshRenderingLayer();

	#if USE_FORWARD_PLUS
	for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++) {
		FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
		Light light = GetAdditionalLight(lightIndex, WorldPosition, Shadowmask);
	#ifdef _LIGHT_LAYERS
		if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
	#endif
		{
			int perObjectLightIndex = GetPerObjectLightIndex(lightIndex);
			light.shadowAttenuation = AdditionalLightShadow(perObjectLightIndex, WorldPosition, light.direction, Shadowmask, _AdditionalLightsOcclusionProbes[perObjectLightIndex]);
			half atten = light.distanceAttenuation * light.shadowAttenuation * max(light.color.r, max(light.color.g, light.color.b));
			float thisDiffuse = NedDiffuse(light.direction, WorldNormal) * atten;
			diffuse += thisDiffuse;
			specular += BenSpecular(light.direction, WorldNormal, WorldView, SpecularFocus, SpecularBrightness) * max(light.color.r, max(light.color.g, light.color.b)) * thisDiffuse;
			
			if (thisDiffuse > highestDiffuse) {
				highestDiffuse = thisDiffuse;
				color = light.color;
			}
		}
	}
	#endif

	// For Foward+ the LIGHT_LOOP_BEGIN macro will use inputData.normalizedScreenSpaceUV, inputData.positionWS, so create that:
	InputData inputData = (InputData)0;
	float4 screenPos = ComputeScreenPos(TransformWorldToHClip(WorldPosition));
	inputData.normalizedScreenSpaceUV = screenPos.xy / screenPos.w;
	inputData.positionWS = WorldPosition;

	LIGHT_LOOP_BEGIN(pixelLightCount)
		Light light = GetAdditionalLight(lightIndex, WorldPosition, Shadowmask);
	#ifdef _LIGHT_LAYERS
		if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
	#endif
		{
			int perObjectLightIndex = GetPerObjectLightIndex(lightIndex);
			light.shadowAttenuation = AdditionalLightShadow(perObjectLightIndex, WorldPosition, light.direction, Shadowmask, _AdditionalLightsOcclusionProbes[perObjectLightIndex]);
			half atten = light.distanceAttenuation * light.shadowAttenuation * max(light.color.r, max(light.color.g, light.color.b));
			float thisDiffuse = NedDiffuse(light.direction, WorldNormal) * atten;
			diffuse += thisDiffuse;
			specular += BenSpecular(light.direction, WorldNormal, WorldView, SpecularFocus, SpecularBrightness) * max(light.color.r, max(light.color.g, light.color.b)) * thisDiffuse;
			
			if (thisDiffuse > highestDiffuse) {
				highestDiffuse = thisDiffuse;
				color = light.color;
			}
		}
	LIGHT_LOOP_END
#endif

	Diffuse = diffuse;
	Specular = specular;
	Color = color;
}

#endif // ADDITIONAL_LIGHTS_INCLUDED