using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Diagnostics;
using IBApi;

namespace Krs.Ats.IBNet
{
    /// <summary>
    /// Interactive Brokers Client
    /// Handles all communications to and from the TWS.
    /// </summary>
    public class IBClient : EWrapper, IDisposable
    {
        private readonly EClientSocket _socket;

        public IBClient()
        {
            _socket = new EClientSocket(this);
            ibTrace.Level = TraceLevel.Verbose;
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method. 
            // Therefore, you should call GC.SupressFinalize to 
            // take this object off the finalization queue  
            // and prevent finalization code for this object 
            // from executing a second time.
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// The bulk of the clean-up code is implemented in Dispose(bool)
        /// </summary>
        /// <param name="disposing">Allows the ondispose method to override the dispose action.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeneralTracer.WriteLineIf(ibTrace.TraceInfo, "IBClient Dispose");
                _socket.eDisconnect();
                _socket.Close();
            }
        }

        #region Tracer
        private GeneralTracer ibTrace = new GeneralTracer("ibInfo", "Interactive Brokers Parameter Info");
        private GeneralTracer ibTickTrace = new GeneralTracer("ibTicks", "Interactive Brokers Tick Info");
        #endregion

        #region EWrapper


        /// <summary>
        /// This event is fired when there is an error with the communication or when TWS wants to send a message to the client.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;
        /// <summary>
        /// Called internally when the receive thread receives an error event.
        /// </summary>
        /// <param name="e">Error Event Arguments</param>
        protected virtual void OnError(ErrorEventArgs e) { RaiseEvent(Error, this, e); }
        public void error(int id, int errorCode, string errorMsg)
        {
            lock (this)
            {
                GeneralTracer.WriteLineIf(ibTrace.TraceError, "IBEvent: Error: tickerId: {0}, errorCode: {1}, errorMsg: {2}", id, errorCode, errorMsg);
                var e = new ErrorEventArgs(id, (ErrorMessage)errorCode, errorMsg);
                OnError(e);
            }
        }

        /// <summary>
        /// This method is triggered for any exceptions caught.
        /// </summary>
        public event EventHandler<ReportExceptionEventArgs> ReportException;
        /// <summary>
        /// Called internally when a exception is being thrown
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnReportException(ReportExceptionEventArgs e) { RaiseEvent(ReportException, this, e); }
        public void error(Exception ex)
        {
            var e = new ReportExceptionEventArgs(ex);
            OnReportException(e);
        }

        public void error(string str)
        {
            error(0, 0, str);
        }


        /// <summary>
        /// This method receives the current system time on the server side.
        /// </summary>
        public event EventHandler<CurrentTimeEventArgs> CurrentTime;
        /// <summary>
        /// Called internally when the receive thread receives a current time event.
        /// </summary>
        /// <param name="e">Current Time Event Arguments</param>
        protected virtual void OnCurrentTime(CurrentTimeEventArgs e) { RaiseEvent(CurrentTime, this, e); }
        public void currentTime(long time)
        {
            DateTime cTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(time);
            var e = new CurrentTimeEventArgs(cTime);
            RaiseEvent(CurrentTime, this, e);
            OnCurrentTime(e);
        }

        /// <summary>
        /// This event is called when the market data changes. Prices are updated immediately with no delay.
        /// </summary>
        public event EventHandler<TickPriceEventArgs> TickPrice;
        /// <summary>
        /// Called internally when the receive thread receives a tick price event.
        /// </summary>
        /// <param name="e">Tick Price event arguments</param>
        protected virtual void OnTickPrice(TickPriceEventArgs e) { RaiseEvent(TickPrice, this, e); }
        public void tickPrice(int tickerId, int field, double price, int canAutoExecute)
        {
            var tickType = (TickType)field;
            var e = new TickPriceEventArgs(tickerId, tickType, price, (canAutoExecute != 0));
            OnTickPrice(e);
        }

        /// <summary>
        /// This event is called when the market data changes. Sizes are updated immediately with no delay.
        /// </summary>
        public event EventHandler<TickSizeEventArgs> TickSize;
        /// <summary>
        /// Called internally when the receive thread receives a tick size event.
        /// </summary>
        /// <param name="e">Tick Size Event Arguments</param>
        protected virtual void OnTickSize(TickSizeEventArgs e) { RaiseEvent(TickSize, this, e); }
        public void tickSize(int tickerId, int field, int size)
        {
            //GeneralTracer.WriteLineIf(ibTickTrace.TraceInfo, "IBEvent: TickSize: tickerId: {0}, tickType: {1}, size: {2}", tickerId, tickType, size);
            var tickType = (TickType)field;
            var e = new TickSizeEventArgs(tickerId, tickType, size);
            OnTickSize(e);
        }

        /// <summary>
        /// This method is called when the market data changes. Values are updated immediately with no delay.
        /// </summary>
        public event EventHandler<TickStringEventArgs> TickString;
        /// <summary>
        /// Called internally when the receive thread receives a Tick String  event.
        /// </summary>
        /// <param name="e">Tick String Event Arguments</param>
        protected virtual void OnTickString(TickStringEventArgs e) { RaiseEvent(TickString, this, e); }
        public void tickString(int tickerId, int field, string value)
        {
            var tickType = (TickType)field;
            var e = new TickStringEventArgs(tickerId, tickType, value);
            OnTickString(e);
        }

        /// <summary>
        /// This method is called when the market data changes. Values are updated immediately with no delay.
        /// </summary>
        public event EventHandler<TickGenericEventArgs> TickGeneric;
        /// <summary>
        /// Called internally when the receive thread receives a generic tick event.
        /// </summary>
        /// <param name="e">Tick Generic Event Arguments</param>
        protected virtual void OnTickGeneric(TickGenericEventArgs e) { RaiseEvent(TickGeneric, this, e); }
        public void tickGeneric(int tickerId, int field, double value)
        {
            var tickType = (TickType)field;
            var e = new TickGenericEventArgs(tickerId, tickType, value);
            OnTickGeneric(e);
        }

        /// <summary>
        /// This method is called when the market data changes. Values are updated immediately with no delay.
        /// </summary>
        public event EventHandler<TickEfpEventArgs> TickEfp;
        /// <summary>
        /// Called internally when the receive thread receives a tick EFP event.
        /// </summary>
        /// <param name="e">Tick Efp Arguments</param>
        protected virtual void OnTickEfp(TickEfpEventArgs e) { RaiseEvent(TickEfp, this, e); }
        public void tickEFP(int tickerId, int tickType, double basisPoints, string formattedBasisPoints, double impliedFuture,
            int holdDays, string futureExpiry, double dividendImpact, double dividendsToExpiry)
        {
            var tickTyp = (TickType)tickType;
            var e = new TickEfpEventArgs(tickerId, tickTyp, basisPoints, formattedBasisPoints, impliedFuture, holdDays, futureExpiry, dividendImpact, dividendsToExpiry);
            OnTickEfp(e);
        }

        /// <summary>
        /// Called once all execution data for a given request are received.
        /// </summary>
        public event EventHandler<DeltaNuetralValidationEventArgs> DeltaNuetralValidation;
        /// <summary>
        /// Called internally when the receive thread receives a Contract Details End Event.
        /// </summary>
        /// <param name="e">Contract Details End Event Arguments</param>
        protected virtual void OnDeltaNuetralValidation(DeltaNuetralValidationEventArgs e) { RaiseEvent(DeltaNuetralValidation, this, e); }
        public void deltaNeutralValidation(int reqId, UnderComp underComp)
        {
            var e = new DeltaNuetralValidationEventArgs(reqId, underComp);
            OnDeltaNuetralValidation(e);
        }

        /// <summary>
        /// This method is called when the market in an option or its underlier moves.
        /// TWS’s option model volatilities, prices, and deltas, along with the present
        /// value of dividends expected on that option’s underlier are received.
        /// </summary>
        public event EventHandler<TickOptionComputationEventArgs> TickOptionComputation;
        /// <summary>
        /// Called internally when the receive thread receives a tick option computation event.
        /// </summary>
        /// <param name="e">Tick Option Computation Arguments</param>
        protected virtual void OnTickOptionComputation(TickOptionComputationEventArgs e) { RaiseEvent(TickOptionComputation, this, e); }
        public void tickOptionComputation(int tickerId, int field, double impliedVolatility, double delta, double optPrice,
            double pvDividend, double gamma, double vega, double theta, double undPrice)
        {
            var tickType = (TickType)field;
            var e = new TickOptionComputationEventArgs(tickerId, tickType, impliedVolatility, delta, optPrice, pvDividend, gamma, vega, theta, undPrice);
            OnTickOptionComputation(e);
        }

        /// <summary>
        /// Called once the tick snap shot is complete.
        /// </summary>
        public event EventHandler<TickSnapshotEndEventArgs> TickSnapshotEnd;
        /// <summary>
        /// Called internally when the receive thread receives a Tick Snapshot End Event.
        /// </summary>
        /// <param name="e">Contract Details End Event Arguments</param>
        protected virtual void OnTickSnapshotEnd(TickSnapshotEndEventArgs e) { RaiseEvent(TickSnapshotEnd, this, e); }
        public void tickSnapshotEnd(int tickerId)
        {
            var e = new TickSnapshotEndEventArgs(tickerId);
            OnTickSnapshotEnd(e);
        }

        /// <summary>
        /// This method is called after a successful connection to TWS.
        /// </summary>
        public event EventHandler<NextValidIdEventArgs> NextValidId;
        /// <summary>
        /// Called internally when the receive thread receives a Next Valid Id event.
        /// </summary>
        /// <param name="e">Next Valid Id Event Arguments</param>
        protected virtual void OnNextValidId(NextValidIdEventArgs e) { RaiseEvent(NextValidId, this, e); }
        public void nextValidId(int orderId)
        {
            //GeneralTracer.WriteLineIf(ibTickTrace.TraceInfo, "IBEvent: NextValidId: orderId: {0}", orderId);
            var e = new NextValidIdEventArgs(orderId);
            OnNextValidId(e);
        }


        /// <summary>
        /// This method is called when a successful connection is made to a Financial Advisor account.
        /// It is also called when the reqManagedAccts() method is invoked.
        /// </summary>
        public event EventHandler<ManagedAccountsEventArgs> ManagedAccounts;
        /// <summary>
        /// Called internally when the receive thread receives a managed accounts event.
        /// </summary>
        /// <param name="e">Managed Accounts Event Arguments</param>
        protected virtual void OnManagedAccounts(ManagedAccountsEventArgs e) { RaiseEvent(ManagedAccounts, this, e); }
        public void managedAccounts(string accountsList)
        {
            var e = new ManagedAccountsEventArgs(accountsList);
            OnManagedAccounts(e);
        }

        /// <summary>
        /// This method is called when TWS closes the sockets connection, or when TWS is shut down.
        /// </summary>
        public event EventHandler<ConnectionClosedEventArgs> ConnectionClosed;
        /// <summary>
        /// Called internally when the receive thread receives a connection closed event.
        /// </summary>
        /// <param name="e">Connection Closed Event Arguments</param>
        protected virtual void OnConnectionClosed(ConnectionClosedEventArgs e) { RaiseEvent(ConnectionClosed, this, e); }
        public void connectionClosed()
        {
            var e = new ConnectionClosedEventArgs();
            OnConnectionClosed(e);
        }

        /// <summary>
        /// This method is called only when reqAccountUpdates() method on the EClientSocket object has been called.
        /// </summary>
        public event EventHandler<UpdateAccountValueEventArgs> UpdateAccountValue;
        /// <summary>
        /// Called internally when the receive thread receives an Update Account Value event.
        /// </summary>
        /// <param name="e">Update Account Value Event Arguments</param>
        protected virtual void OnUpdateAccountValue(UpdateAccountValueEventArgs e) { RaiseEvent(UpdateAccountValue, this, e); }
        public void updateAccountValue(string key, string value, string currency, string accountName)
        {
            var e = new UpdateAccountValueEventArgs(key, value, currency, accountName);
            OnUpdateAccountValue(e);
        }

        /// <summary>
        /// This method is called only when reqAccountUpdates() method on the EClientSocket object has been called.
        /// </summary>
        public event EventHandler<UpdatePortfolioEventArgs> UpdatePortfolio;
        /// <summary>
        /// Called Internally when the receive thread receives an Update Portfolio event.
        /// </summary>
        /// <param name="e">Update Portfolio Event Arguments</param>
        protected virtual void OnUpdatePortfolio(UpdatePortfolioEventArgs e) { RaiseEvent(UpdatePortfolio, this, e); }
        public void updatePortfolio(Contract contract, int position, double marketPrice, double marketValue, double averageCost,
            double unrealisedPNL, double realisedPNL, string accountName)
        {
            var e = new UpdatePortfolioEventArgs(contract, position, marketPrice, marketValue, averageCost, unrealisedPNL, realisedPNL, accountName);
            OnUpdatePortfolio(e);
        }

        /// <summary>
        /// This method is called only when reqAccountUpdates() method on the EClientSocket object has been called.
        /// </summary>
        public event EventHandler<UpdateAccountTimeEventArgs> UpdateAccountTime;
        /// <summary>
        /// Called internally when the receive thread receives an Update Account Time event.
        /// </summary>
        /// <param name="e">Update Account Time Event Arguments</param>
        protected virtual void OnUpdateAccountTime(UpdateAccountTimeEventArgs e) { RaiseEvent(UpdateAccountTime, this, e); }
        public void updateAccountTime(string timestamp)
        {
            var e = new UpdateAccountTimeEventArgs(timestamp);
            OnUpdateAccountTime(e);
        }

        /// <summary>
        /// Called once all Account Details for a given request are received.
        /// </summary>
        public event EventHandler<AccountDownloadEndEventArgs> AccountDownloadEnd;
        /// <summary>
        /// Called internally when the receive thread receives a Account Download End Event.
        /// </summary>
        /// <param name="e">Contract Details End Event Arguments</param>
        protected virtual void OnAccountDownloadEnd(AccountDownloadEndEventArgs e) { RaiseEvent(AccountDownloadEnd, this, e); }
        public void accountDownloadEnd(string account)
        {
            var e = new AccountDownloadEndEventArgs(account);
            OnAccountDownloadEnd(e);
        }

        /// <summary>
        /// This methodis called whenever the status of an order changes. It is also fired after reconnecting
        /// to TWS if the client has any open orders.
        /// </summary>
        public event EventHandler<OrderStatusEventArgs> OrderStatus;
        /// <summary>
        /// Called internally when the receive thread receives an order status event.
        /// </summary>
        /// <param name="e">Order Status Event Arguments</param>
        protected virtual void OnOrderStatus(OrderStatusEventArgs e) { RaiseEvent(OrderStatus, this, e); }
        public void orderStatus(int orderId, string status, int filled, int remaining, double avgFillPrice, int permId, int parentId,
            double lastFillPrice, int clientId, string whyHeld)
        {
            Krs.Ats.IBNet.OrderStatus orderStatus = (string.IsNullOrEmpty(status) ? Krs.Ats.IBNet.OrderStatus.None :
                (Krs.Ats.IBNet.OrderStatus)EnumDescConverter.GetEnumValue(typeof(Krs.Ats.IBNet.OrderStatus), status));
            var e = new OrderStatusEventArgs(orderId, orderStatus, filled, remaining, avgFillPrice, permId, parentId, lastFillPrice, clientId, whyHeld);
            OnOrderStatus(e);
        }

        /// <summary>
        /// This method is called to feed in open orders.
        /// </summary>
        public event EventHandler<OpenOrderEventArgs> OpenOrder;
        /// <summary>
        /// Called internally when the receive thread receives an open order event.
        /// </summary>
        /// <param name="e">Open Order Event Arguments</param>
        protected virtual void OnOpenOrder(OpenOrderEventArgs e) { RaiseEvent(OpenOrder, this, e); }
        public void openOrder(int orderId, Contract contract, Order order, OrderState orderState)
        {
            var e = new OpenOrderEventArgs(orderId, contract, order, orderState);
            OnOpenOrder(e);
        }

        /// <summary>
        /// Called once all the open orders for a given request are received.
        /// </summary>
        public event EventHandler<EventArgs> OpenOrderEnd;
        /// <summary>
        /// Called internally when the receive thread receives a Open Orders End Event.
        /// </summary>
        /// <param name="e">Empty Event Arguments</param>
        protected virtual void OnOpenOrderEnd(EventArgs e) { RaiseEvent(OpenOrderEnd, this, e); }
        public void openOrderEnd()
        {
            var e = new EventArgs();
            OnOpenOrderEnd(e);
        }

        /// <summary>
        /// This event fires in response to the <see cref="RequestContractDetails"/> method.
        /// </summary>
        public event EventHandler<ContractDetailsEventArgs> ContractDetails;
        /// <summary>
        /// Called internally when the receive thread receives a contract details event.
        /// </summary>
        /// <param name="e">Contract Details Event Arguments</param>
        protected virtual void OnContractDetails(ContractDetailsEventArgs e) { RaiseEvent(ContractDetails, this, e); }
        public void contractDetails(int reqId, ContractDetails contractDetails)
        {
            var e = new ContractDetailsEventArgs(reqId, contractDetails);
            OnContractDetails(e);
        }

        /// <summary>
        /// Called once all contract details for a given request are received.
        /// This, for example, helps to define the end of an option chain.
        /// </summary>
        public event EventHandler<ContractDetailsEndEventArgs> ContractDetailsEnd;
        /// <summary>
        /// Called internally when the receive thread receives a Contract Details End Event.
        /// </summary>
        /// <param name="e">Contract Details End Event Arguments</param>
        protected virtual void OnContractDetailsEnd(ContractDetailsEndEventArgs e) { RaiseEvent(ContractDetailsEnd, this, e); }
        public void contractDetailsEnd(int reqId)
        {
            var e = new ContractDetailsEndEventArgs(reqId);
            OnContractDetailsEnd(e);
        }

        /// <summary>
        /// This event fires in response to the <see cref="RequestContractDetails"/> method called on a bond contract.
        /// </summary>
        public event EventHandler<BondContractDetailsEventArgs> BondContractDetails;
        /// <summary>
        /// Called internally when the receive thread receives a Bond Contract Details Event.
        /// </summary>
        /// <param name="e">Bond Contract Details Event Arguments</param>
        protected virtual void OnBondContractDetails(BondContractDetailsEventArgs e) { RaiseEvent(BondContractDetails, this, e); }
        private void bondContractDetails(int requestId, ContractDetails contractDetails)
        {
            var e = new BondContractDetailsEventArgs(requestId, contractDetails);
            OnBondContractDetails(e);
        }

        /// <summary>
        /// This event fires in response to the <see cref="RequestExecutions"/> method or after an order is placed.
        /// </summary>
        public event EventHandler<ExecDetailsEventArgs> ExecDetails;
        /// <summary>
        /// Called internally when the receive thread receives an execution details event.
        /// </summary>
        /// <param name="e">Execution Details Event Arguments</param>
        protected virtual void OnExecDetails(ExecDetailsEventArgs e) { RaiseEvent(ExecDetails, this, e); }
        public void execDetails(int reqId, Contract contract, Execution execution)
        {
            var e = new ExecDetailsEventArgs(reqId, contract, execution);
            OnExecDetails(e);
        }

        /// <summary>
        /// Called once all contract details for a given request are received.
        /// This, for example, helps to define the end of an option chain.
        /// </summary>
        public event EventHandler<ExecDetailsEndEventArgs> ExecDetailsEnd;
        /// <summary>
        /// Called internally when the receive thread receives a Contract Details End Event.
        /// </summary>
        /// <param name="e">Contract Details End Event Arguments</param>
        protected virtual void OnExecDetailsEnd(ExecDetailsEndEventArgs e) { RaiseEvent(ExecDetailsEnd, this, e); }
        public void execDetailsEnd(int reqId)
        {
            var e = new ExecDetailsEndEventArgs(reqId);
            OnExecDetailsEnd(e);
        }

        /// <summary>
        /// Called once all contract details for a given request are received.
        /// This, for example, helps to define the end of an option chain.
        /// </summary>
        public event EventHandler<CommissionReportEventArgs> CommissionReport;
        /// <summary>
        /// Called internally when the receive thread receives a Contract Details End Event.
        /// </summary>
        /// <param name="e">Contract Details End Event Arguments</param>
        protected virtual void OnCommissionReport(CommissionReportEventArgs e) { RaiseEvent(CommissionReport, this, e); }
        public void commissionReport(CommissionReport commissionReport)
        {
            var e = new CommissionReportEventArgs(commissionReport);
            OnCommissionReport(e);
        }

        /// <summary>
        /// Reuters global fundamental market data
        /// </summary>
        public event EventHandler<FundamentalDetailsEventArgs> FundamentalData;
        /// <summary>
        /// Called internally when the receive thread receives a fundamental data event.
        /// </summary>
        /// <param name="e">Fundamental Data Event Arguments</param>
        protected virtual void OnFundamentalData(FundamentalDetailsEventArgs e) { RaiseEvent(FundamentalData, this, e); }
        public void fundamentalData(int reqId, string data)
        {
            var e = new FundamentalDetailsEventArgs(reqId, data);
            OnFundamentalData(e);
        }

        /// <summary>
        /// This method receives the requested historical data results
        /// </summary>
        public event EventHandler<HistoricalDataEventArgs> HistoricalData;
        /// <summary>
        /// Called internally when the receive thread receives a historical data event.
        /// </summary>
        /// <param name="e">Historical Data Event Arguments</param>
        protected virtual void OnHistoricalData(HistoricalDataEventArgs e) { RaiseEvent(HistoricalData, this, e); }
        public void historicalData(int reqId, string date, double open, double high, double low, double close, int volume, int count,
            double WAP, bool hasGaps) { }
        public void historicalData2(int reqId, string date, double open, double high, double low, double close, int volume, int count,
            double WAP, bool hasGaps, int recordNumber, int recordTotal)
        {
            //Comes in as seconds
            //2 - dates are returned as a long integer specifying the number of seconds since 1/1/1970 GMT.
            long longDate = Int64.Parse(date, CultureInfo.InvariantCulture);
            //Check if date time string or seconds
            DateTime timeStamp;
            if (longDate < 30000000)
                timeStamp = new DateTime(Int32.Parse(date.Substring(0, 4)), Int32.Parse(date.Substring(4, 2)), Int32.Parse(date.Substring(6, 2)), 0, 0, 0, DateTimeKind.Utc);
            else
                timeStamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(longDate);

            var e = new HistoricalDataEventArgs(reqId, timeStamp, open, high, low, close, volume, count, WAP, hasGaps, recordNumber, recordTotal);
            OnHistoricalData(e);
        }

        /// <summary>
        /// This method receives the requested historical data results
        /// </summary>
        public event EventHandler<HistoricalDataEndEventArgs> HistoricalDataEnd;
        /// <summary>
        /// Called internally when the receive thread receives a tick price event.
        /// </summary>
        /// <param name="e">Historical Data Event Arguments</param>
        protected virtual void OnHistoricalDataEnd(HistoricalDataEndEventArgs e) { RaiseEvent(HistoricalDataEnd, this, e); }
        public void historicalDataEnd(int reqId, string start, string end)
        {
            var e = new HistoricalDataEndEventArgs(reqId, start, end);
            OnHistoricalDataEnd(e);
        }

        /// <summary>
        /// Called on a market data type call back.
        /// </summary>
        public event EventHandler<MarketDataTypeEventArgs> MarketDataType;
        /// <summary>
        /// Called internally when the receive thread receives a Market Data Type Event.
        /// </summary>
        protected virtual void OnMarketDataType(MarketDataTypeEventArgs e) { RaiseEvent(MarketDataType, this, e); }
        public void marketDataType(int reqId, int marketDataType)
        {
            var dataType = (MarketDataType) marketDataType;
            var e = new MarketDataTypeEventArgs(reqId, dataType);
            OnMarketDataType(e);
        }

        /// <summary>
        /// This method is called when the market depth changes.
        /// </summary>
        public event EventHandler<UpdateMarketDepthEventArgs> UpdateMarketDepth;
        /// <summary>
        /// Called internally when the receive thread receives an update market depth event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnUpdateMarketDepth(UpdateMarketDepthEventArgs e) { RaiseEvent(UpdateMarketDepth, this, e); }
        public void updateMktDepth(int tickerId, int position, int operation, int side, double price, int size)
        {
            var marketDepthOp = (MarketDepthOperation) operation;
            var marketDepthSide = (MarketDepthSide) side;
            var e = new UpdateMarketDepthEventArgs(tickerId, position, marketDepthOp, marketDepthSide, price, size);
            OnUpdateMarketDepth(e);
        }

        /// <summary>
        /// This method is called when the Level II market depth changes.
        /// </summary>
        public event EventHandler<UpdateMarketDepthL2EventArgs> UpdateMarketDepthL2;
        /// <summary>
        /// Called internally when the receive thread receives an update market depth level 2 event.
        /// </summary>
        /// <param name="e">Update Market Depth L2 Event Arguments</param>
        protected virtual void OnUpdateMarketDepthL2(UpdateMarketDepthL2EventArgs e) { RaiseEvent(UpdateMarketDepthL2, this, e); }
        public void updateMktDepthL2(int tickerId, int position, string marketMaker, int operation, int side, double price, int size)
        {
            var marketDepthOp = (MarketDepthOperation)operation;
            var marketDepthSide = (MarketDepthSide)side;
            var e = new UpdateMarketDepthL2EventArgs(tickerId, position, marketMaker, marketDepthOp, marketDepthSide, price, size);
            OnUpdateMarketDepthL2(e);
        }

        /// <summary>
        /// This method is triggered for each new bulletin if the client has subscribed (i.e. by calling the reqNewsBulletins() method.
        /// </summary>
        public event EventHandler<UpdateNewsBulletinEventArgs> UpdateNewsBulletin;

        /// <summary>
        /// Called internally when the receive thread receives an update news bulletin event.
        /// </summary>
        /// <param name="e">Update News Bulletin Event Arguments</param>
        protected virtual void OnUpdateNewsBulletin(UpdateNewsBulletinEventArgs e) { RaiseEvent(UpdateNewsBulletin, this, e); }
        public void updateNewsBulletin(int msgId, int msgType, string message, string origExchange)
        {
            var newsType = (NewsType) msgType;
            var e = new UpdateNewsBulletinEventArgs(msgId, newsType, message, origExchange);
            OnUpdateNewsBulletin(e);
        }

        /// <summary>
        /// This method receives the realtime bars data results.
        /// </summary>
        public event EventHandler<RealTimeBarEventArgs> RealTimeBar;
        /// <summary>
        /// Called internally when the receive thread receives a real time bar event.
        /// </summary>
        /// <param name="e">Real Time Bar Event Arguments</param>
        protected virtual void OnRealTimeBar(RealTimeBarEventArgs e) { RaiseEvent(RealTimeBar, this, e); }
        public void realtimeBar(int reqId, long time, double open, double high, double low, double close, long volume, double WAP,
            int count)
        {
            var e = new RealTimeBarEventArgs(reqId, time, open, high, low, close, volume, WAP, count);
            OnRealTimeBar(e);
        }

        /// <summary>
        /// This method receives an XML document that describes the valid parameters that a scanner subscription can have
        /// </summary>
        public event EventHandler<ScannerParametersEventArgs> ScannerParameters;
        /// <summary>
        /// Called internally when the receive thread receives a scanner parameters event.
        /// </summary>
        /// <param name="e">Scanner Parameters Event Arguments</param>
        protected virtual void OnScannerParameters(ScannerParametersEventArgs e) { RaiseEvent(ScannerParameters, this, e); }
        public void scannerParameters(string xml)
        {
            var e = new ScannerParametersEventArgs(xml);
            OnScannerParameters(e);
        }

        /// <summary>
        /// This method receives the requested market scanner data results
        /// </summary>
        public event EventHandler<ScannerDataEventArgs> ScannerData;
        /// <summary>
        /// Called internally when the receive thread receives a tick price event.
        /// </summary>
        /// <param name="e">Scanner Data Event Arguments</param>
        protected virtual void OnScannerData(ScannerDataEventArgs e) { RaiseEvent(ScannerData, this, e); }
        public void scannerData(int reqId, int rank, ContractDetails contractDetails, string distance, string benchmark,
            string projection, string legsStr)
        {
            var e = new ScannerDataEventArgs(reqId, rank, contractDetails, distance, benchmark, projection, legsStr);
            OnScannerData(e);
        }

        /// <summary>
        /// This method receives the requested market scanner data results
        /// </summary>
        public event EventHandler<ScannerDataEndEventArgs> ScannerDataEnd;
        /// <summary>
        /// Called internally when the receive thread receives a tick price event.
        /// </summary>
        /// <param name="e">Scanner Data Event Arguments</param>
        protected virtual void OnScannerDataEnd(ScannerDataEndEventArgs e) { RaiseEvent(ScannerDataEnd, this, e); }
        public void scannerDataEnd(int reqId)
        {
            var e = new ScannerDataEndEventArgs(reqId);
            OnScannerDataEnd(e);
        }

        #region FA events       
        /// <summary>
        /// This method receives previously requested FA configuration information from TWS.
        /// </summary>
        public event EventHandler<ReceiveFAEventArgs> ReceiveFA;
        /// <summary>
        /// Called internally when the receive thread receives a Receive Finanvial Advisor event.
        /// </summary>
        /// <param name="e">Receive FA Event Arguments</param>
        protected virtual void OnReceiveFA(ReceiveFAEventArgs e) { RaiseEvent(ReceiveFA, this, e); }
        public void receiveFA(int faDataType, string faXmlData)
        {
            var e = new ReceiveFAEventArgs((FADataType)faDataType, faXmlData);
            OnReceiveFA(e);
        }

        /// <summary>
        /// Returns the data from the TWS Account Window Summary tab.
        /// </summary>
        public event EventHandler<AccountSummaryEventArgs> AccountSummaryFA;
        /// <summary>
        /// Called internally when the receive thread receives a accountSummary event.
        /// </summary>
        /// <param name="e">accountSummary Event Arguments</param>
        protected virtual void OnAccountSummary(AccountSummaryEventArgs e) { RaiseEvent(AccountSummaryFA, this, e); }
        public void accountSummary(int reqId, string account, string tag, string value, string currency)
        {
            var e = new AccountSummaryEventArgs(reqId, account, tag, value, currency);
            OnAccountSummary(e);
        }

        /// <summary>
        /// Notifies when all the accounts' information has ben received.
        /// </summary>
        public event EventHandler<AccountSummaryEndEventArgs> AccountSummaryFAEnd;
        /// <summary>
        /// Called internally when the receive thread receives a accountSummaryEnd event.
        /// </summary>
        /// <param name="e">accountSummaryEnd Event Arguments</param>
        protected virtual void OnAccountSummaryEnd(AccountSummaryEndEventArgs e) { RaiseEvent(AccountSummaryFAEnd, this, e); }
        public void accountSummaryEnd(int reqId)
        {
            var e = new AccountSummaryEndEventArgs(reqId);
            OnAccountSummaryEnd(e);
        }

        /// <summary>
        /// Returns the data from the TWS Account Window Summary tab.
        /// </summary>
        public event EventHandler<PositionEventArgs> PositionFA;
        /// <summary>
        /// Called internally when the receive thread receives a position event.
        /// </summary>
        /// <param name="e">position Event Arguments</param>
        protected virtual void OnPosition(PositionEventArgs e) { RaiseEvent(PositionFA, this, e); }
        public void position(string account, Contract contract, int pos)
        {
            var e = new PositionEventArgs(account, contract, pos);
            OnPosition(e);
        }

        /// <summary>
        /// This method receives the requested historical data results
        /// </summary>
        public event EventHandler<PositionEndEventArgs> PositionFAEnd;
        /// <summary>
        /// Called internally when the receive thread receives a positionEnd event.
        /// </summary>
        /// <param name="e">positionEnd Event Arguments</param>
        protected virtual void OnPositionEnd(PositionEndEventArgs e) { RaiseEvent(PositionFAEnd, this, e); }
        public void positionEnd()
        {
            var e = new PositionEndEventArgs();
            OnPositionEnd(e);
        }
        #endregion
        #endregion

        #region IBClientSocket

        #region Properties

        /// <summary>
        /// Returns the status of the connection to TWS.
        /// </summary>
        public bool Connected
        {
            get { return _socket.IsConnected(); }
        }

        /// <summary>
        /// Returns the version of the TWS instance the API application is connected to
        /// </summary>
        public int ServerVersion
        {
            get { return _socket.ServerVersion; }
        }

        /// <summary>
        /// Returns the client version of the TWS API
        /// </summary>
        public static int ClientVersion
        {
            get { return Constants.ClientVersion; }
        }

        /// <summary>
        /// Returns the time the API application made a connection to TWS
        /// </summary>
        public String TwsConnectionTime
        {
            get { return _socket.TwsConnectionTime; }
        }

        public bool ThrowExceptions { get; set; }

        #endregion

        #region TWS Commmands

        /// <summary>
        /// This function must be called before any other. There is no feedback for a successful connection, but a subsequent attempt to connect will return the message "Already connected."
        /// </summary>
        /// <param name="host">host name or IP address of the machine where TWS is running. Leave blank to connect to the local host.</param>
        /// <param name="port">must match the port specified in TWS on the Configure>API>Socket Port field.</param>
        /// <param name="clientId">A number used to identify this client connection. All orders placed/modified from this client will be associated with this client identifier.
        /// Each client MUST connect with a unique clientId.</param>
        public void Connect(String host, int port, int clientId)
        {
            lock (this)
                _socket.eConnect(host, port, clientId);
        }

        /// <summary>
        /// Call this method to terminate the connections with TWS. Calling this method does not cancel orders that have already been sent.
        /// </summary>
        public void Disconnect()
        {
            lock (this)
            {
                GeneralTracer.WriteLineIf(ibTrace.TraceInfo, "IBClient Disconnect");
                _socket.eDisconnect();
            }
        }

        /// <summary>
        /// Call the cancelScannerSubscription() method to stop receiving market scanner results. 
        /// </summary>
        /// <param name="tickerId">the Id that was specified in the call to reqScannerSubscription().</param>
        public void CancelScannerSubscription(int tickerId)
        {
            lock (this)
                _socket.cancelScannerSubscription(tickerId);
        }

        /// <summary>
        /// Call the reqScannerParameters() method to receive an XML document that describes the valid parameters that a scanner subscription can have.
        /// </summary>
        public void RequestScannerParameters()
        {
            lock (this)
                _socket.reqScannerParameters();
        }

        /// <summary>
        /// Call the reqScannerSubscription() method to start receiving market scanner results through the scannerData() EWrapper method. 
        /// </summary>
        /// <param name="tickerId">the Id for the subscription. Must be a unique value. When the subscription  data is received, it will be identified by this Id. This is also used when canceling the scanner.</param>
        /// <param name="subscription">summary of the scanner subscription parameters including filters.</param>
        public void RequestScannerSubscription(int tickerId, ScannerSubscription subscription)
        {
            lock (this)
                _socket.reqScannerSubscription(tickerId, subscription);
        }

        /// <summary>
        /// Call this method to request market data. The market data will be returned by the tickPrice, tickSize, tickOptionComputation(), tickGeneric(), tickString() and tickEFP() methods.
        /// </summary>
        /// <param name="tickerId">the ticker id. Must be a unique value. When the market data returns, it will be identified by this tag. This is also used when canceling the market data.</param>
        /// <param name="contract">this structure contains a description of the contract for which market data is being requested.</param>
        /// <param name="genericTickList">comma delimited list of generic tick types.  Tick types can be found here: (new Generic Tick Types page) </param>
        /// <param name="snapshot">Allows client to request snapshot market data.</param>
        /// <param name="marketDataOff">Market Data Off - used in conjunction with RTVolume Generic tick type causes only volume data to be sent.</param>
        public void RequestMarketData(int tickerId, Contract contract, Collection<GenericTickType> genericTickList, bool snapshot, bool marketDataOff)
        {
            var genList = new StringBuilder();
            if (genericTickList != null)
            {
                foreach (GenericTickType t in genericTickList)
                    genList.AppendFormat("{0},", ((int)t).ToString(CultureInfo.InvariantCulture));
            }
            lock (this)
                _socket.reqMktData(tickerId, contract, genList.ToString().Trim(','), snapshot);
        }

        /// <summary>
        /// Call the CancelHistoricalData method to stop receiving historical data results.
        /// </summary>
        /// <param name="tickerId">the Id that was specified in the call to <see cref="RequestHistoricalData(int,Contract,DateTime,TimeSpan,BarSize,HistoricalDataType,int)"/>.</param>
        public void CancelHistoricalData(int tickerId)
        {
            lock (this)
                _socket.cancelHistoricalData(tickerId);
        }

        /// <summary>
        /// Call the CancelRealTimeBars() method to stop receiving real time bar results. 
        /// </summary>
        /// <param name="tickerId">The Id that was specified in the call to <see cref="RequestRealTimeBars"/>.</param>
        public void CancelRealTimeBars(int tickerId)
        {
            lock (this)
                _socket.cancelRealTimeBars(tickerId);
        }

        /// <summary>
        /// Call the reqHistoricalData() method to start receiving historical data results through the historicalData() EWrapper method. 
        /// </summary>
        /// <param name="tickerId">the Id for the request. Must be a unique value. When the data is received, it will be identified by this Id. This is also used when canceling the historical data request.</param>
        /// <param name="contract">this structure contains a description of the contract for which market data is being requested.</param>
        /// <param name="endDateTime">Date is sent after a .ToUniversalTime, so make sure the kind property is set correctly, and assumes GMT timezone. Use the format yyyymmdd hh:mm:ss tmz, where the time zone is allowed (optionally) after a space at the end.</param>
        /// <param name="duration">This is the time span the request will cover, and is specified using the format:
        /// <integer /> <unit />, i.e., 1 D, where valid units are:
        /// S (seconds)
        /// D (days)
        /// W (weeks)
        /// M (months)
        /// Y (years)
        /// If no unit is specified, seconds are used. "years" is currently limited to one.
        /// </param>
        /// <param name="barSizeSetting">
        /// specifies the size of the bars that will be returned (within IB/TWS limits). Valid values include:
        /// <list type="table">
        /// <listheader>
        ///     <term>Bar Size</term>
        ///     <description>Parametric Value</description>
        /// </listheader>
        /// <item>
        ///     <term>1 sec</term>
        ///     <description>1</description>
        /// </item>
        /// <item>
        ///     <term>5 secs</term>
        ///     <description>2</description>
        /// </item>
        /// <item>
        ///     <term>15 secs</term>
        ///     <description>3</description>
        /// </item>
        /// <item>
        ///     <term>30 secs</term>
        ///     <description>4</description>
        /// </item>
        /// <item>
        ///     <term>1 min</term>
        ///     <description>5</description>
        /// </item>
        /// <item>
        ///     <term>2 mins</term>
        ///     <description>6</description>
        /// </item>
        /// <item>
        ///     <term>5 mins</term>
        ///     <description>7</description>
        /// </item>
        /// <item>
        ///     <term>15 mins</term>
        ///     <description>8</description>
        /// </item>
        /// <item>
        ///     <term>30 mins</term>
        ///     <description>9</description>
        /// </item>
        /// <item>
        ///     <term>1 hour</term>
        ///     <description>10</description>
        /// </item>
        /// <item>
        ///     <term>1 day</term>
        ///     <description>11</description>
        /// </item>
        /// <item>
        ///     <term>1 week</term>
        ///     <description></description>
        /// </item>
        /// <item>
        ///     <term>1 month</term>
        ///     <description></description>
        /// </item>
        /// <item>
        ///     <term>3 months</term>
        ///     <description></description>
        /// </item>
        /// <item>
        ///     <term>1 year</term>
        ///     <description></description>
        /// </item>
        /// </list>
        /// </param>
        /// <param name="whatToShow">determines the nature of data being extracted. Valid values include:
        /// TRADES
        /// MIDPOINT
        /// BID
        /// ASK
        /// BID/ASK
        /// </param>
        /// <param name="useRth">
        /// determines whether to return all data available during the requested time span, or only data that falls within regular trading hours. Valid values include:
        /// 0 - all data is returned even where the market in question was outside of its regular trading hours.
        /// 1 - only data within the regular trading hours is returned, even if the requested time span falls partially or completely outside of the RTH.
        /// </param>
        public void RequestHistoricalData(int tickerId, Contract contract, DateTime endDateTime, TimeSpan duration,
                                      BarSize barSizeSetting, HistoricalDataType whatToShow, int useRth)
        {
            DateTime beginDateTime = endDateTime.Subtract(duration);
            TimeSpan period = endDateTime.Subtract(beginDateTime);
            string dur;

            double secs = period.TotalSeconds;
            long unit;

            if (secs < 1)
                throw new ArgumentOutOfRangeException("Period cannot be less than 1 second.");
            if (secs < 86400)
            {
                unit = (long) Math.Ceiling(secs);
                dur = string.Concat(unit, " S");
            }
            else
            {
                double days = secs / 86400;
                unit = (long)Math.Ceiling(days);
                if (unit <= 34)
                {
                    dur = string.Concat(unit, " D");
                }
                else
                {
                    double weeks = days / 7;
                    unit = (long)Math.Ceiling(weeks);
                    if (unit > 52)
                        throw new ArgumentOutOfRangeException("Period cannot be bigger than 52 weeks.");
                    dur = string.Concat(unit, " W");
                }
            }

            RequestHistoricalData(tickerId, contract, endDateTime, dur, barSizeSetting, whatToShow, useRth);
        }


        /// <summary>
        /// Call the reqHistoricalData() method to start receiving historical data results through the historicalData() EWrapper method. 
        /// </summary>
        /// <param name="tickerId">the Id for the request. Must be a unique value. When the data is received, it will be identified by this Id. This is also used when canceling the historical data request.</param>
        /// <param name="contract">this structure contains a description of the contract for which market data is being requested.</param>
        /// <param name="endDateTime">Date is sent after a .ToUniversalTime, so make sure the kind property is set correctly, and assumes GMT timezone. Use the format yyyymmdd hh:mm:ss tmz, where the time zone is allowed (optionally) after a space at the end.</param>
        /// <param name="duration">This is the time span the request will cover, and is specified using the format:
        /// <integer /> <unit />, i.e., 1 D, where valid units are:
        /// S (seconds)
        /// D (days)
        /// W (weeks)
        /// M (months)
        /// Y (years)
        /// If no unit is specified, seconds are used. "years" is currently limited to one.
        /// </param>
        /// <param name="barSizeSetting">
        /// specifies the size of the bars that will be returned (within IB/TWS limits). Valid values include:
        /// <list type="table">
        /// <listheader>
        ///     <term>Bar Size</term>
        ///     <description>Parametric Value</description>
        /// </listheader>
        /// <item>
        ///     <term>1 sec</term>
        ///     <description>1</description>
        /// </item>
        /// <item>
        ///     <term>5 secs</term>
        ///     <description>2</description>
        /// </item>
        /// <item>
        ///     <term>15 secs</term>
        ///     <description>3</description>
        /// </item>
        /// <item>
        ///     <term>30 secs</term>
        ///     <description>4</description>
        /// </item>
        /// <item>
        ///     <term>1 min</term>
        ///     <description>5</description>
        /// </item>
        /// <item>
        ///     <term>2 mins</term>
        ///     <description>6</description>
        /// </item>
        /// <item>
        ///     <term>5 mins</term>
        ///     <description>7</description>
        /// </item>
        /// <item>
        ///     <term>15 mins</term>
        ///     <description>8</description>
        /// </item>
        /// <item>
        ///     <term>30 mins</term>
        ///     <description>9</description>
        /// </item>
        /// <item>
        ///     <term>1 hour</term>
        ///     <description>10</description>
        /// </item>
        /// <item>
        ///     <term>1 day</term>
        ///     <description>11</description>
        /// </item>
        /// <item>
        ///     <term>1 week</term>
        ///     <description></description>
        /// </item>
        /// <item>
        ///     <term>1 month</term>
        ///     <description></description>
        /// </item>
        /// <item>
        ///     <term>3 months</term>
        ///     <description></description>
        /// </item>
        /// <item>
        ///     <term>1 year</term>
        ///     <description></description>
        /// </item>
        /// </list>
        /// </param>
        /// <param name="whatToShow">determines the nature of data being extracted. Valid values include:
        /// TRADES
        /// MIDPOINT
        /// BID
        /// ASK
        /// BID/ASK
        /// </param>
        /// <param name="useRth">
        /// determines whether to return all data available during the requested time span, or only data that falls within regular trading hours. Valid values include:
        /// 0 - all data is returned even where the market in question was outside of its regular trading hours.
        /// 1 - only data within the regular trading hours is returned, even if the requested time span falls partially or completely outside of the RTH.
        /// </param>
        public void RequestHistoricalData(int tickerId, Contract contract, DateTime endDateTime, string duration,
                                      BarSize barSizeSetting, HistoricalDataType whatToShow, int useRth)
        {
            string endDT = endDateTime.ToUniversalTime().ToString("yyyyMMdd HH:mm:ss", CultureInfo.InvariantCulture) + " UTC";
            string barSize = EnumDescConverter.GetEnumDescription(barSizeSetting);
            string wts = EnumDescConverter.GetEnumDescription(whatToShow);
            lock (this)
                _socket.reqHistoricalData(tickerId, contract, endDT, duration, barSize, wts, useRth, 2);
        }

        /// <summary>
        /// Call this function to download all details for a particular underlying. the contract details will be received via the contractDetails() function on the EWrapper.
        /// </summary>
        /// <param name="requestId">Request Id for Contract Details</param>
        /// <param name="contract">summary description of the contract being looked up.</param>
        public void RequestContractDetails(int requestId, Contract contract)
        {
            if (contract == null)
                throw new ArgumentNullException("contract");
            lock (this)
                _socket.reqContractDetails(requestId, contract);
        }

        /// <summary>
        /// Call the reqRealTimeBars() method to start receiving real time bar results through the realtimeBar() EWrapper method.
        /// </summary>
        /// <param name="tickerId">The Id for the request. Must be a unique value. When the data is received, it will be identified
        /// by this Id. This is also used when canceling the historical data request.</param>
        /// <param name="contract">This structure contains a description of the contract for which historical data is being requested.</param>
        /// <param name="barSize">Currently only 5 second bars are supported, if any other value is used, an exception will be thrown.</param>
        /// <param name="whatToShow">Determines the nature of the data extracted. Valid values include:
        /// TRADES
        /// BID
        /// ASK
        /// MIDPOINT
        /// </param>
        /// <param name="useRth">useRth – Regular Trading Hours only. Valid values include:
        /// 0 = all data available during the time span requested is returned, including time intervals when the market in question was outside of regular trading hours.
        /// 1 = only data within the regular trading hours for the product requested is returned, even if the time time span falls partially or completely outside.
        /// </param>
        public void RequestRealTimeBars(int tickerId, Contract contract, int barSize, RealTimeBarType whatToShow, bool useRth)
        {
            string wts = EnumDescConverter.GetEnumDescription(whatToShow);
            lock (this)
                _socket.reqRealTimeBars(tickerId, contract, barSize, wts, useRth);
        }


        /// <summary>
        /// Call this method to request market depth for a specific contract. The market depth will be returned by the updateMktDepth() and updateMktDepthL2() methods.
        /// </summary>
        /// <param name="tickerId">the ticker Id. Must be a unique value. When the market depth data returns, it will be identified by this tag. This is also used when canceling the market depth.</param>
        /// <param name="contract">this structure contains a description of the contract for which market depth data is being requested.</param>
        /// <param name="numberOfRows">specifies the number of market depth rows to return.</param>
        public void RequestMarketDepth(int tickerId, Contract contract, int numberOfRows)
        {
            if (contract == null)
                throw new ArgumentNullException("contract");
            lock (this)
                _socket.reqMarketDepth(tickerId, contract, numberOfRows);
        }

        /// <summary>
        /// After calling this method, market data for the specified Id will stop flowing.
        /// </summary>
        /// <param name="tickerId">the Id that was specified in the call to reqMktData().</param>
        public void CancelMarketData(int tickerId)
        {
            lock (this)
                _socket.cancelMktData(tickerId);
        }

        /// <summary>
        /// After calling this method, market depth data for the specified Id will stop flowing.
        /// </summary>
        /// <param name="tickerId">the Id that was specified in the call to reqMktDepth().</param>
        public void CancelMarketDepth(int tickerId)
        {
            lock (this)
                _socket.cancelMktDepth(tickerId);
        }

        /// <summary>
        /// Call the exerciseOptions() method to exercise options. 
        /// “SMART” is not an allowed exchange in exerciseOptions() calls, and that TWS does a moneyness request for the position in question whenever any API initiated exercise or lapse is attempted.
        /// </summary>
        /// <param name="tickerId">the Id for the exercise request.</param>
        /// <param name="contract">this structure contains a description of the contract to be exercised.  If no multiplier is specified, a default of 100 is assumed.</param>
        /// <param name="exerciseAction">this can have two values:
        /// 1 = specifies exercise
        /// 2 = specifies lapse
        /// </param>
        /// <param name="exerciseQuantity">the number of contracts to be exercised</param>
        /// <param name="account">specifies whether your setting will override the system's natural action. For example, if your action is "exercise" and the option is not in-the-money, by natural action the option would not exercise. If you have override set to "yes" the natural action would be overridden and the out-of-the money option would be exercised. Values are: 
        /// 0 = no
        /// 1 = yes
        /// </param>
        /// <param name="overrideRenamed">
        /// specifies whether your setting will override the system's natural action. For example, if your action is "exercise" and the option is not in-the-money, by natural action the option would not exercise. If you have override set to "yes" the natural action would be overridden and the out-of-the money option would be exercised. Values are: 
        /// 0 = no
        /// 1 = yes
        /// </param>
        public void ExerciseOptions(int tickerId, Contract contract, int exerciseAction, int exerciseQuantity,
                                    String account, int overrideRenamed)
        {
            if (contract == null)
                throw new ArgumentNullException("contract");
            lock (this)
                _socket.exerciseOptions(tickerId, contract, exerciseAction, exerciseQuantity, account, overrideRenamed);
        }

        /// <summary>
        /// Call this method to place an order. The order status will be returned by the orderStatus event.
        /// </summary>
        /// <param name="orderId">the order Id. You must specify a unique value. When the order status returns, it will be identified by this tag. This tag is also used when canceling the order.</param>
        /// <param name="contract">this structure contains a description of the contract which is being traded.</param>
        /// <param name="order">this structure contains the details of the order.
        /// Each client MUST connect with a unique clientId.</param>
        public void PlaceOrder(int orderId, Contract contract, Order order)
        {
            if (contract == null)
                throw new ArgumentNullException("contract");
            if (order == null)
                throw new ArgumentNullException("order");
            lock (this)
                _socket.placeOrder(orderId, contract, order);
        }

        /// <summary>
        /// Call this function to start getting account values, portfolio, and last update time information.
        /// </summary>
        /// <param name="subscribe">If set to TRUE, the client will start receiving account and portfolio updates. If set to FALSE, the client will stop receiving this information.</param>
        /// <param name="acctCode">the account code for which to receive account and portfolio updates.</param>
        public void RequestAccountUpdates(bool subscribe, String acctCode)
        {
            lock (this)
                _socket.reqAccountUpdates(subscribe, acctCode);
        }

        /// <summary>
        /// When this method is called, the execution reports that meet the filter criteria are downloaded to the client via the execDetails() method.
        /// </summary>
        /// <param name="requestId">Id of the request</param>
        /// <param name="filter">the filter criteria used to determine which execution reports are returned.</param>
        public void RequestExecutions(int requestId, KrsExecutionFilter filter)
        {
            if (filter == null)
                filter = new KrsExecutionFilter(0, "", DateTime.MinValue, "", SecurityType.Undefined, "", ActionSide.Undefined);
            lock (this)
            {
                filter.Convert();
                _socket.reqExecutions(requestId, filter);
            }
        }

        /// <summary>
        /// Call this method to cancel an order.
        /// </summary>
        /// <param name="orderId">Call this method to cancel an order.</param>
        public void CancelOrder(int orderId)
        {
            lock (this)
                _socket.cancelOrder(orderId);
        }

        /// <summary>
        /// Call this method to request the open orders that were placed from this client. Each open order will be fed back through the openOrder() and orderStatus() functions on the EWrapper.
        /// 
        /// The client with a clientId of "0" will also receive the TWS-owned open orders. These orders will be associated with the client and a new orderId will be generated. This association will persist over multiple API and TWS sessions.
        /// </summary>
        public void RequestOpenOrders()
        {
            lock (this)
                _socket.reqOpenOrders();
        }

        /// <summary>
        /// Returns one next valid Id...
        /// </summary>
        /// <param name="numberOfIds">Has No Effect</param>
        public void RequestIds(int numberOfIds)
        {
            lock (this)
                _socket.reqIds(numberOfIds);
        }

        /// <summary>
        /// Call this method to start receiving news bulletins. Each bulletin will be returned by the updateNewsBulletin() method.
        /// </summary>
        /// <param name="allMessages">if set to TRUE, returns all the existing bulletins for the current day and any new ones. IF set to FALSE, will only return new bulletins.</param>
        public void RequestNewsBulletins(bool allMessages)
        {
            lock (this)
                _socket.reqNewsBulletins(allMessages);
        }

        /// <summary>
        /// Call this method to stop receiving news bulletins.
        /// </summary>
        public void CancelNewsBulletins()
        {
            lock (this)
                _socket.cancelNewsBulletin();
        }

        /// <summary>
        /// Call this method to request that newly created TWS orders be implicitly associated with the client. When a new TWS order is created, the order will be associated with the client and fed back through the openOrder() and orderStatus() methods on the EWrapper.
        /// 
        /// TWS orders can only be bound to clients with a clientId of “0”.
        /// </summary>
        /// <param name="autoBind">If set to TRUE, newly created TWS orders will be implicitly associated with the client. If set to FALSE, no association will be made.</param>
        public void RequestAutoOpenOrders(bool autoBind)
        {
            lock (this)
                _socket.reqAutoOpenOrders(autoBind);
        }

        /// <summary>
        /// Call this method to request the open orders that were placed from all clients and also from TWS. Each open order will be fed back through the openOrder() and orderStatus() functions on the EWrapper.
        /// 
        /// No association is made between the returned orders and the requesting client.
        /// </summary>
        public void RequestAllOpenOrders()
        {
            lock (this)
                _socket.reqAllOpenOrders();
        }

        /// <summary>
        /// Call this method to request the list of managed accounts. The list will be returned by the managedAccounts() function on the EWrapper.
        /// 
        /// This request can only be made when connected to a Financial Advisor (FA) account.
        /// </summary>
        public void RequestManagedAccts()
        {
            lock (this)
                _socket.reqManagedAccts();
        }

        /// <summary>
        /// Call this method to request FA configuration information from TWS. The data returns in an XML string via the receiveFA() method.
        /// </summary>
        /// <param name="faDataType">
        /// faDataType - specifies the type of Financial Advisor configuration data being requested. Valid values include:
        /// 1 = GROUPS
        /// 2 = PROFILE
        /// 3 =ACCOUNT ALIASES
        /// </param>
        public void RequestFA(FADataType faDataType)
        {
            lock (this)
                _socket.requestFA((int)faDataType);
        }

        /// <summary>
        /// Call this method to request FA configuration information from TWS. The data returns in an XML string via a "receiveFA" ActiveX event.  
        /// </summary>
        /// <param name="faDataType">
        /// specifies the type of Financial Advisor configuration data being requested. Valid values include:
        /// 1 = GROUPS
        /// 2 = PROFILE
        /// 3 = ACCOUNT ALIASES</param>
        /// <param name="xml">the XML string containing the new FA configuration information.</param>
        public void ReplaceFA(FADataType faDataType, String xml)
        {
            lock (this)
                _socket.replaceFA((int)faDataType, xml);
        }

        /// <summary>
        /// Returns the current system time on the server side.
        /// </summary>
        public void RequestCurrentTime()
        {
            lock (this)
                _socket.reqCurrentTime();
        }

        /// <summary>
        /// Request Fundamental Data
        /// </summary>
        /// <param name="requestId">Request Id</param>
        /// <param name="contract">Contract to request fundamental data for</param>
        /// <param name="reportType">Report Type</param>
        public virtual void RequestFundamentalData(int requestId, Contract contract, String reportType)
        {
            lock (this)
                _socket.reqFundamentalData(requestId, contract, reportType);
        }

        /// <summary>
        /// Call this method to stop receiving Reuters global fundamental data.
        /// </summary>
        /// <param name="requestId">The ID of the data request.</param>
        public virtual void CancelFundamentalData(int requestId)
        {
            lock (this)
                _socket.cancelFundamentalData(requestId);
        }

        public virtual void CancelCalculateImpliedVolatility(int reqId)
        {
            _socket.cancelCalculateImpliedVolatility(reqId);
        }

        /// <summary>
        /// Calculates the Implied Volatility based on the user-supplied option and underlying prices.
        /// The calculated implied volatility is returned by tickOptionComputation( ) in a new tick type, CUST_OPTION_COMPUTATION, which is described below.
        /// </summary>
        /// <param name="requestId">Request Id</param>
        /// <param name="contract">Contract</param>
        /// <param name="optionPrice">Price of the option</param>
        /// <param name="underPrice">Price of teh underlying of the option</param>
        public virtual void RequestCalculateImpliedVolatility(int requestId, Contract contract, double optionPrice, double underPrice)
        {
            lock (this)
                _socket.calculateImpliedVolatility(requestId, contract, optionPrice, underPrice);
        }

        public virtual void RequestCalculateOptionPrice(int reqId, Contract contract, double volatility, double underPrice)
        {
            _socket.calculateOptionPrice(reqId, contract, volatility, underPrice);
        }

        public virtual void CancelCalculateOptionPrice(int reqId)
        {
            _socket.cancelCalculateOptionPrice(reqId);
        }

        public virtual void RequestGlobalCancel()
        {
            _socket.reqGlobalCancel();
        }

        public virtual void RequestMarketDataType(int marketDataType)
        {
            _socket.reqMarketDataType(marketDataType);
        }

        #region FA
        /// <summary>
        /// Requests a specific account's summary.
        /// This method will subscribe to the account summary as presented in the TWS' Account Summary tab. The data is returned at EWrapper::accountSummary
        /// </summary>
        /// <param name="reqId">the unique request idntifier.</param>
        /// <param name="group">set to "All" to return account summary data for all accounts, or set to a specific Advisor Account Group name that has already been created in TWS Global Configuration.</param>
        /// <param name="summaryItems">flags of AccountSummary items.</param>
        /// <seealso cref="CancelAccountSummary"/>
        public virtual void RequestAccountSummary(int reqId, string group, AccountSummary summaryItems)
        {
            var sb = new StringBuilder();
            var values = Enum.GetValues(typeof(AccountSummary)).Cast<AccountSummary>();
            foreach (AccountSummary si in values)
            {
                if (summaryItems.HasFlag(si))
                    sb.Append(si + ",");
            }
            if (sb.Length > 0)
            {
                string tags = sb.ToString().Trim(',');
                _socket.reqAccountSummary(reqId, group, tags);
            }
        }
        /// <summary>
        /// Cancels the account's summary request. After requesting an account's summary, invoke this function to cancel it.
        /// </summary>
        /// <param name="reqId">the identifier of the previously performed account request</param>
        /// <seealso cref="RequestAccountSummary"/>
        public virtual void CancelAccountSummary(int reqId)
        {
            _socket.cancelAccountSummary(reqId);
        }

        /// <summary>
        /// Requests all positions from all accounts
        /// </summary>
        /// <seealso cref="CancelPositions"/>
        public virtual void RequestPositions()
        {
            _socket.reqPositions();
        }
        /// <summary>
        /// Cancels all account's positions request
        /// </summary>
        /// <seealso cref="RequestPositions"/>
        public virtual void CancelPositions()
        {
            _socket.cancelPositions();
        } 
        #endregion

        /// <summary>
        /// The default level is ERROR. Refer to the API logging page for more details.
        /// </summary>
        /// <param name="serverLogLevel">
        /// logLevel - specifies the level of log entry detail used by the server (TWS) when processing API requests. Valid values include: 
        /// 1 = SYSTEM
        /// 2 = ERROR
        /// 3 = WARNING
        /// 4 = INFORMATION
        /// 5 = DETAIL
        /// </param>
        public void SetServerLogLevel(LogLevel serverLogLevel)
        {
            lock (this)
                _socket.setServerLogLevel((int)serverLogLevel);
        }

        #endregion
        #endregion

        #region Helpers
        ///<summary>
        /// Raise the event in a threadsafe manner
        ///</summary>
        ///<param name="event"></param>
        ///<param name="sender"></param>
        ///<param name="e"></param>
        ///<typeparam name="T"></typeparam>
        static private void RaiseEvent<T>(EventHandler<T> @event, object sender, T e)
        where T : EventArgs
        {
            EventHandler<T> handler = @event;
            if (handler == null) return;
            handler(sender, e);
        } 
        #endregion
    }
}