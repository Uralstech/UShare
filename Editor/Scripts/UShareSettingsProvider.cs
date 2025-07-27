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

using UnityEditor;
using UnityEngine;

#nullable enable
namespace Uralstech.UShare.Editor
{
    /// <summary>
    /// Project settings extension for UShare's project-wide settings.
    /// </summary>
    public static class UShareSettingsProvider
    {
        [SettingsProvider]
        private static SettingsProvider CreateSettingsProvider()
        {
            UShareBuildSettings settings = UShareBuildSettings.Get();
            return new SettingsProvider("Project/UShare", SettingsScope.Project, new string[] { "UShare", "Share", "FileProvider" })
            {
                label = "UShare Settings",
                guiHandler = _ =>
                {
                    EditorGUILayout.LabelField("Android Settings");

                    EditorGUI.BeginChangeCheck();
                    settings.UseDefaultSettingsAndroid = EditorGUILayout.Toggle(new GUIContent("Use default settings", "Use default UShare settings for Android?"), settings.UseDefaultSettingsAndroid);

                    EditorGUI.BeginDisabledGroup(settings.UseDefaultSettingsAndroid);

                    if (EditorGUILayout.LinkButton(new GUIContent("Custom FileProvider paths config", "Custom shareable directories for Android's FileProvider.")))
                        Application.OpenURL("https://developer.android.com/training/secure-file-sharing/setup-sharing#DefineMetaData");

                    settings.CustomFileProviderPathsAndroid = EditorGUILayout.TextArea(settings.CustomFileProviderPathsAndroid);
                    EditorGUI.EndDisabledGroup();

                    if (EditorGUI.EndChangeCheck())
                        UShareBuildSettings.Save(settings);
                }
            };
        }
    }
}
