using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;

namespace Jotunn.Utils
{
    internal class ModModule
    {
        public const int LegacyDataLayoutVersion = 0;
        public const int CurrentDataLayoutVersion = 1;
        public static readonly HashSet<int> SupportedDataLayouts = new HashSet<int> { LegacyDataLayoutVersion, CurrentDataLayoutVersion };

        /// <summary>
        ///     DataLayoutVersion indicates the version layout of data within the ZPkg. If equal to 0 then it is a legacy format.
        /// </summary>
        public int DataLayoutVersion { get; private set; }

        private string guid;

        /// <summary>
        ///     Identifier for mod based on DataLayoutVersion. 
        ///     For legacy layout returns mod name, otherwise returns mod GUID. 
        /// </summary>
        public string ModID
        {
            get
            {
                return DataLayoutVersion == LegacyDataLayoutVersion ? ModName : guid;
            }   
        }

        /// <summary>
        ///     Friendly version of mod name.
        /// </summary>
        public string ModName { get; }

        /// <summary>
        ///     Version data for mod.
        /// </summary>
        public System.Version Version { get; }

        /// <summary>
        ///     Compatibility level of the mod.
        /// </summary>
        public CompatibilityLevel CompatibilityLevel { get; }

        /// <summary>
        ///     Version strictness level of the mod.
        /// </summary>
        public VersionStrictness VersionStrictness { get; }

        public ModModule(string guid, string name, System.Version version, CompatibilityLevel compatibilityLevel, VersionStrictness versionStrictness)
        {
            this.DataLayoutVersion = CurrentDataLayoutVersion;
            this.guid = guid;
            this.ModName = name;
            this.Version = version;
            this.CompatibilityLevel = compatibilityLevel;
            this.VersionStrictness = versionStrictness;
        }

        public ModModule(ZPackage pkg, bool legacy)
        {
            if (legacy)
            {
                DataLayoutVersion = LegacyDataLayoutVersion;
                ModName = pkg.ReadString();
                int major = pkg.ReadInt();
                int minor = pkg.ReadInt();
                int build = pkg.ReadInt();
                Version = build >= 0 ? new System.Version(major, minor, build) : new System.Version(major, minor);
                CompatibilityLevel = (CompatibilityLevel)pkg.ReadInt();
                VersionStrictness = (VersionStrictness)pkg.ReadInt();
                return;
            }
        
            // Handle deserialization based on dataLayoutVersion
            DataLayoutVersion = pkg.ReadInt();

            if (!this.IsSupportedDataLayout())
            {
                // Data from a newer version of Jotunn has been received and cannot be read.
                throw new NotSupportedException($"{DataLayoutVersion} is not a supported data layout version.");
            }

            if (DataLayoutVersion == 1)
            {
                guid = pkg.ReadString();
                ModName = pkg.ReadString();
                int major = pkg.ReadInt();
                int minor = pkg.ReadInt();
                int build = pkg.ReadInt();
                Version = build >= 0 ? new System.Version(major, minor, build) : new System.Version(major, minor);
                CompatibilityLevel = (CompatibilityLevel)pkg.ReadInt();
                VersionStrictness = (VersionStrictness)pkg.ReadInt();
            }            
        }

        /// <summary>
        ///     Write to ZPkg
        /// </summary>
        /// <param name="pkg"></param>
        /// <param name="legacy"></param>
        public void WriteToPackage(ZPackage pkg, bool legacy)
        {
            if (legacy)
            {
                pkg.Write(ModName);
                pkg.Write(Version.Major);
                pkg.Write(Version.Minor);
                pkg.Write(Version.Build);
                pkg.Write((int)CompatibilityLevel);
                pkg.Write((int)VersionStrictness);
                return;
            }

            pkg.Write(DataLayoutVersion);
            pkg.Write(guid);
            pkg.Write(ModName);
            pkg.Write(Version.Major);
            pkg.Write(Version.Minor);
            pkg.Write(Version.Build);
            pkg.Write((int)CompatibilityLevel);
            pkg.Write((int)VersionStrictness);
        }

        public ModModule(BepInPlugin plugin, NetworkCompatibilityAttribute networkAttribute)
        {
            this.DataLayoutVersion = CurrentDataLayoutVersion;
            this.guid = plugin.GUID;
            this.ModName = plugin.Name;
            this.Version = plugin.Version;
            this.CompatibilityLevel = networkAttribute.EnforceModOnClients;
            this.VersionStrictness = networkAttribute.EnforceSameVersion;
        }

        public ModModule(BepInPlugin plugin)
        {
            this.DataLayoutVersion = CurrentDataLayoutVersion;
            this.guid = plugin.GUID;
            this.ModName = plugin.Name;
            this.Version = plugin.Version;
            this.CompatibilityLevel = CompatibilityLevel.NotEnforced;
            this.VersionStrictness = VersionStrictness.None;
        }

        public string GetVersionString()
        {
            if (Version.Build >= 0)
            {
                return $"{Version.Major}.{Version.Minor}.{Version.Build}";
            }
            else
            {
                return $"{Version.Major}.{Version.Minor}";
            }
        }

        /// <summary>
        ///     Module must at least be loaded on the server
        /// </summary>
        /// <returns></returns>
        public bool IsNeededOnServer()
        {
            return CompatibilityLevel == CompatibilityLevel.EveryoneMustHaveMod || CompatibilityLevel == CompatibilityLevel.ServerMustHaveMod;
        }

        /// <summary>
        ///     Module must at least be loaded on the client
        /// </summary>
        /// <returns></returns>
        public bool IsNeededOnClient()
        {
            return CompatibilityLevel == CompatibilityLevel.EveryoneMustHaveMod || CompatibilityLevel == CompatibilityLevel.ClientMustHaveMod;
        }

        /// <summary>
        ///    Module is not enforced by the server or client
        /// </summary>
        /// <returns></returns>
        public bool IsNotEnforced()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return CompatibilityLevel == CompatibilityLevel.NotEnforced || CompatibilityLevel == CompatibilityLevel.NoNeedForSync;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        ///     Module is not enforced, only version check if both client and server have it
        /// </summary>
        /// <returns></returns>
        public bool OnlyVersionCheck()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return CompatibilityLevel == CompatibilityLevel.OnlySyncWhenInstalled || CompatibilityLevel == CompatibilityLevel.VersionCheckOnly;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        ///     Module is formatted as in one of the supported data layout versions. 
        ///     Should return false is data was received from a newer version of Jotunn.
        /// </summary>
        /// <returns></returns>
        public bool IsSupportedDataLayout()
        {
            return SupportedDataLayouts.Contains(DataLayoutVersion);
        }

        /// <summary>
        ///     Checks if the compare module has a lower version then the other base module
        /// </summary>
        /// <param name="baseModule"></param>
        /// <param name="compareModule"></param>
        /// <param name="strictness"></param>
        /// <returns></returns>
        public static bool IsLowerVersion(ModModule baseModule, ModModule compareModule, VersionStrictness strictness)
        {
            if (strictness == VersionStrictness.None)
            {
                return false;
            }

            bool majorSmaller = compareModule.Version.Major < baseModule.Version.Major;
            bool minorSmaller = compareModule.Version.Minor < baseModule.Version.Minor;
            bool patchSmaller = compareModule.Version.Build < baseModule.Version.Build;

            bool majorEqual = compareModule.Version.Major == baseModule.Version.Major;
            bool minorEqual = compareModule.Version.Minor == baseModule.Version.Minor;

            if (strictness >= VersionStrictness.Major && majorSmaller)
            {
                return true;
            }

            if (strictness >= VersionStrictness.Minor && minorSmaller && (majorSmaller || majorEqual))
            {
                return true;
            }

            if (strictness >= VersionStrictness.Patch && patchSmaller && (minorSmaller || minorEqual) && (majorSmaller || majorEqual))
            {
                return true;
            }

            return false;
        }
    }
}
