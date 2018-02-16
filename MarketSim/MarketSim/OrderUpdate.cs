using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketSim
{
    public class OrderUpdate : Update
    {
        private double m_Qty;
        private Request.RequestType m_FillType;
        private String m_FillTime;
        private double m_Price;

        public OrderUpdate( String sym, Request.RequestType fill_type, Update.UpdateType upt, String time, int id, double px, double q, String FFT )
            : base(sym, upt, id, FFT )
        {
            m_Price = px;
            m_FillTime = time;
            m_FillType = fill_type;
            m_Qty = q;
        }

        public double Price
        {
            get { return m_Price; }
        }

        public String FillTime
        {
            get { return m_FillTime; }
        }

        public double Qty
        {
            get { return m_Qty; }
        }

        public Request.RequestType FillType
        {
            get { return m_FillType; }
        }
    }
}
