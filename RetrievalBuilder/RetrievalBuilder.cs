// ------------------------------------------------------------------------------------------------------------
// <copyright company="Invensys Systems Inc" file="RetrievalBuilder.cs">
//   Copyright (C) 2013 Invensys Systems Inc.  All rights reserved.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
// </copyright>
// <summary>
//
// </summary>
// ------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.ObjectModel;

namespace RetrievalBuilder
{
    /// <summary>
    /// Retrieval builder will query data using the toolkit
    /// </summary>
    public class RetrievalBuilder
    {
        #region --Variables--
        private ArchestrA.HistorianAccessError mLastError = new ArchestrA.HistorianAccessError();
        public ObservableCollection<ArchestrA.HistoryQueryResult> HistoryCollection;
        public ObservableCollection<ArchestrA.AnalogSummaryQueryResult> AnalogHistoryCollection;
        public ObservableCollection<ArchestrA.StateSummaryQueryResult> StateHistoryCollection;

        private System.Threading.Thread AnalogSummaryThread;
        private System.Threading.Thread HistoryThread;
        private System.Threading.Thread StateSummaryThread;

        /// <summary>
        /// Provides the last error from performing historian operations.
        /// </summary>
        public ArchestrA.HistorianAccessError LastError
        {
            get
            {
                return mLastError;
            }
        }

        #endregion

        public struct ThreadParameters
        {
            public ArchestrA.HistorianAccess access;
            public object arguments;
            public Boolean AutoRefresh;
            public ulong DurationMS;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RetrievalBuilder()
        {
        }

        /// <summary>
        /// Retrieval function for analog summary values. It will query data and place into an observable collection to be used for binding in a datagrid
        /// </summary>
        /// <param name="access">Current historian access connection established</param>
        /// <param name="arguments">Query arguments to pass for retrieval</param>
        /// <returns>Observable collection with query results</returns>
        public ObservableCollection<ArchestrA.AnalogSummaryQueryResult> RetrieveAnalogSummaryValues(ArchestrA.HistorianAccess access, ArchestrA.AnalogSummaryQueryArgs arguments)
        {
            ArchestrA.AnalogSummaryQuery result = access.CreateAnalogSummaryQuery();
            ArchestrA.HistorianAccessError error = null;
            result.StartQuery(arguments, out error);
            AnalogHistoryCollection = new ObservableCollection<ArchestrA.AnalogSummaryQueryResult>();
            while (result.MoveNext(out error))
                AnalogHistoryCollection.Add((ArchestrA.AnalogSummaryQueryResult)result.QueryResult.Clone());
            return AnalogHistoryCollection;
        }

        /// <summary>
        /// Retrieval function for analog summary values. It will query data and place into an observable collection to be used for binding in a datagrid
        /// </summary>
        /// <param name="access">Current historian access connection established</param>
        /// <param name="arguments">Query arguments to pass for retrieval</param>
        /// <param name="AutoRefresh">Enable auto refresh</param>
        /// <param name="DurationMS">duration for query data</param>        
        public void RetrieveAnalogSummaryValues(ArchestrA.HistorianAccess access, ArchestrA.AnalogSummaryQueryArgs arguments, Boolean AutoRefresh, ulong DurationMS)
        {

            AnalogSummaryThread = new System.Threading.Thread(AnalogSummaryThreadFunction);
            ThreadParameters param = new ThreadParameters();
            param.access = access;
            param.arguments = arguments;
            param.AutoRefresh = AutoRefresh;
            param.DurationMS = DurationMS;
            AnalogSummaryThread.Start(param);            
        }

        public void AnalogSummaryThreadFunction(object parameters)
        {
            ThreadParameters param = (ThreadParameters)parameters;

            while (true)
            {
                ArchestrA.HistorianAccessError error = null;
                ArchestrA.AnalogSummaryQuery result = param.access.CreateAnalogSummaryQuery();
                DateTime dt = DateTime.Now;
                ((ArchestrA.AnalogSummaryQueryArgs)param.arguments).EndDateTime = dt;
                ((ArchestrA.AnalogSummaryQueryArgs)param.arguments).StartDateTime = dt.AddMilliseconds(-1 * (int)param.DurationMS);
                result.StartQuery(((ArchestrA.AnalogSummaryQueryArgs)param.arguments), out error);
                AnalogHistoryCollection = new ObservableCollection<ArchestrA.AnalogSummaryQueryResult>();
                while (result.MoveNext(out error))
                    AnalogHistoryCollection.Add((ArchestrA.AnalogSummaryQueryResult)result.QueryResult.Clone());
                AnalogQueryCompleted(AnalogHistoryCollection);

                System.Threading.Thread.Sleep(10000);
            }

        }

        /// <summary>
        /// Retrieval function for history values. It will query data and place into an observable collection to be used for binding in a datagrid
        /// </summary>
        /// <param name="access">Current historian access connection established</param>
        /// <param name="arguments">Query arguments to pass for retrieval</param>
        /// <returns>Observable collection with query results</returns>
        public ObservableCollection<ArchestrA.HistoryQueryResult> RetrieveHistoryValues(ArchestrA.HistorianAccess access, ArchestrA.HistoryQueryArgs arguments)
        {
            ArchestrA.HistorianAccessError error = null;
            ArchestrA.HistoryQuery result = access.CreateHistoryQuery();
            result.StartQuery(arguments, out error);
            HistoryCollection = new ObservableCollection<ArchestrA.HistoryQueryResult>();
            while (result.MoveNext(out error))
                HistoryCollection.Add((ArchestrA.HistoryQueryResult)result.QueryResult.Clone());
            
            return HistoryCollection;
        }
        
        /// <summary>
        /// Retrieval function for history values. It will query data and place into an observable collection to be used for binding in a datagrid
        /// </summary>
        /// <param name="access">Current historian access connection established</param>
        /// <param name="arguments">Query arguments to pass for retrieval</param>
        /// <param name="AutoRefresh">Enable auto refresh</param>
        /// <param name="DurationMS">duration for query data</param>
        public void RetrieveHistoryValues(ArchestrA.HistorianAccess access, ArchestrA.HistoryQueryArgs arguments, Boolean AutoRefresh, ulong DurationMS)
        {
            HistoryThread = new System.Threading.Thread(HistoryThreadFunction);
            ThreadParameters param = new ThreadParameters();
            param.access = access;
            param.arguments = arguments;
            param.AutoRefresh = AutoRefresh;
            param.DurationMS = DurationMS;
            HistoryThread.Start(param); 
        }

        public void HistoryThreadFunction(object parameters)
        {
            ThreadParameters param = (ThreadParameters)parameters;

            while (true)
            {
                ArchestrA.HistorianAccessError error = null;
                ArchestrA.HistoryQuery result = param.access.CreateHistoryQuery();
                DateTime dt = DateTime.Now;
                ((ArchestrA.HistoryQueryArgs)param.arguments).EndDateTime = dt;
                ((ArchestrA.HistoryQueryArgs)param.arguments).StartDateTime = dt.AddMilliseconds(-1 * (int)param.DurationMS);
                result.StartQuery(((ArchestrA.HistoryQueryArgs)param.arguments), out error);
                HistoryCollection = new ObservableCollection<ArchestrA.HistoryQueryResult>();
                while (result.MoveNext(out error))
                    HistoryCollection.Add((ArchestrA.HistoryQueryResult)result.QueryResult.Clone());
                HistoryQueryCompleted(HistoryCollection);                
                System.Threading.Thread.Sleep(10000);
            }

        }        

        /// <summary>
        /// Retrieval function for state summary values. It will query data and place into an observable collection to be used for binding in a datagrid
        /// </summary>
        /// <param name="access">Current historian access connection established</param>
        /// <param name="arguments">Query arguments to pass for retrieval</param>
        /// <returns>Observable collection with query results</returns>
        public ObservableCollection<ArchestrA.StateSummaryQueryResult> RetrieveStateSummaryValues(ArchestrA.HistorianAccess access, ArchestrA.StateSummaryQueryArgs arguments)
        {
            ArchestrA.HistorianAccessError error = null;
            ArchestrA.StateSummaryQuery result = access.CreateStateSummaryQuery();
            result.StartQuery(arguments, out error);
            StateHistoryCollection = new ObservableCollection<ArchestrA.StateSummaryQueryResult>();
            while (result.MoveNext(out error))
                StateHistoryCollection.Add((ArchestrA.StateSummaryQueryResult)result.QueryResult.Clone());
            return StateHistoryCollection;
        }

        /// <summary>
        /// Retrieval function for state summary values. It will query data and place into an observable collection to be used for binding in a datagrid
        /// </summary>
        /// <param name="access">Current historian access connection established</param>
        /// <param name="arguments">Query arguments to pass for retrieval</param>
        /// <returns>Observable collection with query results</returns>
        public void RetrieveStateSummaryValues(ArchestrA.HistorianAccess access, ArchestrA.StateSummaryQueryArgs arguments, Boolean AutoRefresh, ulong DurationMS)
        {
            StateSummaryThread = new System.Threading.Thread(HistoryThreadFunction);
            ThreadParameters param = new ThreadParameters();
            param.access = access;
            param.arguments = arguments;
            param.AutoRefresh = AutoRefresh;
            param.DurationMS = DurationMS;
            StateSummaryThread.Start(param); 
        }

        public void StateSummaryThreadFunction(object parameters)
        {
            ThreadParameters param = (ThreadParameters)parameters;

            while (true)
            {
                ArchestrA.HistorianAccessError error = null;
                ArchestrA.StateSummaryQuery result = param.access.CreateStateSummaryQuery();
                DateTime dt = DateTime.Now;
                ((ArchestrA.StateSummaryQueryArgs)param.arguments).EndDateTime = dt;
                ((ArchestrA.StateSummaryQueryArgs)param.arguments).StartDateTime = dt.AddMilliseconds(-1 * (int)param.DurationMS);
                result.StartQuery(((ArchestrA.StateSummaryQueryArgs)param.arguments), out error);
                StateHistoryCollection = new ObservableCollection<ArchestrA.StateSummaryQueryResult>();
                while (result.MoveNext(out error))
                    StateHistoryCollection.Add((ArchestrA.StateSummaryQueryResult)result.QueryResult.Clone());
                StateQueryCompleted(StateHistoryCollection);

                System.Threading.Thread.Sleep(10000);
            }

        }

        public void StopAllThreads()
        {
            if (AnalogSummaryThread != null)
            {
                AnalogSummaryThread.Abort();
            }

            if (HistoryThread != null)
            {
                HistoryThread.Abort();
            }

            if (StateSummaryThread != null)
            {
                StateSummaryThread.Abort();
            }
        }

        /// <summary>
        /// Provide Async update when query is completed
        /// </summary>
        public event AnalogQueryComplete AnalogQueryCompleted;
        public delegate void AnalogQueryComplete(ObservableCollection<ArchestrA.AnalogSummaryQueryResult> AnalogHistoryCollection);

        public event StateQueryComplete StateQueryCompleted;
        public delegate void StateQueryComplete(ObservableCollection<ArchestrA.StateSummaryQueryResult> StateHistoryCollection);

        public event HistoryQueryComplete HistoryQueryCompleted;
        public delegate void HistoryQueryComplete(ObservableCollection<ArchestrA.HistoryQueryResult> HistoryCollection);

    }
}
