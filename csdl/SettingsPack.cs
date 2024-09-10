// csdl - a cross-platform libtorrent wrapper for .NET
// Licensed under Apache-2.0 - see the license file for more information

using System;
using System.Collections;
using System.Collections.Specialized;
using csdl.Native;

namespace csdl
{
    /// <summary>
    /// Represents a collection used to store arbitrary configuration values for passing to the underlying libtorrent instance.
    /// </summary>
    /// <remarks>
    /// Find the full list of configuration keys in the libtorrent documentation (https://www.libtorrent.org /reference-Settings.html).
    /// </remarks>
    public class SettingsPack
    {
        private readonly HybridDictionary _dictionary = new(true);

        /// <summary>
        /// Gets a string configuration value by key
        /// </summary>
        /// <param name="key">The key to retrieve</param>
        /// <returns>The string value, null if it is not present</returns>
        public string Get(string key)
        {
            var value = _dictionary[key];
            switch (value)
            {
                case string s:
                    return s;

                case null:
                    return null;

                default:
                    throw new ArgumentException($"Type mismatch, expected string, got {value.GetType().Name}");
            }
        }

        /// <summary>
        /// Gets a configuration value by key
        /// </summary>
        /// <param name="key">The key to retrieve</param>
        /// <typeparam name="T">The type of value to get. Can be either <see cref="int"/> or <see cref="bool"/></typeparam>
        /// <returns>The value from the configuration store</returns>
        /// <exception cref="ArgumentException">An invalid type was presented</exception>
        public T? Get<T>(string key) where T : struct
        {
            if (typeof(T) != typeof(bool) && typeof(T) != typeof(int))
            {
                throw new ArgumentException("Only bool and int types are supported");
            }

            var value = _dictionary[key];
            switch (value)
            {
                case T casted:
                    return casted;

                case null:
                    return null;

                default:
                    throw new ArgumentException($"Type mismatch, expected {value.GetType().Name}, got {typeof(T).Name}");
            }
        }

        /// <summary>
        /// Applies a <see cref="string"/> configuration key to the store
        /// </summary>
        public void Set(string key, string value)
        {
            _dictionary[key] = value;
        }

        /// <summary>
        /// Applies an <see cref="int"/> configuration key to the store
        /// </summary>
        public void Set(string key, int value)
        {
            _dictionary[key] = value;
        }

        /// <summary>
        /// Applies a <see cref="bool"/> configuration key to the store
        /// </summary>
        public void Set(string key, bool value)
        {
            _dictionary[key] = value;
        }

        /// <summary>
        /// Builds a native settings pack from the current configuration store
        /// </summary>
        /// <returns>The handle to the built pack.</returns>
        internal IntPtr BuildNative()
        {
            var pack = NativeMethods.CreateSettingsPack();

            if (pack == IntPtr.Zero)
            {
                throw new ApplicationException("Failed to create settings pack container");
            }

            try
            {
                foreach (DictionaryEntry entry in _dictionary)
                {
                    var key = (string)entry.Key;
                    var value = entry.Value;

                    var success = value switch
                    {
                        int i => NativeMethods.SettingsPackSetInt(pack, key, i),
                        bool b => NativeMethods.SettingsPackSetBool(pack, key, b),
                        string s => NativeMethods.SettingsPackSetString(pack, key, s),

                        _ => throw new ArgumentException($"{value?.GetType().Name} type is not supported")
                    };

                    if (!success)
                    {
                        throw new ArgumentException($"Failed to set key {key} in settings pack. Ensure the key exists and the value is the correct type.");
                    }
                }
            }
            catch
            {
                NativeMethods.FreeSettingsPack(pack);
                throw;
            }

            return pack;
        }
    }
}