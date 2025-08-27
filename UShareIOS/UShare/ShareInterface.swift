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

import UIKit
import LinkPresentation

@_cdecl("ushare_interface_share_text")
public func shareText(textPtr: UnsafePointer<CChar>?, titlePtr: UnsafePointer<CChar>?) -> Bool {
    logger.log("Sharing text.")
    guard let textPtr = textPtr else {
        logger.error("Text was nil.")
        return false
    }

    let text = String(cString: textPtr)
    let title = titlePtr.map({ String(cString: $0) })
    return shareData(data: [ShareableText(text: text, title: title)]);
}

@_cdecl("ushare_interface_share_file")
public func shareFile(
    timestamp: Int64,
    filePathPtr: UnsafePointer<CChar>?,
    textPtr: UnsafePointer<CChar>?,
    titlePtr: UnsafePointer<CChar>?,
    onDoneCallback: @convention(c) @escaping (Int64) -> Void
) -> Bool {
    logger.log("Sharing file.")
    guard let filePathPtr = filePathPtr else {
        logger.error("File path was nil.")
        onDoneCallback(timestamp)
        return false
    }
    
    let title = titlePtr.map { String(cString: $0) }
    let text = textPtr.map({ String(cString: $0) })
    let filePath = String(cString: filePathPtr)
    
    let callback = { onDoneCallback(timestamp) }
    if let image = UIImage(contentsOfFile: filePath) {
        let shareableImage = ShareableImage(image: image, title: title, text: text)
        guard let text = text else {
            return shareData(data: [shareableImage], onDone: callback)
        }
        
        return shareData(data: [shareableImage, text], onDone: callback)
    }
    
    let shareableUri = ShareableUri(uri: URL(fileURLWithPath: filePath), title: title ?? text)
    guard let text = text else {
        return shareData(data: [shareableUri], onDone: callback)
    }
    
    return shareData(data: [shareableUri, text], onDone: callback)
}

@_cdecl("ushare_interface_share_files")
public func shareFiles(
    timestamp: Int64,
    filePathsPtr: UnsafePointer<UnsafePointer<CChar>?>?,
    count: Int32,
    textPtr: UnsafePointer<CChar>?,
    titlePtr: UnsafePointer<CChar>?,
    onDoneCallback: @convention(c) @escaping (Int64) -> Void
) -> Bool {
    logger.log("Sharing files.")
    guard let filePathsPtr = filePathsPtr else {
        logger.error("File paths were nil.")
        onDoneCallback(timestamp)
        return false
    }
    
    let title = titlePtr.map { String(cString: $0) }
    let text = textPtr.map({ String(cString: $0) })
    let callback = { onDoneCallback(timestamp) }
    
    var sharedContent: [Any] = []
    for i in 0..<Int(count) {
        guard let filePathPtr = filePathsPtr[i] else {
            logger.error("nil file path encountered in array.")
            onDoneCallback(timestamp)
            return false
        }
        
        let filePath = String(cString: filePathPtr)
        let image = UIImage(contentsOfFile: filePath)
        
        if i == 0 {
            if let image = image {
                sharedContent.append(ShareableImage(image: image, title: title, text: text))
            } else {
                sharedContent.append(ShareableUri(uri: URL(fileURLWithPath: filePath), title: title))
            }
        } else if let image = image {
            sharedContent.append(image)
        } else {
            sharedContent.append(URL(fileURLWithPath: filePath))
        }
    }
    
    if let text = text {
        sharedContent.append(text)
    }
    
    return shareData(data: sharedContent, onDone: callback)
}

@_cdecl("ushare_interface_share_image")
public func shareImage(imagePtr: UnsafeMutableRawPointer?, size: Int32, textPtr: UnsafePointer<CChar>?, titlePtr: UnsafePointer<CChar>?) -> Bool {
    logger.log("Sharing image.")
    guard let imagePtr = imagePtr else {
        logger.log("Image was nil.")
        return false;
    }
    
    let data = Data(bytes: imagePtr, count: Int(size))
    guard let image = UIImage(data: data) else {
        logger.error("Failed to create UIImage from provided data.")
        return false
    }
    
    let title = titlePtr.map { String(cString: $0) }
    let text = textPtr.map { String(cString: $0) }
    let shareableImage = ShareableImage(image: image, title: title, text: text)
    
    if let text = text {
        return shareData(data: [shareableImage, text])
    } else {
        return shareData(data: [shareableImage])
    }
}

@_cdecl("ushare_interface_share_images")
public func shareImages(
    imagePtrs: UnsafePointer<UnsafeMutableRawPointer>?,
    sizes: UnsafePointer<Int32>?,
    count: Int32,
    textPtr: UnsafePointer<CChar>?,
    titlePtr: UnsafePointer<CChar>?
) -> Bool {
    logger.log("Sharing images.")
    guard let imagePtrs = imagePtrs, let sizes = sizes else {
        logger.error("Images or sizes were nil.")
        return false
    }
    
    let title = titlePtr.map { String(cString: $0) }
    let text = textPtr.map({ String(cString: $0) })
    
    var sharedContent: [Any] = []
    for i in 0..<Int(count) {
        let imagePtr = imagePtrs[i]
        let size = sizes[i]
        
        let data = Data(bytes: imagePtr, count: Int(size))
        guard let image = UIImage(data: data) else {
            logger.error("Failed to create UIImage from provided data.")
            return false
        }
        
        if i == 0 {
            sharedContent.append(ShareableImage(image: image, title: title, text: text))
        } else {
            sharedContent.append(image)
        }
    }
    
    if let text = text {
        sharedContent.append(text)
    }
    
    return shareData(data: sharedContent)
}

private func shareData(data: [Any], onDone: (() -> Void)? = nil) -> Bool {
    guard let rootViewController = getRootViewController() else {
        logger.error("Could not find a root view controller to present the share sheet.")
        onDone?()
        return false
    }
    
    let activityViewController = UIActivityViewController(activityItems: data, applicationActivities: nil);
    
    if let onDone = onDone {
        activityViewController.completionWithItemsHandler = { activityType, completed, returnedItems, error in
            logger.log("Shared to \(activityType?.rawValue ?? "nil") with result: \(completed), error: \(error?.localizedDescription ?? "nil")")
            onDone()
        }
    }
    
    if let popoverController = activityViewController.popoverPresentationController {
        popoverController.sourceView = rootViewController.view
        popoverController.sourceRect = CGRect(x: rootViewController.view.bounds.midX, y: rootViewController.view.bounds.midY, width: 0, height: 0)
        popoverController.permittedArrowDirections = []
    }

    rootViewController.present(activityViewController, animated: true, completion: nil)
    logger.log("Sharesheet invoked.")
    return true
}

private func getRootViewController() -> UIViewController? {
    if let rootView = getUnityRootViewController() {
        return rootView
    } else {
        logger.warning("Could not get Unity's root UIViewController, trying to get key window.")
        return UIApplication.shared.windows.first { $0.isKeyWindow }?.rootViewController
    }
}
