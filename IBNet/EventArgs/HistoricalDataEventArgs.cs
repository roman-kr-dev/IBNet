using System;

namespace Krs.Ats.IBNet
{
    /// <summary>
    /// Historical Data Event Arguments
    /// </summary>
    [Serializable]
    public class HistoricalDataEventArgs : EventArgs
    {
        /// <summary>
        /// Full Constructor
        /// </summary>
        /// <param name="requestId">The ticker Id of the request to which this bar is responding.</param>
        /// <param name="date">The date-time stamp of the start of the bar.
        /// The format is determined by the reqHistoricalData() formatDate parameter.</param>
        /// <param name="open">Bar opening price.</param>
        /// <param name="high">High price during the time covered by the bar.</param>
        /// <param name="low">Low price during the time covered by the bar.</param>
        /// <param name="close">Bar closing price.</param>
        /// <param name="volume">Volume during the time covered by the bar.</param>
        /// <param name="trades">When TRADES historical data is returned, represents the number of trades that
        /// occurred during the time period the bar covers.</param>
        /// <param name="wap">Weighted average price during the time covered by the bar.</param>
        /// <param name="hasGaps">Whether or not there are gaps in the data.</param>
        /// <param name="recordNumber">Current Record Number out of Record Total.</param>
        /// <param name="recordTotal">Total Records Returned by Historical Request.</param>
        public HistoricalDataEventArgs(int requestId, DateTime date, double open, double high, double low, double close,
                                       int volume, int trades, double wap, bool hasGaps, int recordNumber, int recordTotal)
        {
            RequestId    = requestId;
            HasGaps      = hasGaps;
            Wap          = wap;
            Trades       = trades;
            Volume       = volume;
            Close        = close;
            Low          = low;
            High         = high;
            Open         = open;
            Date         = date;
            RecordNumber = recordNumber;
            RecordTotal  = recordTotal;
        }

        /// <summary>
        /// Uninitialized Constructor for Serialization
        /// </summary>
        public HistoricalDataEventArgs()
        {}

        /// <summary>
        /// The ticker Id of the request to which this bar is responding.
        /// </summary>
        public int RequestId { get; set; }

        /// <summary>
        /// The date-time stamp of the start of the bar.
        /// The format is determined by the reqHistoricalData() formatDate parameter.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Bar opening price.
        /// </summary>
        public double Open { get; set; }

        /// <summary>
        /// High price during the time covered by the bar.
        /// </summary>
        public double High { get; set; }

        /// <summary>
        /// Low price during the time covered by the bar.
        /// </summary>
        public double Low { get; set; }

        /// <summary>
        /// Bar closing price.
        /// </summary>
        public double Close { get; set; }

        /// <summary>
        /// Volume during the time covered by the bar.
        /// </summary>
        public int Volume { get; set; }

        /// <summary>
        /// When TRADES historical data is returned, represents the number of trades that
        /// occurred during the time period the bar covers.
        /// </summary>
        public int Trades { get; set; }

        /// <summary>
        /// Weighted average price during the time covered by the bar.
        /// </summary>
        public double Wap { get; set; }

        /// <summary>
        /// Whether or not there are gaps in the data.
        /// </summary>
        public bool HasGaps { get; set; }

        /// <summary>
        /// Current Record Number out of Record Total
        /// </summary>
        public int RecordNumber { get; set; }

        /// <summary>
        /// Total records returned by query
        /// </summary>
        public int RecordTotal { get; set; }

    }

    /// <summary>
    /// Historical Data Event Arguments
    /// </summary>
    [Serializable]
    public class HistoricalDataEndEventArgs : EventArgs
    {
        /// <summary>
        /// Full Constructor
        /// </summary>
        /// <param name="requestId">The ticker Id of the request to which this bar is responding.</param>
        /// <param name="start">The date-time stamp of the start of the bar.</param>
        /// <param name="end">The date-time stamp of the end of the bar.</param>
        public HistoricalDataEndEventArgs(int requestId, string start, string end)
        {
            RequestId = requestId;
            Start = start;
            End = end;
        }

        /// <summary>
        /// Uninitialized Constructor for Serialization
        /// </summary>
        public HistoricalDataEndEventArgs()
        {}

        /// <summary>
        /// 
        /// </summary>
        public int RequestId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Start { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string End { get; set; }

    }
}
