using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    public class MetaFeatureException : MetaException
    {
        public const string TYPE_FEATURE_EXCEPTION = "Meta Feature Exception";
        private const string FTERR = $"Feature implementation error. This action should have been disabled in front-end. Please report. \nFeature: ";
        public MetaFeatureException(string msg) : base(TYPE_FEATURE_EXCEPTION, FTERR + msg, null) { }
        public MetaFeatureException(string msg, Exception inner) : base(TYPE_FEATURE_EXCEPTION, msg, inner) { }
    }
}
