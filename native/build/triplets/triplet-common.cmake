set(VCPKG_BUILD_TYPE release)
set(VCPKG_CRT_LINKAGE dynamic)
set(VCPKG_LIBRARY_LINKAGE static)

# build libtorrent as a dynamic library, static link everything else
if(PORT MATCHES "libtorrent")
    set(VCPKG_LIBRARY_LINKAGE dynamic)
endif()
