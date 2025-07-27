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

using UnityEngine;

#if UNITY_ANDROID

#nullable enable
namespace Uralstech.UShare
{
    /// <summary>
    /// Path utilities for Android.
    /// </summary>
    public class AndroidPathHelper
    {
        /// <summary>Returns the path to the application's assigned cache directory (context.cacheDir).</summary>
        public string CacheDirectory => PluginInstance!.Call<string>("getCacheDirPath");

        /// <summary>Returns the path to the application's assigned external cache directory (context.externalCacheDir).</summary>
        public string? ExternalCacheDirectory => PluginInstance!.Call<string?>("getExternalCacheDirPath");

        /// <summary>Returns the path to the application's assigned files directory (context.filesDir).</summary>
        public string FilesDirectory => PluginInstance!.Call<string>("getFilesDirPath");

        /// <summary>Returns the path to the application's assigned external files directory (from context.getExternalFilesDir).</summary>
        public string? ExternalFilesDirectory => PluginInstance!.Call<string?>("getExternalFilesDirPath");

        /// <summary>The native plugin instance.</summary>
        protected AndroidJavaObject? PluginInstance { get; private set; }

        /// <param name="pluginInstance">The native plugin instance.</param>
        internal protected AndroidPathHelper(AndroidJavaObject? pluginInstance)
        {
            PluginInstance = pluginInstance;
        }

        /// <summary>
        /// Invalidates this instance by setting <see cref="PluginInstance"/> to <see langword="null"/>.
        /// It is the responsibility of the creator of this object to dispose of <see cref="PluginInstance"/>.
        /// </summary>
        internal protected void Invalidate()
        {
            PluginInstance = null;
        }
    }
}

#endif