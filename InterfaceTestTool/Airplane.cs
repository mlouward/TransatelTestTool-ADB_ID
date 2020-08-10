﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceTestTool
{
    internal class Airplane : ITest
    {
        public int From { get; set; }
        public int Delay { get; set; }
        public int Duration { get; set; }

        public Airplane(int from, int delay, int duration)
        {
            From = from;
            Delay = delay;
            Duration = duration;
        }

        public string WriteCsv()
        {
            return $"airplane;{From};{Duration};{Delay}";
        }

        public override string ToString()
        {
            return $"Airplane: {Duration}sec., Delay: {Delay}sec.";
        }
    }
}