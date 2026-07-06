using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectEmber.Editor
{
    [InitializeOnLoad]
    internal static class ProjectEmberScaffoldInitializer
    {
        private const string SessionKey = "ProjectEmber.ScaffoldInitializer.Ran";
        private const string PipelineAssetPath = "Assets/ProjectEmber/Settings/ProjectEmberURP2D.asset";
        private const string RendererDataPath = "Assets/ProjectEmber/Settings/ProjectEmber2DRenderer.asset";

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

            SessionState.SetBool(SessionKey, true);
            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
            EnsureUrp2DAssets();
            AssetDatabase.SaveAssets();
        }

        private static void EnsureUrp2DAssets()
        {
            var rendererDataType = Type.GetType(
                "UnityEngine.Rendering.Universal.Renderer2DData, Unity.RenderPipelines.Universal.Runtime");
            var pipelineAssetType = Type.GetType(
                "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, Unity.RenderPipelines.Universal.Runtime");

            if (rendererDataType == null || pipelineAssetType == null)
            {
                Debug.LogWarning("Project Ember scaffold is waiting for the Universal Render Pipeline package before creating URP 2D assets.");
                return;
            }

            var rendererData = AssetDatabase.LoadAssetAtPath<ScriptableObject>(RendererDataPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance(rendererDataType);
                AssetDatabase.CreateAsset(rendererData, RendererDataPath);
            }

            var pipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(PipelineAssetPath);
            if (pipelineAsset == null)
            {
                var pipelineObject = ScriptableObject.CreateInstance(pipelineAssetType);
                AssetDatabase.CreateAsset(pipelineObject, PipelineAssetPath);
                pipelineAsset = (RenderPipelineAsset)pipelineObject;
            }

            AssignRendererData(pipelineAsset, rendererData);
            GraphicsSettings.defaultRenderPipeline = pipelineAsset;
            QualitySettings.renderPipeline = pipelineAsset;
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
