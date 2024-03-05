//
// structs.hpp
// Created by Albie on 29/02/2024.
//

#ifndef CSDL_STRUCTS_HPP
#define CSDL_STRUCTS_HPP

#include <libtorrent/session.hpp>

#ifdef __cplusplus
extern "C" {
#endif

    typedef struct cs_session_config {
        // see https://www.libtorrent.org/reference-Settings.html#settings_pack
        char* user_agent;
        char* fingerprint;

        bool private_mode;
        bool block_seeding;
        bool encrypted_peers_only;

        int32_t max_connections;
    } session_config;

    typedef struct cs_torrent_file_information {
        lt::file_index_t index;

        int64_t offset;
        int64_t file_size;

        time_t modified_time;

        char* file_name;
        char* file_path;

        bool file_path_is_absolute;
        bool pad_file;
    } torrent_file_information;

    typedef struct cs_torrent_meta {
        char* name;
        char* creator;
        char* comment;

        int32_t total_files;
        uint64_t total_size;

        time_t creation_date;

        uint8_t info_hash_v1[20];
        uint8_t info_hash_v2[32];
    } torrent_metadata;

    typedef struct cs_torrent_status {
        int32_t state;
        
        float_t progress;

        int32_t count_peers;
        int32_t count_seeds;

        uint64_t bytes_uploaded;
        uint64_t bytes_downloaded;

        int64_t upload_rate;
        int64_t download_rate;
    } torrent_status;

    typedef struct cs_torrent_file_list {
        int32_t length;
        torrent_file_information* files;
    } torrent_file_list;

#ifdef __cplusplus
}
#endif

#endif //CSDL_STRUCTS_HPP
