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

using System;
using System.Runtime.InteropServices;

#nullable enable
namespace Uralstech.UShare
{
    /// <summary>
    /// An interface for the native iOS plugin.
    /// </summary>
    public static class IOSNativeCalls
    {
        /// <summary>
        /// Shares text data using iOS's UIActivityViewController.
        /// </summary>
        /// <param name="text">The text to share.</param>
        /// <param name="title">An optional title for the share sheet.</param>
        /// <returns>If the share request was executed successfully.</returns>
        [DllImport("__Internal")]
        public static extern bool ushare_interface_share_text(string text, string? title);

        /// <summary>
        /// Shares an image using iOS's UIActivityViewController.
        /// </summary>
        /// <param name="imagePtr">A pointer to the image's data.</param>
        /// <param name="size">The size of the image in bytes.</param>
        /// <param name="additionalText">Optional text to share along with the image.</param>
        /// <param name="title">An optional title for the share sheet.</param>
        /// <returns>If the share request was executed successfully.</returns>
        [DllImport("__Internal")]
        public static extern bool ushare_interface_share_image(IntPtr imagePtr, int size, string? additionalText, string? title);

        /// <summary>
        /// Shares multiple images using iOS's UIActivityViewController.
        /// </summary>
        /// <param name="imagePtrs">An array of pointers to each image's data.</param>
        /// <param name="sizes">An array of the sizes of each image's data in bytes.</param>
        /// <param name="count">The total number of images to be shared.</param>
        /// <param name="additionalText">Optional text to share along with the images.</param>
        /// <param name="title">An optional title for the share sheet.</param>
        /// <returns>If the share request was executed successfully.</returns>
        [DllImport("__Internal")]
        public static extern bool ushare_interface_share_images(IntPtr[] imagePtrs, int[] sizes, int count, string? additionalText, string? title);
    }
}

#endif