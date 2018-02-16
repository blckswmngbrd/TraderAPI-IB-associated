using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TradeMatching;

namespace Technical_System
{
    class SystemManager
    {
        private Instrument m_Instrument;
	    private List< Tick > m_TickList;

    	private bool m_Go;
	    private bool m_Start;

    	private double m_LongMA;
	    private double m_ShortMA;
	    private int m_LongMATicks;
	    private int m_ShortMATicks;
	    
        private int m_Position;
        private int m_NetPos;

        private bool m_Bool;
	    private MA_State m_State;

	    private double m_Qty;

	    private double m_Target;
	    private double m_Stop;
	    private double m_TargetTicks;
	    private double m_StopTicks;

        public event UpdateEventHandler OnSystemUpdate;
	    //public event FillEventHandler OnFill;

        private TradeMatcher m_Matcher;

        public SystemManager()
        {
            m_Matcher = new TradeMatcher( RoundTurnMethod.FIFO );

	        // Create a new Instrument object.
	        m_Instrument = new Instrument();
	        m_Instrument.OnInstrumentUpdate += new InstrumentUpdateEventHandler( OnInstrumentUpdate );
	        m_Instrument.OnFill += new FillEventHandler( OnInstrumentFill );
            
	        // Create a new SortedList to hold the Tick objects.
	        m_TickList = new List< Tick >();

	        m_Position = 0;
	        m_Go = false;
	        m_Qty = 10;
        }

        ~SystemManager()
        {
            //Debug::WriteLine( "SystemManager dying." );
        }

        //homes of the strategy
        private void OnInstrumentUpdate( Tick m_Tick )
        {
	        //Debug::WriteLine( m_Tick.Price );
	        //Add the tick object to the SortedList.
	        m_TickList.Add( m_Tick );
	
	        m_LongMA = 0;
	        m_ShortMA = 0;
	
	        if ( m_Go )
	        {
		        // If we already have a position on, and have either met our target or stop price, get out.
		        if ( m_Position > 0 && ( m_Tick.Price > m_Target || m_Tick.Price < m_Stop ) )
		        {
		        	bool m_Bool = m_Instrument.EnterOrder( "S", m_Qty, "TARGET/STOP OUT" );
		        }
		        if ( m_Position < 0 && ( m_Tick.Price < m_Target || m_Tick.Price > m_Stop ) )
		        {
			        bool m_Bool = m_Instrument.EnterOrder( "B", m_Qty, "TARGET/STOP OUT" );
		        }
        
		        if ( m_TickList.Count > m_LongMATicks )
		        {
			        //Calculate the long moving average.
			        for ( int x = m_TickList.Count - m_LongMATicks; x <= m_TickList.Count - 1; x++ )
			        {
			        	m_LongMA += m_TickList[ x ].Price;
			        }
			        m_LongMA /= m_LongMATicks;
		
			        //Calculate the short moving average.
			        for ( int x = m_TickList.Count - m_ShortMATicks; x <= m_TickList.Count - 1; x++ )
			        {
			        	m_ShortMA += m_TickList[ x ].Price;
			        }
			        m_ShortMA /= m_ShortMATicks;

			        // First time only and on reset, set initial state.
			        if ( m_Start )
			        {
				        if ( m_ShortMA > m_LongMA )
				        	m_State = MA_State.ABOVE;
				        else
					        m_State = MA_State.BELOW;
				        m_Start = false;
			        }

			        // Has there been a crossover up?
			        if ( m_ShortMA > m_LongMA && m_State == MA_State.BELOW )
			        {
				        // Change state.
				        m_State = MA_State.ABOVE;

				        // If we are already short, first get flat.
				        if ( m_Position < 0 )
				        {
				        	m_Bool = m_Instrument.EnterOrder( "B", m_Qty, "GET OUT" );
				        }
	
				        //  Go long.
				        m_Bool = m_Instrument.EnterOrder( "B", m_Qty, "OPEN" );
	
				        // Set target price and stop loss price.
				        m_Target = m_Tick.Price + m_TargetTicks * m_Instrument.TickSize();
				        m_Stop = m_Tick.Price - m_StopTicks * m_Instrument.TickSize();
			        }

			        // Has there been a crossover down?
			        if ( m_ShortMA < m_LongMA && m_State == MA_State.ABOVE )
			        {
				        // Change state.
				        m_State = MA_State.BELOW;

				        // If we are already long, first get flat.
				        if ( m_Position > 0 )
				        {
					        m_Bool = m_Instrument.EnterOrder( "S", m_Qty, "GET OUT" );
				        }

				        // Go short.
				        m_Bool = m_Instrument.EnterOrder( "S", m_Qty, "OPEN" );

				        // Set target price and stop loss price.
				        m_Target = m_Tick.Price - m_TargetTicks * m_Instrument.TickSize();
				        m_Stop = m_Tick.Price + m_StopTicks * m_Instrument.TickSize();
			        }
		        }
	        }
	        //Send the data to the GUI.
	        OnSystemUpdate( m_Tick.Price, m_Tick.Qty, m_LongMA, m_ShortMA, m_Target, m_Stop );
        }

        private void OnInstrumentFill( int qty, String BS, String px, String key )
        {	
	        // Update position.
	        if ( BS == "B" )
	        {
		        m_Position += qty;
	        }
	        else
	        {
		        m_Position -= qty;
	        }

	        // Send the data to the TradeMatcher.
            Fill m_Fill = new Fill();
            if ( BS == "B")
                m_Fill.BS = TradeType.BUY;
            else
                m_Fill.BS = TradeType.SELL;

            m_Fill.Price = Convert.ToDouble( px );
            m_Fill.TradeID = key;
            m_Fill.Qty = qty;
            m_Matcher.Fill_Received( m_Fill );

            m_NetPos = m_Matcher.NetPos;
        }

        public void StartStop()
        {
	        if ( m_Go == false )
	        {
		        m_Go = true;
		        m_Start = true;
	        }
	        else
	        {
		        m_Go = false;
	        }
        }

        public void ShutDown()
        {
	        m_Go = false;
	        m_Instrument.ShutDown();
	        m_Instrument.OnInstrumentUpdate -= new InstrumentUpdateEventHandler( OnInstrumentUpdate );
	        m_Instrument = null;
        }

        public double Qty
	    {
            get { return m_Qty; }
            set { m_Qty = value; }
        }

        public double Bid
        {
            get { return m_Instrument.Bid; }
        }

        public double Ask
        {
            get { return m_Instrument.Ask; }
        }

        public double Position
        {
            get { return m_Position; }
        }

        public double NetPos
        {
            get { return m_NetPos; }
        }

	    public double StopTicks
	    {
            get { return m_StopTicks; }
            set { m_StopTicks = value; }
        }

	    public double TargetTicks
	    {
            get { return m_TargetTicks; }
            set { m_TargetTicks = value; }
        }

        public int ShortMATicks
        {
            get { return m_ShortMATicks; }
            set { m_ShortMATicks = value; }
        }

	    public int LongMATicks
	    {
            get { return m_LongMATicks; }
            set { m_LongMATicks = value; }
        }

        public TradeMatcher Matcher
        {
            get { return m_Matcher; }
        }
    }
}
