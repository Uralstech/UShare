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

package com.uralstech.ushare

import android.app.PendingIntent
import android.content.Context
import android.content.Intent
import android.net.Uri
import android.util.Log
import androidx.core.content.FileProvider
import java.io.File
import java.lang.ref.WeakReference

class ShareInterface private constructor(private val context: Context, internal val callbacks: Callbacks) {

    companion object {
        internal const val TAG = "UShare.Native"
        internal const val SHARE_PI_CALLBACK_ID = "callbackId"

        internal var instance: WeakReference<ShareInterface> = WeakReference(null)

        @JvmStatic
        fun createInstance(context: Context, callbacks: Callbacks) : ShareInterface {

            if (instance.get() != null) {
                Log.w(TAG, "Creating new ShareInterface instance due to getInstance() call. " +
                        "This will redirect all previous unfulfilled callbacks to the new caller.")
            }

            return ShareInterface(context, callbacks).also {
                instance = WeakReference(it)
            }
        }
    }

    interface Callbacks {
        fun onTargetChosen(id: Int)
    }

    fun shareText(id: Int, text: String?, title: String?) : Boolean {

        if (text == null) {
            Log.e(TAG, "Expected non-NULL string for text.")
            return false
        }

        val dataIntent = Intent().apply {
            setAction(Intent.ACTION_SEND)
            setType("text/plain")

            addTextAndTitle(this, text, title)
        }

        return shareData(id, dataIntent)
    }

    fun shareUri(id: Int, uri: Uri?, type: String?,
                  text: String?, title: String?) : Boolean {

        if (uri == null) {
            Log.e(TAG, "Expected non-NULL Uri object for uri.")
            return false
        }

        if (type.isNullOrBlank()) {
            Log.e(TAG, "Expected valid, non-NULL string for type.")
            return false
        }

        val dataIntent = Intent().apply {
            setAction(Intent.ACTION_SEND)
            setType(type)

            putExtra(Intent.EXTRA_STREAM, uri)
            addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION)
            addTextAndTitle(this, text, title)
        }

        return shareData(id, dataIntent)
    }

    fun shareUris(id: Int, uris: Array<Uri>?, type: String?,
                  text: String?, title: String?) : Boolean {

        if (uris.isNullOrEmpty()) {
            Log.e(TAG, "Expected non-NULL Array<Uri> object with size > 0 for uris.")
            return false
        }

        if (type.isNullOrBlank()) {
            Log.e(TAG, "Expected valid, non-NULL string for type.")
            return false
        }

        val dataIntent = Intent().apply {
            setAction(Intent.ACTION_SEND_MULTIPLE)
            setType(type)

            putParcelableArrayListExtra(Intent.EXTRA_STREAM, uris.toCollection(ArrayList()))
            addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION)
            addTextAndTitle(this, text, title)
        }

        return shareData(id, dataIntent)
    }

    fun getFileUri(path: String?, authority: String?) : Uri? {

        if (path.isNullOrBlank() || authority.isNullOrBlank()) {
            Log.e(TAG, "Expected valid, non-NULL strings for path and authority.")
            return null
        }

        try {

            val file = File(path)
            val uri = FileProvider.getUriForFile(context, authority, file)

            Log.i(TAG, "Created shareable URI for file.")
            return uri
        } catch (ex: IllegalArgumentException) {

            Log.e(TAG, "Expected path to be in scope supported by provided authority.", ex)
            return null
        }
    }

    private fun addTextAndTitle(intent: Intent, text: String?, title: String?) {

        if (text != null) {
            intent.putExtra(Intent.EXTRA_TEXT, text)
        }

        if (title != null) {
            intent.putExtra(Intent.EXTRA_TITLE, title)
        }
    }

    private fun shareData(id: Int, dataIntent: Intent) : Boolean {

        val callbackIntent = PendingIntent.getBroadcast(
            context, id,
            Intent(context, ShareBroadcastReceiver::class.java).apply {
                putExtra(SHARE_PI_CALLBACK_ID, id)
            },
            PendingIntent.FLAG_MUTABLE or PendingIntent.FLAG_UPDATE_CURRENT
        )

        val shareIntent = Intent.createChooser(
            dataIntent, null,
            callbackIntent.intentSender
        )

        try {
            context.startActivity(shareIntent)
            Log.i(TAG, "Activity started.")

            return true
        } catch (ex: RuntimeException) {
            Log.e(TAG, "Could not start activity due to exception.", ex)
            return false
        }
    }
}