using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServerBase
{
    internal class Util
    {
        public static long Now 
        { 
            get
            {
                return (long) DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            } 
        }
    }
}
