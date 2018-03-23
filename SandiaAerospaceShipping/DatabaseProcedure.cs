using System;
using System.Collections.Generic;
using System.Windows;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace SandiaAerospaceShipping
{
    public class DatabaseProcedure
    {
        private static List<ComponentsList> MyCollectionList { get; set; }
        public static DataTable GettingInfoFromDatabase(string pQuery)
        {
            DataTable dtInfo = new DataTable();
            string sSqlConnString = BuildingConnectionString();
            if (sSqlConnString != "")
            {
                SqlConnection SqlCon = new SqlConnection(sSqlConnString);
                try
                {
                    SqlCommand SQLCom = new SqlCommand(pQuery, SqlCon);
                    SqlDataAdapter sda = new SqlDataAdapter(SQLCom);
                    sda.Fill(dtInfo);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
            }
            return dtInfo;
        }
        private static void FillingOutList()
        {
            List<ComponentsList> items = new List<ComponentsList>();
            try
            {
                DataTable dtComponents = DatabaseProcedure.GettingInfoFromDatabase("SELECT Components FROM Components ORDER BY Components;");
                foreach (DataRow row in dtComponents.Rows)
                {
                    items.Add(new ComponentsList() { sComponent = row["Components"].ToString().Replace("_", " "), iQuantity = 0 });
                }
                MyCollectionList = items;
            }
            catch { }
        }

        public static string BuildingConnectionString()
        {
            string sqlConn = string.Empty;
            if ((GettingSettings._sServer != "" && GettingSettings._sServer != null) &&
                    (GettingSettings._sDatabaseName != "" && GettingSettings._sDatabaseName != null) &&
                    (GettingSettings._sUserName != "" && GettingSettings._sUserName != null) &&
                    (GettingSettings._sPassword != null))
            {
                GettingSettings Settings = new GettingSettings();
                GettingSettings.SettingValuesFromConfig();
                string sDecryptedPword = Password.ToInsecureString(GettingSettings._sPassword);
                sqlConn = $"Server = {GettingSettings._sServer}; Database = {GettingSettings._sDatabaseName}; User Id = {GettingSettings._sUserName}; Password = {sDecryptedPword};";
            }
            return sqlConn;
        }

        public static void BuildingDatabase(string pSQLConn)

        {

            string sSqlConnString = BuildingConnectionString();
            string sSQLQuery = string.Format("DECLARE @dbname nvarchar(128) " +
                                             "SET @dbname = N'{0}' " +
                                             "IF(NOT EXISTS(SELECT name " +
                                             "FROM master.dbo.sysdatabases " +
                                             "WHERE('[' + name + ']' = @dbname " +
                                             "OR name = @dbname))) CREATE DATABASE {0}", GettingSettings._sDatabaseName);
            if (sSqlConnString != "")
            {
                SqlConnection SqlCon = new SqlConnection(sSqlConnString.Replace(GettingSettings._sDatabaseName, "master"));
                try
                {
                    SqlCon.Open();
                    SqlCommand SQLCom = new SqlCommand(sSQLQuery, SqlCon);
                    SQLCom.ExecuteNonQuery();
                    SqlCon.Close();

                }

                catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
            }
        }
        public static void BuildingTable(string pSQLConn)
        {
            try
            {
                string sSQLQuery = "IF (NOT EXISTS (SELECT * " +
                                     "FROM INFORMATION_SCHEMA.TABLES " +
                                     "WHERE TABLE_SCHEMA = 'dbo' " +
                                     "AND TABLE_NAME = 'Shipping_Log')) " +
                                     "BEGIN " +
                                     "CREATE TABLE[dbo].[Shipping_Log]([Log_ID][int] IDENTITY(1, 1) NOT NULL PRIMARY KEY NONCLUSTERED " +
                                     "([Log_ID] ASC)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, " +
                                     "IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]) ON[PRIMARY] END";
                InsertingIntoDB(sSQLQuery);
                sSQLQuery = "IF (NOT EXISTS (SELECT * " +
                                      "FROM INFORMATION_SCHEMA.TABLES " +
                                      "WHERE TABLE_SCHEMA = 'dbo' " +
                                      "AND TABLE_NAME = 'Components')) " +
                                      "BEGIN " +
                                      "CREATE TABLE[dbo].[Components]([Component_ID][int] IDENTITY(1, 1) NOT NULL PRIMARY KEY NONCLUSTERED " +
                                      "([Component_ID] ASC)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, " +
                                      "IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]) ON[PRIMARY] END";
                InsertingIntoDB(sSQLQuery);
                sSQLQuery = "IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'Components') " +
                            "BEGIN ALTER TABLE Components ADD Components varchar(255); END";
                InsertingIntoDB(sSQLQuery);
                foreach (string sComponent in MainWindow.Components())
                {
                    InsertingIntoComponents(sComponent);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        public static void InsertingIntoComponents(string pComponent)
        {
            string sSQLQuery = string.Format("IF NOT EXISTS (SELECT 1 FROM Components WHERE Components = '{0}') " +
                                     "BEGIN INSERT INTO Components (Components) VALUES ('{0}') END", pComponent);
            InsertingIntoDB(sSQLQuery);
        }

        public static void BuildingColumns(string pSQLConn)
        {
            FillingOutList();
            string SQLQuery = string.Empty;
            InsertingColumns(pSQLConn, "Company", "varchar(255)", "");
            InsertingColumns(pSQLConn, "Shipping_Date", "datetime", "");
            InsertingColumns(pSQLConn, "Cost", "int", "");
            InsertingColumns(pSQLConn, "Shipping_Company", "varchar(255)", "");
            InsertingColumns(pSQLConn, "Repair", "bit", "");
            foreach (var item in MyCollectionList)
            {
                InsertingColumns(pSQLConn, item.sComponent.Replace(" ", "_"), "int", "0");
            }

        }
        public static void InsertingColumns(string pSQLConn, string pColumn, string pFType, string pDefault)
        {
            try
            {
                string SQLQuery = "";
                if (Regex.IsMatch(pColumn, @"^\d"))
                {
                    string sAddingColumn = '"' + pColumn + '"';
                    SQLQuery = string.Format("IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = '{0}') " +
                                     "BEGIN ALTER TABLE Shipping_Log ADD {1} {2} DEFAULT '{3}'; END", pColumn, sAddingColumn, pFType, pDefault);
                }
                else
                    SQLQuery = string.Format("IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = '{0}') " +
                                         "BEGIN ALTER TABLE Shipping_Log ADD {0} {1} DEFAULT '{2}'; END", pColumn, pFType, pDefault);
                InsertingIntoDB(SQLQuery);
            }
            catch (Exception ex)
            {

            }
        }
        public static void InsertingIntoDB(string pQuery)
        {
            string sSqlConnString = BuildingConnectionString();
            if (sSqlConnString != "")
            {
                SqlConnection SqlCon = new SqlConnection(sSqlConnString);
                try
                {
                    SqlCon.Open();
                    SqlCommand SQLCom = new SqlCommand(pQuery, SqlCon);
                    SQLCom.ExecuteNonQuery();
                    SqlCon.Close();

                }

                catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
            }
        }
        public static string OrderingColumns()
        {
            string sRet = string.Empty;
            //Company, Shipping_Date, Shipping_Company, Cost, Repair
            string query1 = $"USE {GettingSettings._sDatabaseName} " +
                            "SELECT COLUMN_NAME " +
                            "FROM INFORMATION_SCHEMA.COLUMNS " +
                            "WHERE TABLE_NAME = 'Shipping_Log' ORDER BY " +
                            "(CASE Column_Name WHEN 'Log_ID' THEN 0 " +
                            "WHEN 'Company' THEN 1 " +
                            "WHEN 'Shipping_Date' THEN 2 " +
                            "WHEN 'Shipping_Company' THEN 3 " +
                            "WHEN 'Cost' THEN 4 " +
                            "WHEN 'Repair' THEN 5 " +
                            "ELSE 6 END), Column_Name ;";
            DataTable rs = GettingInfoFromDatabase(query1);
            string query2 = "select";
            string sep = " ";
            foreach (DataRow row in rs.Rows)
            {
                if (Regex.IsMatch(row["Column_Name"].ToString(), @"^\d"))
                    query2 += sep + '"' + row["Column_Name"] + '"';
                else
                    query2 += sep + row["Column_Name"];
                sep = ", ";
            }
            query2 += $" from Shipping_Log ORDER BY Log_ID";
            sRet = query2;
            return sRet;
        }
        public static void DeletingFromLog(int pLogID)
        {
            string sSqlConnString = BuildingConnectionString();
            string sSQLQuery = string.Empty;
            try
            {
                using (var sc = new SqlConnection(sSqlConnString))
                using (var cmd = sc.CreateCommand())
                {

                    if (pLogID != 0)
                    {
                        //sSQLQuery = string.Format("DELETE FROM dbo.Shipping_Log WHERE Log_ID = {0};", pLogID);
                        cmd.CommandText = "DELETE FROM Shipping_Log WHERE Log_ID = @id";
                        cmd.Parameters.AddWithValue("@id", pLogID);
                    }
                    if (sSqlConnString != "")
                    {
                        //SqlConnection SqlCon = new SqlConnection(sSqlConnString.Replace(GettingSettings._sDatabaseName, "master"));
                        try
                        {
                            sc.Open();
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message.ToString()); }
        }


    }
}
