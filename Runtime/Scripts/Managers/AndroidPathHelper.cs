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
using UnityEngine;
using UnityEngine.Android;

#nullable enable
namespace Uralstech.UShare
{
    /// <summary>Path utilities for Android.</summary>
    public sealed class AndroidPathHelper
    {
        /// <summary>The application's cache directory (context.cacheDir).</summary>
        public string CacheDirectory => GetPath("getCacheDir")!;
        
        /// <summary>The application's external cache directory (context.externalCacheDir).</summary>
        public string? ExternalCacheDirectory => GetPath("getExternalCacheDir");

        /// <summary>The application's directory for persistent files (context.filesDir).</summary>
        public string FilesDirectory => GetPath("getFilesDir")!;

        /// <summary>The application's external directory for persistent files (from context.getExternalFilesDir).</summary>
        public string? ExternalFilesDirectory => GetExternalFilesDirectory();

        /// <summary>Gets the application's external directory for persistent files (from context.getExternalFilesDir).</summary>
        public static string? GetExternalFilesDirectory(string? type = null)
        {
            if (Application.platform != RuntimePlatform.Android)
                throw new PlatformNotSupportedException();
            
            using AndroidJavaObject? javaFile = AndroidApplication.currentContext.Call<AndroidJavaObject>("getExternalFilesDir", type);
            return javaFile?.Call<string>("getAbsolutePath");
        }
        
        private static string? GetPath(string methodName)
        {
            if (Application.platform != RuntimePlatform.Android)
                throw new PlatformNotSupportedException();
            
            using AndroidJavaObject? javaFile = AndroidApplication.currentContext.Call<AndroidJavaObject>(methodName);
            return javaFile?.Call<string>("getAbsolutePath");
        }
    }
}