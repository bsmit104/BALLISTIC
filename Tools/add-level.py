import os
import sys

EMPTY_SCENE = '''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!29 &1
OcclusionCullingSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_OcclusionBakeSettings:
    smallestOccluder: 5
    smallestHole: 0.25
    backfaceThreshold: 100
  m_SceneGUID: 00000000000000000000000000000000
  m_OcclusionCullingData: {fileID: 0}
--- !u!104 &2
RenderSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 9
  m_Fog: 0
  m_FogColor: {r: 0.5, g: 0.5, b: 0.5, a: 1}
  m_FogMode: 3
  m_FogDensity: 0.01
  m_LinearFogStart: 0
  m_LinearFogEnd: 300
  m_AmbientSkyColor: {r: 0.212, g: 0.227, b: 0.259, a: 1}
  m_AmbientEquatorColor: {r: 0.114, g: 0.125, b: 0.133, a: 1}
  m_AmbientGroundColor: {r: 0.047, g: 0.043, b: 0.035, a: 1}
  m_AmbientIntensity: 1
  m_AmbientMode: 0
  m_SubtractiveShadowColor: {r: 0.42, g: 0.478, b: 0.627, a: 1}
  m_SkyboxMaterial: {fileID: 10304, guid: 0000000000000000f000000000000000, type: 0}
  m_HaloStrength: 0.5
  m_FlareStrength: 1
  m_FlareFadeSpeed: 3
  m_HaloTexture: {fileID: 0}
  m_SpotCookie: {fileID: 10001, guid: 0000000000000000e000000000000000, type: 0}
  m_DefaultReflectionMode: 0
  m_DefaultReflectionResolution: 128
  m_ReflectionBounces: 1
  m_ReflectionIntensity: 1
  m_CustomReflection: {fileID: 0}
  m_Sun: {fileID: 0}
  m_IndirectSpecularColor: {r: 0.18028378, g: 0.22571412, b: 0.30692285, a: 1}
  m_UseRadianceAmbientProbe: 0
--- !u!157 &3
LightmapSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 12
  m_GIWorkflowMode: 1
  m_GISettings:
    serializedVersion: 2
    m_BounceScale: 1
    m_IndirectOutputScale: 1
    m_AlbedoBoost: 1
    m_EnvironmentLightingMode: 0
    m_EnableBakedLightmaps: 1
    m_EnableRealtimeLightmaps: 0
  m_LightmapEditorSettings:
    serializedVersion: 12
    m_Resolution: 2
    m_BakeResolution: 40
    m_AtlasSize: 1024
    m_AO: 0
    m_AOMaxDistance: 1
    m_CompAOExponent: 1
    m_CompAOExponentDirect: 0
    m_ExtractAmbientOcclusion: 0
    m_Padding: 2
    m_LightmapParameters: {fileID: 0}
    m_LightmapsBakeMode: 1
    m_TextureCompression: 1
    m_FinalGather: 0
    m_FinalGatherFiltering: 1
    m_FinalGatherRayCount: 256
    m_ReflectionCompression: 2
    m_MixedBakeMode: 2
    m_BakeBackend: 1
    m_PVRSampling: 1
    m_PVRDirectSampleCount: 32
    m_PVRSampleCount: 512
    m_PVRBounces: 2
    m_PVREnvironmentSampleCount: 256
    m_PVREnvironmentReferencePointCount: 2048
    m_PVRFilteringMode: 1
    m_PVRDenoiserTypeDirect: 1
    m_PVRDenoiserTypeIndirect: 1
    m_PVRDenoiserTypeAO: 1
    m_PVRFilterTypeDirect: 0
    m_PVRFilterTypeIndirect: 0
    m_PVRFilterTypeAO: 0
    m_PVREnvironmentMIS: 1
    m_PVRCulling: 1
    m_PVRFilteringGaussRadiusDirect: 1
    m_PVRFilteringGaussRadiusIndirect: 5
    m_PVRFilteringGaussRadiusAO: 2
    m_PVRFilteringAtrousPositionSigmaDirect: 0.5
    m_PVRFilteringAtrousPositionSigmaIndirect: 2
    m_PVRFilteringAtrousPositionSigmaAO: 1
    m_ExportTrainingData: 0
    m_TrainingDataDestination: TrainingData
    m_LightProbeSampleCountMultiplier: 4
  m_LightingDataAsset: {fileID: 0}
  m_LightingSettings: {fileID: 0}
--- !u!196 &4
NavMeshSettings:
  serializedVersion: 2
  m_ObjectHideFlags: 0
  m_BuildSettings:
    serializedVersion: 3
    agentTypeID: 0
    agentRadius: 0.5
    agentHeight: 2
    agentSlope: 45
    agentClimb: 0.4
    ledgeDropHeight: 0
    maxJumpAcrossDistance: 0
    minRegionArea: 2
    manualCellSize: 0
    cellSize: 0.16666667
    manualTileSize: 0
    tileSize: 256
    buildHeightMesh: 0
    maxJobWorkers: 0
    preserveTilesOutsideBounds: 0
    debug:
      m_Flags: 0
  m_NavMeshData: {fileID: 0}
--- !u!1 &323551096
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 323551098}
  - component: {fileID: 323551097}
  - component: {fileID: 323551099}
  m_Layer: 0
  m_Name: Directional Light
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!108 &323551097
Light:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 323551096}
  m_Enabled: 1
  serializedVersion: 10
  m_Type: 1
  m_Shape: 0
  m_Color: {r: 1, g: 0.95686275, b: 0.8392157, a: 1}
  m_Intensity: 1
  m_Range: 10
  m_SpotAngle: 30
  m_InnerSpotAngle: 21.80208
  m_CookieSize: 10
  m_Shadows:
    m_Type: 2
    m_Resolution: -1
    m_CustomResolution: -1
    m_Strength: 1
    m_Bias: 0.05
    m_NormalBias: 0.4
    m_NearPlane: 0.2
    m_CullingMatrixOverride:
      e00: 1
      e01: 0
      e02: 0
      e03: 0
      e10: 0
      e11: 1
      e12: 0
      e13: 0
      e20: 0
      e21: 0
      e22: 1
      e23: 0
      e30: 0
      e31: 0
      e32: 0
      e33: 1
    m_UseCullingMatrixOverride: 0
  m_Cookie: {fileID: 0}
  m_DrawHalo: 0
  m_Flare: {fileID: 0}
  m_RenderMode: 0
  m_CullingMask:
    serializedVersion: 2
    m_Bits: 4294967295
  m_RenderingLayerMask: 1
  m_Lightmapping: 4
  m_LightShadowCasterMode: 0
  m_AreaSize: {x: 1, y: 1}
  m_BounceIntensity: 1
  m_ColorTemperature: 6570
  m_UseColorTemperature: 0
  m_BoundingSphereOverride: {x: 0, y: 0, z: 0, w: 0}
  m_UseBoundingSphereOverride: 0
  m_UseViewFrustumForShadowCasterCull: 1
  m_ShadowRadius: 0
  m_ShadowAngle: 0
--- !u!4 &323551098
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 323551096}
  serializedVersion: 2
  m_LocalRotation: {x: 0.40821788, y: -0.23456968, z: 0.10938163, w: 0.8754261}
  m_LocalPosition: {x: 0, y: 3, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 50, y: -30, z: 0}
--- !u!114 &323551099
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 323551096}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 474bcb49853aa07438625e644c072ee6, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Version: 3
  m_UsePipelineSettings: 1
  m_AdditionalLightsShadowResolutionTier: 2
  m_LightLayerMask: 1
  m_RenderingLayers: 1
  m_CustomShadowLayers: 0
  m_ShadowLayerMask: 1
  m_ShadowRenderingLayers: 1
  m_LightCookieSize: {x: 1, y: 1}
  m_LightCookieOffset: {x: 0, y: 0}
  m_SoftShadowQuality: 0
--- !u!1001 &1707867838
PrefabInstance:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_Modification:
    serializedVersion: 3
    m_TransformParent: {fileID: 0}
    m_Modifications:
    - target: {fileID: 275697748705265996, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: SortKey
      value: 1531937604
      objectReference: {fileID: 0}
    - target: {fileID: 798057231964493584, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: SortKey
      value: 1591826063
      objectReference: {fileID: 0}
    - target: {fileID: 2611273102787778954, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: SortKey
      value: 2966365382
      objectReference: {fileID: 0}
    - target: {fileID: 2697214737487789293, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: SortKey
      value: 1547914652
      objectReference: {fileID: 0}
    - target: {fileID: 2776174001797433552, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: SortKey
      value: 996389337
      objectReference: {fileID: 0}
    - target: {fileID: 3432676379480146616, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: SortKey
      value: 3587562739
      objectReference: {fileID: 0}
    - target: {fileID: 4612309535144288501, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: m_Name
      value: LevelEssentials
      objectReference: {fileID: 0}
    - target: {fileID: 5040506309533289376, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: SortKey
      value: 1075745862
      objectReference: {fileID: 0}
    - target: {fileID: 5283588528472378637, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: SortKey
      value: 2656951239
      objectReference: {fileID: 0}
    - target: {fileID: 5512857257485532251, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: SortKey
      value: 395472578
      objectReference: {fileID: 0}
    - target: {fileID: 6148741352709508263, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: SortKey
      value: 2244578411
      objectReference: {fileID: 0}
    - target: {fileID: 6204254892566965464, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: SortKey
      value: 1262259187
      objectReference: {fileID: 0}
    - target: {fileID: 6508344919766276822, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: SortKey
      value: 3106394001
      objectReference: {fileID: 0}
    - target: {fileID: 6978363855565539175, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: SortKey
      value: 2322514781
      objectReference: {fileID: 0}
    - target: {fileID: 7427991545039650645, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: SortKey
      value: 449648998
      objectReference: {fileID: 0}
    - target: {fileID: 8201417365615801484, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: SortKey
      value: 3944627703
      objectReference: {fileID: 0}
    - target: {fileID: 8462842248272862170, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: m_LocalPosition.x
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: 8462842248272862170, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: m_LocalPosition.y
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: 8462842248272862170, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: m_LocalPosition.z
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: 8462842248272862170, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: m_LocalRotation.w
      value: 1
      objectReference: {fileID: 0}
    - target: {fileID: 8462842248272862170, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: m_LocalRotation.x
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: 8462842248272862170, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: m_LocalRotation.y
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: 8462842248272862170, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: m_LocalRotation.z
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: 8462842248272862170, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: m_LocalEulerAnglesHint.x
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: 8462842248272862170, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: m_LocalEulerAnglesHint.y
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: 8462842248272862170, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: m_LocalEulerAnglesHint.z
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: 8738007866552296342, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
      propertyPath: SortKey
      value: 3807152319
      objectReference: {fileID: 0}
    m_RemovedComponents: []
    m_RemovedGameObjects: []
    m_AddedGameObjects: []
    m_AddedComponents: []
  m_SourcePrefab: {fileID: 100100000, guid: d7d895c44bbeb8444b2c5aa69c2df9b6, type: 3}
--- !u!1660057539 &9223372036854775807
SceneRoots:
  m_ObjectHideFlags: 0
  m_Roots:
  - {fileID: 323551098}
  - {fileID: 1707867838}

'''

LEVEL_PATHS = [
    "../BALLISTIC/Assets/Levels",
    "BALLISTIC/Assets/Levels",
    "Assets/Levels",
    "Levels"
]


def build_level(level_name: str):

    valid_path: None | str = None

    # Find path to the Levels folder
    if os.getcwd().endswith("Levels"):
        valid_path = ""
    else:
        for path in LEVEL_PATHS:
            if os.path.exists(path):
                valid_path = path + "/"
                break
    
    if valid_path is None:
        print("Cannot run script from here. Go to the project root BALLISTIC folder, the Tools folder, or the Levels folder.")
        return
    

    folder_path = valid_path + level_name

    if os.path.exists(folder_path):
        print("A '" + level_name + "' level already exists, please use a different name.")
        return

    # Add folders
    print("Adding files...")

    os.makedirs(folder_path)

    os.makedirs(folder_path + "/Materials")
    with open(folder_path + "/Materials/README.txt", "w") as readme:
        readme.write("Materials folder for any textures used in this level. Assign them to models in their 'Materials' tab.")

    os.makedirs(folder_path + "/Models")
    with open(folder_path + "/Models/README.txt", "w") as readme:
        readme.write("Models folder for any imported models. Create prefabs for them so that they can have properly set-up game objects.")

    os.makedirs(folder_path + "/Prefabs")
    with open(folder_path + "/Prefabs/README.txt", "w") as readme:
        readme.write("Prefabs folder for any game objects you'll be creating.")

    os.makedirs(folder_path + "/Scripts")
    with open(folder_path + "/Scripts/README.txt", "w") as readme:
        readme.write("Scripts folder for any level specific scripts you need for interactable objects.")

    # Create empty scene with level essentials
    with open(folder_path + "/" + level_name + ".unity", "w") as scene:
        scene.write(EMPTY_SCENE)

    print("Set up complete!")

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: $ python add-level.py <LevelName>")
    else:
        level_name = sys.argv[1]
        build_level(level_name)