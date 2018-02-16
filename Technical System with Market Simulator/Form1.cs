using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Technical_System_with_Market_Simulator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private SystemManager m_Manager;

        private void button1_Click(object sender, EventArgs e)
        {
            if ( m_Manager == null )
			{
				m_Manager = new SystemManager( "C:\\Temp\\Symbols.txt" );
				m_Manager.OnSystemUpdate += new UpdateEventHandler( OnSystemUpdate );
				m_Manager.OnFill += new FillEventHandler( OnFillUpdate );

				m_Manager.Qty = Convert.ToDouble( numericUpDown1.Value );
				m_Manager.ShortMATicks = Convert.ToInt32( numericUpDown2.Value );
				m_Manager.LongMATicks = Convert.ToInt32( numericUpDown3.Value );
				m_Manager.TargetTicks = Convert.ToDouble( numericUpDown4.Value );
				m_Manager.StopTicks = Convert.ToDouble( numericUpDown5.Value );
                textBox8.AppendText( Environment.NewLine );
			}
        }
        private void OnSystemUpdate( double bidq, double bid, double ask, double askq, double m_LongMA, double m_ShortMA, double m_Target, double m_Stop )
		{
			// Event handler prints the data to the GUI.

			textBox1.Text = bidq.ToString();
            textBox6.Text = bid.ToString();
            textBox9.Text = ask.ToString();
            textBox10.Text = askq.ToString();

			textBox2.Text = m_LongMA.ToString();
			textBox3.Text = m_ShortMA.ToString();
			textBox4.Text = m_Target.ToString();
			textBox5.Text = m_Stop.ToString();
		}

        private void OnFillUpdate( Instrument pInstr, double m_Q, String m_Pos, String m_Px, String m_FFT )
		{
			textBox7.Text = m_Q.ToString();
			textBox8.AppendText( m_Pos + "    " + m_Q.ToString() + "     " + m_Px + "    " + m_FFT + Environment.NewLine );
		}

        private void button2_Click(object sender, EventArgs e)
        {
             if ( m_Manager != null )
			 {
				m_Manager.ShutDown();
				m_Manager = null;
				GC.Collect();
			 } 
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (m_Manager != null)
            {
                m_Manager.StartStop();
                if (button3.Text == "START")
                    button3.Text = "STOP";
                else
                    button3.Text = "START";
            }
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if ( m_Manager != null )
			{
                m_Manager.ShortMATicks = Convert.ToInt32(numericUpDown2.Value);
			}
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if ( m_Manager != null )
			{
                m_Manager.LongMATicks = Convert.ToInt32(numericUpDown3.Value);
			} 
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            if ( m_Manager != null )
			{
                m_Manager.TargetTicks = Convert.ToDouble(numericUpDown4.Value);
			}
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            if ( m_Manager != null )
			{
                m_Manager.StopTicks = Convert.ToDouble(numericUpDown5.Value);
			}
        }
        	
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
        	if ( m_Manager != null )
			{
                m_Manager.Qty = Convert.ToDouble( numericUpDown1.Value );
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
