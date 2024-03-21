cmake_minimum_required(VERSION 3.21)

set(CMAKE_CXX_STANDARD 20)
set(PRODUCT_VERSION_MAJOR 1)
set(PRODUCT_VERSION_MINOR 0)
set(PRODUCT_VERSION_PATCH 1)

# build target versions
if(${CMAKE_SYSTEM_NAME} MATCHES "Windows")
  configure_file(native/build/version.rc.in version.rc @ONLY)
  set(CMAKE_SYSTEM_VERSION 10.0 CACHE TYPE INTERNAL FORCE)
elseif(${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
  set(CMAKE_OSX_DEPLOYMENT_TARGET 11)
endif()

# CSDL_EXPORT
include(GenerateExportHeader)
include_directories(${CMAKE_BINARY_DIR}/include ${CMAKE_SOURCE_DIR}/native/include)

project(cs_native LANGUAGES CXX VERSION ${PRODUCT_VERSION_MAJOR}.${PRODUCT_VERSION_MINOR}.${PRODUCT_VERSION_PATCH})
find_package(LibtorrentRasterbar CONFIG REQUIRED)

# version fixes on windows, rpath fixes on macOS/Linux
if(${CMAKE_SYSTEM_NAME} MATCHES "Windows")
  target_sources(csdl PRIVATE version.rc)
elseif(${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
  set(CMAKE_BUILD_WITH_INSTALL_RPATH TRUE)
  set(CMAKE_INSTALL_RPATH "@loader_path")
elseif(${CMAKE_SYSTEM_NAME} MATCHES "Linux")
  set(CMAKE_BUILD_WITH_INSTALL_RPATH TRUE)
  set(CMAKE_INSTALL_RPATH "$ORIGIN")
endif()

add_library(csdl SHARED native/src/library.cpp native/src/events.cpp)
generate_export_header(csdl EXPORT_FILE_NAME ${CMAKE_BINARY_DIR}/include/lib_export.h)

set_target_properties(csdl PROPERTIES SOVERSION ${PRODUCT_VERSION_MAJOR} VERSION ${PRODUCT_VERSION_MAJOR}.${PRODUCT_VERSION_MINOR}.${PRODUCT_VERSION_PATCH})
target_link_libraries(csdl PUBLIC LibtorrentRasterbar::torrent-rasterbar)
