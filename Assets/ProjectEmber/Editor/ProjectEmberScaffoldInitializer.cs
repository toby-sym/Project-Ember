using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectEmber.Editor
{
    [InitializeOnLoad]
    internal static class ProjectEmberScaffoldInitializer
    {
        private const string SessionKey = "ProjectEmber.ScaffoldInitializer.V2.Ran";
        private const string PipelineAssetPath = "Assets/ProjectEmber/Settings/ProjectEmberURP2D.asset";
        private const string RendererDataPath = "Assets/ProjectEmber/Settings/ProjectEmber2DRenderer.asset";
        private const string UrpPackagePath = "Packages/com.unity.render-pipelines.universal";

        static ProjectEmberScaffoldInitializer()
        {
            EditorApplication.delayCall += ConfigureProject;
        }

        private static void ConfigureProject()
        {
            if (SessionState.GetBool(SessionKey, false))
            {
                return;
            }

            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
            if (!EnsureUrp2DAssets())
            {
                EditorApplication.delayCall += ConfigureProject;
                return;
            }

            SessionState.SetBool(SessionKey, true);
            AssetDatabase.SaveAssets();
        }

        private static bool EnsureUrp2DAssets()
        {
            var rendererDataType = Type.GetType(
                "UnityEngine.Rendering.Universal.Renderer2DData, Unity.RenderPipelines.Universal.2D.Runtime");
            var pipelineAssetType = Type.GetType(
                "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, Unity.RenderPipelines.Universal.Runtime");

            if (rendererDataType == null || pipelineAssetType == null)
            {
                Debug.LogWarning("Project Ember scaffold is waiting for the Universal Render Pipeline package before creating URP 2D assets.");
                return false;
            }

            var rendererData = AssetDatabase.LoadAssetAtPath<ScriptableObject>(RendererDataPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance(rendererDataType);
                Apply2DRendererDefaults(rendererData);
                AssetDatabase.CreateAsset(rendererData, RendererDataPath);
                ReloadUrpResources(rendererData);
            }

            var pipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(PipelineAssetPath);
            if (pipelineAsset == null)
            {
                var createMethod = pipelineAssetType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
                var pipelineObject = createMethod != null
                    ? (ScriptableObject)createMethod.Invoke(null, new object[] { rendererData })
                    : ScriptableObject.CreateInstance(pipelineAssetType);
                AssetDatabase.CreateAsset(pipelineObject, PipelineAssetPath);
                pipelineAsset = (RenderPipelineAsset)pipelineObject;
            }

            AssignRendererData(pipelineAsset, rendererData);
            GraphicsSettings.defaultRenderPipeline = pipelineAsset;
            QualitySettings.renderPipeline = pipelineAsset;
            return true;
        }

        private static void Apply2DRendererDefaults(ScriptableObject rendererData)
        {
            var postProcessDataType = Type.GetType(
                "UnityEngine.Rendering.Universal.PostProcessData, Unity.RenderPipelines.Universal.Runtime");
            var getDefaultPostProcessData = postProcessDataType?.GetMethod(
                "GetDefaultPostProcessData",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var postProcessData = getDefaultPostProcessData?.Invoke(null, null);

            if (postProcessData == null)
            {
                return;
            }

            var serializedRenderer = new SerializedObject(rendererData);
            var postProcessProperty = serializedRenderer.FindProperty("m_PostProcessData")
                ?? serializedRenderer.FindProperty("postProcessData");
            if (postProcessProperty != null)
            {
                postProcessProperty.objectReferenceValue = (UnityEngine.Object)postProcessData;
                serializedRenderer.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void ReloadUrpResources(ScriptableObject asset)
        {
            var resourceReloaderType = Type.GetType(
                "UnityEngine.Rendering.ResourceReloader, Unity.RenderPipelines.Core.Runtime");
            var reloadMethod = resourceReloaderType?.GetMethod(
                "ReloadAllNullIn",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new[] { typeof(UnityEngine.Object), typeof(string) },
                null);

            reloadMethod?.Invoke(null, new object[] { asset, UrpPackagePath });
        }

        private static void AssignRendererData(RenderPipelineAsset pipelineAsset, ScriptableObject rendererData)
        {
            var serializedPipeline = new SerializedObject(pipelineAsset);
            var rendererDataList = serializedPipeline.FindProperty("m_RendererDataList");
            if (rendererDataList != null)
            {
                rendererDataList.arraySize = 1;
                rendererDataList.GetArrayElementAtIndex(0).objectReferenceValue = rendererData;
            }

            var defaultRendererIndex = serializedPipeline.FindProperty("m_DefaultRendererIndex");
            if (defaultRendererIndex != null)
            {
                defaultRendererIndex.intValue = 0;
            }

            serializedPipeline.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pipelineAsset);
        }
    }
}
