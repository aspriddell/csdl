//
// library.hpp
// Created by Albie on 29/02/2024.
//

#ifndef CSDL_LIBRARY_HPP
#define CSDL_LIBRARY_HPP

#include "events.h"
#include "structs.h"
#include "lib_export.h"

#include <libtorrent/torrent_handle.hpp>

#ifdef __cplusplus
extern "C" {
#endif

    // session control
    CSDL_EXPORT lt::session* create_session(lt::settings_pack* pack);
    CSDL_EXPORT void destroy_session(lt::session* session);

    CSDL_EXPORT void set_event_callback(lt::session* session, cs_alert_callback callback, bool include_unmapped_events);
    CSDL_EXPORT void clear_event_callback(lt::session* session);

    CSDL_EXPORT void apply_settings(lt::session* session, lt::settings_pack* settings);

    // torrent control
    CSDL_EXPORT lt::add_torrent_params* create_torrent_file(const char* file_path);
    CSDL_EXPORT lt::add_torrent_params* create_torrent_bytes(const char* data, long length);
    CSDL_EXPORT void destroy_torrent(lt::add_torrent_params* torrent);

    CSDL_EXPORT lt::torrent_handle* attach_torrent(lt::session* session, lt::add_torrent_params* torrent, const char* save_path);
    CSDL_EXPORT lt::torrent_handle* attach_magnet(lt::session* session, const char* magnet_uri, const char* save_path);
    CSDL_EXPORT void detach_torrent(lt::session* session, lt::torrent_handle* torrent);

    CSDL_EXPORT lt::add_torrent_params* get_handle_torrent_info(lt::torrent_handle* handle);
    CSDL_EXPORT void get_torrent_handle_info_hash(lt::torrent_handle* handle, char* hash_out);

    // torrent info
    CSDL_EXPORT torrent_metadata* get_torrent_info(lt::add_torrent_params* torrent);
    CSDL_EXPORT void destroy_torrent_info(torrent_metadata* info);

    CSDL_EXPORT bool save_torrent_to_file(lt::add_torrent_params* torrent, const char* file_path);
    CSDL_EXPORT void get_torrent_bytes(lt::add_torrent_params* torrent, char** out_data, long* out_size);
    CSDL_EXPORT void free_torrent_bytes(char* data);

    // file listing
    CSDL_EXPORT void get_torrent_file_list(lt::add_torrent_params* torrent, torrent_file_list* file_list);
    CSDL_EXPORT void destroy_torrent_file_list(torrent_file_list* file_list);

    // priority control
    CSDL_EXPORT uint8_t get_file_dl_priority(lt::torrent_handle* torrent, int32_t file_index);
    CSDL_EXPORT void set_file_dl_priority(lt::torrent_handle* torrent, int32_t file_index, uint8_t priority);

    // download control
    CSDL_EXPORT void start_torrent(lt::torrent_handle* torrent);
    CSDL_EXPORT void stop_torrent(lt::torrent_handle* torrent);
    CSDL_EXPORT void reannounce_torrent(lt::torrent_handle* torrent, const int32_t seconds, const uint8_t ignore_min_interval);

    CSDL_EXPORT void get_torrent_status(lt::torrent_handle* torrent, torrent_status* torrent_status);

#ifdef __cplusplus
}
#endif
#endif //CSDL_LIBRARY_HPP
