﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading;
using System.Collections.Concurrent;

namespace Destrier
{
    /// <summary>
    /// This is main class for setting up the connection strings associated with the application database context.
    /// </summary>
    public class DatabaseConfigurationContext
    {
        private static ConcurrentDictionary<String, DbProviderFactory> _providerFactories = null;
        private static ConcurrentDictionary<String, String> _connectionStrings = null;
        private static object _connectionLock = new object();
        private static object _providerLock = new object();

        private static object _defaultProviderLock = new object();
        private static DbProviderFactory _defaultProvider = null;

        /// <summary>
        /// A mapping between connection 'name's (think, short hand names for connections) and their connection strings.
        /// </summary>
        public static ConcurrentDictionary<String, String> ConnectionStrings
        {
            get
            {
                if (null == _connectionStrings)
                    lock(_connectionLock)
                        if(null == _connectionStrings)
                            _connectionStrings = new ConcurrentDictionary<string, string>();
                
                return _connectionStrings;
            }
        }

        /// <summary>
        /// A mapping between connection strings and their db providers (if not using mssql).
        /// </summary>
        public static ConcurrentDictionary<String, DbProviderFactory> DbProviders
        {
            get
            {
                if (null == _providerFactories)
                    lock (_providerLock)
                        if (null == _providerFactories)
                            _providerFactories = new ConcurrentDictionary<String, DbProviderFactory>();

                return _providerFactories;
            }
        }

        /// <summary>
        /// Add a new connection to the configuration context.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="connectionString"></param>
        /// <param name="providerName"></param>
        public static void AddConnectionString(String connectionName, String connectionString, String providerName = null)
        {
            ConnectionStrings.AddOrUpdate(connectionName, connectionString, (name, old) => connectionString);
            if (providerName != null)
            {
				var factory = DatabaseConfigurationContext.GetProviderFactory(providerName);
                DbProviders.AddOrUpdate(connectionName, factory, (name, oldFactory) => factory);
            }
        }

        /// <summary>
        /// The default database name.
        /// </summary>
        public static String DefaultDatabaseName { get; set; }

        /// <summary>
        /// The default schema name.
        /// </summary>
        public static String DefaultSchemaName { get; set; }

        private static String _defaultConnectionName = null;
        /// <summary>
        /// This is the default connection 'name' to use when there is no connection name specified.
        /// </summary>
        public static String DefaultConnectionName
        {
            get
            {
                if (_defaultConnectionName != null)
                    return _defaultConnectionName;

                if (ConnectionStrings.Count == 1)
                    return ConnectionStrings.First().Key;

                return null;
            }
            set
            {
                _defaultConnectionName = value;
            }
        }

        /// <summary>
        /// The corresponding connection string associated with the DefaultConnectionName
        /// </summary>
        public static String DefaultConnectionString
        {
            get
            {
                if (!String.IsNullOrEmpty(DefaultConnectionName))
                    return ConnectionStrings[DefaultConnectionName];
                else if (ConnectionStrings.Count == 1)
                    return ConnectionStrings.First().Value;
                else
                    return String.Empty;
            }
        }

        /// <summary>
        /// The corresponding provider for the default connection name. 
        /// </summary>
        public static DbProviderFactory DefaultProviderFactory
        {
            get
            {
                if (_defaultProvider != null)
                    return _defaultProvider;

                lock (_defaultProviderLock)
                {
                    if (!String.IsNullOrEmpty(DefaultConnectionName) && DbProviders.ContainsKey(DefaultConnectionName))
                        _defaultProvider = DbProviders[DefaultConnectionName];
                    else if (DbProviders.Any() && DbProviders.Count == 1)
                        _defaultProvider = DbProviders.First().Value;
                    else
                        _defaultProvider = DbProviderFactories.GetFactory("System.Data.SqlClient");
                }

                return _defaultProvider;
            }
        }

        /// <summary>
        /// Reads Connection Strings from Web.config or App.config.
        /// </summary>
        public static void ReadFromConfiguration()
        {
            if (ConfigurationManager.ConnectionStrings != null && ConfigurationManager.ConnectionStrings.Count > 0)
            {
                for(int index = 0; index < ConfigurationManager.ConnectionStrings.Count; index++)
                {
                    var connString = ConfigurationManager.ConnectionStrings[index];

                    ConnectionStrings.AddOrUpdate(connString.Name, connString.ConnectionString, (oldString, newString) => newString);

					DbProviderFactory provider = GetProviderFactory(connString.ProviderName);

                    DbProviders.AddOrUpdate(connString.Name, provider, (name, oldProvider) => provider);
                }
            }
        }

        public static DbProviderFactory GetProviderForConnection(String connectionName)
        {
            connectionName = connectionName ?? DefaultConnectionName;

            if (String.IsNullOrEmpty(connectionName))
                return DefaultProviderFactory;

            DbProviderFactory provider = null;
            DbProviders.TryGetValue(connectionName, out provider);

            return provider ?? DefaultProviderFactory;
        }

		public static DbProviderFactory GetProviderFactory(String invariantName)
		{
            //if(invariantName.Equals("pg", StringComparison.InvariantCultureIgnoreCase) 
            //   || invariantName.Equals("pgsql", StringComparison.InvariantCultureIgnoreCase)
            //   || invariantName.Equals("npgsql", StringComparison.InvariantCultureIgnoreCase) 
            //   || invariantName.Equals("psql", StringComparison.InvariantCultureIgnoreCase))
            //    return Npgsql.NpgsqlFactory.Instance;
            //else if(!String.IsNullOrEmpty(invariantName))
            //    return DbProviderFactories.GetFactory(invariantName);
            //else
			return DbProviderFactories.GetFactory("System.Data.SqlClient");
		}
    }
}

//We need to add the following provider somewhere.
//<add name="Npgsql Data Provider" invariant="Npgsql" support="FF" description=".Net Framework Data Provider for Postgresql Server" type="Npgsql.NpgsqlFactory, Npgsql" />