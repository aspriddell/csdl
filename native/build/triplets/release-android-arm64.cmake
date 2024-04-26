set(VCPKG_TARGET_ARCHITECTURE arm64)
set(VCPKG_CMAKE_CONFIGURE_OPTIONS -DANDROID_ABI=arm64-v8a)
set(VCPKG_MAKE_BUILD_TRIPLET "--host=aarch64-linux-android")

include("${CMAKE_CURRENT_LIST_DIR}/triplet-common-android.cmake")