cmake_minimum_required(VERSION 3.21)

include(GenerateExportHeader)
include_directories(${CMAKE_BINARY_DIR}/include native/include)

set(CMAKE_CXX_STANDARD 20)
set(PRODUCT_VERSION_MAJOR 1)
set(PRODUCT_VERSION_MINOR 2)
set(PRODUCT_VERSION_PATCH 2)

# build target versions
if(${CMAKE_SYSTEM_NAME} MATCHES "Windows")
  set(CMAKE_SYSTEM_VERSION 10.0 CACHE TYPE INTERNAL FORCE)
elseif(${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
  set(CMAKE_OSX_DEPLOYMENT_TARGET 11 CACHE TYPE INTERNAL FORCE)
endif()

project(cs_native LANGUAGES CXX VERSION ${PRODUCT_VERSION_MAJOR}.${PRODUCT_VERSION_MINOR}.${PRODUCT_VERSION_PATCH})

find_package(LibtorrentRasterbar 2.0 CONFIG REQUIRED)
find_package(magic_enum CONFIG REQUIRED)

# rpath fixes (needs to go after project but before add_library)
if(${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
  set(CMAKE_BUILD_WITH_INSTALL_RPATH TRUE)
  set(CMAKE_INSTALL_RPATH "@loader_path")
elseif(${CMAKE_SYSTEM_NAME} MATCHES "Linux")
  set(CMAKE_BUILD_WITH_INSTALL_RPATH TRUE)
  set(CMAKE_INSTALL_RPATH "$ORIGIN")
endif()

add_library(csdl SHARED
        native/src/settings.cpp
        native/src/library.cpp
        native/src/events.cpp
        native/include/struct_align.h
        native/include/settings.h
        native/include/locks.hpp)

# version.rc file for windows
if(${CMAKE_SYSTEM_NAME} MATCHES "Windows")
  configure_file(native/build/version.rc.in version.rc @ONLY)
  target_sources(csdl PRIVATE version.rc)
endif()

set_target_properties(csdl PROPERTIES SOVERSION ${PRODUCT_VERSION_MAJOR} VERSION ${PRODUCT_VERSION_MAJOR}.${PRODUCT_VERSION_MINOR}.${PRODUCT_VERSION_PATCH})
generate_export_header(csdl EXPORT_FILE_NAME ${CMAKE_BINARY_DIR}/include/lib_export.h)

target_link_libraries(csdl PUBLIC LibtorrentRasterbar::torrent-rasterbar)
target_link_libraries(csdl PRIVATE magic_enum::magic_enum)
