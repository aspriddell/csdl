//
// events.cpp - handles alert callbacks
// Created by Albie on 04/03/2024.
//

#include "events.h"

#include <libtorrent/session.hpp>
#include <libtorrent/alert_types.hpp>

void fill_event_info(cs_alert* alert, lt::alert* lt_alert) {
    alert->type = lt_alert->type();
    alert->category = lt_alert->category();
    alert->epoch = lt_alert->timestamp().time_since_epoch().count();
    alert->message = lt_alert->message().c_str();
}

void on_events_available(lt::session* session, void (*callback)(void* alert)) {
    auto events = std::vector<lt::alert*>();
    session->pop_alerts(&events);

    handle_events:
    for (auto &alert: events) {
        switch (alert->type()) {

            // torrent state changed
            case lt::state_changed_alert::alert_type:
            {
                auto state_alert = lt::alert_cast<lt::state_changed_alert>(alert);
                cs_torrent_status_alert status_alert;

                status_alert.handle = &state_alert->handle;
                status_alert.new_state = state_alert->state;
                status_alert.old_state = state_alert->prev_state;

                fill_event_info(&status_alert.alert, alert);
                callback(&status_alert);
                break;
            }

            // performance warning
            case lt::performance_alert::alert_type:
            {
                auto perf_alert = lt::alert_cast<lt::performance_alert>(alert);
                cs_client_performance_alert perf_warning;

                perf_warning.warning_type = perf_alert->warning_code;

                fill_event_info(&perf_warning.alert, alert);
                callback(&perf_warning);
                break;
            }
        }
    }

    events.clear();
    session->pop_alerts(&events);

    if (!events.empty()) {
        goto handle_events;
    }
}

