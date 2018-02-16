using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace MarketSim
{
    public class Market
    {
        private Dictionary< String, Stock > m_Stocks;
        private SortedDictionary< int, Request > m_RequestLatencyBook;
        private SortedDictionary< int, Update > m_UpdateLatencyBook;
        private List< int > m_OrdersToDelete;
        private List< int > m_UpdatesToDelete;
        private long m_TimeSize;
        private int order_count;
        private Time m_Clock;
        private int m_Latency;

        public Dictionary<String, Stock> Stocks
        {
            get { return m_Stocks; }
        }

        public int Latency
        {
            get { return m_Latency; }
            set { m_Latency = value; }
        }

        public long TimeSize
        {
            get { return m_TimeSize; }
            set { m_TimeSize = value; }
        }
     
        public Time Clock
        {
            get { return m_Clock; }
        }

        public Dictionary< String, int > Changers
        {
            get { return Stock.Changers; }
        }
        
        public delegate void FillUpdate_EventHandler( OrderUpdate update );
        public delegate void CancelUpdate_EventHandler( CancelUpdate update );
        public delegate void PriceUpdate_EventHandler( String symbol );

        public event FillUpdate_EventHandler OnFillUpdate;
        public event CancelUpdate_EventHandler OnCancelUpdate;
        // public event PriceUpdate_EventHandler OnPriceUpdate;
        
        /// //////////Singleton Instance//////////////////////////////////////////////
 
        private static Market Instance;

        public static Market get_Instance( Time T )
        {
            if (Instance == null)
            {
                Instance = new Market( T );
            }
            return Instance;
        }
                
        /// Singleton Private constructor
       
        private Market( Time T )
        {
            Logger.Log.AutoFlush = true;

            m_Clock = T;  // new Time(T.CurrentTime);
            m_Stocks = new Dictionary< string, Stock >();
            m_RequestLatencyBook = new SortedDictionary< int, Request >();
            m_UpdateLatencyBook = new SortedDictionary< int, Update >();
            m_OrdersToDelete = new List< int >();
            m_UpdatesToDelete = new List< int >();

            // Start order count at 1000
            order_count = 1000; 
        }

        public bool add_Stock( String symbol )
        {
            try
            {
                Stock m_Stock = new Stock(symbol, m_Clock);
                m_Stock.OnOrderUpdate += new Stock.OrderUpdate_EventHandler(m_Stock_OnOrderUpdate);
                m_Stocks.Add(symbol, m_Stock);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
                   
        public void ShutDown()
        {
            Logger.Log.Close();

            foreach (KeyValuePair<String, Stock> x in m_Stocks)
            {
                x.Value.Close();
            }
        }

        public void m_Stock_OnOrderUpdate( Update update )
        {

            Logger.Log.WriteLine("UPDATE RECEIVED: " + update.Symbol + " " + update.Update_Type.ToString());

            update.Add_Latency( m_Clock, m_Latency );
            m_UpdateLatencyBook.Add( update.ID, update );
        }

        public int Send_OrderRequest( Request m_request )   
        {
            order_count++;

            Logger.Log.WriteLine( "REQUEST RECEIVED: " + order_count.ToString() + " " + m_request.Symbol + " " + m_request.ReqType.ToString() );

            ///////////////////////////////////////////////////
            //  Put requests into purgatory                  //
            ///////////////////////////////////////////////////

            m_request.Add_Latency( m_Clock, m_Latency );
            m_RequestLatencyBook.Add( order_count, m_request );        

            ///////////////////////////////////////////////////
            // QA:  Cancel request found?                    //
            ///////////////////////////////////////////////////

            if ( m_request.ReqType == Request.RequestType.CANCEL )
            {
                int found;

                // Found in Latency Book?
                if (m_RequestLatencyBook.ContainsKey(((CancelRequest)m_request).ID))
                {
                    found = 1;
                }
                else
                {
                    // Found in stock order book?
                    found = m_Stocks[ m_request.Symbol ].OrderPresent( ( ( CancelRequest ) m_request ).ID );
                }
                return found;
            }
            else
            {
                // Not found?
                return order_count;
            }
        }

        /// Advance method moves the clock forward by m_TimeSize
        
	    public void Advance()
	    {
            Stock.Clear_Changers();

            m_Clock.CurrentTime += m_TimeSize;

            //////////////////////////////////////////
            // Let order requests out of purgatory  //
            //////////////////////////////////////////

            foreach (KeyValuePair<int, Request> x in m_RequestLatencyBook)
            {
                if ( m_Clock.CurrentTime >= x.Value.ActiveTime )
                {
                    m_Stocks[ x.Value.Symbol ].Enter_OrderRequest( x.Key, x.Value );
                    m_OrdersToDelete.Add( x.Key );
                }
            }

            /////////////////////////////////////////////////
            // Remove orders and cancels for latency books //
            /////////////////////////////////////////////////

            foreach ( int key in m_OrdersToDelete )
            {
                m_RequestLatencyBook.Remove(key);
            }
            m_OrdersToDelete.Clear();

            //////////////////////////////////
            // Let updates out of purgatory //
            //////////////////////////////////

            foreach ( KeyValuePair<int, Update> x in m_UpdateLatencyBook )
            {
                if ( m_Clock.CurrentTime >= x.Value.ActiveTime )
                {
                    if ( x.Value.Update_Type == Update.UpdateType.FILL )
                    {
                        OnFillUpdate( ( OrderUpdate ) x.Value );
                    }
                    else
                    {
                        OnCancelUpdate( ( CancelUpdate ) x.Value );
                    }
                    m_UpdatesToDelete.Add(x.Key);
                }
            }

            /////////////////////////////////////////////////
            // Remove orders and cancels for latency books //
            /////////////////////////////////////////////////

            foreach (int key in m_UpdatesToDelete)
            {
                Logger.Log.WriteLine("DELETING CANCEL: " + key.ToString());
                m_UpdateLatencyBook.Remove( key );
            }
            m_UpdatesToDelete.Clear();

            /////////////////////////////////////////////////
            // Advance each stocks clock.                  //
            /////////////////////////////////////////////////
		    foreach( KeyValuePair< String, Stock > x in m_Stocks )
            {
                x.Value.Advance( m_Clock );
            }
        }   
    }
}
