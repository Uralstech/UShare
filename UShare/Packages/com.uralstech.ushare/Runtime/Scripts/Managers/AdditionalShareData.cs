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
    /// <summary>Additional data for a share event.</summary>
    public struct AdditionalShareData
    {
        /// <summary>Overrides the path to the directory containing the file(s) to share.</summary>
        public string? BasePath;

        /// <summary>Override the Android FileProvider authority for the file(s) being shared.</summary>
        public string? AndroidFileProviderAuthority;

        /// <summary>Additional text to be shared along with the main content.</summary>
        /// <remarks>
        /// You can usually get away with having text in shared content along with other data
        /// <b>without</b> having to declare it in the MIME type you provide for the request.
        /// For example, most apps will accept "image/png" share requests with additional text,
        /// but WhatsApp will send each image as individual messages with the text attached to
        /// <i>every</i> message.
        /// </remarks>
        public string? AdditionalText;

        /// <summary>Optional title for the share sheet.</summary>
        /// <remarks>
        /// For Android:<br/>
        /// Requires Android 10+ and is not guaranteed to work when sharing non-text media
        /// </remarks>
        public string? Title;
    }
}
