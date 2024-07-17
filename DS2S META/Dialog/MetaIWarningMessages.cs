using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Dialog
{
    public static class MetaWarningMessages
    {
        public static void BasePtrNotFound(DS2VER ver)
        {
            // Riva not found
            string msg = $"Cannot find BasePtr for Version: {ver}, META likely won't work. Possible alleviations are to " +
                $"verify steam game files, check you're using the newest bbj mod dll and check that no other mods are editing " +
                $"the game code. If you're still having issues, ping pseudostripy on Discord and we can investigate.";
            MetaWarningWindow.ShowMetaWarning(msg);
        }
        
    }
}
