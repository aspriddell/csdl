set(VCPKG_CMAKE_SYSTEM_NAME Android)

set(VCPKG_BUILD_TYPE release)
set(VCPKG_CRT_LINKAGE dynamic)
set(VCPKG_LIBRARY_LINKAGE static)

# Add -fPIC flag for all platforms (required for linking static libs into shared libs)
set(VCPKG_CXX_FLAGS "-fPIC")
set(VCPKG_C_FLAGS "-fPIC")

# Ensure CMake-based ports build static libs as PIC as well.
list(APPEND VCPKG_CMAKE_CONFIGURE_OPTIONS "-DCMAKE_POSITION_INDEPENDENT_CODE=ON")