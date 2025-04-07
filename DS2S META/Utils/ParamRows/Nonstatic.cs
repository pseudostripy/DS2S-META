using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    public class Nonstatic
    {

        public int testing { get; set; }
        public List<MyTestClass> MyClasses { get; set; }

        public Nonstatic(int x)
        {
            testing = x + 10;
            MyClasses = new List<MyTestClass>();
            MyClasses.Add(new MyTestClass(x));
            MyClasses.Add(new MyTestClass(12));
        }
    }
}
