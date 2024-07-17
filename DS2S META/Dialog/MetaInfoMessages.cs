using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Dialog
{
    public static class MetaInfoMessages
    {
        public static void RivaNotFound(string rivaExePath)
        {
            // Riva not found
            string msg = @$"Cannot find RIVA at expected location:
{rivaExePath}
If you want to enable this feature, please paste the full RIVA.exe path
in to the Settings tab textbox next time you open Meta.
                                
Reverting to unhooking RIVA the slow way";
            MetaInfoWindow.ShowMetaInfo(msg);
        }

        public static void OldBbjNotImplemented()
        {
            string msg = @"The base pointer setup has not yet been implemented properly for old bbj mod. 

This is a work in progress, or the feature will be deprecated entirely.";

            MetaInfoWindow.ShowMetaInfo(msg);
        }
        
    }
}
