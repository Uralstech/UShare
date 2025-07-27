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
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Android;
using Uralstech.Utils.Loggers;
using Uralstech.Utils.Singleton;

#nullable enable
namespace Uralstech.UShare
{
    /// <summary>
    /// Class to handle the share sheet functionality.
    /// </summary>
    [AddComponentMenu("Uralstech/UShare/Share Sheet Manager")]
    public class ShareSheetManager : DontCreateNewSingleton<ShareSheetManager>
    {
        /// <summary>
        /// The fully qualified name of the native Android plugin class.
        /// </summary>
        protected const string AndroidNativeClass = "com.uralstech.ushare.ShareHelper";

        /// <summary>
        /// The default directory used to temporarily save data that is shared.
        /// </summary>
        protected const string DefaultSaveSubDirectory = "ShareCache";

        /// <summary>
        /// Default format for the Android FileProvider URI authority.
        /// </summary>
        protected const string DefaultFileProviderAuthorityFormat = "{0}.FileProvider";

        private static readonly string s_loggerTag = $"{nameof(UShare)}.{nameof(ShareSheetManager)}";
        private static readonly TaggedRALogger s_logger = new(s_loggerTag);

        /// <summary>
        /// The Android FileProvider URI authority the application has set in its manifest to use for share actions.
        /// If not provided, defaults to "{Application.identifier}.FileProvider".
        /// </summary>
        [Tooltip("The Android FileProvider URI authority the application has set in its manifest to use for share actions. If not provided, defaults to \"{Application.identifier}.FileProvider\".")]
        public string AndroidFileProviderAuthority;

        /// <summary>
        /// Should this object persist between scenes?
        /// </summary>
        [Tooltip("Should this object persist between scenes?")]
        [SerializeField] protected bool _persistBetweenScenes = false;

#if UNITY_ANDROID
        /// <summary>
        /// Utility object for Android which provides paths to where shareable data can be saved.
        /// </summary>
        public AndroidPathHelper AndroidPathHelper { get; protected set; }

        /// <summary>
        /// The native plugin instance.
        /// </summary>
        protected AndroidJavaObject? _pluginInstance;

        /// <summary>
        /// List of files scheduled for deletion after a share action has been completed.
        /// </summary>
        protected readonly List<string> _filesScheduledForDeletion = new();
#endif

        /// <inheritdoc/>
        protected override void Awake()
        {
            base.Awake();

            if (_persistBetweenScenes)
                DontDestroyOnLoad(gameObject);

#if UNITY_ANDROID
            s_logger.Log("Initializing for Android.");

            using AndroidJavaClass classObject = new(AndroidNativeClass);
            _pluginInstance = classObject.CallStatic<AndroidJavaObject>("getInstance", AndroidApplication.currentContext);
            AndroidPathHelper = new AndroidPathHelper(_pluginInstance);

            if (string.IsNullOrEmpty(AndroidFileProviderAuthority))
                AndroidFileProviderAuthority = string.Format(DefaultFileProviderAuthorityFormat, Application.identifier);
#endif
        }

#if UNITY_ANDROID
        /// <inheritdoc/>
        protected void OnApplicationFocus(bool focus)
        {
            if (focus && _filesScheduledForDeletion.Count > 0)
            {
                s_logger.Log("Focus regained, erasing shared files from app storage.");
                foreach (string file in _filesScheduledForDeletion)
                {
                    try
                    {
                        if (File.Exists(file))
                        {
                            File.Delete(file);
                            s_logger.Log("Deleted file: {0}", file);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }

                _filesScheduledForDeletion.Clear();
            }
        }

        /// <inheritdoc/>
        protected void OnDestroy()
        {
            s_logger.Log("Releasing native resources.");
            AndroidPathHelper.Invalidate();
            _pluginInstance?.Dispose();
            _pluginInstance = null;
        }
#endif

        /// <summary>
        /// Gets the default shareable directory as defined in the native plugin.
        /// </summary>
        public string GetDefaultBasePath()
        {
#if UNITY_ANDROID
            return Path.Join(AndroidPathHelper.CacheDirectory, DefaultSaveSubDirectory);
#else
            throw new NotSupportedException($"{nameof(ShareSheetManager)} does not have an implementation for {nameof(GetDefaultBasePath)} for the current platform.");
#endif
        }

        /// <summary>
        /// Shares text to other apps using the system share sheet.
        /// </summary>
        /// <param name="text">The text to share.</param>
        /// <param name="title">Optional title for the share sheet (Android 10+).</param>
        public void ShareText(string text, string? title = null)
        {
#if UNITY_ANDROID
            s_logger.Log("Sharing text using Android plugin.");
            _pluginInstance!.Call("shareText", text, title);
#else
            throw new NotSupportedException($"{nameof(ShareSheetManager)} does not have an implementation for {nameof(ShareText)} for the current platform.");
#endif
        }

        /// <summary>
        /// Shares data as a file URI to other apps using the system share sheet.
        /// </summary>
        /// <remarks>
        /// This method writes the data to a file in a folder that is accessible by other apps, and then shares its location as a URI.
        /// By default, this location is in the app's cache directory under a subdirectory named "ShareCache". When the user regains
        /// focus on the app after sharing, the file will be deleted to prevent the cache directory from growing too large. These
        /// settings can be overridden by setting the <see cref="AdditionalShareData"/> parameter.
        /// </remarks>
        /// <param name="contentType">The MIME type of the content to share. See <see cref="CommonMimeTypes"/> for common MIME types.</param>
        /// <param name="fileName">The file name of the content to share (eg. "image.png").</param>
        /// <param name="data">The data to share.</param>
        /// <param name="additionalData">Additional data for the event.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool ShareData(string contentType, string fileName, byte[] data, AdditionalShareData additionalData = default)
        {
#if UNITY_ANDROID
            try
            {
                s_logger.Log("Writing data for singular file to shareable directory.");
                string basePath = additionalData.BasePath ?? GetDefaultBasePath();
                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);

                string path = Path.Join(basePath, fileName);
                File.WriteAllBytes(path, data);

                s_logger.Log("Data written to {0}.", path);
                if (!additionalData.KeepDataAfterFocusRegain)
                    _filesScheduledForDeletion.Add(path);
            }
            catch (IOException ex)
            {
                Debug.LogException(ex);
                return false;
            }

            return ShareFile(contentType, fileName, additionalData);
#else
            throw new NotSupportedException($"{nameof(ShareSheetManager)} does not have an implementation for {nameof(ShareData)} for the current platform.");
#endif
        }

        /// <summary>
        /// Shares data as multiple file URIs to other apps using the system share sheet.
        /// </summary>
        /// <remarks>
        /// This method writes the data to files in a folder that is accessible by other apps, and then shares its location as a URI.
        /// By default, this location is in the app's cache directory under a subdirectory named "ShareCache". When the user regains
        /// focus on the app after sharing, the file will be deleted to prevent the cache directory from growing too large. These
        /// settings can be overridden by setting the <see cref="AdditionalShareData"/> parameter.
        /// </remarks>
        /// <param name="contentType">The MIME type of the content to share. See <see cref="CommonMimeTypes"/> for common MIME types.</param>
        /// <param name="files">An array of tuples containing file names and their corresponding data to share.</param>
        /// <param name="additionalData">Additional data for the event.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool ShareData(string contentType, (string FileName, byte[] Data)[] files, AdditionalShareData additionalData = default)
        {
#if UNITY_ANDROID
            string[] fileNames = new string[files.Length];

            try
            {
                s_logger.Log("Writing data for multiple files to shareable directory.");
                string basePath = additionalData.BasePath ?? GetDefaultBasePath();
                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);

                for (int i = 0; i < files.Length; i++)
                {
                    (string fileName, byte[] data) = files[i];
                    string path = Path.Join(basePath, fileName);
                    File.WriteAllBytes(path, data);

                    s_logger.Log("Data written to {0}.", path);
                    if (!additionalData.KeepDataAfterFocusRegain)
                        _filesScheduledForDeletion.Add(path);

                    fileNames[i] = fileName;
                }
            }
            catch (IOException ex)
            {
                Debug.LogException(ex);
                return false;
            }

            return ShareFiles(contentType, fileNames, additionalData);
#else
            throw new NotSupportedException($"{nameof(ShareSheetManager)} does not have an implementation for {nameof(ShareData)} for the current platform.");
#endif
        }

        /// <summary>
        /// Shares a file as a URI to other apps using the system share sheet.
        /// </summary>
        /// <remarks>
        /// By default, the file is expected to be in the app's cache directory under a subdirectory named "ShareCache".
        /// This can be overridden by setting the <see cref="AdditionalShareData"/> parameter.
        /// </remarks>
        /// <param name="contentType">The MIME type of the content to share. See <see cref="CommonMimeTypes"/> for common MIME types.</param>
        /// <param name="fileName">The name of the file to share.</param>
        /// <param name="additionalData">Additional data for the event.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool ShareFile(string contentType, string fileName, AdditionalShareData additionalData = default)
        {
#if UNITY_ANDROID
            s_logger.Log("Sharing file using Android plugin.");
            return _pluginInstance!.Call<bool>("shareFile",
                contentType,
                fileName,
                additionalData.AndroidFileProviderAuthority ?? AndroidFileProviderAuthority,
                additionalData.BasePath,
                additionalData.AdditionalText,
                additionalData.Title);
#else
            throw new NotSupportedException($"{nameof(ShareSheetManager)} does not have an implementation for {nameof(ShareFile)} for the current platform.");
#endif
        }

        /// <summary>
        /// Shares multiple files as URIs to other apps using the system share sheet.
        /// </summary>
        /// <remarks>
        /// By default, the files are expected to be in the app's cache directory under a subdirectory named "ShareCache".
        /// This can be overridden by setting the <see cref="AdditionalShareData"/> parameter.
        /// 
        /// If you want to share files that are present in different directories, you will have to get the shareable URIs for
        /// those files individually using (Android) <see cref="GetShareableFileUri(string, string?, string?)"/> and then share
        /// them using (Android) <see cref="ShareUris(string, AndroidJavaObject[], string?, string?)"/> method.
        /// </remarks>
        /// <param name="contentType">The MIME type of the content to share. See <see cref="CommonMimeTypes"/> for common MIME types.</param>
        /// <param name="fileNames">The names of the files to share.</param>
        /// <param name="additionalData">Additional data for the event.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool ShareFiles(string contentType, string[] fileNames, AdditionalShareData additionalData = default)
        {
#if UNITY_ANDROID
            s_logger.Log("Sharing files using Android plugin.");
            return _pluginInstance!.Call<bool>("shareFiles",
                contentType,
                fileNames,
                additionalData.AndroidFileProviderAuthority ?? AndroidFileProviderAuthority,
                additionalData.BasePath,
                additionalData.AdditionalText,
                additionalData.Title);
#else
            throw new NotSupportedException($"{nameof(ShareSheetManager)} does not have an implementation for {nameof(ShareFiles)} for the current platform.");
#endif
        }

#if UNITY_ANDROID
        /// <summary>
        /// (Android) Shares URIs, which point to files, to other apps using the system share sheet.
        /// </summary>
        /// <param name="contentType">The MIME type of the content to share. See <see cref="CommonMimeTypes"/> for common MIME types.</param>
        /// <param name="uris">The native file URI objects to share.</param>
        /// <param name="textContent">Additional text to be shared along with the main content.</param>
        /// <param name="title">Optional title for the share sheet (Android 10+, but not guaranteed to work when sharing non-text media).</param>
        public void ShareUris(string contentType, AndroidJavaObject[] uris, string? textContent = null, string? title = null)
        {
            s_logger.Log("Sharing URIs using Android plugin.");
            _pluginInstance!.Call("shareUris", contentType, uris, textContent, title);
        }

        /// <summary>
        /// (Android) Gets a shareable URI for a file <b>located in a shareable directory</b> in the app's storage.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="basePath">The path to the file. If not given, it will expect the file in a subdirectory named "ShareCache" under the app's cache directory.</param>
        /// <param name="fileProviderAuthority">The FileProvider authority for the file being shared.</param>
        /// <returns>The native Android URI object or <see langword="null"/> if it could not create the URI.</returns>
        public AndroidJavaObject? GetShareableFileUri(string fileName, string? basePath = null, string? fileProviderAuthority = null)
        {
            s_logger.Log("Getting shareable file URI using Android plugin.");
            return _pluginInstance!.Call<AndroidJavaObject?>("getShareableFileUri",
                fileName,
                fileProviderAuthority ?? AndroidFileProviderAuthority,
                basePath);
        }
#endif
    }
}
