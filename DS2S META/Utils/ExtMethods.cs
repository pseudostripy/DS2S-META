using PropertyHook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    public static class ExtMethods
    {
        public static byte[]? NopExtend(this byte[] data, int inj_sz)
        {
            if (data.Length > inj_sz)
            {
                MetaExceptionStaticHandler.Raise($"NopExtend input bytes cannot fit in {inj_sz} size inject");
                return null;
            }
                
            // Extend with NOPs
            var xdata = Enumerable.Repeat(Inject.NOP, inj_sz).ToArray(); // preallocate NOPs
            Array.Copy(data, 0x0, xdata, 0x0, data.Length);
            return xdata;
        }
        public static int UDSaturate(this int val, int min, int max)
        {
            if (val < min) val = min;
            if (val > max) val = max;
            return val;
        }
        public static void SetProperty(this object obj, string propName, object? value)
        {
            var pinfo = obj.GetType().GetProperty(propName) ?? throw new Exception($"Property {propName} cannot be found in class {obj.GetType()}");
            pinfo.SetValue(obj, value);
        }
    }
}
