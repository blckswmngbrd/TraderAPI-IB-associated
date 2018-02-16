using System;
using System.Collections;

class NormCalc
{
    public static double CalcNormalizedPrice( ArrayList list )
	{
		// This method will run on the Strategy thread.
		double m_Mean = 0;
		double m_StDev = 0;

		if ( list.Count == 30 )
		{
			// Calculate the 30 tick MA.
			for( int x = 0; x < 30; x++ )
			{
				m_Mean += Convert.ToDouble( list[ x ] );
			}
			m_Mean /= 30;
			
			// Calculate the 30 tick St Dev.
			for( int x = 0; x < 30; x++ )
			{
				m_StDev += Math.Pow( Convert.ToDouble( list[ x ] ) - m_Mean, 2 );
			}
			m_StDev = Math.Sqrt( m_StDev / 30 );

			list.RemoveAt( 0 );

			// Return ( LastPx - 30 tick MA ) /  30 tick St Dev
			return ( Convert.ToDouble( list[ 28 ] ) - m_Mean ) / m_StDev;
		}
		else
		{
			return 0;
		}
	}

};
