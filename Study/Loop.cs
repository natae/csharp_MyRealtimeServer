using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Study
{
    internal class Loop
    {
        public static void StartFor(int count)
        {
            var recorder = new Recorder();
            var temp = 0;

            var list = new List<int>();
            for (var i = 0; i < count; i++)
            {
                list.Add(i);
            }

            recorder.Start();
            for (var i = 0; i < count; i++)
            {
                temp += list[i];
            }
            recorder.Stop();
        }

        public static void StartForeach(int count)
        {
            var recorder = new Recorder();
            var temp = 0;

            var list = new List<int>();
            for (var i = 0; i < count; i++)
            {
                list.Add(i);
            }

            recorder.Start();
            foreach (var val in list)
            {
                temp += val;
            }
            recorder.Stop();
        }
    }
}
