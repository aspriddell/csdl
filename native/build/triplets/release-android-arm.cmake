set(VCPKG_TARGET_ARCHITECTURE arm)
set(VCPKG_CMAKE_CONFIGURE_OPTIONS -DANDROID_ABI=armeabi-v7a -DANDROID_ARM_NEON=OFF)
set(VCPKG_MAKE_BUILD_TRIPLET "--host=armv7a-linux-androideabi")

include("${CMAKE_CURRENT_LIST_DIR}/triplet-common-android.cmake")