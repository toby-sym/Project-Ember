using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace ProjectEmber.Editor
{
    public static class ProjectEmberBuildAutomation
    {
        private const string DefaultScene = "Assets/ProjectEmber/Scenes/Main.unity";

        public static void BuildWindows64()
        {
            Build(
                BuildTarget.StandaloneWindows64,
                Path.Combine("Builds", "StandaloneWindows64", "ProjectEmber.exe"));
        }

        public static void BuildCurrentTarget()
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var outputPath = Environment.GetEnvironmentVariable("PROJECT_EMBER_BUILD_PATH");
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                outputPath = GetDefaultOutputPath(buildTarget);
            }

            Build(buildTarget, outputPath);
        }

        private static void Build(BuildTarget buildTarget, string outputPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? "Builds");

            var report = BuildPipeline.BuildPlayer(
                new BuildPlayerOptions
                {
                    scenes = new[] { DefaultScene },
                    locationPathName = outputPath,
                    target = buildTarget,
                    options = BuildOptions.None
                });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Project Ember build failed: {report.summary.result} with {report.summary.totalErrors} errors.");
            }
        }

        private static string GetDefaultOutputPath(BuildTarget buildTarget)
        {
            return buildTarget switch
            {
                BuildTarget.StandaloneWindows64 => Path.Combine("Builds", "StandaloneWindows64", "ProjectEmber.exe"),
                BuildTarget.StandaloneLinux64 => Path.Combine("Builds", "StandaloneLinux64", "ProjectEmber.x86_64"),
                BuildTarget.StandaloneOSX => Path.Combine("Builds", "StandaloneOSX", "ProjectEmber.app"),
                _ => Path.Combine("Builds", buildTarget.ToString(), "ProjectEmber")
            };
        }
    }
}
