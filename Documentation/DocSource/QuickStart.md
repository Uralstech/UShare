# Quick Start

Please note that the code provided in this page is *purely* for learning purposes and is far from perfect. Remember to null-check all responses!

## Breaking Changes Notice

As of right now, the plugin has been written with Android support in mind. I plan to add iOS support soon, which will definitely
cause breaking changes to this plugin in the state it is now. So I don't recommend using this in production apps.

If you've just updated the package, it is recommended to check the [*changelogs*](https://github.com/Uralstech/UShare/releases) for information on breaking changes.

## `ShareSheetManager`

[`ShareSheetManager`](~/api/Uralstech.UShare.ShareSheetManager.yml) is the main class which you'll use to access the system share-sheet.

Right now, all you need to do to start sharing text is call the [`ShareText()`](~/api/Uralstech.UShare.ShareSheetManager.yml#Uralstech_UShare_ShareSheetManager_ShareText_System_String_System_String_) method.

```csharp
using Uralstech.UShare;

public void ShareText()
{
    ShareSheetManager.Instance.ShareText("Text", title: "Share it with your friends!");
}
```

The "title" parameter is optional, but it override's Android's default title for the share sheet on devices running Android 10 and higher.

### Sharing Binary Data (Android)

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

Now, to actually share data at runtime, you can call the [`ShareData()`](~/api/Uralstech.UShare.ShareSheetManager.yml#Uralstech_UShare_ShareSheetManager_ShareData_System_String_System_String_System_Byte___Uralstech_UShare_AdditionalShareData_) method, like so:

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
You also have to give a name to the file that will be created from the data.

When the share sheet is opened, the app loses focus. By default, UShare will delete all files it creates for sharing after focus
has been regained.

`ShareData` accepts a fourth, optional argument of type [`AdditionalShareData`](~/api/Uralstech.UShare.AdditionalShareData.yml).
Here, you can define additional text data to be shared with the binary data, a title for the share sheet (although it is not guaranteed to work in these cases),
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

You can also share existing files directly using UShare. Of course, these files must be declared in shareable folders as declared in `file_provider_paths.xml`.

To do so, check out the documentation for the following methods:

- [`ShareFile()`](~/api/Uralstech.UShare.ShareSheetManager.yml#Uralstech_UShare_ShareSheetManager_ShareFile_System_String_System_String_Uralstech_UShare_AdditionalShareData_)
- [`ShareFiles()`](~/api/Uralstech.UShare.ShareSheetManager.yml#Uralstech_UShare_ShareSheetManager_ShareFiles_System_String_System_String___Uralstech_UShare_AdditionalShareData_)
- [`GetShareableFileUri()`](~/api/Uralstech.UShare.ShareSheetManager.yml#Uralstech_UShare_ShareSheetManager_GetShareableFileUri_System_String_System_String_System_String_)
- [`ShareUris()`](~/api/Uralstech.UShare.ShareSheetManager.yml#Uralstech_UShare_ShareSheetManager_ShareUris_System_String_AndroidJavaObject___System_String_System_String_)