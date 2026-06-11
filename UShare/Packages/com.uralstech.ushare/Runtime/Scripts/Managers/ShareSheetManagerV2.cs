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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Events;
using Uralstech.UShare.Native;
using Uralstech.Utils.Singleton;

#nullable enable
namespace Uralstech.UShare
{
    /// <summary>Manages the native share sheet plugins.</summary>
    [AddComponentMenu("Uralstech/UShare/Share Sheet Manager V2")]
    public sealed class ShareSheetManagerV2 : Singleton<ShareSheetManagerV2>
    {
        private const string DefaultSubDirectory = "ShareCache";
        private const string DefaultAuthorityFormat = "{0}.FileProvider";
        
        private static readonly bool s_isAndroid = Application.platform == RuntimePlatform.Android;
        private static readonly bool s_isIOS = Application.platform == RuntimePlatform.IPhonePlayer;
        private static readonly bool s_isSupported = s_isAndroid || s_isIOS;

        private static int s_eventIdCounter;
        private static int GetEventId() => Interlocked.Increment(ref s_eventIdCounter);

        /// <summary>Result for QOL share methods that may generate temporary files.</summary>
        /// <param name="Success"><see langword="true"/> if the share sheet activity was presented; <see langword="false"/> otherwise.</param>
        /// <param name="CreatedFiles">
        /// Any temporary files created for the event, regardless of <see cref="Success"/>. <b>DO NOT</b> delete these immediately after the event is invoked,
        /// or even after <see cref="ShareSheetManagerV2.OnResult"/> is invoked. Delete these files later, like on subsequent app launches.
        /// </param>
        public sealed record EventStatus(bool Success, IReadOnlyList<string> CreatedFiles);
        
        /// <summary>
        /// The Android <a href="https://developer.android.com/reference/androidx/core/content/FileProvider"><c>FileProvider</c></a>
        /// authority used for sharing. Defaults to <c>"{Application.identifier}.FileProvider"</c>, the provider defined by the Android UShare plugin.
        /// </summary>
        [Tooltip("The Android FileProvider authority used for sharing. Defaults to \"{Application.identifier}.FileProvider\", the provider defined by the Android UShare plugin.")]
        public string? FileProviderAuthority;
        
        /// <summary>
        /// Invoked when the share activity has "completed", with its assigned event ID.
        /// This has different meanings in iOS and Android, see remarks for more info.
        /// </summary>
        /// <remarks>
        /// <para>
        /// On iOS, this is invoked after the completion or dismissal of the share event.
        /// </para>
        /// <para>
        /// On Android, this is invoked only if the user has chosen a target to share to,
        /// and is <b>not</b> reliable for all targets.
        /// </para>
        /// </remarks>
        /// <seealso cref="AndroidInterop.Callbacks.OnTargetChosen"/>
        [field: SerializeField, Tooltip("Invoked when the share activity has \"completed\". This has different meanings in iOS and Android, see remarks/manual for more info.")]
        public UnityEvent<int> OnResult { get; private set; } = new();
        
        /// <summary>Utility for providing commonly used Android base paths like <c>cacheDir</c>, <c>filesDir</c>, etc.</summary>
        public AndroidPathHelper AndroidPathHelper { get; } = new();
        
        private AndroidInterop.Callbacks? _androidCallbacks;
        private AndroidJavaObject? _androidNative;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            if (s_isAndroid)
            {
                _androidCallbacks = new AndroidInterop.Callbacks();
                _androidCallbacks.OnTargetChosen += OnActivityResult;
                _androidNative = AndroidInterop.CreateInstance(_androidCallbacks);
            }
        }

        private void OnDestroy()
        {
            if (s_isAndroid)
            {
                _androidNative?.Dispose();
                if (_androidCallbacks != null)
                    _androidCallbacks.OnTargetChosen -= OnActivityResult;
            }
        }
        
        #region Simple APIs
        
        /// <inheritdoc cref="TryShareText(int,string,string?)"/>
        /// <remarks>Uses an automatically generated event ID.</remarks>
        public bool TryShareText(string text, string? title = null) =>
            TryShareText(GetEventId(), text, title);
        
        /// <inheritdoc cref="TryShareFile(int,string,string,Uralstech.UShare.ShareOptions?)"/>
        /// <remarks>
        /// <para>Uses an automatically generated event ID.</para>
        /// <para>(Android) Ensure the path is under the scope of the FileProvider; see <see cref="GetDefaultBasePath"/> for this plugin's default folder for shareable data.</para>
        /// </remarks>
        public bool TryShareFile(string path, string contentType, ShareOptions? options = null) =>
            TryShareFile(GetEventId(), path, contentType, options);
        
        /// <inheritdoc cref="TryShareFiles(int,string[],string,Uralstech.UShare.ShareOptions?)"/>
        /// <remarks>
        /// <para>Uses an automatically generated event ID.</para>
        /// <para>(Android) Ensure the paths are under the scope of the FileProvider; see <see cref="GetDefaultBasePath"/> for this plugin's default folder for shareable data.</para>
        /// </remarks>
        public bool TryShareFiles(string[] paths, string contentType, ShareOptions? options = null) =>
            TryShareFiles(GetEventId(), paths, contentType, options);
        
        #endregion

        #region Implementation APIs
        
        /// <summary>Shares text to other apps using the system share sheet.</summary>
        /// <remarks>Duplicate IDs in successive calls may prevent reliable <see cref="OnResult"/> invocations in Android.</remarks>
        /// <param name="id">The ID of this share event, returned in <see cref="OnResult"/>.</param>
        /// <param name="text">The text to share.</param>
        /// <param name="title">Optional title for the share sheet.</param>
        /// <returns><see langword="true"/> if the share sheet activity was presented; <see langword="false"/> otherwise.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android or iOS.</exception>
        public bool TryShareText(int id, string text, string? title = null)
        {
            if (!s_isSupported) throw new PlatformNotSupportedException();
            return s_isIOS
                ? IOSInterop.ushare_ios_share_text(id, text, title, OnActivityResult)
                : AndroidInterop.ShareText(_androidNative!, id, text, title);
        }

        /// <summary>Shares a file to other apps using the system share sheet.</summary>
        /// <remarks>
        /// <para>Duplicate IDs in successive calls may prevent reliable <see cref="OnResult"/> invocations in Android.</para>
        /// <para>(Android) Ensure the path is under the scope of the FileProvider; see <see cref="GetDefaultBasePath"/> for this plugin's default folder for shareable data.</para>
        /// </remarks>
        /// <param name="id">The ID of this share event, returned in <see cref="OnResult"/>.</param>
        /// <param name="path">The path to the file to share.</param>
        /// <param name="contentType">The MIME type of the file's data; see <see cref="CommonMimeTypes"/> for common MIME types.</param>
        /// <param name="options">Additional options for the event.</param>
        /// <returns><see langword="true"/> if the share sheet activity was presented; <see langword="false"/> otherwise.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android or iOS.</exception>
        public bool TryShareFile(int id, string path, string contentType, ShareOptions? options = null)
        {
            if (!s_isSupported) throw new PlatformNotSupportedException();
            
            if (s_isIOS)
                return IOSInterop.ushare_ios_share_files(id, ArrayOf(path), 1,
                    options?.Text, options?.Title, OnActivityResult);

            using AndroidJavaObject? fileUri = GetFileURIAndroid(path, options?.Authority);
            if (fileUri == null) return false;

            return AndroidInterop.ShareURI(_androidNative!, id, fileUri, contentType,
                options?.Text, options?.Title);
        }

        /// <summary>Shares multiple files to other apps using the system share sheet.</summary>
        /// <remarks>
        /// <para>Duplicate IDs in successive calls may prevent reliable <see cref="OnResult"/> invocations in Android.</para>
        /// <para>(Android) Ensure the paths are under the scope of the FileProvider; see <see cref="GetDefaultBasePath"/> for this plugin's default folder for shareable data.</para>
        /// </remarks>
        /// <param name="id">The ID of this share event, returned in <see cref="OnResult"/>.</param>
        /// <param name="paths">The paths to files to share.</param>
        /// <param name="contentType">The MIME type of the combined data of the files; see <see cref="CommonMimeTypes"/> for common MIME types.</param>
        /// <param name="options">Additional options for the event.</param>
        /// <returns><see langword="true"/> if the share sheet activity was presented; <see langword="false"/> otherwise.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android or iOS.</exception>
        public bool TryShareFiles(int id, string[] paths, string contentType, ShareOptions? options = null)
        {
            if (!s_isSupported) throw new PlatformNotSupportedException();

            int count = paths.Length;
            if (s_isIOS)
                return IOSInterop.ushare_ios_share_files(id, paths, count,
                    options?.Text, options?.Title, OnActivityResult);
            
            AndroidJavaObject?[] fileUris = new AndroidJavaObject?[count];
            try
            {
                for (int i = 0; i < count; i++)
                {
                    AndroidJavaObject? fileUri = GetFileURIAndroid(paths[i], options?.Authority);
                    if (fileUri == null) return false;
                    fileUris[i] = fileUri;
                }
                
                return AndroidInterop.ShareURIs(_androidNative!, id, fileUris!, contentType,
                    options?.Text, options?.Title);
            }
            finally
            {
                for (int i = 0; i < count; i++)
                    fileUris[i]?.Dispose();
            }
        }
        
        #endregion

        #region iOS Specific APIs

        /// <summary>Shares an image to other apps using the iOS share sheet.</summary>
        /// <remarks>
        /// <para>
        /// This is an iOS-specific method which shares the given image data directly with the native plugin.
        /// The plugin then creates a <a href="https://developer.apple.com/documentation/uikit/uiimage"><c>UIImage</c></a>
        /// and forwards it to the share sheet.
        /// </para>
        /// <para>For a multi-platform method that uses this for iOS, see <see cref="TryShareImage(System.ReadOnlySpan{byte},string,string,Uralstech.UShare.ShareOptions?)"/></para>
        /// </remarks>
        /// <param name="id">The ID of this share event, returned in <see cref="OnResult"/>.</param>
        /// <param name="image">The image to share, must be a format supported by <c>UIImage</c>.</param>
        /// <param name="options">Additional options for the event.</param>
        /// <returns><see langword="true"/> if the share sheet activity was presented; <see langword="false"/> otherwise.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than iOS.</exception>
        public unsafe bool TryShareImageIOS(int id, ReadOnlySpan<byte> image, ShareOptions? options = null)
        {
            if (!s_isIOS) throw new PlatformNotSupportedException();
            
            try
            {
                fixed (byte* imagePtr = image)
                    return IOSInterop.ushare_ios_share_images(id, ArrayOf(new IntPtr(imagePtr)),
                        ArrayOf(image.Length), 1, options?.Text, options?.Title, OnActivityResult);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }

        /// <summary>Shares multiple images to other apps using the iOS share sheet.</summary>
        /// <remarks>
        /// <para>
        /// This is an iOS-specific method which shares the given image data directly with the native plugin.
        /// The plugin then creates <a href="https://developer.apple.com/documentation/uikit/uiimage"><c>UIImage</c></a>s
        /// and forwards them to the share sheet.
        /// </para>
        /// <para>For a multi-platform method that uses this for iOS, see <see cref="TryShareImages(int,System.Collections.Generic.IReadOnlyList{byte[]},System.Collections.Generic.IReadOnlyList{string},string,Uralstech.UShare.ShareOptions?)"/></para>
        /// </remarks>
        /// <param name="id">The ID of this share event, returned in <see cref="OnResult"/>.</param>
        /// <param name="images">The images to share, must be in formats supported by <c>UIImage</c>.</param>
        /// <param name="options">Additional options for the event.</param>
        /// <returns><see langword="true"/> if the share sheet activity was presented; <see langword="false"/> otherwise.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than iOS.</exception>
        public bool TryShareImagesIOS(int id, IReadOnlyList<byte[]> images, ShareOptions? options = null)
        {
            if (!s_isIOS) throw new PlatformNotSupportedException();
            
            int count = images.Count;
            int[] sizes = new int[count];
            IntPtr[] imagePtrs = new IntPtr[count];
            GCHandle?[] gcHandles = new GCHandle?[count];

            try
            {
                for (int i = 0; i < count; i++)
                {
                    byte[] image = images[i];
                    GCHandle handle = GCHandle.Alloc(image, GCHandleType.Pinned);

                    gcHandles[i] = handle;
                    sizes[i] = image.Length;
                    imagePtrs[i] = handle.AddrOfPinnedObject();
                }

                return IOSInterop.ushare_ios_share_images(id, imagePtrs, sizes,
                    count, options?.Text, options?.Title, OnActivityResult);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
            finally
            {
                for (int i = 0; i < count; i++)
                    gcHandles[i]?.Free();
            }
        }

        /// <inheritdoc cref="TryShareImagesIOS(int,System.Collections.Generic.IReadOnlyList{byte[]},Uralstech.UShare.ShareOptions?)"/>
        /// <remarks>
        /// <para>
        /// This is an iOS-specific method which shares the given image data directly with the native plugin.
        /// The plugin then creates <a href="https://developer.apple.com/documentation/uikit/uiimage"><c>UIImage</c></a>s
        /// and forwards them to the share sheet.
        /// </para>
        /// <para>For a multi-platform method that uses this for iOS, see <see cref="TryShareImages(int,System.Collections.Generic.IReadOnlyList{Unity.Collections.NativeArray{byte}.ReadOnly},System.Collections.Generic.IReadOnlyList{string},string,Uralstech.UShare.ShareOptions?)"/></para>
        /// </remarks>
        public unsafe bool TryShareImagesIOS(int id, IReadOnlyList<NativeArray<byte>.ReadOnly> images, ShareOptions? options = null)
        {
            if (!s_isIOS) throw new PlatformNotSupportedException();
            
            int count = images.Count;
            int[] sizes = new int[count];
            IntPtr[] imagePtrs = new IntPtr[count];

            try
            {
                for (int i = 0; i < count; i++)
                {
                    NativeArray<byte>.ReadOnly image = images[i];
                    
                    sizes[i] = image.Length;
                    imagePtrs[i] = new IntPtr(image.GetUnsafeReadOnlyPtr());
                }
                
                return IOSInterop.ushare_ios_share_images(id, imagePtrs, sizes,
                    count, options?.Text, options?.Title, OnActivityResult);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }

        #endregion

        #region Android Specific APIs

        // internal so that ShareSheetManager can access this while it is deprecated.
        internal AndroidJavaObject? GetFileURIAndroid(string path, string? authority = null)
        {
            if (!s_isAndroid) throw new PlatformNotSupportedException();
            if (string.IsNullOrEmpty(authority))
                authority = string.IsNullOrEmpty(FileProviderAuthority)
                    ? string.Format(DefaultAuthorityFormat, Application.identifier)
                    : FileProviderAuthority;

            return AndroidInterop.GetFileURI(_androidNative!, path, authority);
        }

        // internal so that ShareSheetManager can access this while it is deprecated.
        [Obsolete("This method exists solely to make " + nameof(ShareSheetManager) + " backwards compatible.")]
        internal void ShareFileURIsAndroid(AndroidJavaObject[] uris, string contentType, ShareOptions options)
        {
            if (!s_isAndroid) throw new PlatformNotSupportedException();
            AndroidInterop.ShareURIs(_androidNative!, Environment.TickCount, uris, contentType,
                options.Text, options.Title);
        }

        #endregion
        
        #region TryShareBytes APIs
        
        /// <inheritdoc cref="TryShareBytes(int,System.ReadOnlySpan{byte},string,string,Uralstech.UShare.ShareOptions?)"/>
        /// <remarks>
        /// <para>
        /// This method creates a file for the data in a cache folder which can be accessed by the share sheet. The path to this file is
        /// returned in <see cref="EventStatus"/>. See <see cref="EventStatus.CreatedFiles"/> for more details.
        /// </para>
        /// <para>Uses an automatically generated event ID.</para>
        /// </remarks>
        public EventStatus TryShareBytes(ReadOnlySpan<byte> data, string fileName, string contentType, ShareOptions? options = null) =>
            TryShareBytes(GetEventId(), data, fileName, contentType, options);
        
        /// <inheritdoc cref="TryShareBytes(int,System.Collections.Generic.IReadOnlyList{byte[]},System.Collections.Generic.IReadOnlyList{string},string,Uralstech.UShare.ShareOptions?)"/>
        /// <remarks>
        /// <para>
        /// This method creates a file for each byte array in a cache folder which can be accessed by the share sheet. The paths to these files are
        /// returned in <see cref="EventStatus"/>. See <see cref="EventStatus.CreatedFiles"/> for more details.
        /// </para>
        /// <para>Uses an automatically generated event ID.</para>
        /// </remarks>
        public EventStatus TryShareBytes(IReadOnlyList<byte[]> data, IReadOnlyList<string> fileNames, string contentType, ShareOptions? options = null) =>
            TryShareBytes(GetEventId(), data, fileNames, contentType, options);
        
        /// <inheritdoc cref="TryShareBytes(System.Collections.Generic.IReadOnlyList{byte[]},System.Collections.Generic.IReadOnlyList{string},string,Uralstech.UShare.ShareOptions?)"/>
        public EventStatus TryShareBytes(IReadOnlyList<NativeArray<byte>.ReadOnly> data, IReadOnlyList<string> fileNames, string contentType, ShareOptions? options = null) =>
            TryShareBytes(GetEventId(), data, fileNames, contentType, options);
        
        /// <summary>Shares data to other apps using the system share sheet.</summary>
        /// <remarks>
        /// <para>
        /// This method creates a file for the data in a cache folder which can be accessed by the share sheet. The path to this file is
        /// returned in <see cref="EventStatus"/>. See <see cref="EventStatus.CreatedFiles"/> for more details.
        /// </para>
        /// <para>Duplicate IDs in successive calls may prevent reliable <see cref="OnResult"/> invocations in Android.</para>
        /// </remarks>
        /// <param name="id">The ID of this share event, returned in <see cref="OnResult"/>.</param>
        /// <param name="data">The data to share.</param>
        /// <param name="fileName">
        /// The name of the temporary file. Since this is displayed to the user, it's recommended to set a
        /// human-readable description of the data. Some platforms like iOS may even obfuscate files with non-standard extensions.
        /// </param>
        /// <param name="contentType">The MIME type of the data; see <see cref="CommonMimeTypes"/> for common MIME types.</param>
        /// <param name="options">Additional options for the event.</param>
        /// <returns>An <see cref="EventStatus"/> object with <see cref="EventStatus.Success"/> = <see langword="true"/> if the share sheet activity was presented; <see langword="false"/> otherwise.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android or iOS.</exception>
        public EventStatus TryShareBytes(int id, ReadOnlySpan<byte> data, string fileName, string contentType, ShareOptions? options = null)
        {
            if (!s_isSupported) throw new PlatformNotSupportedException();
            
            if (!TryCreateTempFile(data, fileName, out string path))
                return new EventStatus(false, Array.Empty<string>());
            
            bool success = TryShareFile(id, path, contentType, options);
            return new EventStatus(success, ArrayOf(path));
        }

        /// <summary>Shares multiple separate pieces of data to other apps using the system share sheet.</summary>
        /// <remarks>
        /// <para>
        /// This method creates a file for each byte array in a cache folder which can be accessed by the share sheet. The paths to these files are
        /// returned in <see cref="EventStatus"/>. See <see cref="EventStatus.CreatedFiles"/> for more details.
        /// </para>
        /// <para>Duplicate IDs in successive calls may prevent reliable <see cref="OnResult"/> invocations in Android.</para>
        /// </remarks>
        /// <param name="id">The ID of this share event, returned in <see cref="OnResult"/>.</param>
        /// <param name="data">The data to share.</param>
        /// <param name="fileNames">
        /// The names of the temporary files. Since these are displayed to the user, it's recommended to set
        /// human-readable descriptions of the data. Some platforms like iOS may even obfuscate files with non-standard extensions.
        /// </param>
        /// <param name="contentType">The MIME type of the combined data; see <see cref="CommonMimeTypes"/> for common MIME types.</param>
        /// <param name="options">Additional options for the event.</param>
        /// <returns>An <see cref="EventStatus"/> object with <see cref="EventStatus.Success"/> = <see langword="true"/> if the share sheet activity was presented; <see langword="false"/> otherwise.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android or iOS.</exception>
        /// <exception cref="ArgumentException">If <paramref name="fileNames"/> is a different size to <paramref name="data"/>.</exception>
        public EventStatus TryShareBytes(int id, IReadOnlyList<byte[]> data, IReadOnlyList<string> fileNames, string contentType, ShareOptions? options = null) =>
            TryShareBytes(id, data, fileNames, static data => data, contentType, options);
        
        /// <inheritdoc cref="TryShareBytes(int,System.Collections.Generic.IReadOnlyList{byte[]},System.Collections.Generic.IReadOnlyList{string},string,Uralstech.UShare.ShareOptions?)"/>
        public EventStatus TryShareBytes(int id, IReadOnlyList<NativeArray<byte>.ReadOnly> data, IReadOnlyList<string> fileNames, string contentType, ShareOptions? options = null) =>
            TryShareBytes(id, data, fileNames, static data => data, contentType, options);
        
        private delegate ReadOnlySpan<byte> SpanTransformer<in T>(T data);
        private EventStatus TryShareBytes<T>(int id, IReadOnlyList<T> data, IReadOnlyList<string> fileNames, SpanTransformer<T> transformer, string contentType, ShareOptions? options)
        {
            if (!s_isSupported) throw new PlatformNotSupportedException();
            if (fileNames.Count != data.Count)
                throw new ArgumentException($"{nameof(fileNames)} and {nameof(data)} must have the same length.", nameof(fileNames));

            int count = data.Count;
            List<string> paths = new(count);

            for (int i = 0; i < count; i++)
            {
                if (!TryCreateTempFile(transformer(data[i]), fileNames[i], out string path))
                    return new EventStatus(false, paths);
                paths.Add(path);
            }
            
            bool success = TryShareFiles(id, paths.ToArray(), contentType, options);
            return new EventStatus(success, paths);
        }
        
        #endregion

        #region TryShareImage APIs
        
        /// <summary>Equivalent to <see cref="TryShareBytes(System.ReadOnlySpan{byte},string,string,Uralstech.UShare.ShareOptions?)"/>, but falls back to <see cref="TryShareImageIOS"/> on iOS.</summary>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android or iOS.</exception>
        /// <seealso cref="TryShareBytes(System.ReadOnlySpan{byte},string,string,Uralstech.UShare.ShareOptions?)"/>
        /// <seealso cref="TryShareImageIOS"/>
        public EventStatus TryShareImage(ReadOnlySpan<byte> image, string fileName, string contentType, ShareOptions? options = null) =>
            TryShareImage(GetEventId(), image, fileName, contentType, options);
        
        /// <summary>Equivalent to <see cref="TryShareBytes(System.Collections.Generic.IReadOnlyList{byte[]},System.Collections.Generic.IReadOnlyList{string},string,Uralstech.UShare.ShareOptions?)"/>, but falls back to <see cref="TryShareImagesIOS(int,System.Collections.Generic.IReadOnlyList{byte[]},Uralstech.UShare.ShareOptions?)"/> on iOS.</summary>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android or iOS.</exception>
        /// <seealso cref="TryShareBytes(System.Collections.Generic.IReadOnlyList{byte[]},System.Collections.Generic.IReadOnlyList{string},string,Uralstech.UShare.ShareOptions?)"/>
        /// <seealso cref="TryShareImagesIOS(int,System.Collections.Generic.IReadOnlyList{byte[]},Uralstech.UShare.ShareOptions?)"/>
        public EventStatus TryShareImages(IReadOnlyList<byte[]> images, IReadOnlyList<string> fileNames, string contentType, ShareOptions? options = null) =>
            TryShareImages(GetEventId(), images, fileNames, contentType, options);
        
        /// <summary>Equivalent to <see cref="TryShareBytes(System.Collections.Generic.IReadOnlyList{Unity.Collections.NativeArray{byte}.ReadOnly},System.Collections.Generic.IReadOnlyList{string},string,Uralstech.UShare.ShareOptions?)"/>, but falls back to <see cref="TryShareImagesIOS(int,System.Collections.Generic.IReadOnlyList{Unity.Collections.NativeArray{byte}.ReadOnly},Uralstech.UShare.ShareOptions?)"/> on iOS.</summary>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android or iOS.</exception>
        /// <seealso cref="TryShareBytes(System.Collections.Generic.IReadOnlyList{Unity.Collections.NativeArray{byte}.ReadOnly},System.Collections.Generic.IReadOnlyList{string},string,Uralstech.UShare.ShareOptions?)"/>
        /// <seealso cref="TryShareImagesIOS(int,System.Collections.Generic.IReadOnlyList{Unity.Collections.NativeArray{byte}.ReadOnly},Uralstech.UShare.ShareOptions?)"/>
        public EventStatus TryShareImages(IReadOnlyList<NativeArray<byte>.ReadOnly> images, IReadOnlyList<string> fileNames, string contentType, ShareOptions? options = null) =>
            TryShareImages(GetEventId(), images, fileNames, contentType, options);
        
        /// <summary>Equivalent to <see cref="TryShareBytes(int,System.ReadOnlySpan{byte},string,string,Uralstech.UShare.ShareOptions?)"/>, but falls back to <see cref="TryShareImageIOS"/> on iOS.</summary>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android or iOS.</exception>
        /// <seealso cref="TryShareBytes(int,System.ReadOnlySpan{byte},string,string,Uralstech.UShare.ShareOptions?)"/>
        /// <seealso cref="TryShareImageIOS"/>
        public EventStatus TryShareImage(int id, ReadOnlySpan<byte> image, string fileName, string contentType,
            ShareOptions? options = null)
        {
            if (!s_isSupported) throw new PlatformNotSupportedException();
            if (s_isAndroid) return TryShareBytes(id, image, fileName, contentType, options);
            
            bool success = TryShareImageIOS(id, image, options);
            return new EventStatus(success, Array.Empty<string>());
        }
        
        /// <summary>Equivalent to <see cref="TryShareBytes(int,System.Collections.Generic.IReadOnlyList{byte[]},System.Collections.Generic.IReadOnlyList{string},string,Uralstech.UShare.ShareOptions?)"/>, but falls back to <see cref="TryShareImagesIOS(int,System.Collections.Generic.IReadOnlyList{byte[]},Uralstech.UShare.ShareOptions?)"/> on iOS.</summary>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android or iOS.</exception>
        /// <seealso cref="TryShareBytes(int,System.Collections.Generic.IReadOnlyList{byte[]},System.Collections.Generic.IReadOnlyList{string},string,Uralstech.UShare.ShareOptions?)"/>
        /// <seealso cref="TryShareImagesIOS(int,System.Collections.Generic.IReadOnlyList{byte[]},Uralstech.UShare.ShareOptions?)"/>
        public EventStatus TryShareImages(int id, IReadOnlyList<byte[]> images, IReadOnlyList<string> fileNames, string contentType,
            ShareOptions? options = null)
        {
            if (!s_isSupported) throw new PlatformNotSupportedException();
            if (s_isAndroid) return TryShareBytes(id, images, fileNames, contentType, options);
            
            bool success = TryShareImagesIOS(id, images, options);
            return new EventStatus(success, Array.Empty<string>());
        }

        /// <summary>Equivalent to <see cref="TryShareBytes(int,System.Collections.Generic.IReadOnlyList{Unity.Collections.NativeArray{byte}.ReadOnly},System.Collections.Generic.IReadOnlyList{string},string,Uralstech.UShare.ShareOptions?)"/>, but falls back to <see cref="TryShareImagesIOS(int,System.Collections.Generic.IReadOnlyList{Unity.Collections.NativeArray{byte}.ReadOnly},Uralstech.UShare.ShareOptions?)"/> on iOS.</summary>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android or iOS.</exception>
        /// <seealso cref="TryShareBytes(int,System.Collections.Generic.IReadOnlyList{Unity.Collections.NativeArray{byte}.ReadOnly},System.Collections.Generic.IReadOnlyList{string},string,Uralstech.UShare.ShareOptions?)"/>
        /// <seealso cref="TryShareImagesIOS(int,System.Collections.Generic.IReadOnlyList{Unity.Collections.NativeArray{byte}.ReadOnly},Uralstech.UShare.ShareOptions?)"/>
        public EventStatus TryShareImages(int id, IReadOnlyList<NativeArray<byte>.ReadOnly> images, IReadOnlyList<string> fileNames, string contentType,
            ShareOptions? options = null)
        {
            if (!s_isSupported) throw new PlatformNotSupportedException();
            if (s_isAndroid) return TryShareBytes(id, images, fileNames, contentType, options);
            
            bool success = TryShareImagesIOS(id, images, options);
            return new EventStatus(success, Array.Empty<string>());
        }
        
        #endregion
        
        #region Utils, P/Invokes
        
        /// <summary>Returns the default shareable directory as defined in the native plugin.</summary>
        /// <remarks>
        /// Files in this directory are not automatically deleted after sharing. Since this is a system cache location,
        /// cleanup is optional, but periodically clearing old files may help avoid unnecessary storage use.
        /// </remarks>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android or iOS.</exception>
        public string GetDefaultBasePath()
        {
            if (!s_isSupported) throw new PlatformNotSupportedException();
            return Path.Join(
                s_isIOS ? Application.temporaryCachePath : AndroidPathHelper.CacheDirectory, 
                DefaultSubDirectory
            );
        }
        
        private bool TryCreateTempFile(ReadOnlySpan<byte> data, string fileName, out string path)
        {
            path = string.Empty;
            
            try
            {
                string directory = GetDefaultBasePath();
                Directory.CreateDirectory(directory);

                path = Path.Join(directory, fileName);
                using FileStream stream = new(path, FileMode.Create, FileAccess.Write, FileShare.Read);
                stream.Write(data);

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }
        
        [AOT.MonoPInvokeCallback(typeof(IOSInterop.ActivityFinishedCallback))]
        private static async void OnActivityResult(int id)
        {
            try
            {
                await Awaitable.MainThreadAsync();
                if (!HasInstance) return;

                Instance.OnResult.Invoke(id);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        
        private static T[] ArrayOf<T>(T obj) => new[] { obj };
        
        #endregion
    }
}