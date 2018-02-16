using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MarketSim
{
    public class OrderRequest : Request
    {
        public enum OrderType
        {
            MARKET,
            LIMIT
        };

        public OrderType OrdType { get; private set; }

        public double Price { get; private set; }

        public double Qty { get; private set; }
        
        public OrderRequest( String symbol, Request.RequestType request_type, OrderRequest.OrderType order_type, double qty, double price = 0, String FFT = "000" ) : base( symbol, request_type, 0, FFT )
        {
            OrdType = order_type;
            Price = price;
            //m_Time = new Time(clock.CurrentTime);
            Qty = qty;
            
            //Debug.WriteLine("START:  OrderTime: " + Time.TimeToString( clock.CurrentTime ) + "  " + "ActiveTime: " + Time.TimeToString( m_ActiveTime.CurrentTime ));
        } 
    }
}
