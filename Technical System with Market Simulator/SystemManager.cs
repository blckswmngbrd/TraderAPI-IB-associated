using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;


namespace Technical_System_with_Market_Simulator
{
    class SystemManager
    {

        private Dictionary<String, Instrument> m_Instruments;

        private bool m_Go;
        private bool m_Start;

        private double m_LongMA;
        private double m_ShortMA;
        private int m_LongMATicks;
        private int m_ShortMATicks;

        private MA_State m_State;

        private double m_Qty;

        private double m_TargetTicks;
        private double m_StopTicks;

        public event UpdateEventHandler OnSystemUpdate;
        public event FillEventHandler OnFill;

        public SystemManager( String SymbolsFile )
        {
            // Create a new Instrument collection.
            m_Instruments = new Dictionary<String, Instrument>();
          
            using ( StreamReader m_SymbolsStream = new StreamReader( SymbolsFile ) )
            {
                  String symbol;
                  while ( ( symbol = m_SymbolsStream.ReadLine() ) != null )
                  {
                      Instrument m_Instrument = new Instrument( symbol );
                      m_Instrument.TickSize = .01;
                      m_Instrument.OnInstrumentUpdate += new InstrumentUpdateEventHandler( OnInstrumentUpdate_EventHandler );
                      m_Instrument.OnFill += new FillEventHandler( OnFill_EventHandler );
                      m_Instruments.Add( symbol, m_Instrument ); 
                  }
            }
            m_Go = false;
            m_Qty = 10;
        }

        ~SystemManager()
        {
            Debug.WriteLine("SystemManager dying.");
        }
 
        private void OnInstrumentUpdate_EventHandler( Instrument pInstr )
        {
            m_LongMA = 0;
            m_ShortMA = 0;

            if ( m_Go )
            {
                // If we already have a position on, and have either met our target or stop price, get out.
                if ( pInstr.Position > 0 && ( pInstr.Price >= pInstr.Target || pInstr.Price <= pInstr.Stop))
                {
                    bool m_Bool = pInstr.EnterOrder("S", m_Qty, "TARGET/STOP OUT");
                    pInstr.Position -= 1;
                }
                if (pInstr.Position < 0 && (pInstr.Price <= pInstr.Target || pInstr.Price >= pInstr.Stop))
                {
                    bool m_Bool = pInstr.EnterOrder("B", m_Qty, "TARGET/STOP OUT");
                    pInstr.Position += 1;
                }

                if (pInstr.TickList.Count > m_LongMATicks)
                {
                    //Calculate the long moving average.
                    for (int i = pInstr.TickList.Count - m_LongMATicks; i < pInstr.TickList.Count; i++)
                    {
                        m_LongMA += pInstr.TickList[i];
                    }
                    m_LongMA /= m_LongMATicks;

                    //Calculate the short moving average.
                    for (int i = pInstr.TickList.Count - m_ShortMATicks; i < pInstr.TickList.Count; i++)
                    {
                        m_ShortMA += pInstr.TickList[i]; 
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
                    if (m_ShortMA > m_LongMA && m_State == MA_State.BELOW)
                    {
                        // Change state.
                        m_State = MA_State.ABOVE;

                        // If we are already short, first get flat.
                        if (pInstr.Position < 0)
                        {
                            bool m_Bool = pInstr.EnterOrder("B", m_Qty * 2, "SWITCH");
                            pInstr.Position += (m_Qty * 2);
                        }
                        else
                        {
                            //  Go long.
                            bool result = pInstr.EnterOrder("B", m_Qty, "OPEN");
                            pInstr.Position += m_Qty;
                        }

                        // Set target price and stop loss price.
                        pInstr.Target = pInstr.Price + m_TargetTicks * pInstr.TickSize;
                        pInstr.Stop = pInstr.Price - m_StopTicks * pInstr.TickSize;
                    }

                    // Has there been a crossover down?
                    if (m_ShortMA < m_LongMA && m_State == MA_State.ABOVE)
                    {
                        // Change state.
                        m_State = MA_State.BELOW;

                        // If we are already long, first get flat.
                        if (pInstr.Position > 0)
                        {
                            bool m_Bool = pInstr.EnterOrder("S", m_Qty * 2, "SWITCH");
                            pInstr.Position -= (m_Qty * 2);
                        }
                        else
                        {
                            // Go short.
                            bool result = pInstr.EnterOrder("S", m_Qty, "OPEN");
                            pInstr.Position -= m_Qty;
                        }
                        // Set target price and stop loss price.
                        pInstr.Target = pInstr.Price - m_TargetTicks * pInstr.TickSize;
                        pInstr.Stop = pInstr.Price + m_StopTicks * pInstr.TickSize;
                    }
                }
            }
            //Send the data to the GUI.
            OnSystemUpdate(pInstr.BidQty, pInstr.Bid, pInstr.Ask, pInstr.AskQty, m_LongMA, m_ShortMA, pInstr.Target, pInstr.Stop);
        }

        private void OnFill_EventHandler(Instrument pInstr, double m_Q, String m_BS, String m_Px, String m_FFT )
        {
            // Update position.
            if (m_BS == "B")
            {
               // pInstr.Position += m_Q;
            }
            else
            {
               // pInstr.Position -= m_Q;
            }

            // Send the data to the GUI.
            OnFill( pInstr, pInstr.Position, m_BS, m_Px, m_FFT);
        }

        public void StartStop()
        {
            if (m_Go == false)
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
            foreach( KeyValuePair< String, Instrument > kvp in m_Instruments )
            {
                kvp.Value.ShutDown();
                kvp.Value.OnInstrumentUpdate -= new InstrumentUpdateEventHandler(OnInstrumentUpdate_EventHandler);
            }
        }

        public double Qty
        {
            get
            {
                return m_Qty;
            }
            set
            {
                m_Qty = value;
            }
        }

        public double StopTicks
        {
            get
            {
                return m_StopTicks;
            }
            set
            {
                m_StopTicks = value;
            }
        }

        public double TargetTicks
        {
            get
            {
                return m_TargetTicks;
            }
            set
            {
                m_TargetTicks = value;
            }
        }

        public int ShortMATicks
        {
            get
            {
                return m_ShortMATicks;
            }
            set
            {
                m_ShortMATicks = value;
            }
        }

        public int LongMATicks
        {
            get
            {
                return m_LongMATicks;
            }
            set
            {
                m_LongMATicks = value;
            }
        }
    }
}
