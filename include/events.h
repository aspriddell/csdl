//
// events.hpp - ported event structures
// Created by Albie on 04/03/2024.
//

#ifndef CS_NATIVE_EVENTS_H
#define CS_NATIVE_EVENTS_H

#include "lib_export.h"

#include <ctime>
#include <libtorrent/alert.hpp>
#include <libtorrent/error_code.hpp>
#include <libtorrent/torrent_status.hpp>
#include <libtorrent/torrent_handle.hpp>

// used internally in main library, not intended for public use
typedef void (__cdecl *cs_alert_callback)(void* alert);
CSDL_NO_EXPORT void on_events_available(lt::session* session, cs_alert_callback callback, bool include_unmapped);

#ifdef __cplusplus
extern "C" {
#endif

    enum cs_alert_type : int32_t {
        alert_generic = 0,
        alert_torrent_status = 1,
        alert_client_performance = 2,
        alert_peer_notification = 3,
        alert_torrent_removed = 4
    };

    // base format for all alerts
    struct cs_alert {
        cs_alert_type type;

        int32_t category;
        int64_t epoch;

        const char* message;
    };

    struct cs_torrent_status_alert {
        cs_alert alert;

        lt::torrent_handle* handle;

        uint32_t old_state;
        uint32_t new_state;
    };

    struct cs_torrent_remove_alert {
        cs_alert alert;

        lt::torrent_handle* handle;
    };

    struct cs_client_performance_alert {
        cs_alert alert;

        uint8_t warning_type;
    };

    enum cs_peer_alert_type : uint8_t {
        connected_in = 0,
        connected_out = 1,
        disconnected = 2,
        banned = 3,
        snubbed = 4,
        unsnubbed = 5,
        errored = 6
    };

    struct cs_peer_alert {
        cs_alert alert;

        lt::torrent_handle* handle;
        cs_peer_alert_type type;

        uint8_t ipv6_address[16];
    };

#ifdef __cplusplus
}
#endif
#endif //CS_NATIVE_EVENTS_H
