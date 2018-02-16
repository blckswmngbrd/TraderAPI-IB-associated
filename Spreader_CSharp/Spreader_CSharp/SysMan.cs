using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spreader_CSharp
{
    enum Position
    {
        FLAT, LONG, SHORT
    };


    class SysMan
    {

        private Instrument _Instr_A;
        private Instrument _Instr_B;

        public SortedList BuyOrderBook { get; private set; }
        public SortedList SellOrderBook { get; private set; }

        private ArrayList _PriceList;

        public Position Pos_A { get; private set; }
        public Position Pos_B { get; private set; }
        public Position SpreadPos { get; private set; }


        public double Ask_A { get; private set; }
        public double Bid_A { get; private set; }
        public double Ask_B { get; private set; }
        public double Bid_B { get; private set; }

        public double SpreadPrice { get; private set; }
        public double SpreadPosPrice { get; private set; }
        public double NormPrice { get; private set; }
        public double StopPrice { get; private set; }
        public double TargetPrice { get; private set; }

        public double FillPrice_A { get; private set; }
       
        public event UpdateEventHandler OnPriceUpdate;
	    public event FillEventHandler OnFillUpdate;

        public bool Go { get; set; }

       public SysMan()
       {
            _Instr_A = new Instrument( "CME", "ES", "Future", "Sep16", 1250.00 );
            _Instr_A.OnInstrumentUpdate += OnInstrUpdate;
		    _Instr_A.OnFill += OnFill_A;
	
	        _Instr_B = new Instrument( "CME", "NQ", "Future", "Sep16", 5000.00 );
            _Instr_B.OnInstrumentUpdate += OnInstrUpdate;
		    _Instr_B.OnFill += OnFill_B;

        	// SortedLists will keep track of working orders in the market.
	        BuyOrderBook = new SortedList();
	        SellOrderBook = new SortedList();

	        // An ArrayList will keep track of the last 30 bid/ask changes.
	        _PriceList = new ArrayList();

	        // Start with flat positions, obviously.
	        SpreadPos = Position.FLAT;
	        Pos_A = Position.FLAT;
	        Pos_B = Position.FLAT;
        }

        ~SysMan()
        {
            _Instr_A = null;
            _Instr_B = null;
        }


        private void OnInstrUpdate()
        {
       
            // Get latest Bid/Ask data.
            Bid_A = _Instr_A.Bid;
            Ask_A = _Instr_A.Ask;

            Bid_B = _Instr_B.Bid;
            Ask_B = _Instr_B.Ask;

            // Calculate the spread Bid/Ask for 1x2 and add it to the list.
            SpreadPrice = -1 * Bid_A + 2 * Ask_B;
            _PriceList.Add(SpreadPrice);

            // Calculate the normalized price.
            NormPrice = NormCalc.CalcNormalizedPrice(_PriceList);
            
            if (Go)
            {
                // Have we hit a stop or have we hit our target?  If so, close positions.
                if (SpreadPos == Position.LONG && (SpreadPrice <= StopPrice || NormPrice > 0))
                {
                    bool m_Bool = _Instr_A.EnterMarketOrder("B", 1, "CLOSE");
                    m_Bool = _Instr_B.EnterMarketOrder("S", 2, "CLOSE");
                }
                if (SpreadPos == Position.SHORT && (SpreadPrice >= StopPrice || NormPrice < 0))
                {
                    bool m_Bool = _Instr_A.EnterMarketOrder("S", 1, "CLOSE");
                    m_Bool = _Instr_B.EnterMarketOrder("B", 2, "CLOSE");
                }

                // If the reason for buying or selling the spread no longer exists, 
                // cancel the working order.  i.e. if we missed the trade, cancel it.
                if (NormPrice < 2 && BuyOrderBook.Count > 0)
                {
                    // Cancel buy order and remove from order book.
                    _Instr_A.CancelOrder(((Order)(BuyOrderBook.GetByIndex(0))).Key);
                    BuyOrderBook.RemoveAt(0);
                }
                if (NormPrice > -2 && SellOrderBook.Count > 0)
                {
                    // Cancel sell order and remove from order book.
                    _Instr_A.CancelOrder(((Order)(SellOrderBook.GetByIndex(0))).Key);
                    SellOrderBook.RemoveAt(0);
                }

                // Make a decision as to whether or not to enter a trade.
                // Make a long trade in A if normalized price > 2 and we are 
                // flat and not already working an order.
                // Enter order method calls run on the Strategy thread.
                if (NormPrice > 2 && Pos_A == Position.FLAT && BuyOrderBook.Count == 0)
                {
                    // Try to buy 1 on the bid.
                    Order m_Order = _Instr_A.EnterLimitOrder("B", 1, Bid_A, "OPEN");
                    BuyOrderBook.Add(m_Order.Key, m_Order);
                }

                // Make a short in A if normalized price < -2 and we are flat and not already working an order.
                if (NormPrice < -2 && Pos_A == Position.FLAT && SellOrderBook.Count == 0)
                {
                    // Try to sell 1 on the ask.
                    Order m_Order = _Instr_A.EnterLimitOrder("S", 1, Ask_A, "OPEN");
                    SellOrderBook.Add(m_Order.Key, m_Order);
                }
            }

            // Update the form on the main thread.
            OnPriceUpdate();
        }


        private void OnFill_A(String product, String key, String BS, int qty, int price, String time)
        {
            // When fill is received in A (ES), enter market order in B (NQ).
            // This method will run on the RTD thread.

            FillPrice_A = price;

            if (Pos_A == Position.FLAT)
            {
                // This is an openning trade.  Place trade in Instrument_B (NQ)
                // in the opposite direction.
                if (BS == "B")
                {
                    Pos_A = Position.LONG;
                    BuyOrderBook.Remove( key );
                    bool m_Bool = _Instr_B.EnterMarketOrder("S", 2, "OPEN");
                }
                else
                {
                    Pos_A = Position.SHORT;
                    SellOrderBook.Remove( key );
                    bool m_Bool = _Instr_B.EnterMarketOrder("B", 2, "OPEN");
                }
            }
            else
            {
                // this is a closing trade.
                Pos_A = Position.FLAT;
            }
            // Update form on Main thread.
            OnFillUpdate(product, key, BS, qty, price, time);
        }

        private void OnFill_B(String product, String key, String BS, int qty, int price, String time)
        {
            // When fill is received in B, update spread position and set bracket.
            // This method will run on the RTD thread.

            if (Pos_B == Position.FLAT)
            {
                // This is an openning trade.
                if ( BS == "B")
                {
                    Pos_B = Position.LONG;
                }
                else
                {
                    Pos_B = Position.SHORT;
                }

                // Set spread position, spread price and stop price.
                if (Pos_A == Position.LONG && Pos_B == Position.SHORT)
                {
                    SpreadPos = Position.SHORT;
                    SpreadPosPrice = -1 * FillPrice_A + 2 * price;
                    StopPrice = SpreadPosPrice + 4 * _Instr_A.TickSize();

                }

                if (Pos_A == Position.SHORT && Pos_B == Position.LONG)
                {
                    SpreadPos = Position.LONG;
                    SpreadPosPrice = -1 * FillPrice_A + 2 * price;
                    StopPrice = SpreadPosPrice - 4 * _Instr_A.TickSize();
                    // Debug.WriteLine( "SPREAD PX: " + m_SpreadPosPrice.ToString() + "    STOP PX: " + m_StopPrice.ToString() );
                }
            }
            else
            {
                // This is a closing trade.

                Pos_B = Position.FLAT;
                if (Pos_A == Position.FLAT && Pos_B == Position.FLAT)
                {
                    SpreadPos = Position.FLAT;
                    StopPrice = 0;
                    SpreadPosPrice = 0;
                }
            }
            // Update the form on the Main thread.
            OnFillUpdate( product, key, BS, qty, price, time);
        }
    }
}
