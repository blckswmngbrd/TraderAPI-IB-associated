using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketSim
{
    public class CancelRequest : Request
    {
        public CancelRequest( String symbol, Request.RequestType request_type, int id, String fft = "000" ) : base( symbol, request_type, id, fft )
        {
        }
    }
}
