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

import UIKit
import LinkPresentation

@_cdecl("ushare_ios_share_text")
public func shareText(id: Int32, textPtr: UnsafePointer<UInt8>?, titlePtr: UnsafePointer<UInt8>?,
                      onActivityFinished: @convention(c) @escaping (Int32) -> Void) -> Bool {
    
    guard let text = getString(from: textPtr) else {
        logger.error("Expected valid, non-NULL string for text.");
        return false
    }
    
    let title = getString(from: titlePtr)
    return shareData(id: id, data: [ShareableText(text, title)], callback: onActivityFinished);
}


@_cdecl("ushare_ios_share_images")
public func shareImages(id: Int32, imagesPtr: UnsafePointer<UnsafeRawPointer?>?, sizes: UnsafePointer<Int32>?, count: Int32,
                        textPtr: UnsafePointer<UInt8>?, titlePtr: UnsafePointer<UInt8>?, onActivityFinished: @convention(c) @escaping (Int32) -> Void) -> Bool {
    
    guard let imagesPtr = imagesPtr, let sizes = sizes else {
        logger.error("Expected non-NULL array of images and sizes.")
        return false
    }
    
    guard count > 0 else {
        logger.error("Expected count > 0.")
        return false
    }
    
    let text = getString(from: textPtr)
    let title = getString(from: titlePtr)
    
    var data: [Any] = []
    for i in 0..<Int(count) {
        
        guard
            let imagePtr = imagesPtr[i],
            let image = UIImage(data: Data(bytes: imagePtr, count: Int(sizes[i])))
        else {
            logger.error("Expected all elements in images to be valid, non-NULL images.")
            return false
        }
        
        if i == 0 {
            data.append(ShareableImage(image, title, text))
        } else {
            data.append(image)
        }
    }
    
    if let text = text {
        data.append(text)
    }
    
    return shareData(id: id, data: data, callback: onActivityFinished);
}

@_cdecl("ushare_ios_share_files")
public func shareFiles(id: Int32, pathsPtr: UnsafePointer<UnsafePointer<UInt8>?>?, count: Int32, textPtr: UnsafePointer<UInt8>?,
                       titlePtr: UnsafePointer<UInt8>?, onActivityFinished: @convention(c) @escaping (Int32) -> Void) -> Bool {
    
    guard let pathsPtr = pathsPtr else {
        logger.error("Expected non-NULL array of paths.");
        return false
    }
    
    guard count > 0 else {
        logger.error("Expected count > 0.")
        return false
    }
    
    let text = getString(from: textPtr)
    let title = getString(from: titlePtr)
    
    var data: [Any] = []
    for i in 0..<Int(count) {
        
        guard let path = getString(from: pathsPtr[i]) else {
            logger.error("Expected all elements in paths to be valid, non-NULL strings.")
            return false
        }
        
        guard isRegularFile(at: path) else {
            logger.error("File not found at path: \(path)")
            return false
        }
        
        let url = URL(fileURLWithPath: path)
        if i == 0 {
            data.append(ShareableUri(url, title))
        } else {
            data.append(url)
        }
    }
    
    if let text = text {
        data.append(text)
    }
    
    return shareData(id: id, data: data, callback: onActivityFinished);
}

private func shareData(id: Int32, data: [Any], callback onActivityFinished: @escaping (Int32) -> Void) -> Bool {

    guard Thread.isMainThread else {
        logger.error("Method called from non-main thread.")
        return false
    }
    
    guard let viewController = getUnityRootViewController() else {
        logger.error("Could not get view controller.")
        return false
    }
    
    let activityViewController = UIActivityViewController(activityItems: data, applicationActivities: nil)
    activityViewController.completionWithItemsHandler = { activityType, completed, _, error in
        logger.debug("Activity completed with completed: \(completed), consumed by: \(activityType?.rawValue ?? "nil"), error: \(error?.localizedDescription ?? "nil")")
        
        onActivityFinished(id)
    }
    
    if let popoverController = activityViewController.popoverPresentationController {
        popoverController.sourceView = viewController.view
        popoverController.permittedArrowDirections = []
        
        let viewBounds = viewController.view.bounds
        popoverController.sourceRect = CGRect(x: viewBounds.midX, y: viewBounds.midY, width: 0, height: 0)
    }
    
    viewController.present(activityViewController, animated: true)
    logger.info("Presented sharesheet.")
    return true
}

private func getString(from pointer: UnsafePointer<UInt8>?) -> String? {
    pointer.flatMap({ String.decodeCString($0, as: UTF8.self, repairingInvalidCodeUnits: false)?.result })
}

private func isRegularFile(at path: String) -> Bool {
    
    var isDirectory: ObjCBool = false
    return FileManager.default.fileExists(atPath: path, isDirectory: &isDirectory)
        && !isDirectory.boolValue
}
