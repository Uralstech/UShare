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
                    EditorGUI.BeginChangeCheck();

                    #region Android
                    EditorGUILayout.LabelField("Android Settings");
                    settings.UseDefaultSettingsAndroid = EditorGUILayout.Toggle(new GUIContent("Use default settings", "Use default UShare settings for Android?"), settings.UseDefaultSettingsAndroid);

                    EditorGUI.BeginDisabledGroup(settings.UseDefaultSettingsAndroid);

                    if (EditorGUILayout.LinkButton(new GUIContent("Custom FileProvider paths config", "Custom shareable directories for Android's FileProvider.")))
                        Application.OpenURL("https://developer.android.com/training/secure-file-sharing/setup-sharing#DefineMetaData");

                    settings.CustomFileProviderPathsAndroid = EditorGUILayout.TextArea(settings.CustomFileProviderPathsAndroid);
                    EditorGUI.EndDisabledGroup();
                    #endregion

                    #region iOS
                    EditorGUILayout.LabelField("iOS Settings");
                    settings.PatchInfoPlistIOS = EditorGUILayout.Toggle(new GUIContent("Patch Info.plist for iOS", "Patch the exported XCode project's Info.plist file with additional permissions."), settings.PatchInfoPlistIOS);

                    EditorGUI.BeginDisabledGroup(!settings.PatchInfoPlistIOS);

                    settings.PhotoLibraryAdditionsUsageDescriptionIOS = EditorGUILayout.DelayedTextField(
                        new GUIContent("Photos Addition Usage", "A message that tells people why the app is requesting add-only access to their photo library. Grants permission for the share sheet to save to the user's gallery. Ignored if empty."),
                        settings.PhotoLibraryAdditionsUsageDescriptionIOS
                    );

                    EditorGUI.EndDisabledGroup();
                    #endregion

                    if (EditorGUI.EndChangeCheck())
                        UShareBuildSettings.Save(settings);
                }
            };
        }
    }
}
