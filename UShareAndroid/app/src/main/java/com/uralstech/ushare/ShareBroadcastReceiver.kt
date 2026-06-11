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

import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.util.Log

class ShareBroadcastReceiver : BroadcastReceiver() {

    override fun onReceive(context: Context, intent: Intent) {

        if (!intent.hasExtra(ShareInterface.SHARE_PI_CALLBACK_ID)) {
            Log.e(ShareInterface.TAG, "Broadcasted intent doesn't have callback ID!")
            return
        }

        val callbackId = intent.getIntExtra(ShareInterface.SHARE_PI_CALLBACK_ID, 0)
        val instance = ShareInterface.instance.get()

        if (instance == null) {
            Log.i(ShareInterface.TAG, "Missed callback (ID: $callbackId): no active ShareInterface instance.")
            return
        }

        instance.callbacks.onTargetChosen(callbackId)
        Log.i(ShareInterface.TAG, "Share activity completed with choice.")
    }

    // Workaround for the older androidx version used by Unity.
    // private fun <T: Parcelable> getParcelableExtra(intent: Intent, name: String, clazz: Class<T>) : T? {
    //     return if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
    //         intent.getParcelableExtra(name, clazz)
    //     } else {
    //         @Suppress("DEPRECATION")
    //         intent.getParcelableExtra(name) as T?
    //     }
    // }
}