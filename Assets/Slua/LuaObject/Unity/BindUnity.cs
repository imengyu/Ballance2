﻿using System;
using System.Collections.Generic;
namespace SLua {
	[LuaBinder(0)]
	public class BindUnity {
		public static Action<IntPtr>[] GetBindList() {
			Action<IntPtr>[] list= {
				Lua_Unity_Profiling_ProfilerMarker.reg,
				Lua_Unity_Profiling_ProfilerMarker_AutoScope.reg,
				Lua_Unity_Profiling_LowLevel_MarkerFlags.reg,
				Lua_JetBrains_Annotations_ImplicitUseKindFlags.reg,
				Lua_JetBrains_Annotations_ImplicitUseTargetFlags.reg,
				Lua_UnityEngine_SendMessageOptions.reg,
				Lua_UnityEngine_PrimitiveType.reg,
				Lua_UnityEngine_Space.reg,
				Lua_UnityEngine_RuntimePlatform.reg,
				Lua_UnityEngine_SystemLanguage.reg,
				Lua_UnityEngine_LogType.reg,
				Lua_UnityEngine_LogOption.reg,
				Lua_UnityEngine_ThreadPriority.reg,
				Lua_UnityEngine_SortingLayer.reg,
				Lua_UnityEngine_WeightedMode.reg,
				Lua_UnityEngine_Keyframe.reg,
				Lua_UnityEngine_WrapMode.reg,
				Lua_UnityEngine_AnimationCurve.reg,
				Lua_UnityEngine_Application.reg,
				Lua_UnityEngine_StackTraceLogType.reg,
				Lua_UnityEngine_UserAuthorization.reg,
				Lua_UnityEngine_ApplicationInstallMode.reg,
				Lua_UnityEngine_ApplicationSandboxType.reg,
				Lua_UnityEngine_AudioType.reg,
				Lua_UnityEngine_CachedAssetBundle.reg,
				Lua_UnityEngine_Cache.reg,
				Lua_UnityEngine_Caching.reg,
				Lua_UnityEngine_Object.reg,
				Lua_UnityEngine_Component.reg,
				Lua_UnityEngine_Behaviour.reg,
				Lua_UnityEngine_Camera.reg,
				Lua_UnityEngine_Camera_GateFitMode.reg,
				Lua_UnityEngine_Camera_FieldOfViewAxis.reg,
				Lua_UnityEngine_Camera_GateFitParameters.reg,
				Lua_UnityEngine_Camera_StereoscopicEye.reg,
				Lua_UnityEngine_Camera_MonoOrStereoscopicEye.reg,
				Lua_UnityEngine_BoundingSphere.reg,
				Lua_UnityEngine_CullingGroupEvent.reg,
				Lua_UnityEngine_CullingGroup.reg,
				Lua_UnityEngine_FlareLayer.reg,
				Lua_UnityEngine_ReflectionProbe.reg,
				Lua_UnityEngine_ReflectionProbe_ReflectionProbeEvent.reg,
				Lua_UnityEngine_CrashReport.reg,
				Lua_UnityEngine_Debug.reg,
				Lua_UnityEngine_ExposedPropertyResolver.reg,
				Lua_UnityEngine_IExposedPropertyTable.reg,
				Lua_UnityEngine_Bounds.reg,
				Lua_UnityEngine_BoundsInt.reg,
				Lua_UnityEngine_GeometryUtility.reg,
				Lua_UnityEngine_Plane.reg,
				Lua_UnityEngine_Ray.reg,
				Lua_UnityEngine_Ray2D.reg,
				Lua_UnityEngine_Rect.reg,
				Lua_UnityEngine_RectInt.reg,
				Lua_UnityEngine_RectOffset.reg,
				Lua_UnityEngine_DynamicGI.reg,
				Lua_UnityEngine_BillboardAsset.reg,
				Lua_UnityEngine_Renderer.reg,
				Lua_UnityEngine_BillboardRenderer.reg,
				Lua_UnityEngine_Display.reg,
				Lua_UnityEngine_FullScreenMode.reg,
				Lua_UnityEngine_SleepTimeout.reg,
				Lua_UnityEngine_Screen.reg,
				Lua_UnityEngine_RenderBuffer.reg,
				Lua_UnityEngine_ComputeBufferMode.reg,
				Lua_UnityEngine_Graphics.reg,
				Lua_UnityEngine_GL.reg,
				Lua_UnityEngine_ScalableBufferManager.reg,
				Lua_UnityEngine_FrameTiming.reg,
				Lua_UnityEngine_FrameTimingManager.reg,
				Lua_UnityEngine_D3DHDRDisplayBitDepth.reg,
				Lua_UnityEngine_HDROutputSettings.reg,
				Lua_UnityEngine_Resolution.reg,
				Lua_UnityEngine_RenderTargetSetup.reg,
				Lua_UnityEngine_QualitySettings.reg,
				Lua_UnityEngine_RendererExtensions.reg,
				Lua_UnityEngine_ImageEffectTransformsToLDR.reg,
				Lua_UnityEngine_ImageEffectAllowedInSceneView.reg,
				Lua_UnityEngine_ImageEffectOpaque.reg,
				Lua_UnityEngine_ImageEffectAfterScale.reg,
				Lua_UnityEngine_ImageEffectUsesCommandBuffer.reg,
				Lua_UnityEngine_Mesh.reg,
				Lua_UnityEngine_Projector.reg,
				Lua_UnityEngine_Shader.reg,
				Lua_UnityEngine_TrailRenderer.reg,
				Lua_UnityEngine_LineRenderer.reg,
				Lua_UnityEngine_MaterialPropertyBlock.reg,
				Lua_UnityEngine_RenderSettings.reg,
				Lua_UnityEngine_Material.reg,
				Lua_UnityEngine_GraphicsBuffer.reg,
				Lua_UnityEngine_GraphicsBuffer_Target.reg,
				Lua_UnityEngine_OcclusionPortal.reg,
				Lua_UnityEngine_OcclusionArea.reg,
				Lua_UnityEngine_Flare.reg,
				Lua_UnityEngine_LensFlare.reg,
				Lua_UnityEngine_Skybox.reg,
				Lua_UnityEngine_MeshFilter.reg,
				Lua_UnityEngine_TransparencySortMode.reg,
				Lua_UnityEngine_StereoTargetEyeMask.reg,
				Lua_UnityEngine_CameraType.reg,
				Lua_UnityEngine_ComputeBufferType.reg,
				Lua_UnityEngine_FogMode.reg,
				Lua_UnityEngine_MixedLightingMode.reg,
				Lua_UnityEngine_ReceiveGI.reg,
				Lua_UnityEngine_ShadowProjection.reg,
				Lua_UnityEngine_ShadowQuality.reg,
				Lua_UnityEngine_ShadowResolution.reg,
				Lua_UnityEngine_ShadowmaskMode.reg,
				Lua_UnityEngine_CameraClearFlags.reg,
				Lua_UnityEngine_DepthTextureMode.reg,
				Lua_UnityEngine_TexGenMode.reg,
				Lua_UnityEngine_AnisotropicFiltering.reg,
				Lua_UnityEngine_SkinWeights.reg,
				Lua_UnityEngine_MeshTopology.reg,
				Lua_UnityEngine_SkinQuality.reg,
				Lua_UnityEngine_ColorSpace.reg,
				Lua_UnityEngine_ColorGamut.reg,
				Lua_UnityEngine_ScreenOrientation.reg,
				Lua_UnityEngine_FilterMode.reg,
				Lua_UnityEngine_TextureWrapMode.reg,
				Lua_UnityEngine_NPOTSupport.reg,
				Lua_UnityEngine_TextureFormat.reg,
				Lua_UnityEngine_CubemapFace.reg,
				Lua_UnityEngine_RenderTextureFormat.reg,
				Lua_UnityEngine_VRTextureUsage.reg,
				Lua_UnityEngine_RenderTextureCreationFlags.reg,
				Lua_UnityEngine_RenderTextureReadWrite.reg,
				Lua_UnityEngine_RenderTextureMemoryless.reg,
				Lua_UnityEngine_MaterialGlobalIlluminationFlags.reg,
				Lua_UnityEngine_CustomRenderTextureInitializationSource.reg,
				Lua_UnityEngine_CustomRenderTextureUpdateMode.reg,
				Lua_UnityEngine_CustomRenderTextureUpdateZoneSpace.reg,
				Lua_UnityEngine_LineTextureMode.reg,
				Lua_UnityEngine_LineAlignment.reg,
				Lua_UnityEngine_SkinnedMeshRenderer.reg,
				Lua_UnityEngine_MeshRenderer.reg,
				Lua_UnityEngine_LineUtility.reg,
				Lua_UnityEngine_LODFadeMode.reg,
				Lua_UnityEngine_LOD.reg,
				Lua_UnityEngine_LODGroup.reg,
				Lua_UnityEngine_BoneWeight.reg,
				Lua_UnityEngine_BoneWeight1.reg,
				Lua_UnityEngine_CombineInstance.reg,
				Lua_UnityEngine_Texture.reg,
				Lua_UnityEngine_Texture2D.reg,
				Lua_UnityEngine_Texture2D_EXRFlags.reg,
				Lua_UnityEngine_Cubemap.reg,
				Lua_UnityEngine_Texture3D.reg,
				Lua_UnityEngine_Texture2DArray.reg,
				Lua_UnityEngine_CubemapArray.reg,
				Lua_UnityEngine_SparseTexture.reg,
				Lua_UnityEngine_RenderTexture.reg,
				Lua_UnityEngine_CustomRenderTextureUpdateZone.reg,
				Lua_UnityEngine_CustomRenderTexture.reg,
				Lua_UnityEngine_RenderTextureDescriptor.reg,
				Lua_UnityEngine_Hash128.reg,
				Lua_UnityEngine_HashUtilities.reg,
				Lua_UnityEngine_HashUnsafeUtilities.reg,
				Lua_UnityEngine_KeyCode.reg,
				Lua_UnityEngine_ILogger.reg,
				Lua_UnityEngine_ILogHandler.reg,
				Lua_UnityEngine_Logger.reg,
				Lua_UnityEngine_Color.reg,
				Lua_UnityEngine_Color32.reg,
				Lua_UnityEngine_ColorUtility.reg,
				Lua_UnityEngine_GradientColorKey.reg,
				Lua_UnityEngine_GradientAlphaKey.reg,
				Lua_UnityEngine_GradientMode.reg,
				Lua_UnityEngine_Gradient.reg,
				Lua_UnityEngine_FrustumPlanes.reg,
				Lua_UnityEngine_Matrix4x4.reg,
				Lua_UnityEngine_Vector3.reg,
				Lua_UnityEngine_Quaternion.reg,
				Lua_UnityEngine_Mathf.reg,
				Lua_UnityEngine_Vector2.reg,
				Lua_UnityEngine_Vector2Int.reg,
				Lua_UnityEngine_Vector3Int.reg,
				Lua_UnityEngine_Vector4.reg,
				Lua_UnityEngine_PlayerPrefsException.reg,
				Lua_UnityEngine_PlayerPrefs.reg,
				Lua_UnityEngine_PropertyName.reg,
				Lua_UnityEngine_Random.reg,
				Lua_UnityEngine_Random_State.reg,
				Lua_UnityEngine_AsyncOperation.reg,
				Lua_UnityEngine_ResourceRequest.reg,
				Lua_UnityEngine_Resources.reg,
				Lua_UnityEngine_ExecuteAlways.reg,
				Lua_UnityEngine_DefaultExecutionOrder.reg,
				Lua_UnityEngine_CustomYieldInstruction.reg,
				Lua_UnityEngine_GameObject.reg,
				Lua_UnityEngine_LayerMask.reg,
				Lua_UnityEngine_MonoBehaviour.reg,
				Lua_UnityEngine_RangeInt.reg,
				Lua_UnityEngine_RuntimeInitializeLoadType.reg,
				Lua_UnityEngine_ScriptableObject.reg,
				Lua_UnityEngine_StackTraceUtility.reg,
				Lua_UnityEngine_UnityException.reg,
				Lua_UnityEngine_MissingComponentException.reg,
				Lua_UnityEngine_UnassignedReferenceException.reg,
				Lua_UnityEngine_MissingReferenceException.reg,
				Lua_UnityEngine_TextAsset.reg,
				Lua_UnityEngine_HideFlags.reg,
				Lua_UnityEngine_WaitForEndOfFrame.reg,
				Lua_UnityEngine_WaitForFixedUpdate.reg,
				Lua_UnityEngine_WaitForSeconds.reg,
				Lua_UnityEngine_WaitForSecondsRealtime.reg,
				Lua_UnityEngine_WaitUntil.reg,
				Lua_UnityEngine_WaitWhile.reg,
				Lua_UnityEngine_Security.reg,
				Lua_UnityEngine_SerializeReference.reg,
				Lua_UnityEngine_PreferBinarySerialization.reg,
				Lua_UnityEngine_ISerializationCallbackReceiver.reg,
				Lua_UnityEngine_ComputeBuffer.reg,
				Lua_UnityEngine_ComputeShader.reg,
				Lua_UnityEngine_SnapAxis.reg,
				Lua_UnityEngine_Snapping.reg,
				Lua_UnityEngine_StaticBatchingUtility.reg,
				Lua_UnityEngine_BatteryStatus.reg,
				Lua_UnityEngine_OperatingSystemFamily.reg,
				Lua_UnityEngine_DeviceType.reg,
				Lua_UnityEngine_SystemInfo.reg,
				Lua_UnityEngine_Time.reg,
				Lua_UnityEngine_UnityEventQueueSystem.reg,
				Lua_UnityEngine_Pose.reg,
				Lua_UnityEngine_DrivenTransformProperties.reg,
				Lua_UnityEngine_Transform.reg,
				Lua_UnityEngine_RectTransform.reg,
				Lua_UnityEngine_RectTransform_Edge.reg,
				Lua_UnityEngine_RectTransform_Axis.reg,
				Lua_UnityEngine_SpriteDrawMode.reg,
				Lua_UnityEngine_SpriteTileMode.reg,
				Lua_UnityEngine_SpriteMaskInteraction.reg,
				Lua_UnityEngine_SpriteRenderer.reg,
				Lua_UnityEngine_SpriteMeshType.reg,
				Lua_UnityEngine_SpriteAlignment.reg,
				Lua_UnityEngine_SpritePackingMode.reg,
				Lua_UnityEngine_SpritePackingRotation.reg,
				Lua_UnityEngine_SpriteSortPoint.reg,
				Lua_UnityEngine_SecondarySpriteTexture.reg,
				Lua_UnityEngine_Sprite.reg,
				Lua_UnityEngine_Sprites_DataUtility.reg,
				Lua_UnityEngine_U2D_PixelPerfectRendering.reg,
				Lua_UnityEngine_U2D_SpriteBone.reg,
				Lua_UnityEngine_U2D_SpriteDataAccessExtensions.reg,
				Lua_UnityEngine_U2D_SpriteRendererDataAccessExtensions.reg,
				Lua_UnityEngine_U2D_SpriteAtlasManager.reg,
				Lua_UnityEngine_U2D_SpriteAtlas.reg,
				Lua_UnityEngine_Profiling_ProfilerArea.reg,
				Lua_UnityEngine_Profiling_Profiler.reg,
				Lua_UnityEngine_Profiling_Recorder.reg,
				Lua_UnityEngine_Profiling_Sampler.reg,
				Lua_UnityEngine_Profiling_CustomSampler.reg,
				Lua_UnityEngine_Profiling_Experimental_DebugScreenCapture.reg,
				Lua_UnityEngine_Profiling_Memory_Experimental_CaptureFlags.reg,
				Lua_UnityEngine_Profiling_Memory_Experimental_MetaData.reg,
				Lua_UnityEngine_Profiling_Memory_Experimental_MemoryProfiler.reg,
				Lua_UnityEngine_Jobs_IJobParallelForTransform.reg,
				Lua_UnityEngine_Jobs_IJobParallelForTransformExtensions.reg,
				Lua_UnityEngine_Jobs_TransformAccess.reg,
				Lua_UnityEngine_Jobs_TransformAccessArray.reg,
				Lua_UnityEngine_Events_PersistentListenerMode.reg,
				Lua_UnityEngine_Events_UnityEventCallState.reg,
				Lua_UnityEngine_Events_UnityEventBase.reg,
				Lua_UnityEngine_Events_UnityEvent.reg,
				Lua_UnityEngine_Scripting_GarbageCollector.reg,
				Lua_UnityEngine_Scripting_GarbageCollector_Mode.reg,
				Lua_UnityEngine_SceneManagement_Scene.reg,
				Lua_UnityEngine_SceneManagement_SceneManager.reg,
				Lua_UnityEngine_SceneManagement_LoadSceneMode.reg,
				Lua_UnityEngine_SceneManagement_LocalPhysicsMode.reg,
				Lua_UnityEngine_SceneManagement_LoadSceneParameters.reg,
				Lua_UnityEngine_SceneManagement_CreateSceneParameters.reg,
				Lua_UnityEngine_SceneManagement_UnloadSceneOptions.reg,
				Lua_UnityEngine_SceneManagement_SceneUtility.reg,
				Lua_UnityEngine_LowLevel_PlayerLoopSystem.reg,
				Lua_UnityEngine_LowLevel_PlayerLoop.reg,
				Lua_UnityEngine_PlayerLoop_Initialization.reg,
				Lua_UnityEngine_PlayerLoop_Initialization_PlayerUpdateTime.reg,
				Lua_UnityEngine_PlayerLoop_Initialization_DirectorSampleTime.reg,
				Lua_UnityEngine_PlayerLoop_Initialization_AsyncUploadTimeSlicedUpdate.reg,
				Lua_UnityEngine_PlayerLoop_Initialization_SynchronizeState.reg,
				Lua_UnityEngine_PlayerLoop_Initialization_SynchronizeInputs.reg,
				Lua_UnityEngine_PlayerLoop_Initialization_XREarlyUpdate.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_PollPlayerConnection.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_ProfilerStartFrame.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_PollHtcsPlayerConnection.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_GpuTimestamp.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_AnalyticsCoreStatsUpdate.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_UnityWebRequestUpdate.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_UpdateStreamingManager.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_ExecuteMainThreadJobs.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_ProcessMouseInWindow.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_ClearIntermediateRenderers.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_ClearLines.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_PresentBeforeUpdate.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_ResetFrameStatsAfterPresent.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_UpdateAsyncReadbackManager.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_UpdateTextureStreamingManager.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_UpdatePreloading.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_RendererNotifyInvisible.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_PlayerCleanupCachedData.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_UpdateMainGameViewRect.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_UpdateCanvasRectTransform.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_UpdateInputManager.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_ProcessRemoteInput.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_XRUpdate.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_ScriptRunDelayedStartupFrame.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_UpdateKinect.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_DeliverIosPlatformEvents.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_DispatchEventQueueEvents.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_PhysicsResetInterpolatedTransformPosition.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_SpriteAtlasManagerUpdate.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_TangoUpdate.reg,
				Lua_UnityEngine_PlayerLoop_EarlyUpdate_PerformanceAnalyticsUpdate.reg,
				Lua_UnityEngine_PlayerLoop_FixedUpdate.reg,
				Lua_UnityEngine_PlayerLoop_FixedUpdate_ClearLines.reg,
				Lua_UnityEngine_PlayerLoop_FixedUpdate_DirectorFixedSampleTime.reg,
				Lua_UnityEngine_PlayerLoop_FixedUpdate_AudioFixedUpdate.reg,
				Lua_UnityEngine_PlayerLoop_FixedUpdate_ScriptRunBehaviourFixedUpdate.reg,
				Lua_UnityEngine_PlayerLoop_FixedUpdate_DirectorFixedUpdate.reg,
				Lua_UnityEngine_PlayerLoop_FixedUpdate_LegacyFixedAnimationUpdate.reg,
				Lua_UnityEngine_PlayerLoop_FixedUpdate_XRFixedUpdate.reg,
				Lua_UnityEngine_PlayerLoop_FixedUpdate_PhysicsFixedUpdate.reg,
				Lua_UnityEngine_PlayerLoop_FixedUpdate_Physics2DFixedUpdate.reg,
				Lua_UnityEngine_PlayerLoop_FixedUpdate_DirectorFixedUpdatePostPhysics.reg,
				Lua_UnityEngine_PlayerLoop_FixedUpdate_ScriptRunDelayedFixedFrameRate.reg,
				Lua_UnityEngine_PlayerLoop_FixedUpdate_NewInputFixedUpdate.reg,
				Lua_UnityEngine_PlayerLoop_PreUpdate.reg,
				Lua_UnityEngine_PlayerLoop_PreUpdate_PhysicsUpdate.reg,
				Lua_UnityEngine_PlayerLoop_PreUpdate_Physics2DUpdate.reg,
				Lua_UnityEngine_PlayerLoop_PreUpdate_CheckTexFieldInput.reg,
				Lua_UnityEngine_PlayerLoop_PreUpdate_AIUpdate.reg,
				Lua_UnityEngine_PlayerLoop_PreUpdate_WindUpdate.reg,
				Lua_UnityEngine_PlayerLoop_PreUpdate_UpdateVideo.reg,
				Lua_UnityEngine_PlayerLoop_PreUpdate_NewInputUpdate.reg,
				Lua_UnityEngine_PlayerLoop_Update.reg,
				Lua_UnityEngine_PlayerLoop_Update_ScriptRunBehaviourUpdate.reg,
				Lua_UnityEngine_PlayerLoop_Update_DirectorUpdate.reg,
				Lua_UnityEngine_PlayerLoop_Update_ScriptRunDelayedDynamicFrameRate.reg,
				Lua_UnityEngine_PlayerLoop_Update_ScriptRunDelayedTasks.reg,
				Lua_UnityEngine_PlayerLoop_PreLateUpdate.reg,
				Lua_UnityEngine_PlayerLoop_PreLateUpdate_AIUpdatePostScript.reg,
				Lua_UnityEngine_PlayerLoop_PreLateUpdate_DirectorUpdateAnimationBegin.reg,
				Lua_UnityEngine_PlayerLoop_PreLateUpdate_LegacyAnimationUpdate.reg,
				Lua_UnityEngine_PlayerLoop_PreLateUpdate_DirectorUpdateAnimationEnd.reg,
				Lua_UnityEngine_PlayerLoop_PreLateUpdate_DirectorDeferredEvaluate.reg,
				Lua_UnityEngine_PlayerLoop_PreLateUpdate_UNetUpdate.reg,
				Lua_UnityEngine_PlayerLoop_PreLateUpdate_EndGraphicsJobsAfterScriptUpdate.reg,
				Lua_UnityEngine_PlayerLoop_PreLateUpdate_ParticleSystemBeginUpdateAll.reg,
				Lua_UnityEngine_PlayerLoop_PreLateUpdate_ScriptRunBehaviourLateUpdate.reg,
				Lua_UnityEngine_PlayerLoop_PreLateUpdate_ConstraintManagerUpdate.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_PlayerSendFrameStarted.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_UpdateRectTransform.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_UpdateCanvasRectTransform.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_PlayerUpdateCanvases.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_UpdateAudio.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_UpdateVideo.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_DirectorLateUpdate.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_ScriptRunDelayedDynamicFrameRate.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_VFXUpdate.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_ParticleSystemEndUpdateAll.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_EndGraphicsJobsAfterScriptLateUpdate.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_UpdateSubstance.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_UpdateCustomRenderTextures.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_UpdateAllRenderers.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_EnlightenRuntimeUpdate.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_UpdateAllSkinnedMeshes.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_ProcessWebSendMessages.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_SortingGroupsUpdate.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_UpdateVideoTextures.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_DirectorRenderImage.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_PlayerEmitCanvasGeometry.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_FinishFrameRendering.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_BatchModeUpdate.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_PlayerSendFrameComplete.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_UpdateCaptureScreenshot.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_PresentAfterDraw.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_ClearImmediateRenderers.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_XRPostPresent.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_UpdateResolution.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_InputEndFrame.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_ShaderHandleErrors.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_ResetInputAxis.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_ThreadedLoadingDebug.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_ProfilerSynchronizeStats.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_MemoryFrameMaintenance.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_ExecuteGameCenterCallbacks.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_ProfilerEndFrame.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_PlayerSendFramePostPresent.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_PhysicsSkinnedClothBeginUpdate.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_PhysicsSkinnedClothFinishUpdate.reg,
				Lua_UnityEngine_PlayerLoop_PostLateUpdate_TriggerEndOfFrameCallbacks.reg,
				Lua_UnityEngine_Diagnostics_ForcedCrashCategory.reg,
				Lua_UnityEngine_Diagnostics_Utils.reg,
				Lua_UnityEngine_Diagnostics_PlayerConnection.reg,
				Lua_UnityEngine_Assertions_Assert.reg,
				Lua_UnityEngine_Assertions_AssertionException.reg,
				Lua_UnityEngine_Assertions_Comparers_FloatComparer.reg,
				Lua_UnityEngine_TestTools_CoveredSequencePoint.reg,
				Lua_UnityEngine_TestTools_CoveredMethodStats.reg,
				Lua_UnityEngine_TestTools_Coverage.reg,
				Lua_UnityEngine_ICanvasRaycastFilter.reg,
				Lua_UnityEngine_CanvasGroup.reg,
				Lua_UnityEngine_CanvasRenderer.reg,
				Lua_UnityEngine_RectTransformUtility.reg,
				Lua_UnityEngine_RenderMode.reg,
				Lua_UnityEngine_AdditionalCanvasShaderChannels.reg,
				Lua_UnityEngine_Canvas.reg,
				Lua_UnityEngine_UISystemProfilerApi.reg,
				Lua_UnityEngine_UISystemProfilerApi_SampleType.reg,
				Lua_UnityEngine_TextGenerationSettings.reg,
				Lua_UnityEngine_TextAnchor.reg,
				Lua_UnityEngine_HorizontalWrapMode.reg,
				Lua_UnityEngine_VerticalWrapMode.reg,
				Lua_UnityEngine_TextMesh.reg,
				Lua_UnityEngine_CharacterInfo.reg,
				Lua_UnityEngine_UICharInfo.reg,
				Lua_UnityEngine_UILineInfo.reg,
				Lua_UnityEngine_UIVertex.reg,
				Lua_UnityEngine_Font.reg,
				Lua_UnityEngine_PhysicsScene2D.reg,
				Lua_UnityEngine_PhysicsSceneExtensions2D.reg,
				Lua_UnityEngine_Physics2D.reg,
				Lua_UnityEngine_CapsuleDirection2D.reg,
				Lua_UnityEngine_RigidbodyConstraints2D.reg,
				Lua_UnityEngine_RigidbodyInterpolation2D.reg,
				Lua_UnityEngine_RigidbodySleepMode2D.reg,
				Lua_UnityEngine_CollisionDetectionMode2D.reg,
				Lua_UnityEngine_RigidbodyType2D.reg,
				Lua_UnityEngine_ForceMode2D.reg,
				Lua_UnityEngine_JointLimitState2D.reg,
				Lua_UnityEngine_EffectorSelection2D.reg,
				Lua_UnityEngine_EffectorForceMode2D.reg,
				Lua_UnityEngine_ColliderDistance2D.reg,
				Lua_UnityEngine_ContactFilter2D.reg,
				Lua_UnityEngine_Collision2D.reg,
				Lua_UnityEngine_ContactPoint2D.reg,
				Lua_UnityEngine_JointAngleLimits2D.reg,
				Lua_UnityEngine_JointTranslationLimits2D.reg,
				Lua_UnityEngine_JointMotor2D.reg,
				Lua_UnityEngine_JointSuspension2D.reg,
				Lua_UnityEngine_RaycastHit2D.reg,
				Lua_UnityEngine_PhysicsJobOptions2D.reg,
				Lua_UnityEngine_Rigidbody2D.reg,
				Lua_UnityEngine_Collider2D.reg,
				Lua_UnityEngine_CircleCollider2D.reg,
				Lua_UnityEngine_CapsuleCollider2D.reg,
				Lua_UnityEngine_EdgeCollider2D.reg,
				Lua_UnityEngine_BoxCollider2D.reg,
				Lua_UnityEngine_PolygonCollider2D.reg,
				Lua_UnityEngine_CompositeCollider2D.reg,
				Lua_UnityEngine_CompositeCollider2D_GeometryType.reg,
				Lua_UnityEngine_CompositeCollider2D_GenerationType.reg,
				Lua_UnityEngine_Joint2D.reg,
				Lua_UnityEngine_AnchoredJoint2D.reg,
				Lua_UnityEngine_SpringJoint2D.reg,
				Lua_UnityEngine_DistanceJoint2D.reg,
				Lua_UnityEngine_FrictionJoint2D.reg,
				Lua_UnityEngine_HingeJoint2D.reg,
				Lua_UnityEngine_RelativeJoint2D.reg,
				Lua_UnityEngine_SliderJoint2D.reg,
				Lua_UnityEngine_TargetJoint2D.reg,
				Lua_UnityEngine_FixedJoint2D.reg,
				Lua_UnityEngine_WheelJoint2D.reg,
				Lua_UnityEngine_Effector2D.reg,
				Lua_UnityEngine_AreaEffector2D.reg,
				Lua_UnityEngine_BuoyancyEffector2D.reg,
				Lua_UnityEngine_PointEffector2D.reg,
				Lua_UnityEngine_PlatformEffector2D.reg,
				Lua_UnityEngine_SurfaceEffector2D.reg,
				Lua_UnityEngine_PhysicsUpdateBehaviour2D.reg,
				Lua_UnityEngine_ConstantForce2D.reg,
				Lua_UnityEngine_PhysicsMaterial2D.reg,
				Lua_UnityEngine_Animator.reg,
				Lua_UnityEngine_IAnimationClipSource.reg,
				Lua_UnityEngine_StateMachineBehaviour.reg,
				Lua_UnityEngine_AnimationClip.reg,
				Lua_UnityEngine_AvatarTarget.reg,
				Lua_UnityEngine_AvatarIKGoal.reg,
				Lua_UnityEngine_AvatarIKHint.reg,
				Lua_UnityEngine_AnimatorControllerParameterType.reg,
				Lua_UnityEngine_AnimatorRecorderMode.reg,
				Lua_UnityEngine_DurationUnit.reg,
				Lua_UnityEngine_AnimatorCullingMode.reg,
				Lua_UnityEngine_AnimatorUpdateMode.reg,
				Lua_UnityEngine_AnimatorClipInfo.reg,
				Lua_UnityEngine_AnimatorStateInfo.reg,
				Lua_UnityEngine_AnimatorTransitionInfo.reg,
				Lua_UnityEngine_MatchTargetWeightMask.reg,
				Lua_UnityEngine_AnimatorControllerParameter.reg,
				Lua_UnityEngine_RuntimeAnimatorController.reg,
				Lua_UnityEngine_AnimatorOverrideController.reg,
				Lua_UnityEngine_AnimatorUtility.reg,
				Lua_UnityEngine_BodyDof.reg,
				Lua_UnityEngine_HeadDof.reg,
				Lua_UnityEngine_LegDof.reg,
				Lua_UnityEngine_ArmDof.reg,
				Lua_UnityEngine_FingerDof.reg,
				Lua_UnityEngine_HumanPartDof.reg,
				Lua_UnityEngine_HumanBodyBones.reg,
				Lua_UnityEngine_Avatar.reg,
				Lua_UnityEngine_SkeletonBone.reg,
				Lua_UnityEngine_HumanLimit.reg,
				Lua_UnityEngine_HumanBone.reg,
				Lua_UnityEngine_HumanDescription.reg,
				Lua_UnityEngine_AvatarBuilder.reg,
				Lua_UnityEngine_AvatarMaskBodyPart.reg,
				Lua_UnityEngine_AvatarMask.reg,
				Lua_UnityEngine_HumanPose.reg,
				Lua_UnityEngine_HumanPoseHandler.reg,
				Lua_UnityEngine_HumanTrait.reg,
				Lua_UnityEngine_AnimationEvent.reg,
				Lua_UnityEngine_PlayMode.reg,
				Lua_UnityEngine_QueueMode.reg,
				Lua_UnityEngine_AnimationBlendMode.reg,
				Lua_UnityEngine_AnimationPlayMode.reg,
				Lua_UnityEngine_AnimationCullingType.reg,
				Lua_UnityEngine_Animation.reg,
				Lua_UnityEngine_AnimationState.reg,
				Lua_UnityEngine_Animations_AnimationPlayableBinding.reg,
				Lua_UnityEngine_Animations_IAnimationJob.reg,
				Lua_UnityEngine_Animations_IAnimationJobPlayable.reg,
				Lua_UnityEngine_Animations_AimConstraint.reg,
				Lua_UnityEngine_Animations_AimConstraint_WorldUpType.reg,
				Lua_UnityEngine_Animations_AnimationClipPlayable.reg,
				Lua_UnityEngine_Animations_AnimationHumanStream.reg,
				Lua_UnityEngine_Animations_AnimationLayerMixerPlayable.reg,
				Lua_UnityEngine_Animations_AnimationMixerPlayable.reg,
				Lua_UnityEngine_Animations_AnimationPlayableExtensions.reg,
				Lua_UnityEngine_Animations_AnimationPlayableOutput.reg,
				Lua_UnityEngine_Animations_AnimationScriptPlayable.reg,
				Lua_UnityEngine_Animations_AnimationStream.reg,
				Lua_UnityEngine_Animations_TransformStreamHandle.reg,
				Lua_UnityEngine_Animations_PropertyStreamHandle.reg,
				Lua_UnityEngine_Animations_TransformSceneHandle.reg,
				Lua_UnityEngine_Animations_PropertySceneHandle.reg,
				Lua_UnityEngine_Animations_AnimationSceneHandleUtility.reg,
				Lua_UnityEngine_Animations_AnimationStreamHandleUtility.reg,
				Lua_UnityEngine_Animations_AnimatorControllerPlayable.reg,
				Lua_UnityEngine_Animations_CustomStreamPropertyType.reg,
				Lua_UnityEngine_Animations_AnimatorJobExtensions.reg,
				Lua_UnityEngine_Animations_Axis.reg,
				Lua_UnityEngine_Animations_ConstraintSource.reg,
				Lua_UnityEngine_Animations_IConstraint.reg,
				Lua_UnityEngine_Animations_PositionConstraint.reg,
				Lua_UnityEngine_Animations_RotationConstraint.reg,
				Lua_UnityEngine_Animations_ScaleConstraint.reg,
				Lua_UnityEngine_Animations_LookAtConstraint.reg,
				Lua_UnityEngine_Animations_MuscleHandle.reg,
				Lua_UnityEngine_Animations_ParentConstraint.reg,
				Lua_UnityEngine_Event.reg,
				Lua_UnityEngine_PointerType.reg,
				Lua_UnityEngine_ScaleMode.reg,
				Lua_UnityEngine_FocusType.reg,
				Lua_UnityEngine_ImagePosition.reg,
				Lua_UnityEngine_WWWForm.reg,
				Lua_UnityEngine_RigidbodyConstraints.reg,
				Lua_UnityEngine_ForceMode.reg,
				Lua_UnityEngine_JointProjectionMode.reg,
				Lua_UnityEngine_MeshColliderCookingOptions.reg,
				Lua_UnityEngine_WheelFrictionCurve.reg,
				Lua_UnityEngine_SoftJointLimit.reg,
				Lua_UnityEngine_SoftJointLimitSpring.reg,
				Lua_UnityEngine_JointDrive.reg,
				Lua_UnityEngine_RigidbodyInterpolation.reg,
				Lua_UnityEngine_JointMotor.reg,
				Lua_UnityEngine_JointSpring.reg,
				Lua_UnityEngine_JointLimits.reg,
				Lua_UnityEngine_ControllerColliderHit.reg,
				Lua_UnityEngine_PhysicMaterialCombine.reg,
				Lua_UnityEngine_Collision.reg,
				Lua_UnityEngine_CollisionFlags.reg,
				Lua_UnityEngine_QueryTriggerInteraction.reg,
				Lua_UnityEngine_CollisionDetectionMode.reg,
				Lua_UnityEngine_RotationDriveMode.reg,
				Lua_UnityEngine_PhysicMaterial.reg,
				Lua_UnityEngine_RaycastHit.reg,
				Lua_UnityEngine_Rigidbody.reg,
				Lua_UnityEngine_Collider.reg,
				Lua_UnityEngine_CharacterController.reg,
				Lua_UnityEngine_MeshCollider.reg,
				Lua_UnityEngine_CapsuleCollider.reg,
				Lua_UnityEngine_BoxCollider.reg,
				Lua_UnityEngine_SphereCollider.reg,
				Lua_UnityEngine_ConstantForce.reg,
				Lua_UnityEngine_Joint.reg,
				Lua_UnityEngine_HingeJoint.reg,
				Lua_UnityEngine_SpringJoint.reg,
				Lua_UnityEngine_FixedJoint.reg,
				Lua_UnityEngine_CharacterJoint.reg,
				Lua_UnityEngine_ConfigurableJoint.reg,
				Lua_UnityEngine_ContactPoint.reg,
				Lua_UnityEngine_PhysicsScene.reg,
				Lua_UnityEngine_PhysicsSceneExtensions.reg,
				Lua_UnityEngine_Physics.reg,
				Lua_UnityEngine_RaycastCommand.reg,
				Lua_UnityEngine_SpherecastCommand.reg,
				Lua_UnityEngine_CapsulecastCommand.reg,
				Lua_UnityEngine_BoxcastCommand.reg,
				Lua_UnityEngine_UI_AnimationTriggers.reg,
				Lua_UnityEngine_EventSystems_UIBehaviour.reg,
				Lua_UnityEngine_UI_Selectable.reg,
				Lua_UnityEngine_UI_Button.reg,
				Lua_UnityEngine_UI_CanvasUpdate.reg,
				Lua_UnityEngine_UI_ICanvasElement.reg,
				Lua_UnityEngine_UI_CanvasUpdateRegistry.reg,
				Lua_UnityEngine_UI_ColorBlock.reg,
				Lua_UnityEngine_UI_ClipperRegistry.reg,
				Lua_UnityEngine_UI_Clipping.reg,
				Lua_UnityEngine_UI_IClipper.reg,
				Lua_UnityEngine_UI_IClippable.reg,
				Lua_UnityEngine_UI_DefaultControls.reg,
				Lua_UnityEngine_UI_Dropdown.reg,
				Lua_UnityEngine_UI_FontData.reg,
				Lua_UnityEngine_UI_FontUpdateTracker.reg,
				Lua_UnityEngine_UI_Graphic.reg,
				Lua_UnityEngine_EventSystems_BaseRaycaster.reg,
				Lua_UnityEngine_UI_GraphicRaycaster.reg,
				Lua_UnityEngine_UI_GraphicRegistry.reg,
				Lua_UnityEngine_UI_IMaskable.reg,
				Lua_UnityEngine_UI_MaskableGraphic.reg,
				Lua_UnityEngine_UI_Image.reg,
				Lua_UnityEngine_UI_InputField.reg,
				Lua_UnityEngine_UI_AspectRatioFitter.reg,
				Lua_UnityEngine_UI_CanvasScaler.reg,
				Lua_UnityEngine_UI_ContentSizeFitter.reg,
				Lua_UnityEngine_UI_LayoutGroup.reg,
				Lua_UnityEngine_UI_GridLayoutGroup.reg,
				Lua_UnityEngine_UI_HorizontalOrVerticalLayoutGroup.reg,
				Lua_UnityEngine_UI_HorizontalLayoutGroup.reg,
				Lua_UnityEngine_UI_ILayoutElement.reg,
				Lua_UnityEngine_UI_ILayoutController.reg,
				Lua_UnityEngine_UI_ILayoutGroup.reg,
				Lua_UnityEngine_UI_ILayoutSelfController.reg,
				Lua_UnityEngine_UI_ILayoutIgnorer.reg,
				Lua_UnityEngine_UI_LayoutElement.reg,
				Lua_UnityEngine_UI_LayoutRebuilder.reg,
				Lua_UnityEngine_UI_LayoutUtility.reg,
				Lua_UnityEngine_UI_VerticalLayoutGroup.reg,
				Lua_UnityEngine_UI_Mask.reg,
				Lua_UnityEngine_UI_MaskUtilities.reg,
				Lua_UnityEngine_UI_IMaterialModifier.reg,
				Lua_UnityEngine_UI_Navigation.reg,
				Lua_UnityEngine_UI_RawImage.reg,
				Lua_UnityEngine_UI_RectMask2D.reg,
				Lua_UnityEngine_UI_ScrollRect.reg,
				Lua_UnityEngine_UI_Scrollbar.reg,
				Lua_UnityEngine_UI_Slider.reg,
				Lua_UnityEngine_UI_SpriteState.reg,
				Lua_UnityEngine_UI_StencilMaterial.reg,
				Lua_UnityEngine_UI_Text.reg,
				Lua_UnityEngine_UI_Toggle.reg,
				Lua_UnityEngine_UI_ToggleGroup.reg,
				Lua_UnityEngine_UI_VertexHelper.reg,
				Lua_UnityEngine_UI_BaseMeshEffect.reg,
				Lua_UnityEngine_UI_IMeshModifier.reg,
				Lua_UnityEngine_UI_Shadow.reg,
				Lua_UnityEngine_UI_Outline.reg,
				Lua_UnityEngine_UI_PositionAsUV1.reg,
				Lua_UnityEngine_EventSystems_AbstractEventData.reg,
				Lua_UnityEngine_EventSystems_BaseEventData.reg,
				Lua_UnityEngine_EventSystems_AxisEventData.reg,
				Lua_UnityEngine_EventSystems_PointerEventData.reg,
				Lua_UnityEngine_EventSystems_EventHandle.reg,
				Lua_UnityEngine_EventSystems_IEventSystemHandler.reg,
				Lua_UnityEngine_EventSystems_IPointerEnterHandler.reg,
				Lua_UnityEngine_EventSystems_IPointerExitHandler.reg,
				Lua_UnityEngine_EventSystems_IPointerDownHandler.reg,
				Lua_UnityEngine_EventSystems_IPointerUpHandler.reg,
				Lua_UnityEngine_EventSystems_IPointerClickHandler.reg,
				Lua_UnityEngine_EventSystems_IBeginDragHandler.reg,
				Lua_UnityEngine_EventSystems_IInitializePotentialDragHandler.reg,
				Lua_UnityEngine_EventSystems_IDragHandler.reg,
				Lua_UnityEngine_EventSystems_IEndDragHandler.reg,
				Lua_UnityEngine_EventSystems_IDropHandler.reg,
				Lua_UnityEngine_EventSystems_IScrollHandler.reg,
				Lua_UnityEngine_EventSystems_IUpdateSelectedHandler.reg,
				Lua_UnityEngine_EventSystems_ISelectHandler.reg,
				Lua_UnityEngine_EventSystems_IDeselectHandler.reg,
				Lua_UnityEngine_EventSystems_IMoveHandler.reg,
				Lua_UnityEngine_EventSystems_ISubmitHandler.reg,
				Lua_UnityEngine_EventSystems_ICancelHandler.reg,
				Lua_UnityEngine_EventSystems_EventSystem.reg,
				Lua_UnityEngine_EventSystems_EventTrigger.reg,
				Lua_UnityEngine_EventSystems_EventTriggerType.reg,
				Lua_UnityEngine_EventSystems_ExecuteEvents.reg,
				Lua_UnityEngine_EventSystems_BaseInput.reg,
				Lua_UnityEngine_EventSystems_BaseInputModule.reg,
				Lua_UnityEngine_EventSystems_PointerInputModule.reg,
				Lua_UnityEngine_EventSystems_StandaloneInputModule.reg,
				Lua_UnityEngine_EventSystems_MoveDirection.reg,
				Lua_UnityEngine_EventSystems_RaycastResult.reg,
				Lua_UnityEngine_EventSystems_PhysicsRaycaster.reg,
				Lua_UnityEngine_EventSystems_Physics2DRaycaster.reg,
				Lua_UnityEngine_UI_Button_ButtonClickedEvent.reg,
				Lua_UnityEngine_UI_DefaultControls_IFactoryControls.reg,
				Lua_UnityEngine_UI_DefaultControls_Resources.reg,
				Lua_UnityEngine_UI_Dropdown_OptionData.reg,
				Lua_UnityEngine_UI_Dropdown_OptionDataList.reg,
				Lua_UnityEngine_UI_Dropdown_DropdownEvent.reg,
				Lua_UnityEngine_UI_GraphicRaycaster_BlockingObjects.reg,
				Lua_UnityEngine_UI_Image_Type.reg,
				Lua_UnityEngine_UI_Image_FillMethod.reg,
				Lua_UnityEngine_UI_Image_OriginHorizontal.reg,
				Lua_UnityEngine_UI_Image_OriginVertical.reg,
				Lua_UnityEngine_UI_Image_Origin90.reg,
				Lua_UnityEngine_UI_Image_Origin180.reg,
				Lua_UnityEngine_UI_Image_Origin360.reg,
				Lua_UnityEngine_UI_InputField_ContentType.reg,
				Lua_UnityEngine_UI_InputField_InputType.reg,
				Lua_UnityEngine_UI_InputField_CharacterValidation.reg,
				Lua_UnityEngine_UI_InputField_LineType.reg,
				Lua_UnityEngine_UI_InputField_SubmitEvent.reg,
				Lua_UnityEngine_UI_InputField_OnChangeEvent.reg,
				Lua_UnityEngine_UI_AspectRatioFitter_AspectMode.reg,
				Lua_UnityEngine_UI_CanvasScaler_ScaleMode.reg,
				Lua_UnityEngine_UI_CanvasScaler_ScreenMatchMode.reg,
				Lua_UnityEngine_UI_CanvasScaler_Unit.reg,
				Lua_UnityEngine_UI_ContentSizeFitter_FitMode.reg,
				Lua_UnityEngine_UI_GridLayoutGroup_Corner.reg,
				Lua_UnityEngine_UI_GridLayoutGroup_Axis.reg,
				Lua_UnityEngine_UI_GridLayoutGroup_Constraint.reg,
				Lua_UnityEngine_UI_MaskableGraphic_CullStateChangedEvent.reg,
				Lua_UnityEngine_UI_Navigation_Mode.reg,
				Lua_UnityEngine_UI_ScrollRect_MovementType.reg,
				Lua_UnityEngine_UI_ScrollRect_ScrollbarVisibility.reg,
				Lua_UnityEngine_UI_ScrollRect_ScrollRectEvent.reg,
				Lua_UnityEngine_UI_Scrollbar_Direction.reg,
				Lua_UnityEngine_UI_Scrollbar_ScrollEvent.reg,
				Lua_UnityEngine_UI_Selectable_Transition.reg,
				Lua_UnityEngine_UI_Slider_Direction.reg,
				Lua_UnityEngine_UI_Slider_SliderEvent.reg,
				Lua_UnityEngine_UI_Toggle_ToggleTransition.reg,
				Lua_UnityEngine_UI_Toggle_ToggleEvent.reg,
				Lua_UnityEngine_EventSystems_PointerEventData_InputButton.reg,
				Lua_UnityEngine_EventSystems_PointerEventData_FramePressState.reg,
				Lua_UnityEngine_EventSystems_EventTrigger_TriggerEvent.reg,
				Lua_UnityEngine_EventSystems_EventTrigger_Entry.reg,
				Lua_UnityEngine_EventSystems_PointerInputModule_MouseButtonEventData.reg,
				Lua_UnityEngine_AudioBehaviour.reg,
				Lua_UnityEngine_AudioSource.reg,
				Lua_UnityEngine_AudioLowPassFilter.reg,
				Lua_UnityEngine_AudioHighPassFilter.reg,
				Lua_UnityEngine_AudioReverbFilter.reg,
				Lua_UnityEngine_AudioSpeakerMode.reg,
				Lua_UnityEngine_AudioDataLoadState.reg,
				Lua_UnityEngine_AudioConfiguration.reg,
				Lua_UnityEngine_AudioCompressionFormat.reg,
				Lua_UnityEngine_AudioClipLoadType.reg,
				Lua_UnityEngine_AudioVelocityUpdateMode.reg,
				Lua_UnityEngine_AudioRolloffMode.reg,
				Lua_UnityEngine_AudioSourceCurveType.reg,
				Lua_UnityEngine_AudioReverbPreset.reg,
				Lua_UnityEngine_AudioClip.reg,
				Lua_UnityEngine_AudioListener.reg,
				Lua_UnityEngine_AudioReverbZone.reg,
				Lua_UnityEngine_AudioDistortionFilter.reg,
				Lua_UnityEngine_AudioEchoFilter.reg,
				Lua_UnityEngine_AudioChorusFilter.reg,
				Lua_UnityEngine_Microphone.reg,
				Lua_UnityEngine_AudioRenderer.reg,
				Lua_UnityEngine_WebCamFlags.reg,
				Lua_UnityEngine_WebCamKind.reg,
				Lua_UnityEngine_WebCamDevice.reg,
				Lua_UnityEngine_WebCamTexture.reg,
				Lua_UnityEngine_Audio_AudioClipPlayable.reg,
				Lua_UnityEngine_Audio_AudioMixerUpdateMode.reg,
				Lua_UnityEngine_Audio_AudioMixer.reg,
				Lua_UnityEngine_Audio_AudioMixerGroup.reg,
				Lua_UnityEngine_Audio_AudioMixerPlayable.reg,
				Lua_UnityEngine_Audio_AudioMixerSnapshot.reg,
				Lua_UnityEngine_Audio_AudioPlayableBinding.reg,
				Lua_UnityEngine_Audio_AudioPlayableOutput.reg,
			};
			return list;
		}
	}
}
