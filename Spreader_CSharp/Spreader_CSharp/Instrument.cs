using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TraderAPI;


namespace Spreader_CSharp
{
    delegate void UpdateEventHandler();
    delegate void FillEventHandler(String product, String key, String BS, int qty, int price, String time);

    class Instrument
    {
        private InstrNotifyClass m_Notify;
        private InstrObjClass m_Instr;
        private OrderSetClass m_OrderSet;



        public event UpdateEventHandler OnInstrumentUpdate;
	    public event FillEventHandler OnFill;
        
        public Instrument(string exch, string product, string type, string contract, double price)
        {
           	// Create a new InstrObjClass object
			m_Instr = new InstrObjClass( exch, product, InstrObjClass.ProdType.FUTURE, contract, price );

			// Create a new InstrNotifyClass object from the InstrObjClass object.
			m_Notify = ( InstrNotifyClass ) m_Instr.CreateNotifyObj;
			// Enable price updates.
			m_Notify.EnablePriceUpdates = true;
			// Set UpdateFilter so event will fire anytime any one of these changes in the 
			// associated InstrObjClass object.
			m_Notify.UpdateFilter = "LAST,LASTQTY";
			// Subscribe to the OnNotifyUpdate event.
			m_Notify.OnNotifyUpdate += new InstrNotifyClass.OnNotifyUpdateEventHandler( OnNotifyUpdate );
			// Set the exchange, product, contract and product type.

			// Open m_Instr.
			m_Instr.Open(true);
	
			// Create a new OrderSetClass object.
			m_OrderSet = new OrderSetClass();
			// Set the limits accordingly.  If any of these limits is reached, 
			// trading through the API will be shut down automatically.
			m_OrderSet.set_Set( "MAXORDERS", 1000 );
			m_OrderSet.set_Set( "MAXORDERQTY", 1000 );
			m_OrderSet.set_Set( "MAXWORKING", 1000 );
			m_OrderSet.set_Set( "MAXPOSITION", 1000 );
			// Enable deleting of orders.  Enable the OnOrderFillData event.  Enable order sending.
			m_OrderSet.EnableOrderAutoDelete = true;
			m_OrderSet.EnableOrderFillData = true;
			m_OrderSet.EnableOrderSend = true;
			// Subscribe to the OnOrderFillData event.
			m_OrderSet.OnOrderFillData += new OrderSetClass.OnOrderFillDataEventHandler( OnOrderFillData );
			// Open the m_OrderSet.
			m_OrderSet.Open(true);
			// Associate m_OrderSet with m_Instr.
			m_Instr.OrderSet = m_OrderSet;
        }

        private void OnNotifyUpdate( InstrNotifyClass pNotify, InstrObjClass pInstr )
        {
	        OnInstrumentUpdate();
        }

        public bool EnterMarketOrder( String BS, double qty, String FFT )
        {
	        try
	        {
		        OrderProfileClass m_Profile = new OrderProfileClass();
		        m_Profile.Instrument = m_Instr;
		        m_Profile.set_Set( "ACCT", "12345" );
		        m_Profile.set_Set( "BUYSELL", BS );
		        m_Profile.set_Set( "ORDERTYPE", "M" );
		        m_Profile.set_Set( "ORDERQTY", qty.ToString() );
		        m_Profile.set_Set( "FFT", FFT );
		        long  myResult = m_OrderSet.SendOrder( m_Profile );
		        return true;
	        }
	        catch ( Exception e )
	        {
		        return false;
	        }
        }
        public Order EnterLimitOrder(String BS, double qty, double price, String FFT)
        {
            try
            {
                OrderProfileClass m_Profile = new OrderProfileClass();
                m_Profile.Instrument = m_Instr;
                m_Profile.set_Set("ACCT", "12345");
                m_Profile.set_Set("BUYSELL", BS);
                m_Profile.set_Set("ORDERTYPE", "L");
                m_Profile.set_Set("LIMIT", price.ToString());
                m_Profile.set_Set("ORDERQTY", qty.ToString());
                m_Profile.set_Set("FFT", FFT);
                long myResult = m_OrderSet.SendOrder(m_Profile);

                return new Order(m_Profile.get_GetLast("SITEORDERKEY"), price);
            }
            catch (Exception e)
            {
                return null;
            }
        }
        private void OnOrderFillData( FillObj m_Fill )
        {
            //(String product, String key, String BS, int qty, int price, String time)

	        OnFill( Convert.ToString( m_Fill.get_Get( "PRODUCT" ) ),
		            Convert.ToString( m_Fill.get_Get( "KEY" ) ),
			        Convert.ToString( m_Fill.get_Get( "BUYSELL" ) ),
                    Convert.ToInt32( m_Fill.get_Get( "QTY" ) ),
                    Convert.ToInt32( m_Fill.get_Get( "PRICE" ) ),
                    Convert.ToString( m_Fill.get_Get( "TIME" ) ) );                   
        }

        public void CancelOrder( String ID )
        {
            m_OrderSet.DeleteOrders(ID);
        }
        public double Bid
        {
            get { return Convert.ToDouble( m_Instr.get_Get("BID") ); }
        }

        public double Ask
        {
            get { return Convert.ToDouble( m_Instr.get_Get("ASK") ); }
        }

       public double TickSize()
       {
           return m_Instr.TickSize;
       }

        public void ShutDown()
        {
	        m_Notify.OnNotifyUpdate -= new InstrNotifyClass.OnNotifyUpdateEventHandler(OnNotifyUpdate);
	        m_OrderSet.OnOrderFillData -= new OrderSetClass.OnOrderFillDataEventHandler(OnOrderFillData );
	        m_Notify = null;
	        m_Instr = null;
	        m_OrderSet = null;
        }
    }
}
