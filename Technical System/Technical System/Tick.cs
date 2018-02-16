using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Technical_System
{
    class Tick
    {
        public double Price;
	    public double Qty;
	    public DateTime Time;

        public Tick( DateTime T, double P, double Q )
        {
            Time = T;
            Qty = Q;
	        Price = P;
        }
    }
}
