using System;
using BepInEx.Configuration;

namespace Jotunn.Extensions
{

    /// <summary>
    ///     Extends ConfigFile with a convenience method to bind config entries with less boilerplate code 
    ///     and explicitly expose commonly used configuration manager attributes.
    /// </summary>
    public static class ConfigFileExtensions
    {
        internal static string GetExtendedDescription(string description, bool synchronizedSetting)
        {
            return description + (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]");
        }

        /// <summary>
        ///     Bind a new config entry to the config file and modify description to state whether the config entry is synced or not.
        /// </summary>
        /// <typeparam name="T">Type of the value the config entry holds.</typeparam>
        /// <param name="configFile">Configuration file to bind the config entry to.</param>
        /// <param name="section">Configuration file section to list the config entry in.</param>
        /// <param name="name">Display name of the config entry.</param>
        /// <param name="value">Default value of the config entry.</param>
        /// <param name="description">Plain text description of the config entry to display as hover text in configuration manager.</param>
        /// <param name="acceptVals">Acceptable values for config entry as an AcceptableValueRange, AcceptableValueList, or custom subclass.</param>
        /// <param name="synced">Whether the config entry IsAdminOnly and should be synced with server.</param>
        /// <param name="order">Order of the setting on the settings list relative to other settings in a category. 0 by default, higher number is higher on the list.</param>
        /// <param name="drawer">Custom setting editor (OnGUI code that replaces the default editor provided by ConfigurationManager).</param>
        /// <param name="configAttributes">Optional config manager attributes for additional user specified functionality. Any optional fields specified by the arguments of BindConfig will be overwritten by the parameters passed to BindConfig.</param>
        /// <returns>ConfigEntry bound to the config file.</returns>
        public static ConfigEntry<T> BindConfig<T>(
            this ConfigFile configFile,
            string section,
            string name,
            T value,
            string description,
            AcceptableValueBase acceptVals = null,
            bool synced = true,
            int order = 0,
            Action<ConfigEntryBase> drawer = null,
            ConfigurationManagerAttributes configAttributes = null
        )
        {
            string extendedDescription = GetExtendedDescription(description, synced);

            configAttributes ??= new ConfigurationManagerAttributes();         
            configAttributes.IsAdminOnly = synced;
            configAttributes.Order = order;
            if (drawer != null)
            {
                configAttributes.CustomDrawer = drawer;
            }

            ConfigEntry<T> configEntry = configFile.Bind(
                section,
                name,
                value,
                new ConfigDescription(
                    extendedDescription,
                    acceptVals,
                    configAttributes
                )
            );
            return configEntry;
        }    
    }
}
