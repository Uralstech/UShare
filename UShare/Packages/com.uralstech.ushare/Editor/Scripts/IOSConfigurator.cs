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

#if UNITY_IOS

using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

#nullable enable
namespace Uralstech.UShare.Editor
{
    /// <summary>
    /// iOS project configurator for UShare.
    /// </summary>
    public class IOSConfigurator : IPostprocessBuildWithReport
    {
        /// <inheritdoc/>
        public int callbackOrder { get; } = 0;

        /// <inheritdoc/>
        public void OnPostprocessBuild(BuildReport report)
        {
            UShareBuildSettings settings = UShareBuildSettings.Get();
            if (!settings.PatchInfoPlistIOS || string.IsNullOrEmpty(settings.PhotoLibraryAdditionsUsageDescriptionIOS))
            {
                Debug.Log("UShare iOS patching disabled or Photo Library Additions Usage Description was empty.");
                return;
            }

            string plistPath = Path.Combine(report.summary.outputPath, "Info.plist");

            PlistDocument plist = new();
            plist.ReadFromFile(plistPath);

            plist.root.SetString("NSPhotoLibraryAddUsageDescription", settings.PhotoLibraryAdditionsUsageDescriptionIOS);
            
            File.WriteAllText(plistPath, plist.WriteToString());
            Debug.Log("UShare: Patched Info.plist with Photo Library usage description.");
        }
    }
}

#endif