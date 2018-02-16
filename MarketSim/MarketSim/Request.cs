using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketSim
{
    public class Request
    {
        public enum RequestType
        {
            BUY,
            SELL,
            CANCEL
        };

        public String FFT { get; private set; }
        public int ID { get; private set; }
        public RequestType ReqType { get; private set; }
        
        public String Symbol { get; private set; }
      
        public long ActiveTime { get; private set; }
       
        public Request( String symbol, RequestType request_type, int id, String fft )
        {

            ReqType = request_type;
            Symbol = symbol;
            FFT = fft;
            ID = id;
        }

        public void Add_Latency( Time clock, int latency )
        {
             ActiveTime = clock.CurrentTime + latency;
        }

 
    }
}
