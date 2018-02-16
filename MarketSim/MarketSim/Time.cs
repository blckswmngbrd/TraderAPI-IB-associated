using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketSim
{
    public class Time
    {
        private double m_Hour;
        private double m_Minute;
        private double m_Seconds;
        private long m_StartTime;
        private long m_CurrentTime;
        
        /// //////////Singleton Instance//////////////////////////////////////////////

        private static Time Instance;

        public static Time get_Instance( double H, double M, double S )
        {
            if (Instance == null)
            {
                Instance = new Time( H, M, S );
            }
            return Instance;
        }

        // Private constructor

        private Time(double H, double M, double S)
        {
            m_Hour = H;
            m_Minute = M;
            m_Seconds = S;
            m_StartTime = Convert.ToInt64( ( m_Minute / 60.0 + m_Hour ) * 36.0 * Math.Pow( 10.0, 8.0 ) + m_Seconds * Math.Pow( 10.0, 6.0 ) );
            m_CurrentTime = m_StartTime;
        }

        // Cant figure this out.

        //public Time( long current )
        //{
        //    m_StartTime = current;
        //    m_CurrentTime = m_StartTime;
        //}
        
        public static String TimeToString( long time )
        {
            //Hour =INT(C2/3600000000)
            //Minute =INT((C2/3600000000-D2)*60)
            //Second =(C2/60000000-INT(C2/60000000))*60 

            double Hour = Math.Floor( time / 3600000000.0 );
            double Minutes = Math.Floor( ( time / 3600000000.0 - Hour ) * 60.0 );
            double Seconds = ( time / 60000000.0 - Math.Floor( time / 60000000.0 ) ) * 60.0;
            return Hour.ToString( "00" ) + ":" + Minutes.ToString( "00 ") + ":" + Seconds.ToString( "00.00000000" );        
        }

        public override String ToString()
        {
            double Hour = Math.Floor(m_CurrentTime / 3600000000.0);
            double Minutes = Math.Floor((m_CurrentTime / 3600000000.0 - Hour) * 60.0);
            double Seconds = (m_CurrentTime / 60000000.0 - Math.Floor(m_CurrentTime / 60000000.0)) * 60.0;
            return Hour.ToString("00") + ":" + Minutes.ToString("00") + ":" + Seconds.ToString("00.00000000");    
        }

        public long StartTime
        {
            get { return m_StartTime; }
        }
        
        public long CurrentTime
        {
            get { return m_CurrentTime; }
            set { m_CurrentTime = value; }
        }
    }
}
