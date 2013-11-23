using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoLite
{
    public class DBContext : IDisposable
    {
        #region Default Connection String Name
        private static String _DefaultConnectionStringName;

        /// <summary>
        /// Gets the name of the connection string to use if one is not specified in the constructor 
        /// </summary>
        public static String DefaultConnectionStringName
        {
            get {
                if (String.IsNullOrWhiteSpace(_DefaultConnectionStringName))
                {
                    //if a default connection string has been explicitly set, then use it, otherwise, use "ConnectionString"
                    if (HostSpecific.ConfigurationManager.Settings.Keys.Cast<String>().Contains("AdoLite.DefaultConnectionString"))
                    { _DefaultConnectionStringName = HostSpecific.ConfigurationManager.Settings["AdoLite.DefaultConnectionString"]; }
                    else
                    { _DefaultConnectionStringName = "ConnectionString"; }
                }

                return _DefaultConnectionStringName;
            }
        }
        #endregion

        #region Connection String Name

        private String _ConnectionStringName;

        /// <summary>
        /// Gets the name of the connection string this context context is working with
        /// </summary>
        public String ConnectionStringName
        {
            get { return _ConnectionStringName; }
        }

        #endregion

        #region Provider property

        private DbProviderFactory _ProviderFactory;

        internal DbProviderFactory ProviderFactory
        {
            get
            {
                if(_ProviderFactory == null)
                { _ProviderFactory = DbProviderFactories.GetFactory(HostSpecific.ConfigurationManager.ConnectionStrings[this.ConnectionStringName].ProviderName); }

                return _ProviderFactory;
            }
        }

        #endregion

        #region Uses Transaction

        private Boolean _UsesTransaction;

        /// <summary>
        /// Gets whether or not the commands executed by this context will be part of a transaction
        /// </summary>
        protected Boolean UsesTransaction { get { return _UsesTransaction; } }

        #endregion

        /// <summary>
        /// Creates a context, optionaly specifying a connection string name and optionally using a transaction
        /// <param name="cnStringName">The name of the connection string to use</param>
        /// <param name="useTransaction">Whether or not to use this context with a transaction</param>
        /// </summary>
        public DBContext(String cnStringName = null, Boolean useTransaction = false)
        {
            if (String.IsNullOrWhiteSpace(cnStringName))
            { this._ConnectionStringName = DefaultConnectionStringName; }
            else
            { this._ConnectionStringName = cnStringName; }

            this._UsesTransaction = useTransaction;
        }

        #region Connection

        private IDbConnection _Connection;

        /// <summary>
        /// Gets the connection used by this context
        /// </summary>
        protected IDbConnection Connection
        {
            get
            {
                if(_Connection == null)
                {
                    _Connection = ProviderFactory.CreateConnection();
                    _Connection.ConnectionString = HostSpecific.ConfigurationManager.ConnectionStrings[this.ConnectionStringName].ConnectionString;
                    _Connection.Open();

                    if(this.UsesTransaction)
                    { _Transaction = _Connection.BeginTransaction(); }
                }

                return _Connection;
            }
        }

        #endregion

        #region Transaction
        private IDbTransaction _Transaction;

        /// <summary>
        /// Gets the transaction used by this context
        /// </summary>
        private IDbTransaction Transaction
        { get { return _Transaction; } }

        #endregion

        /// <summary>
        /// Executes a reader against this context's database
        /// </summary>
        /// <param name="cmd">The command to execute</param>
        /// <returns>The results of executing the reader</returns>
        public IDataReader ExecuteReader(IDbCommand cmd)
        {
            cmd.Connection = this.Connection;
            if(this.UsesTransaction)
            { cmd.Transaction = this.Transaction; }

            return cmd.ExecuteReader();
        }

        /// <summary>
        /// Executes a non-query against this context's database
        /// </summary>
        /// <param name="cmd">The command to execute</param>
        /// <returns>The number of rows affected</returns>
        public int ExecuteNonQuery(IDbCommand cmd)
        {
            cmd.Connection = this.Connection;

            if(this.UsesTransaction)
            { cmd.Transaction = this.Transaction; }

            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes an atomic non-query against the default database
        /// </summary>
        /// <param name="cmd">The command to execute</param>
        /// <returns>The number of rows affected</returns>
        public static int NonQuery(IDbCommand cmd)
        {
            var rowsAffected = 0;

            using (var context = new DBContext())
            {
                try {
                    context.ExecuteNonQuery(cmd);
                    context.Commit();
                }
                catch {
                    context.RollBack();
                    throw;
                }
            }

            return rowsAffected;
        }

        /// <summary>
        /// Executes a command on the database, and fills a dataset with the result set
        /// </summary>
        /// <param name="cmd">The command to execute</param>
        /// <returns>The result set</returns>
        public DataSet ExecuteQuery(IDbCommand cmd)
        {
            var ds = new DataSet();

            cmd.Connection = this.Connection;

            if(UsesTransaction)
            { cmd.Transaction = this.Transaction; }

            var da = ProviderFactory.CreateDataAdapter();
            da.SelectCommand = cmd as DbCommand;
            da.Fill(ds); //if an exception is thrown here, intentionally do not catch it.
            //let the caller handle it and rollback the transaction appropriately.

            if (da is IDisposable)
            { (da as IDisposable).Dispose(); }

            ds = null;

            //do not dispose the command, let the caller do that
            return ds;
        }

        /// <summary>
        /// Executes an atomic query on the default database
        /// </summary>
        /// <param name="cmd">The command to execute</param>
        /// <returns>The result set</returns>
        public static DataSet Query(IDbCommand cmd)
        {
            DataSet ds;

            using (var context = new DBContext())
            {
                try
                { ds = context.ExecuteQuery(cmd); }
                catch 
                {  ds = new DataSet(); }
            }

            return ds;
        }

        /// <summary>
        /// Creates a command to be used on this context's instance
        /// </summary>
        /// <param name="commandText">The command text</param>
        /// <returns>A command to be used on this context's instance</returns>
        public IDbCommand CreateCommand(StringBuilder commandText)
        { return CreateCommand(commandText.ToString()); }

        /// <summary>
        /// Creates a command to be used on this context's instance
        /// </summary>
        /// <param name="commandText">The command text</param>
        /// <returns>A command to be used on this context's instance</returns>
        public IDbCommand CreateCommand(String commandText)
        {
            var cmd = ProviderFactory.CreateCommand();
            cmd.CommandText = commandText;
            return cmd;
        }

        /// <summary>
        /// Creates a command to be used on the default context
        /// </summary>
        /// <param name="commandText">the command text</param>
        /// <returns>A command to be used on the default context</returns>
        public static IDbCommand Command(String commandText)
        {
            IDbCommand cmd;
            using (var context = new DBContext())
            { cmd = context.CreateCommand(commandText); }

            return cmd;
        }

        /// <summary>
        /// Commits the transaction if a transaction is used with this instance
        /// </summary>
        public void Commit()
        {
            if (this.UsesTransaction)
            {
                this.Transaction.Commit();
                _Transaction = _Connection.BeginTransaction();
            }
        }

        /// <summary>
        /// Rolls the transaction back if a transaction is used with this instance
        /// </summary>
        public void RollBack()
        { 
            if(this.UsesTransaction)
            { this.Transaction.Rollback(); }
        }

        public void Dispose()
        {
            if (this.UsesTransaction && this.Transaction != null)
            {
                this.Transaction.Commit();
                this.Transaction.Dispose();
                this._Transaction = null;
            }

            if(this.Connection != null)
            {
                this.Connection.Close();
                this.Connection.Dispose();
                this._Connection = null;
            }
        }
    }
}
