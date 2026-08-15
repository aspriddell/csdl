set(VCPKG_BUILD_TYPE release)
set(VCPKG_CRT_LINKAGE dynamic)
set(VCPKG_LIBRARY_LINKAGE static)

# Add -fPIC flag for all platforms (required for linking static libs into shared libs)
set(VCPKG_CXX_FLAGS "-fPIC")
set(VCPKG_C_FLAGS "-fPIC")

# build libtorrent as a dynamic library, static link everything else
if(PORT MATCHES "libtorrent")
    set(VCPKG_LIBRARY_LINKAGE dynamic)
endif()
