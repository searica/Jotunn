using System;


namespace Jotunn.Utils
{

    /// <summary>
    ///     Enum used for telling whether AdminOnly settings for Config Entries should always be enforced 
    ///     or if they should only be enforced when Jotunn is installed on the server.
    /// </summary>
    public enum AdminOnlyStrictness : int
    {
        /// <summary>
        ///     AdminOnly is only enforced for Config Entries if Jotunn is installed on the server.
        /// </summary>
        IfJotunnOnServer = 0,
        
        /// <summary>
        ///     AdminOnly is always enforced for Config Entries even if Jotunn is not installed on the server. 
        ///     This means that AdminOnly configs cannot be editted in multiplayer if Jotunn is not on the server.
        /// </summary>
        Always = 1,

    }
    internal class SynchronizationModeAttribute
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
