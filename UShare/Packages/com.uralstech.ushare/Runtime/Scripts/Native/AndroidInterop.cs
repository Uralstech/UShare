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
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Android;

#nullable enable
namespace Uralstech.UShare.Native
{
    /// <summary>Interface for the native Android plugin.</summary>
    public static class AndroidInterop
    {
        /// <summary>Handles callbacks from the native interface.</summary>
        public sealed class Callbacks : AndroidJavaProxy
        {
            private const string ClassName = "com.uralstech.ushare.ShareInterface$Callbacks";

            /// <summary>Called when the user has chosen a target to share to, along with the event ID.</summary>
            /// <remarks>
            /// <para>
            /// This callback indicates that a target application was selected.
            /// It does not guarantee that the shared content was successfully
            /// received or processed by the target application.
            /// </para>
            /// <para>
            /// This callback is invoked from an Android BroadcastReceiver.
            /// Do not use Unity APIs unless switching back to the main thread.
            /// </para>
            /// <para>
            /// This callback may not be invoked for all share targets.
            /// For example, it is not triggered by "Copy to clipboard" on Meta Quest.
            /// </para>
            /// </remarks>
            public event Action<int>? OnTargetChosen;
            
            /// <exception cref="PlatformNotSupportedException">Thrown if this constructor is called on a runtime other than Android.</exception>
#if UNITY_ANDROID && !UNITY_EDITOR
            public Callbacks() : base(ClassName) { }
#else
            public Callbacks() : base((AndroidJavaClass)null!)
            {
                throw new PlatformNotSupportedException();
            }
#endif

            [UnityEngine.Scripting.Preserve]
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            private void onTargetChosen(int id)
            {
                try
                {
                    OnTargetChosen?.Invoke(id);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
        
        /// <summary>Gets an instance of the native interface.</summary>
        /// <remarks>
        /// <para>
        /// This method should generally be called only once during the application's lifetime,
        /// as the native object is supposed to act like a singleton. If you create a new instance
        /// of the native object while another one is active, all unfulfilled callbacks meant for
        /// the existing instance will be forwarded to the new instance's <paramref name="callbacks"/> instead.
        /// </para>
        /// <para>
        /// The returned <see cref="AndroidJavaObject"/> should be disposed when no longer needed.
        /// </para>
        /// </remarks>
        /// <returns>A new instance of the native interface.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
        public static AndroidJavaObject CreateInstance(Callbacks callbacks)
        {
            ThrowIfNotAndroid();
            
            const string ClassName = "com.uralstech.ushare.ShareInterface";
            using AndroidJavaClass nativeInterfaceClass = new(ClassName);
            
            return nativeInterfaceClass.CallStatic<AndroidJavaObject>("createInstance",
                AndroidApplication.currentContext, callbacks);
        }

        /// <summary>Shares text data using ChooserActivity.</summary>
        /// <param name="native">The native plugin instance.</param>
        /// <param name="id">The ID of this event, to be returned in <see cref="Callbacks.OnTargetChosen"/>.</param>
        /// <param name="text">The text to share.</param>
        /// <param name="title">Optional title for the sharesheet.</param>
        /// <returns><see langword="true"/> if the activity was started; <see langword="false"/> otherwise.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
        public static bool ShareText(AndroidJavaObject native, int id, string text, string? title)
        {
            ThrowIfNotAndroid();
            return native.Call<bool>("shareText", id, text, title);
        }

        /// <summary>Shares data sourced from a URI using ChooserActivity.</summary>
        /// <param name="native">The native plugin instance.</param>
        /// <param name="id">The ID of this event, to be returned in <see cref="Callbacks.OnTargetChosen"/>.</param>
        /// <param name="uri">The <c>android.net.Uri</c> object to share.</param>
        /// <param name="type">The MIME type of the content being shared through the <paramref name="uri"/>.</param>
        /// <param name="text">Optional additional text to share.</param>
        /// <param name="title">Optional title for the sharesheet.</param>
        /// <returns><see langword="true"/> if the activity was started; <see langword="false"/> otherwise.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
        public static bool ShareURI(AndroidJavaObject native, int id, AndroidJavaObject uri, string type, string? text, string? title)
        {
            ThrowIfNotAndroid();
            return native.Call<bool>("shareUri", id, uri, type, text, title);
        }
        
        /// <summary>Shares multipart data sourced from URIs using ChooserActivity.</summary>
        /// <param name="native">The native plugin instance.</param>
        /// <param name="id">The ID of this event, to be returned in <see cref="Callbacks.OnTargetChosen"/>.</param>
        /// <param name="uris">The array of <c>android.net.Uri</c> objects to share.</param>
        /// <param name="type">The MIME type of the contents being shared through the <paramref name="uris"/>.</param>
        /// <param name="text">Optional additional text to share.</param>
        /// <param name="title">Optional title for the sharesheet.</param>
        /// <returns><see langword="true"/> if the activity was started; <see langword="false"/> otherwise.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
        public static bool ShareURIs(AndroidJavaObject native, int id, AndroidJavaObject[] uris, string type, string? text, string? title)
        {
            ThrowIfNotAndroid();
            return native.Call<bool>("shareUris", id, uris, type, text, title);
        }

        /// <summary>
        /// Creates a shareable <c>android.net.Uri</c> from a filepath under the given
        /// <a href="https://developer.android.com/reference/androidx/core/content/FileProvider"><c>FileProvider</c></a> authority's scope.
        /// </summary>
        /// <remarks>The returned <see cref="AndroidJavaObject"/> should be disposed when no longer needed.</remarks>
        /// <param name="native">The native plugin instance.</param>
        /// <param name="path">The filepath.</param>
        /// <param name="authority">The <c>FileProvider</c> authority which <paramref name="path"/> is under.</param>
        /// <returns>A shareable <c>android.net.Uri</c> object if successful; <see langword="null"/> otherwise.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
        public static AndroidJavaObject? GetFileURI(AndroidJavaObject native, string path, string authority)
        {
            ThrowIfNotAndroid();
            return native.Call<AndroidJavaObject>("getFileUri", path, authority);
        }
        
        private static void ThrowIfNotAndroid()
        {
            if (Application.platform != RuntimePlatform.Android)
                throw new PlatformNotSupportedException();
        }
    }
}