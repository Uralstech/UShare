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

using System.IO;
using UnityEditor.PackageManager;
using UnityEngine;

#nullable enable
namespace Uralstech.UShare.Editor
{
    /// <summary>
    /// Path utilities for editor scripts.
    /// </summary>
    public static class PathUtils
    {
        /// <summary>
        /// The package ID of the UShare package.
        /// </summary>
        public const string PackageId = "com.uralstech.ushare";

        /// <summary>
        /// Cached result of <see cref="GetPackagePath(bool)"/>.
        /// </summary>
        private static string? s_packagePathCached = string.Empty;

        /// <summary>
        /// Validates a path and logs an error if it doesn't exist.
        /// </summary>
        /// <param name="path">The path to validate.</param>
        /// <returns>The validity of the path.</returns>
        public static bool ValidateFilePath(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"Asset required by UShare at path {path} could not be found.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the path to the UShare package folder.
        /// </summary>
        /// <returns>The path if found.</returns>
        public static string? GetPackagePath()
        {
            if (!string.IsNullOrEmpty(s_packagePathCached) && Directory.Exists(s_packagePathCached))
                return s_packagePathCached;

            s_packagePathCached = PackageInfo.FindForPackageName(PackageId)?.assetPath;
            if (string.IsNullOrEmpty(s_packagePathCached) || !Directory.Exists(s_packagePathCached))
                Debug.LogError("Could not resolve the path to the UShare package.");

            return s_packagePathCached;
        }
    }
}
