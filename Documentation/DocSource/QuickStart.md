# Quick Start

Please note that the example code provided in this page is *purely* for learning purposes and is far from perfect.

## Breaking Changes Notice

This plugin supports Android 6+ (API Level 23) and iOS 14+.
If you've just updated the package, it is recommended to check the [*changelogs*](https://github.com/Uralstech/UShare/releases) for information on breaking changes.

## `ShareSheetManager`

[`ShareSheetManager`](~/api/Uralstech.UShare.ShareSheetManager.yml) is the main class which you'll use to access the system share-sheet.

All you need to do to start sharing text is call the [`ShareText()`](~/api/Uralstech.UShare.ShareSheetManager.yml#Uralstech_UShare_ShareSheetManager_ShareText_System_String_System_String_) method.

```csharp
using Uralstech.UShare;

public void ShareText()
{
    ShareSheetManager.Instance.ShareText("Text", title: "Share it with your friends!");
}
```

The "title" parameter is optional, but it overrides the default title for the share sheet on Android 10+ and iOS.
On iOS, if a title is not provided, the plugin will use the text data as the title.

### Sharing Binary Data

To share data at runtime, you can call the [`ShareData()`](~/api/Uralstech.UShare.ShareSheetManager.yml#Uralstech_UShare_ShareSheetManager_ShareData_System_String_System_String_System_Byte___Uralstech_UShare_AdditionalShareData_) method, like so:

```csharp
using Uralstech.UShare;

public void ShareImage()
{
    // The following creates a 512x512 image with randomly generated colors for each pixel.
    Texture2D image = new(512, 512);
    Color32[] colors = new Color32[512*512];

    for (int i = colors.Length - 1; i >= 0; i--)
        colors[i] = Random.ColorHSV();

    image.SetPixels32(colors);

    // This line shares it.
    ShareSheetManager.Instance.ShareData(CommonMimeTypes.ImageJpg, "test.jpg", image.EncodeToJPG());
}
```

As you can see, `ShareData` expects a MIME type declaring the type of the data being shared. The data itself has to be a `byte[]`.
For most content, the method will create a temporary file with the given name ("test.jpg" here) and share the URI to it. On iOS,
for image content, the plugin can skip this part and share the data directly. This optimization **requires** that the content type
start with "images/".

On Android, when the share sheet is opened, the app loses focus. By default, UShare will delete all files it creates for sharing after focus has been regained. On iOS, UShare will immediately delete created files after the user is done with the share sheet.

`ShareData` accepts a fourth, optional argument of type [`AdditionalShareData`](~/api/Uralstech.UShare.AdditionalShareData.yml).
Here, you can define additional text data to be shared with the binary data, a title for the share sheet (not guaranteed to work on Android in this case),
tell UShare not to delete the file when regaining focus, etc. Please check the reference documentation for the type to learn more.

### Sharing Multiple Pieces Of Data

You can share multiple pieces of data using [a variant of the `ShareData` method](~/api/Uralstech.UShare.ShareSheetManager.yml#Uralstech_UShare_ShareSheetManager_ShareData_System_String_System_ValueTuple_System_String_System_Byte______Uralstech_UShare_AdditionalShareData_), like so:

```csharp
using System.Text;
using Uralstech.UShare;

public void ShareMultiple()
{
    byte[] textPart1 = Encoding.UTF8.GetBytes("This is considered as a file being shared.");
    byte[] textPart2 = Encoding.UTF8.GetBytes("This is also considered as a file being shared.");

    ShareSheetManager.Instance.ShareData(CommonMimeTypes.TextPlain, new (string, byte[])[]
    {
        ("text_1.txt", textPart1), // Don't forget to give unique names for each part being shared!
        ("text_2.txt", textPart2),
    });
}
```

### Sharing Files Directly

You can also share existing files directly using UShare. For Android, these files must be declared in shareable folders as declared in `file_provider_paths.xml`.

The following methods work on both Android and iOS, but all shared files *must* be from the same directory:

- [`ShareFile()`](~/api/Uralstech.UShare.ShareSheetManager.yml#Uralstech_UShare_ShareSheetManager_ShareFile_System_String_System_String_Uralstech_UShare_AdditionalShareData_)
- [`ShareFiles()`](~/api/Uralstech.UShare.ShareSheetManager.yml#Uralstech_UShare_ShareSheetManager_ShareFiles_System_String_System_String___Uralstech_UShare_AdditionalShareData_)

The following are specific to Android, and allow you to generate and share native file URIs directly:

- [`GetShareableFileUri()`](~/api/Uralstech.UShare.ShareSheetManager.yml#Uralstech_UShare_ShareSheetManager_GetShareableFileUri_System_String_System_String_System_String_)
- [`ShareUris()`](~/api/Uralstech.UShare.ShareSheetManager.yml#Uralstech_UShare_ShareSheetManager_ShareUris_System_String_AndroidJavaObject___System_String_System_String_)

### iOS Photos App Permission

If you want your users to be able to save shared images directly to their Photos app, you will have to declare [`NSPhotoLibraryAddUsageDescription`](https://developer.apple.com/documentation/BundleResources/Information-Property-List/NSPhotoLibraryAddUsageDescription) for your generated XCode project.
UShare doesn't do this by default, but you can enable the editor script that will patch the generated XCode project's Info.plist
file with the permission by enabling Project Settings -> UShare Settings -> iOS Settings -> Patch Info.plist for iOS.
You can also change the default usage description in the same page.

### How Sharing Binary Data Works On Android

When sharing binary data, UShare saves it to a temporary location in the user's device that has been declared as accessible to other applications.
Android's service for doing so is [*FileProvider*](https://developer.android.com/reference/androidx/core/content/FileProvider). By default, UShare
patches your build's `AndroidManifest.xml` with the following additional data:

```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
    <application>
        ...

        <provider android:name="androidx.core.content.FileProvider"
                  android:authorities="[Your app's ID].FileProvider"
                  android:grantUriPermissions="true"
                  android:exported="false">
            <meta-data android:name="android.support.FILE_PROVIDER_PATHS"
                       android:resource="@xml/file_provider_paths" />
		</provider>
    </application>

    ...
</manifest>
```

The `provider` tag declares that `FileProvider` is being used by your app. `"[Your app's ID].FileProvider"` is the authority that Android will use
to create shareable URIs to the files you want to share. The `file_provider_paths` XML file referenced here declares folders which will contain
files to be shared from your app's storage. By default, UShare patches the following XML file as `file_provider_paths.xml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<paths>
    <cache-path name="shared_data" path="ShareCache/" />
</paths>
```

This declares that the folder `ShareCache` under the app's [*cache folder*](https://developer.android.com/reference/android/content/Context.html#getCacheDir())
will contain files that will be shared to other apps. "shared_data" is the alias which will be exposed to the other apps as part of the URI.

This is an example of a shareable URI that would be generated from the above configs:

```
content://com.yourcompany.yourapp.fileprovider/shared_data/image.jpg
```
