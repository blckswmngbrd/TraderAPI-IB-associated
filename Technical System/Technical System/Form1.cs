using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TradeMatching;

namespace Technical_System
{
    public partial class Form1 : Form
    {
       
        private SystemManager m_Manager;
        private Timer m_Timer;
        System.IO.StreamWriter data_file;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            data_file = new System.IO.StreamWriter( "D:\\Data\\Price_Data.csv", true );

            m_Timer = new Timer();
            m_Timer.Interval = 20000;
            m_Timer.Tick += new EventHandler(m_Timer_Tick);
            m_Timer.Enabled = true;

            if ( m_Manager == null )
			{
				m_Manager = new SystemManager();
				m_Manager.OnSystemUpdate += new UpdateEventHandler( OnSystemUpdate );
				//m_Manager.OnFill += new FillEventHandler( OnFillUpdate );

				m_Manager.Qty = Convert.ToDouble( numericUpDown1.Value );
				m_Manager.ShortMATicks = Convert.ToInt32( numericUpDown2.Value );
				m_Manager.LongMATicks = Convert.ToInt32( numericUpDown3.Value );
				m_Manager.TargetTicks = Convert.ToInt32( numericUpDown4.Value );
				m_Manager.StopTicks = Convert.ToInt32( numericUpDown5.Value );
			}

            dataGridView1.DataSource = m_Manager.Matcher.BuyTable;
            dataGridView2.DataSource = m_Manager.Matcher.SellTable;
            dataGridView3.DataSource = m_Manager.Matcher.RoundTurns;
        }

        private void OnSystemUpdate( double m_Price, double m_Qty, double m_LongMA, double m_ShortMA, double m_Target, double m_Stop )
		{
			// Event handler prints the data to the GUI.
			textBox1.Text = ( m_Price / 100.0 ).ToString("0,000.00");
            textBox2.Text = (m_LongMA / 100.0).ToString("0,000.000");
            textBox3.Text = (m_ShortMA / 100.0).ToString("0,000.000");
            textBox4.Text = (m_Target / 100.0).ToString("0,000.000");
            textBox5.Text = (m_Stop / 100.0).ToString("0,000.000");
			textBox6.Text = m_Qty.ToString();
            textBox7.Text = m_Manager.Position.ToString();
            textBox8.Text = m_Manager.NetPos.ToString();
		}

        //private void OnFillUpdate( int m_Q, String m_Pos, String m_Px )
        //{
        //}

        private void button2_Click(object sender, EventArgs e)
        {
             if ( m_Manager != null )
			 {
				m_Manager.ShutDown();
				
                m_Manager.Matcher.WriteBuys( "D:\\Data\\TRADE MATCHING ALGOS\\buys.csv" );
                m_Manager.Matcher.WriteSells( "D:\\Data\\TRADE MATCHING ALGOS\\sells.csv" );
                m_Manager.Matcher.WriteRoundTurns( "D:\\Data\\TRADE MATCHING ALGOS\\roundturns.csv" );
                
                m_Timer.Tick -= new EventHandler(m_Timer_Tick);
                m_Timer = null;
                
                m_Manager = null;
                GC.Collect();
                data_file.Close();
			 } 
        }

        private void button3_Click(object sender, EventArgs e)
        {
             if ( m_Manager != null )
			 {
				m_Manager.StartStop();
				if ( button3.Text == "START" )
					button3.Text = "STOP";
				else
					button3.Text = "START";
			}
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if ( m_Manager != null )
			{
				m_Manager.Qty = Convert.ToDouble( numericUpDown1.Value );
			}
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if ( m_Manager != null )
			{
				m_Manager.ShortMATicks = Convert.ToInt32( numericUpDown2.Value );
			} 
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if ( m_Manager != null )
			{
				m_Manager.LongMATicks = Convert.ToInt32( numericUpDown3.Value );
			}
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            if ( m_Manager != null )
			{
				m_Manager.TargetTicks = Convert.ToDouble( numericUpDown4.Value );
			}
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            if ( m_Manager != null )
			{
				m_Manager.StopTicks = Convert.ToDouble( numericUpDown5.Value );
			}
        }

        private void m_Timer_Tick(Object o, EventArgs e)
        {
            
            // Write mid-point price to file here. ////////////////////////////////////

            data_file.WriteLine( (( m_Manager.Bid + m_Manager.Ask ) / 2.0 ).ToString() );
   
            //////////////////////////////////////////////////////////////////////////

            m_Timer.Interval = 5000;

            if( textBox1.Text != "" && Convert.ToDouble( textBox1.Text ) != 0.0 )
                chart1.Series[0].Points.AddXY(DateTime.Now.ToOADate(), textBox1.Text);

           	chart1.ChartAreas[ 0 ].AxisY.Maximum = Math.Ceiling( chart1.Series[ 0 ].Points.FindMaxByValue().YValues[0] ) + 2.0;
			chart1.ChartAreas[ 0 ].AxisY.Minimum = Math.Floor( chart1.Series[ 0 ].Points.FindMinByValue().YValues[0] ) - 2.0;
            chart1.ChartAreas[ 0 ].AxisY.Interval = Math.Floor( ( ( chart1.ChartAreas[ 0 ].AxisY.Maximum ) - ( chart1.ChartAreas[ 0 ].AxisY.Minimum ) ) / 5.0 ) ;

            double removeBefore = DateTime.Now.AddSeconds(-4900.0).ToOADate();

            while (chart1.Series[0].Points[0].XValue < removeBefore)
            {
                chart1.Series[0].Points.RemoveAt(0);
            }

            chart1.ChartAreas[0].AxisX.Minimum = chart1.Series[0].Points[0].XValue;
            chart1.ChartAreas[0].AxisX.Maximum = DateTime.FromOADate(chart1.Series[0].Points[0].XValue).AddSeconds(5000).ToOADate();
            chart1.Invalidate();
        }
    }
}
