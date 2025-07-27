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

using System;
using System.IO;
using UnityEngine;

#nullable enable
namespace Uralstech.UShare.Editor
{
    /// <summary>
    /// Project-wide settings for UShare.
    /// </summary>
    [Serializable]
    public class UShareBuildSettings
    {
        /// <summary>Save path for project-wide UShare settings.</summary>
        public const string SavePath = "ProjectSettings/UShareSettings.json";

        /// <summary>Use default UShare settings for Android?</summary>
        public bool UseDefaultSettingsAndroid = true;

        /// <summary>Custom shareable directories for Android's FileProvider. See <see href="https://developer.android.com/training/secure-file-sharing/setup-sharing#DefineMetaData">"Specify shareable directories"</see>.</summary>
        public string CustomFileProviderPathsAndroid = string.Empty;

        /// <summary>
        /// Gets/creates an instance of <see cref="UShareBuildSettings"/> from the Unity ProjectSettings folder.
        /// </summary>
        public static UShareBuildSettings Get()
        {
            if (File.Exists(SavePath))
                return JsonUtility.FromJson<UShareBuildSettings>(File.ReadAllText(SavePath));

            UShareBuildSettings newInstance = new();
            File.WriteAllText(SavePath, JsonUtility.ToJson(newInstance));

            return newInstance;
        }

        /// <summary>
        /// Saves the settings to the Unity ProjectSettings folder.
        /// </summary>
        /// <param name="settings">The settings to save.</param>
        public static void Save(UShareBuildSettings settings)
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(settings));
        }
    }
}
