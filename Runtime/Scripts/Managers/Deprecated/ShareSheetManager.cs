// Copyright 2026 URAV ADVANCED LEARNING SYSTEMS PRIVATE LIMITED
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
using Uralstech.Utils.Singleton;

#nullable enable
namespace Uralstech.UShare
{
    [AddComponentMenu("Uralstech/UShare/Share Sheet Manager")]
    [Obsolete("Deprecated in favour of " + nameof(ShareSheetManagerV2) + ". Subclassing ShareSheetManager is no longer supported.")]
    public sealed class ShareSheetManager : Singleton<ShareSheetManager>
    {
        internal const string DefaultAuthorityFormat = "{0}.FileProvider";
        
        /// <summary>
        /// The Android FileProvider URI authority the application has set in its manifest to use for share actions.
        /// If not provided, defaults to "{Application.identifier}.FileProvider".
        /// </summary>
        [Tooltip("The Android FileProvider URI authority the application has set in its manifest to use for share actions. If not provided, defaults to \"{Application.identifier}.FileProvider\".")]
        public string AndroidFileProviderAuthority;
        
        [Tooltip("Should this object persist between scenes?")]
        [SerializeField] private bool _persistBetweenScenes;
        
#if UNITY_ANDROID
        public AndroidPathHelper AndroidPathHelper => ShareSheetManagerV2.Instance.AndroidPathHelper;
#endif

        protected override void Awake()
        {
            base.Awake();
            if (_persistBetweenScenes)
                DontDestroyOnLoad(gameObject);
            
#if UNITY_ANDROID
            if (string.IsNullOrEmpty(AndroidFileProviderAuthority))
                AndroidFileProviderAuthority = string.Format(DefaultAuthorityFormat, Application.identifier);
#endif
        }

        /// <seealso cref="ShareSheetManagerV2.GetDefaultBasePath"/>
        public string GetDefaultBasePath() =>
            ShareSheetManagerV2.Instance.GetDefaultBasePath();

        /// <seealso cref="ShareSheetManagerV2.TryShareText(string,string?)"/>
        public bool ShareText(string text, string? title = null) =>
            ShareSheetManagerV2.Instance.TryShareText(text, title);

        public bool ShareData(string contentType, string fileName, byte[] data,
            AdditionalShareData additionalData = default)
        {
            int id = Environment.TickCount;
            
#if UNITY_IOS
            if (contentType.StartsWith("image/"))
                return ShareSheetManagerV2.Instance.TryShareImageIOS(id, data,
                    additionalData.ToShareOptions(AndroidFileProviderAuthority));
#endif
            
            try
            {
                string basePath = additionalData.BasePath ?? GetDefaultBasePath();
                Directory.CreateDirectory(basePath);

                string path = Path.Join(basePath, fileName);
                File.WriteAllBytes(path, data);

                return ShareSheetManagerV2.Instance.TryShareFile(id, path, contentType,
                    additionalData.ToShareOptions(AndroidFileProviderAuthority));
            }
            catch (IOException ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }

        public bool ShareData(string contentType, (string FileName, byte[] Data)[] files,
            AdditionalShareData additionalData = default)
        {
            int id = Environment.TickCount;
            
#if UNITY_IOS
            if (contentType.StartsWith("image/"))
            {
                byte[][] images = Array.ConvertAll(files, static file => file.Data);
                return ShareSheetManagerV2.Instance.TryShareImagesIOS(id, images,
                    additionalData.ToShareOptions(AndroidFileProviderAuthority));
            }
#endif
            
            string[] paths = new string[files.Length];

            try
            {
                string basePath = additionalData.BasePath ?? GetDefaultBasePath();
                Directory.CreateDirectory(basePath);

                for (int i = 0; i < files.Length; i++)
                {
                    (string fileName, byte[] data) = files[i];
                    string path = Path.Join(basePath, fileName);
                    File.WriteAllBytes(path, data);

                    paths[i] = path;
                }

                return ShareSheetManagerV2.Instance.TryShareFiles(id, paths, contentType,
                    additionalData.ToShareOptions(AndroidFileProviderAuthority));
            }
            catch (IOException ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }

        public bool ShareFile(string contentType, string fileName, AdditionalShareData additionalData = default)
        {
            string dirPath = additionalData.BasePath ?? GetDefaultBasePath();
            string filePath = Path.Join(dirPath, fileName);

            return ShareSheetManagerV2.Instance.TryShareFile(0, filePath, contentType,
                additionalData.ToShareOptions(AndroidFileProviderAuthority));
        }

        public bool ShareFiles(string contentType, string[] fileNames, AdditionalShareData additionalData = default)
        {
            int count = fileNames.Length;
            string[] paths = new string[count];
            string dirPath = additionalData.BasePath ?? GetDefaultBasePath();

            for (int i = 0; i < count; i++)
                paths[i] = Path.Join(dirPath, fileNames[i]);

            return ShareSheetManagerV2.Instance.TryShareFiles(0, paths, contentType,
                additionalData.ToShareOptions(AndroidFileProviderAuthority));
        }
        
#if UNITY_ANDROID
        public AndroidJavaObject? GetShareableFileUri(string fileName, string? basePath = null,
            string? fileProviderAuthority = null)
        {
            AdditionalShareData data = new() { AndroidFileProviderAuthority = fileProviderAuthority };
            fileProviderAuthority = data.ToShareOptions(AndroidFileProviderAuthority).Authority;

            if (string.IsNullOrEmpty(basePath))
                basePath = GetDefaultBasePath();
            
            string path = Path.Join(basePath, fileName);
            return ShareSheetManagerV2.Instance.GetFileURIAndroid(path, fileProviderAuthority);
        }

        public void ShareUris(string contentType, AndroidJavaObject[] uris, string? textContent = null,
            string? title = null)
        {
            ShareOptions options = new() { Text = textContent, Title = title };
            ShareSheetManagerV2.Instance.ShareFileURIsAndroid(uris, contentType, options);
        }
#endif
    }
}