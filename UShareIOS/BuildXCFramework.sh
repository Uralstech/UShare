#!/bin/bash

SCHEME="UShare"
OUTPUT="build"
XCFRAMEWORK_NAME="UShare.xcframework"

xcodebuild archive \
  -scheme "$SCHEME" \
  -configuration Release \
  -destination "generic/platform=iOS" \
  -archivePath "$OUTPUT/ios_devices.xcarchive" \
  SKIP_INSTALL=NO BUILD_LIBRARY_FOR_DISTRIBUTION=YES

xcodebuild archive \
  -scheme "$SCHEME" \
  -configuration Release \
  -destination "generic/platform=iOS Simulator" \
  -archivePath "$OUTPUT/ios_simulator.xcarchive" \
  SKIP_INSTALL=NO BUILD_LIBRARY_FOR_DISTRIBUTION=YES

xcodebuild -create-xcframework \
  -framework "$OUTPUT/ios_devices.xcarchive/Products/Library/Frameworks/UShare.framework" \
  -framework "$OUTPUT/ios_simulator.xcarchive/Products/Library/Frameworks/UShare.framework" \
  -output "$OUTPUT/$XCFRAMEWORK_NAME"
