cmake_minimum_required(VERSION 3.27)
set(CMAKE_CXX_STANDARD 20)

include(GenerateExportHeader)
include_directories(${CMAKE_BINARY_DIR}/include ${CMAKE_SOURCE_DIR}/include)

project(cs_native VERSION 0.1 LANGUAGES CXX)

add_library(csdl SHARED src/library.cpp src/events.cpp)

set_property(TARGET csdl PROPERTY VERSION 0.1)
set_property(TARGET csdl PROPERTY SOVERSION 0.1)

generate_export_header(csdl EXPORT_FILE_NAME ${CMAKE_BINARY_DIR}/include/lib_export.h)

# platform-specific settings
if (${CMAKE_SYSTEM_NAME} MATCHES "Linux")
    find_library(LIBTORRENT_LIBRARY NAMES libtorrent-rasterbar.so.2.0)

elseif (${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
    include_directories(/opt/homebrew/include)
    find_library(LIBTORRENT_LIBRARY NAMES libtorrent-rasterbar.2.0.dylib PATHS /opt/homebrew/lib)

    set_target_properties(csdl PROPERTIES BUILD_WITH_INSTALL_RPATH TRUE INSTALL_RPATH "/opt/homebrew/lib")

endif ()

if (NOT LIBTORRENT_LIBRARY)
    message(FATAL_ERROR "libtorrent not found")
endif ()

# link to libtorrent
target_link_libraries(csdl PUBLIC ${LIBTORRENT_LIBRARY})
