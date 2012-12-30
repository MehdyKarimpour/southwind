﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Southwind.Logic;
using Signum.Utilities;
using Southwind.Services;
using Signum.Engine.Disconnected;
using Signum.Engine;
using Signum.Entities.Disconnected;
using Signum.Entities.DynamicQuery;
using Signum.Engine.DynamicQuery;
using Signum.Engine.Maps;
using Signum.Entities;
using System.Data.SqlClient;
using Signum.Engine.Authorization;

namespace Southwind.Local
{
    public static class LocalServer
    {
        public static void Start(string connectionString)
        {
            Starter.Start(UserConnections.Replace(connectionString), null);

            DisconnectedLogic.OfflineMode = true;

            Schema.Current.Initialize();
        }

        public static IServerSouthwind GetServer()
        {
            return new ServerSouthwindLocal();
        }

        public static IServerSouthwindTransfer GetServerTransfer()
        {
            return new ServerSouthwindTransferLocal();
        }

        public static void RestoreDatabase(string connectionString, string backupFile, string databaseFile, string databaseLogFile)
        {
            DisconnectedLogic.LocalBackupManager.RestoreLocalDatabase(
                UserConnections.Replace(connectionString),
                backupFile,
                databaseFile,
                databaseLogFile);
        }

        public static void BackupDatabase(string connectionString, string backupFile)
        {
            SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder(UserConnections.Replace(connectionString));
            string databaseName = csb.InitialCatalog;
            csb.InitialCatalog = "";

            using (Connector.Override(new SqlConnector(csb.ToString(), null, null)))
            {
                DisconnectedLogic.LocalBackupManager.BackupDatabase(databaseName, backupFile);
            }
        }

        public static void DropDatabase(string connectionString)
        {
            SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder(UserConnections.Replace(connectionString));
            string databaseName = csb.InitialCatalog;
            csb.InitialCatalog = "";

            using (Connector.Override(new SqlConnector(csb.ToString(), null, null)))
            {
                DisconnectedLogic.LocalBackupManager.DropDatabase(databaseName);
            }
        }

        public static void OverrideCommonEvents()
        {
            QueryToken.EntityExtensions = parent => DynamicQueryManager.Current.GetExtensions(parent);
            PropertyRoute.SetFindImplementationsCallback(Schema.Current.FindImplementations);
        }

        public static DisconnectedExportDN LastExport()
        {
            using (AuthLogic.Disable())
                return Database.Query<DisconnectedExportDN>().SingleEx();
        }
    }
}
