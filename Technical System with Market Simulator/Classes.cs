using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Technical_System_with_Market_Simulator
{
    delegate void InstrumentUpdateEventHandler( Instrument pInstr );
    delegate void UpdateEventHandler( double a, double b, double c, double d, double e, double f, double g, double h );
    delegate void FillEventHandler( Instrument pInstr, double a, String b, String c, String f );
    
    enum MA_State
    {   
	    // ABOVE means that the short MA is above the long MA
	    // BELOW means that the short MA is below the long MA
	    ABOVE, BELOW
    }

    enum Position
    {
	    FLAT, LONG, SHORT
    }

    class Tick
    {
        private double m_Price;
	    private double m_Qty;
	    private DateTime m_Time;

        public Tick( DateTime time, double price, double qty  )
        {
            m_Price = price;
            m_Qty = qty;
            m_Time = time;
        }

	    ~Tick() {;}

    	public double Price
	    {
            get
	        {
		        return m_Price;
	        }
    	}

	    public double Qty
	    {
            get
	        {
		        return m_Qty;
	        }
	    }
	    
        public DateTime Time
	    {
            get
	        {
		        return m_Time;
	        }
        }
    }

    
}
