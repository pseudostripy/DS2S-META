using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    public class UpdateIni
    {
        public string ThisVer { get; set; }
        public string UpdatePath { get; set; }

        public UpdateIni(string thisver, string updatepath)
        {
            ThisVer = thisver;
            UpdatePath = updatepath;
        }
    }
}
