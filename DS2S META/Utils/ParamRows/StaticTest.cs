using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    public class StaticTest
    {
        public static List<MyTestClass>? MyClasses {  get; set; }

        public static void initialize()
        {
            MyClasses = new List<MyTestClass>();
            MyClasses.Add(new MyTestClass(11));
            MyClasses.Add(new MyTestClass(12));
        }
    }
}
