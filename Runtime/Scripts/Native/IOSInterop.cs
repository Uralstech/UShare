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
using System.Runtime.InteropServices;

#nullable enable
namespace Uralstech.UShare.Native
{
    /// <summary>Interface for the native iOS plugin.</summary>
    public static class IOSInterop
    {
        /// <summary>Shares text data using UIActivityViewController.</summary>
        /// <param name="id">The ID of this event, to be returned in <paramref name="callback"/>.</param>
        /// <param name="text">The text to share.</param>
        /// <param name="title">Optional title for the sharesheet.</param>
        /// <param name="callback">Callback for when the activity has finished.
        /// Note that this is <b>not</b> an indication of the operation being completed.</param>
        /// <returns><see langword="true"/> if the view was presented; <see langword="false"/> otherwise.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than iOS.</exception>
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        public static extern bool ushare_ios_share_text(int id, [MarshalAs(UnmanagedType.LPUTF8Str)] string text,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string? title, ActivityFinishedCallback callback);
#else
        public static bool ushare_ios_share_text(int id, string text, string? title, ActivityFinishedCallback callback) => 
            throw new PlatformNotSupportedException();
#endif

        /// <summary>Shares images using UIActivityViewController.</summary>
        /// <param name="id">The ID of this event, to be returned in <paramref name="callback"/>.</param>
        /// <param name="images">The images being shared.</param>
        /// <param name="sizes">The size of each image in <paramref name="images"/>, in bytes.</param>
        /// <param name="count">Length of <paramref name="images"/> <b>and</b> <paramref name="sizes"/>.</param>
        /// <param name="text">Optional additional text to share.</param>
        /// <param name="title">Optional title for the sharesheet.</param>
        /// <param name="callback">Callback for when the activity has finished.
        /// Note that this is <b>not</b> an indication of the operation being completed.</param>
        /// <returns><see langword="true"/> if the view was presented; <see langword="false"/> otherwise.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than iOS.</exception>
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        public static extern bool ushare_ios_share_images(int id, IntPtr[] images, int[] sizes, int count,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string? text, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title,
            ActivityFinishedCallback callback);
#else
        public static bool ushare_ios_share_images(int id, IntPtr[] images, int[] sizes, int count, string? text,
            string? title, ActivityFinishedCallback callback) => throw new PlatformNotSupportedException();
#endif

        /// <summary>Shares files using UIActivityViewController.</summary>
        /// <param name="id">The ID of this event, to be returned in <paramref name="callback"/>.</param>
        /// <param name="paths">The paths to the files being shared.</param>
        /// <param name="count">Length of <paramref name="paths"/>.</param>
        /// <param name="text">Optional additional text to share.</param>
        /// <param name="title">Optional title for the sharesheet.</param>
        /// <param name="callback">Callback for when the activity has finished.
        /// Note that this is <b>not</b> an indication of the operation being completed.</param>
        /// <returns><see langword="true"/> if the view was presented; <see langword="false"/> otherwise.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than iOS.</exception>
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        public static extern bool ushare_ios_share_files(int id,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPUTF8Str)] string[] paths, int count,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string? text, [MarshalAs(UnmanagedType.LPUTF8Str)] string? title,
            ActivityFinishedCallback callback);
#else
        public static bool ushare_ios_share_files(int id, string[] paths, int count, string? text, string? title,
            ActivityFinishedCallback callback) => throw new PlatformNotSupportedException();
#endif

        public delegate void ActivityFinishedCallback(int id);
    }
}
