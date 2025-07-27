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
    /// Contains constants for common MIME types used in sharing data.
    /// </summary>
    public static class CommonMimeTypes
    { 
        /// <summary>
        /// Use this when sharing data of multiple unrelated types.
        /// </summary>
        public const string Multitype       = "*/*";

        /// <summary>
        /// Use this when sharing text of multiple types.
        /// </summary>
        public const string TextMultitype   = "text/*";

        /// <summary>
        /// Plain text.
        /// </summary>
        public const string TextPlain       = "text/plain";

        /// <summary>
        /// RTF files.
        /// </summary>
        public const string TextRtf         = "text/rtf";

        /// <summary>
        /// HTML files.
        /// </summary>
        public const string TextHtml        = "text/html";

        /// <summary>
        /// JSON files.
        /// </summary>
        public const string TextJson        = "text/json";

        /// <summary>
        /// Use this when sharing images of multiple types.
        /// </summary>
        public const string ImageMultitype  = "image/*";

        /// <summary>
        /// JPG/JPEG images.
        /// </summary>
        public const string ImageJpg        = "image/jpg";

        /// <summary>
        /// PNG images.
        /// </summary>
        public const string ImagePng        = "image/png";

        /// <summary>
        /// GIF images/videos.
        /// </summary>
        public const string ImageGif        = "image/gif";

        /// <summary>
        /// Use this when sharing videos of multiple types.
        /// </summary>
        public const string VideoMultitype  = "video/*";

        /// <summary>
        /// MP4 videos.
        /// </summary>
        public const string VideoMp4        = "video/mp4";

        /// <summary>
        /// 3GP videos.
        /// </summary>
        public const string Video3gp        = "video/3gp";

        /// <summary>
        /// PDF files.
        /// </summary>
        public const string ApplicationPdf  = "application/pdf";
    }
}
