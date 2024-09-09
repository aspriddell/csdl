//
// settings.cpp
// Created by Albie on 09/09/24.
//

#include "settings.h"

#include <magic_enum.hpp>

template <typename T>
bool set_value(const char* key, std::function<bool(T)> setter);

lt::settings_pack* create_settings_pack()
{
    return new lt::settings_pack;
}

void destroy_settings_pack(lt::settings_pack* pack)
{
    delete pack;
}

bool set_str(lt::settings_pack* pack, const char* key, const char* value)
{
    if (pack == nullptr)
    {
        return false;
    }

    return set_value<lt::settings_pack::string_types>(key, [pack, value](lt::settings_pack::string_types key)
    {
        if (value == nullptr)
        {
            return false;
        }

        pack->set_str(key, std::string(value));
        return true;
    });
}

bool set_bool(lt::settings_pack* pack, const char* key, bool value)
{
    if (pack == nullptr)
    {
        return false;
    }

    return set_value<lt::settings_pack::bool_types>(key, [pack, value](lt::settings_pack::bool_types key)
    {
        pack->set_bool(key, value);
        return true;
    });
}

bool set_int(lt::settings_pack* pack, const char* key, int value)
{
    if (pack == nullptr)
    {
        return false;
    }

    return set_value<lt::settings_pack::int_types>(key, [pack, value](lt::settings_pack::int_types key)
    {
        pack->set_int(key, value);
        return true;
    });
}

// helper method for converting strings to enums and performing callbacks based on the result
template <typename T>
typename std::enable_if<std::is_enum<T>::value, bool>::type
set_value(const char* key, std::function<bool(T)> setter)
{
    if (key == nullptr)
    {
        return false;
    }

    auto enum_key = magic_enum::enum_cast<T>(key, magic_enum::case_insensitive);
    if (!enum_key.has_value())
    {
        return false;
    }

    return setter(enum_key.value());
}
