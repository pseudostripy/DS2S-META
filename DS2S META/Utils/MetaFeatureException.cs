using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    public class MetaFeatureException : Exception
    {
        private const string FTERR = $"Feature implementation error. This action should have been disabled in front-end. Please report. \nFeature: ";
        public MetaFeatureException(string msg) : base(FTERR + msg) { }
        public MetaFeatureException(string msg, Exception inner) : base(msg, inner) { }
    }
}
