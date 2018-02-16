using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketSim
{
    public class Update
    {
        public enum UpdateType
        {
            FILL,
            CANCEL
        }

        public String FFT { get; private set; }
     
        public int ID { get; private set; }

        public UpdateType Update_Type { get; private set; }

        public String Symbol { get; private set; }

        public long ActiveTime { get; private set; }

        public Update( String sym, UpdateType update_type, int id, String fft )
        {
            FFT = fft;
            ID = id;
            Symbol = sym;
            Update_Type = update_type;
        }

        public void Add_Latency( Time clock, int latency )
        {
            ActiveTime = clock.CurrentTime + latency;
        }


    }
}
