using System;


namespace Jotunn.Utils
{
    /// <summary>
    ///     Enum used for telling whether AdminOnly settings for Config Entries should always be enforced
    ///     or if they should only be enforced when the mod is installed on the server.
    /// </summary>
    public enum AdminOnlyStrictness : int
    {
        /// <summary>
        ///     AdminOnly is always enforced for Config Entries even if the mod is not installed on the server.
        ///     This means that AdminOnly configs cannot be edited in multiplayer if the mod is not on the server.
        /// </summary>
        Always = 0,

        /// <summary>
        ///     AdminOnly is only enforced for Config Entries if the mod is installed on the server.
        /// </summary>
        IfOnServer = 1,
    }

    /// <summary>
    ///     This attribute is used to determine how Jotunn should enforce synchronization of Config Entries.<br/>
    ///     Only relevant for Config Entries that have the <see cref="ConfigurationManagerAttributes.IsAdminOnly"/> applied.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class SynchronizationModeAttribute : Attribute
    {
        /// <summary>
        ///     AdminOnly ConfigEntry Strictness
        /// </summary>
        public AdminOnlyStrictness EnforceAdminOnly { get; set; }

        /// <summary>
        ///     Synchronization mode Attribute
        /// </summary>
        /// <param name="enforceAdminOnly"></param>
        public SynchronizationModeAttribute(AdminOnlyStrictness enforceAdminOnly)
        {
            EnforceAdminOnly = enforceAdminOnly;
        }

        /// <summary>
        ///     Check if AdminOnly Config Entries should always be locked if player is not an admin.
        /// </summary>
        /// <returns></returns>
        public bool ShouldAlwaysEnforceAdminOnly()
        {
            return EnforceAdminOnly == AdminOnlyStrictness.Always;
        }
    }
}
