using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Technical_System
{
    delegate void InstrumentUpdateEventHandler( Tick a );
    delegate void UpdateEventHandler( double a, double b, double c, double d, double e, double f );
    delegate void FillEventHandler( int a, String b, String c, String d );

    enum MA_State
    {
	    // ABOVE means that the short MA is above the long MA
	    // BELOW means that the short MA is below the long MA
	    ABOVE, BELOW
    };

    enum Position
    {
	    FLAT, LONG, SHORT
    };
}
