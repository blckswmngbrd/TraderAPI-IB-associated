using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using MarketSim;

namespace Technical_System_with_Market_Simulator
{
    class Instrument : System.Windows.Forms.ApplicationContext
    {
        private Market _Sim;

        private Timer _Timer;
        private String _Symbol;
        
    	public event InstrumentUpdateEventHandler OnInstrumentUpdate;
	    public event FillEventHandler OnFill;

        public Instrument( String sym )
        {
            TickList = new List<double>();

            _Symbol = sym;
            
            _Timer = new Timer();
            _Timer.Tick += new EventHandler( GoForward );
            _Timer.Interval = 100;
            _Timer.Enabled = true;
             
            Time m_Time = Time.get_Instance(9, 30, 0);
            
            _Sim = Market.get_Instance( m_Time );
            _Sim.add_Stock(sym);

            _Sim.Latency = 100;
            _Sim.OnFillUpdate += m_Sim_OnFillUpdate;

            //m_Sim.OnCancelUpdate += new MarketSimulator.CancelUpdate_EventHandler( m_Sim_OnCancelUpdate );
        }

        ~Instrument()
        {
	        Debug.WriteLine( "Instrument dying." );
        }

        private void GoForward( Object o, EventArgs e )
        {
            _Sim.TimeSize = 100000;
            _Sim.Advance();
            if (_Sim.Changers.ContainsKey(_Symbol))
            {
                // define price here
                Price = (this.Ask + this.Bid) / 2.0;
                TickList.Add(Price);
                OnInstrumentUpdate(this);
            }
        }
 
        public bool EnterOrder( String m_BS, double m_Qty, String m_FFT )
        {
	        try
	        {
                if (m_BS == "B" )
                    _Sim.Send_OrderRequest(new OrderRequest(_Symbol, Request.RequestType.BUY, OrderRequest.OrderType.MARKET, m_Qty, 0, m_FFT ));
                else
                    _Sim.Send_OrderRequest(new OrderRequest(_Symbol, Request.RequestType.SELL, OrderRequest.OrderType.MARKET, m_Qty, 0, m_FFT ));
		        return true;
	        }
	        catch ( Exception e )
	        {
		        return false;
	        }
        }

        public double Price { get; private set; }
       
        public double Target { get; set; }
       
        public double Stop { get; set; }
        
        public double Position { get; set; }
       
        public List< double > TickList { get; private set; }
       
        
        public double BidQty
        {
            get
            {
                return _Sim.Stocks[_Symbol].BidQty;
            }
        }

        public double Bid
        {
            get
            {
                return _Sim.Stocks[_Symbol].Bid;
            }
        }

        public double AskQty
        {
            get
            {
                return _Sim.Stocks[_Symbol].AskQty;
            }
        }

        public double Ask
        {
            get
            {
                return _Sim.Stocks[_Symbol].Ask;
            }
        }
        
        public double TickSize { get; set; }
        
        private void m_Sim_OnFillUpdate( OrderUpdate f )
        {
            Debug.WriteLine(f.FFT);
            String BS = "B";
            if (f.FillType == Request.RequestType.SELL)
                BS = "S";
            OnFill( this, Convert.ToDouble( f.Qty ), BS, Convert.ToString( f.Price ), Convert.ToString( f.FFT ) );
        }
        
        public void ShutDown()
        {
        }
    }
}
