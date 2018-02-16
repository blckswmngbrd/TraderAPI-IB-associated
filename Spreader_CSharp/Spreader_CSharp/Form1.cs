using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Spreader_CSharp
{
    public partial class Form1 : Form
    {
        private SysMan _Manager;
		private DataSet _FillData;
	
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _FillData = new DataSet();
			SetUpDataSet();

			// Create new SystemManager object and subscribe to delegates.
			_Manager = new SysMan();
			_Manager.OnPriceUpdate += OnUpdateEventHandler;
			_Manager.OnFillUpdate += OnFillEventHandler;

			// Set buttons for secure start up / shut down.
			button1.Enabled = false;
			button2.Enabled = true;
			this.ControlBox = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Start the trading system.
            _Manager.Go = true;
            button2.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            				// Shut down system gracefully.
            _Manager.Go = false;
				if ( _Manager != null )
				{

					_Manager.OnPriceUpdate -= OnUpdateEventHandler;
					_Manager.OnFillUpdate -= OnFillEventHandler;

					_Manager = null;
					GC.Collect();
				}
				this.ControlBox = true;
        }

        	private void SetUpDataSet()
			 {
				DataTable _DataTable = new DataTable( "FillData" );
				_DataTable.Columns.Add( new DataColumn( "CONTRACT" ) );
				_DataTable.Columns.Add( new DataColumn( "TRADEID" ) );
				_DataTable.Columns.Add( new DataColumn( "BUYSELL" ) );
				_DataTable.Columns.Add( new DataColumn( "QUANTITY" ) );
				_DataTable.Columns.Add( new DataColumn( "PRICE" ) );
				_DataTable.Columns.Add( new DataColumn( "TRADETIME" ) );
				
				_FillData.Tables.Add( _DataTable );
				
				dataGridView1.DataSource = _FillData;
				dataGridView1.DataMember = "FillData";
			 }

	        public void OnUpdateEventHandler()
		    {
				// Populate the text boxes with market data.
				// This method will run on the Main thread.
				tbBid_A.Text = _Manager.Bid_A.ToString();
				tbAsk_A.Text = _Manager.Ask_A.ToString();
				tbBid_B.Text = _Manager.Bid_B.ToString();
				tbAsk_B.Text = _Manager.Ask_B.ToString();
				tbSpreadPrice.Text = _Manager.SpreadPrice.ToString();
				tbNormPrice.Text = _Manager.NormPrice.ToString( "#.000" );
				tbPos_A.Text = _Manager.Pos_A.ToString();
				tbPos_B.Text = _Manager.Pos_B.ToString();
				tbPosSpread.Text = _Manager.SpreadPos.ToString();
				tbSpreadPosPx.Text = _Manager.SpreadPosPrice.ToString();
				tbStopPrice.Text = _Manager.StopPrice.ToString();
			 }

	        public void OnFillEventHandler( String contract, String key, String BS, int qty, int price, String time )
			{
				// Add fill data to the dataset.
				// This method will run on the Main thread.
				DataRow _Row = _FillData.Tables[ 0 ].NewRow();
				_Row[ 0 ] = contract;
				_Row[ 1 ] = key;
				_Row[ 2 ] = BS;
				_Row[ 3 ] = qty;
				_Row[ 4 ] = price;
				_Row[ 5 ] = time;
				_FillData.Tables[ 0 ].Rows.Add( _Row );
			}

    }
}
