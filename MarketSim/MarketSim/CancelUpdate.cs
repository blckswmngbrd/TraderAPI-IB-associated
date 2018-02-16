using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketSim
{
    public class CancelUpdate : Update
    {
        public CancelUpdate( String sym, UpdateType update_type, int id, String fft ) : base( sym, update_type, id, fft )
        {
        }
    }
}
