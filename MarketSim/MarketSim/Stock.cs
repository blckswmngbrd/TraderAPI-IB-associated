using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;

namespace MarketSim
{
    public class Stock
    {

        private class Quote
        {
            public double BidQty;
            public double Bid;
            public double Ask;
            public double AskQty;
        };

        ////////// Static list of changers ///////////////////
        
        private static Dictionary< String, int > m_Changers = new Dictionary<string, int >();

        public static Dictionary< String, int > Changers
        {
            get { return m_Changers; }
        }
        
        public static void Clear_Changers()
        {
            m_Changers.Clear();
        }

        /////////////////////////////////////////////////////
 
        private String m_Symbol;
        //       private int Cursor; 
        private int null_value;
        private Dictionary< int, OrderRequest > m_BuyOrders;
        private Dictionary< int, OrderRequest > m_SellOrders;
        private List< int > OrdersToDelete;
        private String FileName;
        private String PathName;

        Quote Current;
        //       bool Scooch;

        private DataSet m_DataSet;
        private OleDbConnection m_Conn;
        //private OleDbDataAdapter m_Adapter;
        private OleDbCommand m_Command;
        private OleDbDataReader m_Reader;

        public delegate void OrderUpdate_EventHandler( Update x );
        public delegate void PriceUpdate_EventHandler( String symbol );

        public event OrderUpdate_EventHandler OnOrderUpdate;

        public Stock( String symbol, Time time )
        {
            OrdersToDelete = new List< int >();
            m_Symbol = symbol;
            m_DataSet = new DataSet();

            m_BuyOrders = new Dictionary< int, OrderRequest >();
            m_SellOrders = new Dictionary< int, OrderRequest >();

            ////////////////////////////////////////
            // FILL IN THE APPROPRIATE PATH HERE! //
            ////////////////////////////////////////

            PathName = "C:\\Temp";
            FileName = "20101101_" + m_Symbol + ".csv";

            Current = new Quote();
            
            m_Conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + PathName + ";Extended Properties=\"text;HDR=No;FMT=Delimited\"");
            m_Conn.Open();

            //m_Command = new OleDbCommand("SELECT * FROM [" + FileName + "] WHERE F3 >= " + time.CurrentTime.ToString(), m_Conn);
            m_Command = new OleDbCommand("SELECT * FROM [" + FileName + "] WHERE F3 BETWEEN " + time.CurrentTime.ToString() + " AND " + (time.CurrentTime + 23400000000).ToString(), m_Conn); //600000000
            m_Reader = m_Command.ExecuteReader();

            m_Reader.Read();
                     
            Current.Bid = Convert.ToDouble( m_Reader[3]);
            Current.Ask = Convert.ToDouble(m_Reader[4]);
            Current.BidQty = Convert.ToDouble(m_Reader[5]);
            Current.AskQty = Convert.ToDouble(m_Reader[6]);
            
            m_Reader.Read(); 
        }

        public bool Close()
        {
            m_Reader.Close();
            return true;
        }

        //private int get_Data_A( Time time )
        //{
        //    m_Adapter = new OleDbDataAdapter("SELECT * FROM [" + FileName + "] WHERE F3 BETWEEN " + time.CurrentTime.ToString() + " AND " + (time.CurrentTime + 600000000).ToString(), m_Conn);
        //    // m_Adapter = new OleDbDataAdapter("SELECT * FROM [" + FileName + "]", m_Conn );  // + " WHERE XXXXX < " + Time, m_Conn);
        //    m_Conn.Open();
        //    m_Adapter.Fill(m_DataSet);
        //    m_Conn.Close();
        //    return 0;
        //}
       
        public void Advance( Time clock )
        {
            //// if the next time in data set is before the current time, the scooch.
            //if ( Convert.ToInt64(m_DataSet.Tables[0].Rows[Cursor + 1][2]) < clock.CurrentTime )
            //{
            //    // Scooch until, because there may be more than one row between now and future time.
            //    while ( Convert.ToInt64(m_DataSet.Tables[0].Rows[Cursor + 1][2]) < clock.CurrentTime )
            //    {
            //        // Therefore, cursor always points to the current state of the stock.
            //        Cursor++;
            //        if ( !Stock.m_Changers.ContainsKey( this.m_Symbol ) )
            //            Stock.m_Changers.Add(this.m_Symbol, null_value );
            //    }
            //}
        
            while ( Convert.ToInt64( m_Reader[ 2 ] ) <= clock.CurrentTime )
            {
                m_Reader.Read();
                if ( !Stock.m_Changers.ContainsKey(this.m_Symbol) )
                    Stock.m_Changers.Add(this.m_Symbol, null_value);

                Current.Bid = Convert.ToDouble(m_Reader[3]);
                Current.Ask = Convert.ToDouble(m_Reader[4]);
                Current.BidQty = Convert.ToDouble(m_Reader[5]);
                Current.AskQty = Convert.ToDouble(m_Reader[6]);
            }
            
            ///////////////////////////////
            // DID ANY BUY ORDERS FILL?  //
            ///////////////////////////////
            
            foreach ( KeyValuePair< int, OrderRequest > x in m_BuyOrders )
            {
                if ( x.Value.OrdType == OrderRequest.OrderType.MARKET )
                {
                    // Fill Market Order

                    Logger.Log.WriteLine("FILLING MARKET ORDER: " + x.Value.Symbol + " " + x.Value.ReqType.ToString() + " " + x.Value.Qty.ToString() + " @ " + this.Ask.ToString() );

                    OnOrderUpdate(new OrderUpdate( m_Symbol, x.Value.ReqType, Update.UpdateType.FILL, clock.ToString(), x.Key, this.Ask, x.Value.Qty, x.Value.FFT ));
                    OrdersToDelete.Add( x.Key );
                }

                // Limit order fill assumptions here.
                if (x.Value.OrdType == OrderRequest.OrderType.LIMIT && x.Value.Price >= this.Ask)
                {
                    // Fill Limit Order

                    Logger.Log.WriteLine("FILLING LIMIT ORDER: " + x.Value.Symbol + " " + x.Value.ReqType.ToString() + " " + x.Value.Qty.ToString() + " @ " + this.Ask.ToString());

                    OnOrderUpdate(new OrderUpdate( m_Symbol, x.Value.ReqType, Update.UpdateType.FILL, clock.ToString(), x.Key, x.Value.Price, x.Value.Qty, x.Value.FFT ));
                    OrdersToDelete.Add( x.Key );
                }
            }
            
            ///////////////////////////////
            // DID ANY SELL ORDERS FILL? //
            ///////////////////////////////

            foreach (KeyValuePair<int, OrderRequest> x in m_SellOrders)
            {
                if ( x.Value.OrdType == OrderRequest.OrderType.MARKET )
                {
                    // Fill Market Order

                    Logger.Log.WriteLine("FILLING MARKET ORDER: " + x.Value.Symbol + " " + x.Value.ReqType.ToString() + " " + x.Value.Qty.ToString() + " @ " + this.Bid.ToString());

                    OnOrderUpdate(new OrderUpdate(m_Symbol, x.Value.ReqType, Update.UpdateType.FILL, clock.ToString(), x.Key, this.Bid, x.Value.Qty, x.Value.FFT));
                    OrdersToDelete.Add( x.Key );
                }

                // Limit order fill assumptions here.
                if (x.Value.OrdType == OrderRequest.OrderType.LIMIT && x.Value.Price <= this.Bid)
                {
                    // Fill Limit Order

                    Logger.Log.WriteLine("FILLING ORDER: " + x.Value.Symbol + " " + x.Value.ReqType.ToString() + " " + x.Value.Qty.ToString() + " @ " + this.Bid.ToString());

                    OnOrderUpdate(new OrderUpdate(m_Symbol, x.Value.ReqType, Update.UpdateType.FILL, clock.ToString(), x.Key, x.Value.Price, x.Value.Qty, x.Value.FFT));
                    OrdersToDelete.Add( x.Key );
                }
            }
            
            foreach (int key in OrdersToDelete) 
            {
                Logger.Log.WriteLine("DELETING FROM ORDER BOOK: " + key.ToString());

                m_BuyOrders.Remove(key);
                m_SellOrders.Remove(key);
            }
            OrdersToDelete.Clear();
 
        }

        public bool Enter_OrderRequest( int orderID, Request m_Request )
        {
            Logger.Log.WriteLine("ORDER REQUEST RECEIVED:  " + orderID.ToString());
            
            if (m_Request.ReqType == OrderRequest.RequestType.CANCEL)
            {
                return Cancel_Order( ( CancelRequest ) m_Request );
            }
            else
            {
                if (m_Request.ReqType == OrderRequest.RequestType.BUY)
                {
                    // Add to buy book
                    m_BuyOrders.Add(orderID, ( OrderRequest ) m_Request);
                }
                else
                {
                    // Add to sell book
                    m_SellOrders.Add(orderID, ( OrderRequest ) m_Request);
                }
                return true;
            }
        }

        public int OrderPresent(int orderID)
        {
            int found = 0;
            if ( m_BuyOrders.ContainsKey( orderID ))
            {
                found = 2;
            }
            if ( m_SellOrders.ContainsKey( orderID ))
            {
                found = 3;
            }
            return found;
        }


        private bool Cancel_Order( CancelRequest x )
        {
            bool found = false;

            Logger.Log.WriteLine("CANCELING ORDER:  " + m_BuyOrders.ContainsKey(x.ID).ToString());
           
            found = m_BuyOrders.Remove( x.ID );

            if( !found )
            {
                found = m_SellOrders.Remove( x.ID );
            }

            if ( found )
            {
                OnOrderUpdate( new CancelUpdate( m_Symbol, Update.UpdateType.CANCEL, x.ID, x.FFT ) );
            }

            return found;
        }

        public double Bid
        {
            get { return Current.Bid; }
            //get { return Convert.ToDouble( m_DataSet.Tables[0].Rows[ Cursor ][3] ); }
        }
        public double Ask
        {
            get { return Current.Ask; }
            //get { return Convert.ToDouble( m_DataSet.Tables[0].Rows[ Cursor ][4]); }
        }
        public double BidQty
        {
            get { return Current.BidQty; }
            //get { return Convert.ToDouble( m_DataSet.Tables[0].Rows[ Cursor ][5]); }
        }
        public double AskQty
        {
            get { return Current.AskQty; }
            //get { return Convert.ToDouble( m_DataSet.Tables[0].Rows[ Cursor ][6]); }
        }
        //public DataTable Table
        //{
        //    get { return m_DataSet.Tables[0]; }
        //}
    }
}
