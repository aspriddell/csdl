//
// structs.hpp
// Created by Albie on 29/02/2024.
//

#ifndef CSDL_STRUCTS_HPP
#define CSDL_STRUCTS_HPP

#include "struct_align.h"

#include <libtorrent/session.hpp>

#ifdef __cplusplus
extern "C" {
#endif

CSDL_STRUCT typedef struct cs_torrent_file_information {
    int32_t index;

    int64_t offset;
    int64_t file_size;

    time_t modified_time;

    char* file_name;
    char* file_path;

    bool file_path_is_absolute;
    bool pad_file;
} torrent_file_information;

CSDL_STRUCT typedef struct cs_torrent_meta {
    char* name;
    char* creator;
    char* comment;

    int32_t total_files;
    int64_t total_size;

    time_t creation_date;

    uint8_t info_hash_v1[20];
    uint8_t info_hash_v2[32];
} torrent_metadata;

CSDL_STRUCT typedef struct cs_torrent_file_list {
    int32_t length;
    torrent_file_information* files;
} torrent_file_list;

enum cs_torrent_state : int32_t {
    torrent_state_unknown = 0,
    torrent_checking = 1,
    torrent_checking_resume = 2,
    torrent_metadata_downloading = 3,
    torrent_downloading = 4,
    torrent_seeding = 5,
    torrent_finished = 6,
    torrent_error = 7
};

CSDL_STRUCT typedef struct cs_torrent_status {
    cs_torrent_state state;

    float progress;

    int32_t count_peers;
    int32_t count_seeds;

    int64_t bytes_uploaded;
    int64_t bytes_downloaded;

    int64_t upload_rate;
    int64_t download_rate;
} torrent_status;

#ifdef __cplusplus
}
#endif

#endif //CSDL_STRUCTS_HPP
