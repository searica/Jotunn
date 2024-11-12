using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jotunn.Utils
{
    internal class ServerVersionData
    {
        public ModuleVersionData moduleVersionData { get; set; }
        //public HashSet<string> moduleNames { get; set; }
        public HashSet<string> moduleGUIDs { get; set; }

        /// <summary>
        ///     Create empty ServerData
        /// </summary>
        internal ServerVersionData() { }

        /// <summary>
        ///     Create from module data
        /// </summary>
        /// <param name="versionData"></param>
        internal ServerVersionData(List<ModModule> versionData)
        {
            moduleVersionData = new ModuleVersionData(versionData);
            moduleGUIDs = new HashSet<string>(moduleVersionData.Modules.Select(mod => mod.guid).ToList());
        }

        internal ServerVersionData(System.Version valheimVersion, List<ModModule> versionData)
        {
            moduleVersionData = new ModuleVersionData(valheimVersion, versionData);
            moduleGUIDs = new HashSet<string>(moduleVersionData.Modules.Select(mod => mod.guid).ToList());
        }

        /// <summary>
        ///     Create from ZPackage
        /// </summary>
        /// <param name="pkg"></param>
        internal ServerVersionData(ZPackage pkg)
        {
            moduleVersionData = new ModuleVersionData(pkg);
            moduleGUIDs = new HashSet<string>(moduleVersionData.Modules.Select(mod => mod.guid).ToList());
        }

        internal bool IsValid()
        {
            return moduleVersionData != null && moduleGUIDs != null;
        }

        internal void Reset()
        {
            moduleVersionData = null;
            moduleGUIDs = null;
        }
    }
}
