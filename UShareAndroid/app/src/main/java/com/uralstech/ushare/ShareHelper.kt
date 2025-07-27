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

package com.uralstech.ushare

import android.content.Context
import android.content.Intent
import android.net.Uri
import android.os.Build
import android.util.Log
import androidx.core.content.FileProvider
import java.io.File
import java.lang.ref.WeakReference

/** A helper class for Android's share sheet functions. */
class ShareHelper(private val context: Context) {
    companion object {
        private const val  LOGGING_TAG = "UShare_ShareHelper"

        private var Instance: WeakReference<ShareHelper> = WeakReference(null)

        /** Gets the static instance of [ShareHelper]. */
        @JvmStatic
        fun getInstance(context: Context): ShareHelper {
            val instance = Instance.get()
            return if (instance != null) {
                instance
            } else {
                val newInstance = ShareHelper(context)
                Instance = WeakReference(newInstance)

                newInstance
            }
        }
    }

    /** Util method to call [Context.getCacheDir]. */
    fun getCacheDirPath(): String {
        return context.cacheDir.absolutePath
    }

    /** Util method to call [Context.getExternalCacheDir]. */
    fun getExternalCacheDirPath(): String? {
        return context.externalCacheDir?.absolutePath
    }

    /** Util method to call [Context.getFilesDir]. */
    fun getFilesDirPath(): String {
        return context.filesDir.absolutePath
    }

    /** Util method to call [Context.getExternalFilesDir]. */
    fun getExternalFilesDirPath(): String? {
        return context.getExternalFilesDir(null)?.absolutePath
    }

    /**
     * Shares text to other apps using Android's share sheet.
     * @param text The text to share.
     * @param title Optional title for the share sheet (Android 10+).
     */
    fun shareText(text: String, title: String?) {
        Log.i(LOGGING_TAG, "Sharing text content.")

        val shareIntent = Intent.createChooser(Intent().apply {
            action = Intent.ACTION_SEND
            type = "text/plain"

            putExtra(Intent.EXTRA_TEXT, text)
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q && title != null) {
                putExtra(Intent.EXTRA_TITLE, title)
            }
        }, null)

        context.startActivity(shareIntent)
    }

    /**
     * Shares a file to other apps using Android's share sheet.
     *
     * @param contentType The MIME type of the content shared in this action.
     * @param fileName The name of the file to share.
     * @param fileProviderAuthority The FileProvider URI authority the application has set in its manifest to use for this share action.
     * @param basePath The path to the directory containing the file to share. Defaults to "(cacheDir)/ShareCache" if not given.
     * @param textContent Optional text content to share.
     * @param title Optional title for the share sheet (Android 10+).
     * @return true if all files could be shared successfully, false otherwise.
     */
    fun shareFile(
        contentType: String,
        fileName: String,
        fileProviderAuthority: String,
        basePath: String?,
        textContent: String?,
        title: String?) : Boolean {
        Log.i(LOGGING_TAG, "Sharing file.")

        val fileUri = getShareableFileUri(fileName, fileProviderAuthority, basePath)
        if (fileUri == null) {
            return false
        }

        val shareIntent = Intent.createChooser(Intent().apply {
            action = Intent.ACTION_SEND
            type = contentType

            putExtra(Intent.EXTRA_STREAM, fileUri)
            addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION)

            if (textContent != null) {
                putExtra(Intent.EXTRA_TEXT, textContent)
            }

            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q && title != null) {
                putExtra(Intent.EXTRA_TITLE, title)
            }
        }, null)

        context.startActivity(shareIntent)
        return true
    }

    /**
     * Shares multiple files to other apps using Android's share sheet.
     * This method doesn't support sharing files from different directories.
     *
     * @param contentType The MIME type of the content shared in this action.
     * @param fileNames The names of the files to share.
     * @param fileProviderAuthority The FileProvider URI authority the application has set in its manifest to use for this share action.
     * @param basePath The path to the directory containing the file to share. Defaults to "(cacheDir)/ShareCache" if not given.
     * @param textContent Optional text content to share.
     * @param title Optional title for the share sheet (Android 10+).
     * @return true if all files could be shared successfully, false otherwise.
     */
    fun shareFiles(
        contentType: String,
        fileNames: Array<String>,
        fileProviderAuthority: String,
        basePath: String?,
        textContent: String?,
        title: String?) : Boolean {
        Log.i(LOGGING_TAG, "Setting up FileProvider URIs for sharing files.")

        val baseDir = if (basePath != null) File(basePath) else File(context.cacheDir, "ShareCache")
        val fileUris = ArrayList<Uri>(fileNames.size)

        for (fileName in fileNames) {
            val file = File(baseDir, fileName)

            try {
                fileUris.add(FileProvider.getUriForFile(context, fileProviderAuthority, file))
            } catch (ex: IllegalArgumentException) {
                Log.e(LOGGING_TAG, "Could not create shareable file URI for file $fileName due to exception.", ex)
                return false
            }
        }

        shareUris(contentType, fileUris, textContent, title)
        return true
    }

    /**
     * Shares multiple pieces of content to other apps using Android's share sheet.
     * This method expects share-ready file URIs from FileProvider. Use [getShareableFileUri] to generate them.
     *
     * @param contentType The MIME type of the content shared in this action.
     * @param fileUris The shareable URIs of the files to share generated by FileProvider.
     * @param textContent Optional text content to share.
     * @param title Optional title for the share sheet (Android 10+).
     */
    fun shareUris(contentType: String, fileUris: Array<Uri>, textContent: String?, title: String?) {
        shareUris(contentType, ArrayList(fileUris.asList()), textContent, title)
    }

    /**
     * Shares multiple pieces of content to other apps using Android's share sheet.
     * This method expects share-ready file URIs from FileProvider. Use [getShareableFileUri] to generate them.
     *
     * @param contentType The MIME type of the content shared in this action.
     * @param fileUris The shareable URIs of the files to share generated by FileProvider.
     * @param textContent Optional text content to share.
     * @param title Optional title for the share sheet (Android 10+).
     */
    private fun shareUris(contentType: String, fileUris: ArrayList<Uri>, textContent: String?, title: String?) {
        Log.i(LOGGING_TAG, "Sharing multiple files.")

        val shareIntent = Intent.createChooser(Intent().apply {
            action = Intent.ACTION_SEND_MULTIPLE
            type = contentType

            putParcelableArrayListExtra(Intent.EXTRA_STREAM, fileUris)
            addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION)

            if (textContent != null) {
                putExtra(Intent.EXTRA_TEXT, textContent)
            }

            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q && title != null) {
                putExtra(Intent.EXTRA_TITLE, title)
            }
        }, null)

        context.startActivity(shareIntent)
    }

    /**
     * Uses FileProvider to generate a shareable URI to a file.
     *
     * @param fileName The name of the file.
     * @param fileProviderAuthority The FileProvider URI authority the application has set in its manifest to use to generate the URI.
     * @param basePath The path to the directory containing the file to share. Defaults to "(cacheDir)/ShareCache" if not given.
     */
    fun getShareableFileUri(fileName: String, fileProviderAuthority: String, basePath: String?) : Uri? {
        Log.i(LOGGING_TAG, "Generating shareable URI for file.")

        val baseDir = if (basePath != null) File(basePath) else File(context.cacheDir, "ShareCache")
        val file = File(baseDir, fileName)

        try {
            return FileProvider.getUriForFile(context, fileProviderAuthority, file)
        } catch (ex: IllegalArgumentException) {
            Log.e(LOGGING_TAG, "Could not create shareable file URI for file $fileName due to exception.", ex)
            return null
        }
    }
}