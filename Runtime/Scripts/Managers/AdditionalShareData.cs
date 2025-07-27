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

#nullable enable
namespace Uralstech.UShare
{
    /// <summary>
    /// Additional data regarding a share event.
    /// </summary>
    public struct AdditionalShareData
    {
        /// <summary>
        /// Override the path to the directory containing the file(s) to share.
        /// </summary>
        public string? BasePath;

        /// <summary>
        /// (Android) Override the FileProvider authority for the file(s) being shared.
        /// </summary>
        public string? AndroidFileProviderAuthority;

        /// <summary>
        /// Additional text to be shared along with the main content.
        /// </summary>
        /// <remarks>
        /// You can usually get away with having text in shared content along with other data
        /// <b>without</b> having to declare it in the MIME type you provide for the request.
        /// For example, most apps will accept "image/png" share requests with additional text,
        /// but WhatsApp will send each image as individual messages with the text attached to
        /// <i>every</i> message.
        /// </remarks>
        public string? AdditionalText;

        /// <summary>
        /// Optional title for the share sheet (Android 10+, but not guaranteed to work when sharing non-text media).
        /// </summary>
        public string? Title;

        /// <summary>
        /// Whether to keep the file(s) after the user regains focus on the app after sharing.
        /// This only applies to share events where you provide the raw data to be shared,
        /// like with <see cref="ShareSheetManager.ShareData(string, string, byte[], AdditionalShareData)"/>,
        /// and not for cases where the data is already present in the app's storage.
        /// </summary>
        /// <remarks>
        /// By default, the data is deleted from the app's storage when the user
        /// regains focus on the app after sharing to prevent the share directory
        /// from getting too big.
        /// </remarks>
        public bool KeepDataAfterFocusRegain;
    }
}
