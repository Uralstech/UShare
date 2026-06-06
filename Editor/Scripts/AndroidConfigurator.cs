// Copyright 2025 URAV ADVANCED LEARNING SYSTEMS PRIVATE LIMITED
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#if UNITY_ANDROID

using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

#nullable enable
namespace Uralstech.UShare.Editor
{
    /// <summary>
    /// Android project configurator for UShare.
    /// </summary>
    public class AndroidConfigurator : IPreprocessBuildWithReport
    {
        public const string DefaultFilePathsXmlPath = "Editor/Android/DefaultFileProviderPaths.xml";
        public const string PatchFilePathsXmlPath = "Runtime/Plugins/Android/com.uralstech.ushare.patch.androidlib/res/xml/file_provider_paths.xml";

        /// <inheritdoc/>
        public int callbackOrder { get; } = 0;

        /// <inheritdoc/>
        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("Configuring UShare FileProvider paths.");
            
            string? packagePath = PathUtils.GetPackagePath();
            if (string.IsNullOrEmpty(packagePath))
                return;

            string targetFilePathsXml = Path.Join(packagePath, PatchFilePathsXmlPath);
            if (!PathUtils.ValidateFilePath(targetFilePathsXml))
                return;

            UShareBuildSettings settings = UShareBuildSettings.Get();
            if (!settings.UseDefaultSettingsAndroid && !string.IsNullOrEmpty(settings.CustomFileProviderPathsAndroid))
            {
                File.WriteAllText(targetFilePathsXml, settings.CustomFileProviderPathsAndroid);
                return;
            }

            string defaultFilePathsXml = Path.Join(packagePath, DefaultFilePathsXmlPath);
            if (!PathUtils.ValidateFilePath(defaultFilePathsXml))
                return;

            File.Copy(defaultFilePathsXml, targetFilePathsXml, true);
            Debug.Log("Patch module updated successfully.");
        }
    }
}

#endif