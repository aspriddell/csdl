set(VCPKG_CMAKE_SYSTEM_NAME Android)

set(VCPKG_BUILD_TYPE release)
set(VCPKG_CRT_LINKAGE dynamic)
set(VCPKG_LIBRARY_LINKAGE static)

# Add -fPIC flag for all platforms (required for linking static libs into shared libs)
string(APPEND VCPKG_CXX_FLAGS_RELEASE " -fPIC")
string(APPEND VCPKG_C_FLAGS_RELEASE " -fPIC")