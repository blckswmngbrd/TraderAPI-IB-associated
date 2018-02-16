using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MarketSim
{
    public class Logger
    {
        public static StreamWriter Log = new StreamWriter("C:\\Temp\\BacktestLogFile.txt");
    }
}
