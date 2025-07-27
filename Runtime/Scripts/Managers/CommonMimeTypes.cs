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
        public const string Multitype       = "*/*";

        public const string TextMultitype   = "text/*";
        public const string TextPlain       = "text/plain";
        public const string TextRtf         = "text/rtf";
        public const string TextHtml        = "text/html";
        public const string TextJson        = "text/json";

        public const string ImageMultitype  = "image/*";
        public const string ImageJpg        = "image/jpg";
        public const string ImagePng        = "image/png";
        public const string ImageGif        = "image/gif";

        public const string VideoMultitype  = "video/*";
        public const string VideoMp4        = "video/mp4";
        public const string Video3gp        = "video/3gp";

        public const string ApplicationPdf  = "application/pdf";
    }
}
