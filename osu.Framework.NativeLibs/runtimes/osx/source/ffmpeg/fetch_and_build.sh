#!/bin/bash
PS3='Build for which arch? '
archs=("arm64" "x86_64")
select arch in "${archs[@]}"; do
    case $arch in
        "arm64")
            break;;
        "x86_64")
            break;;
        *) echo "invalid option";;
    esac
done

if [ ! -d "ffmpeg-5.0" ]
then
    echo "-> Fetching FFmpeg 5.0..."
    curl https://ffmpeg.org/releases/ffmpeg-5.0.tar.gz | tar zxf -
else
    echo "-> ffmpeg-5.0 already exists, not re-downloading."
fi
cd ffmpeg-5.0

echo "-> Configuring..."
./configure --disable-programs --disable-doc --disable-static --disable-debug --enable-shared --arch=$arch --prefix=build-$arch --libdir=build-$arch/lib
CORES=$(sysctl -n hw.ncpu)
echo "-> Building using $CORES threads..."
make -j$CORES
make install

mv build-$arch/lib/libavcodec.59.18.100.dylib build-$arch/lib/libavcodec.59.dylib
mv build-$arch/lib/libavdevice.59.4.100.dylib build-$arch/lib/libavdevice.59.dylib
mv build-$arch/lib/libavfilter.8.24.100.dylib build-$arch/lib/libavfilter.8.dylib
mv build-$arch/lib/libavformat.59.16.100.dylib build-$arch/lib/libavformat.59.dylib
mv build-$arch/lib/libavutil.57.17.100.dylib build-$arch/lib/libavutil.57.dylib
mv build-$arch/lib/libswresample.4.3.100.dylib build-$arch/lib/libswresample.4.dylib
mv build-$arch/lib/libswscale.6.4.100.dylib build-$arch/lib/libswscale.6.dylib

echo "-> Fixing dylibs paths..."
BUILDPATH=build-$arch/lib
LIBS="libavcodec.59.dylib libavdevice.59.dylib libavfilter.8.dylib libavformat.59.dylib libavutil.57.dylib libswresample.4.dylib libswscale.6.dylib"
for f in $LIBS; do
    install_name_tool $BUILDPATH/$f -id @loader_path/$f \
        -change $BUILDPATH/libavcodec.59.dylib @loader_path/libavcodec.59.dylib \
        -change $BUILDPATH/libavdevice.59.dylib @loader_path/libavdevice.59.dylib \
        -change $BUILDPATH/libavfilter.8.dylib @loader_path/libavfilter.8.dylib \
        -change $BUILDPATH/libavformat.59.dylib @loader_path/libavformat.59.dylib \
        -change $BUILDPATH/libavutil.57.dylib @loader_path/libavutil.57.dylib \
        -change $BUILDPATH/libswresample.4.dylib @loader_path/libswresample.4.dylib \
        -change $BUILDPATH/libswscale.6.dylib @loader_path/libswscale.6.dylib

    mkdir -p ../build-$arch
    cp $BUILDPATH/$f ../build-$arch/$f
done
