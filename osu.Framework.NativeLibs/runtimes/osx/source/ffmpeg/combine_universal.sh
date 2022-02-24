#!/bin/sh

lipo -create build-arm64/libavcodec.59.dylib build-x86_64/libavcodec.59.dylib -output ../../native/libavcodec.59.dylib
lipo -create build-arm64/libavdevice.59.dylib build-x86_64/libavdevice.59.dylib -output ../../native/libavdevice.59.dylib
lipo -create build-arm64/libavfilter.8.dylib build-x86_64/libavfilter.8.dylib -output ../../native/libavfilter.8.dylib
lipo -create build-arm64/libavformat.59.dylib build-x86_64/libavformat.59.dylib -output ../../native/libavformat.59.dylib
lipo -create build-arm64/libavutil.57.dylib build-x86_64/libavutil.57.dylib -output ../../native/libavutil.57.dylib
lipo -create build-arm64/libswresample.4.dylib build-x86_64/libswresample.4.dylib -output ../../native/libswresample.4.dylib
lipo -create build-arm64/libswscale.6.dylib build-x86_64/libswscale.6.dylib -output ../../native/libswscale.6.dylib
