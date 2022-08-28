using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Study
{
    internal class Recorder
    {
        Stopwatch _timer = new Stopwatch();
        long _bytesPhysicalBefore = 0;        

        public void Start()
        {            
            _timer.Reset();

            var process = Process.GetCurrentProcess();
            _bytesPhysicalBefore = process.WorkingSet64;            

            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();

            var process = Process.GetCurrentProcess();
            var bytesPhysicalAfter = process.WorkingSet64;

            Console.WriteLine($"{bytesPhysicalAfter - _bytesPhysicalBefore} physical bytes used.");
            Console.WriteLine($"{_timer.ElapsedTicks} total ticks ellapsed.");

            Console.WriteLine("--------------------------------------");
        }
    }
}
