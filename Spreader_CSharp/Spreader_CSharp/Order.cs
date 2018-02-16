using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spreader_CSharp
{
    class Order
    {
        public String Key { get; private set; }
        public double Price { get; private set; }
        public Order( String key, double price )
        {
            Key = key; 
            Price = price;
        }
    };
}
