﻿#if NETFRAMEWORK4_X
namespace Microshaoft
{
    using System;
    using System.IO;
    using System.Data;
    using System.Data.SqlClient;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using Microsoft.CSharp;

    using Microsoft.Data;
    public class Class1
    {
        [STAThread]
        static void Main(string[] args)
        {
            string spName = "usp_CheckRolesPermission";
            //spName = args[0];
            //string _ConnectionString = @"application name=test;user=sa;password=password01!;Initial Catalog=BSMSParameter;Data Source=22.9.8.167";
            string _ConnectionString = @"Application Name=GlimpseApp;Persist Security Info=False;Data Source=YUer-win10E\sql2016;User ID=sa;Password=!@#123QWE;Initial Catalog=GSP20AuthZ";
            //_ConnectionString = @"application name=test;User ID=sa;Password=password01!;Initial Catalog=FundManagement;Data Source=22.11.128.247";
            SqlConnection sc = new SqlConnection(_ConnectionString);
            string s = GenerateCode(spName, true, sc, "Microshaoft", "Class1");
            //string s = "asdsad";
            //System.Windows.Forms.Clipboard.SetDataObject(s);
            Console.WriteLine(s);
        }

        public static string GenerateCode
            (
                string spName
                , bool includeReturnValueParameter
                , SqlConnection connection
                , string nameSpace
                , string className
            )
        {
            SqlParameter[] spa = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
            int length = spa.Length;

            CSharpCodeProvider provider = new CSharpCodeProvider();
            //ICodeGenerator generator = provider.CreateGenerator();

            CodeCompileUnit ccu = new CodeCompileUnit();
            CodeNamespace cn = new CodeNamespace(nameSpace); //添加名称空间
            cn.Imports.Add(new CodeNamespaceImport("System"));
            cn.Imports.Add(new CodeNamespaceImport("System.Data"));
            cn.Imports.Add(new CodeNamespaceImport("System.Data.SqlClient"));
            //cn.Imports.Add(new CodeNamespaceImport("Microsoft.Data"));
            CodeTypeDeclaration ctd = new CodeTypeDeclaration(className);

            System.CodeDom.CodeMemberField cmf = new CodeMemberField(new CodeTypeReference(typeof(string)), "_ConnectionString");

            cmf.Attributes = MemberAttributes.Static | MemberAttributes.Public;
            cmf.InitExpression = new CodeSnippetExpression("@\"" + connection.ConnectionString + "\"");//new CodePrimitiveExpression(connection.ConnectionString);

            ctd.Members.Add(cmf);
            //int rows = spa.Length;
            CodeMemberMethod cmm = new CodeMemberMethod();
            CodeSnippetStatement css = null;

            css = new CodeSnippetStatement("SqlConnection connection = new SqlConnection(_ConnectionString);");
            cmm.Statements.Add(css);

            //css = new CodeSnippetStatement("SqlParameter[] parameters = new SqlParameter[" + (length+1).ToString() + "];");
            //cmm.Statements.Add(css);
            css = new CodeSnippetStatement(string.Format("SqlCommand command = new SqlCommand(\"{0}\", connection);", spName));
            cmm.Statements.Add(css);
            css = new CodeSnippetStatement("command.CommandType = CommandType.StoredProcedure;");
            cmm.Statements.Add(css);

            cmm.ReturnType = new CodeTypeReference(typeof(int));
            cmm.Name = "ExecProc_" + spName;
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static;

            int i = 1;
            CodeStatementCollection csc = new CodeStatementCollection();

            foreach (SqlParameter parameter in spa)
            {
                string paramName = "p_" + parameter.ParameterName.Substring(1);

                string paramTypeName = GetTypeName(parameter.SqlDbType);

                CodeTypeReference paramType = new CodeTypeReference(paramTypeName);
                //paramType.ToString();

                CodeParameterDeclarationExpression cpde = new CodeParameterDeclarationExpression(paramType, paramName);
                //css = new CodeSnippetStatement("parameters[" + i.ToString() + "] = new SqlParameter(\"" + parameter.ParameterName + "\", SqlDbType." + parameter.SqlDbType + ");");
                //css = new CodeSnippetStatement("SqlParameter parameter" + i.ToString() + " = new SqlParameter(\"" + parameter.ParameterName + "\", SqlDbType." + parameter.SqlDbType + ");");
                //cmm.Statements.Add(css);

                if (paramTypeName == "System.String")
                {
                    css = new CodeSnippetStatement(string.Format("SqlParameter parameter" + i.ToString() + " = command.Parameters.Add(\"{0}\", SqlDbType.{1}, {2});", parameter.ParameterName, parameter.SqlDbType, parameter.Size));
                    cmm.Statements.Add(css);
                }
                else if (paramTypeName == "System.String")
                {
                }
                else
                {
                    css = new CodeSnippetStatement(string.Format("SqlParameter parameter" + i.ToString() + " = command.Parameters.Add(\"{0}\", SqlDbType.{1});", parameter.ParameterName, parameter.SqlDbType));
                    cmm.Statements.Add(css);
                }

                if (parameter.Direction != ParameterDirection.Input)
                {
                    cpde.Direction = FieldDirection.Out;

                    css = new CodeSnippetStatement("parameter" + i.ToString() + ".Direction = ParameterDirection.Output;");
                    cmm.Statements.Add(css);

                    css = new CodeSnippetStatement(paramName + " = null;");
                    csc.Add(css);

                    css = new CodeSnippetStatement("if (parameter" + i.ToString() + ".Value != DBNull.Value)");
                    csc.Add(css);

                    css = new CodeSnippetStatement("{");
                    csc.Add(css);
                    CodeSnippetExpression cse = new CodeSnippetExpression(paramName);
                    CodeCastExpression cce = new CodeCastExpression(paramTypeName, new CodeSnippetExpression("parameter" + i.ToString() + ".Value"));
                    CodeAssignStatement cas = new CodeAssignStatement(cse, cce);
                    csc.Add(cas);

                    css = new CodeSnippetStatement("}");
                    csc.Add(css);

                }
                else
                {
                    string ss = "";
                    if (paramTypeName == "System.String")
                    {
                        ss = string.Format("parameter{0}.Value = ({1} != null ? (object) {1} : DBNull.Value);", i, paramName);
                    }
                    else
                    {
                        ss = string.Format("parameter{0}.Value = {1};", i, paramName);
                    }
                    css = new CodeSnippetStatement(ss);
                    cmm.Statements.Add(css);
                }

                cmm.Parameters.Add(cpde);
                i++;

            }

            //			css = new CodeSnippetStatement("SqlParameter parameterReturn = new SqlParameter(\"@RETURN_VALUE\", SqlDbType.Int);");
            css = new CodeSnippetStatement("SqlParameter parameterReturn = command.Parameters.Add(\"@RETURN_VALUE\", SqlDbType.Int);");
            cmm.Statements.Add(css);

            css = new CodeSnippetStatement("parameterReturn.Direction = ParameterDirection.ReturnValue;");
            cmm.Statements.Add(css);

            css = new CodeSnippetStatement("connection.Open();");
            cmm.Statements.Add(css);

            css = new CodeSnippetStatement("//SqlDataAdapter sda = new SqlDataAdapter(command);");
            cmm.Statements.Add(css);

            css = new CodeSnippetStatement("//DataSet ds = new DataSet();");
            cmm.Statements.Add(css);

            css = new CodeSnippetStatement("//sda.Fill(ds);");
            cmm.Statements.Add(css);

            css = new CodeSnippetStatement("command.ExecuteNonQuery();");
            cmm.Statements.Add(css);

            css = new CodeSnippetStatement("connection.Close();");
            cmm.Statements.Add(css);

            for (int j = 0; j < csc.Count; j++)
            {
                cmm.Statements.Add(csc[j]);
            }

            css = new CodeSnippetStatement("//return ds.Tables[0];");
            cmm.Statements.Add(css);

            if (includeReturnValueParameter)
            {
                //CodeSnippetExpression cse = new CodeSnippetExpression("p_RETURN_VALUE");
                CodeCastExpression cce = new CodeCastExpression(typeof(int), new CodeSnippetExpression("parameterReturn.Value"));
                //CodeAssignStatement cas = new CodeAssignStatement(cse,cce);
                //cmm.Statements.Add(cas);
                cmm.Statements.Add(new CodeMethodReturnStatement(cce));
            }

            ctd.Members.Add(cmm);

            cmm = new CodeMemberMethod();
            cmm.Name = "Main";
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static;

            ctd.Members.Add(cmm);

            cn.Types.Add(ctd);
            ccu.Namespaces.Add(cn);
            using (StreamWriter sw = new StreamWriter(@"test.cs", false))
            {
                provider.GenerateCodeFromCompileUnit(ccu, sw, new CodeGeneratorOptions());
                sw.Close();
            }
            string s = null;
            using (StreamReader sr = new StreamReader(@"test.cs"))
            {
                s = sr.ReadToEnd();
                sr.Close();
            }
            return s;
        }

        public static string GenerateCodeWithSqlHelper
            (
                string spName
                , bool includeReturnValueParameter
                , SqlConnection connection
                , string nameSpace
                , string className
            )
        {
            SqlParameter[] spa = SqlHelperParameterCache.GetSpParameterSet(connection, spName);
            int length = spa.Length;

            CSharpCodeProvider provider = new CSharpCodeProvider();
            //ICodeGenerator generator = provider.CreateGenerator();

            CodeCompileUnit ccu = new CodeCompileUnit();
            CodeNamespace cn = new CodeNamespace(nameSpace); //添加名称空间
            cn.Imports.Add(new CodeNamespaceImport("System"));
            cn.Imports.Add(new CodeNamespaceImport("System.Data"));
            cn.Imports.Add(new CodeNamespaceImport("System.Data.SqlClient"));
            cn.Imports.Add(new CodeNamespaceImport("Microsoft.Data"));
            CodeTypeDeclaration ctd = new CodeTypeDeclaration(className);

            System.CodeDom.CodeMemberField cmf = new CodeMemberField(new CodeTypeReference(typeof(string)), "_ConnectionString");

            cmf.Attributes = MemberAttributes.Static | MemberAttributes.Public;
            cmf.InitExpression = new CodeSnippetExpression("@\"" + connection.ConnectionString + "\"");//new CodePrimitiveExpression(connection.ConnectionString);

            ctd.Members.Add(cmf);
            //int rows = spa.Length;
            CodeMemberMethod cmm = new CodeMemberMethod();
            CodeSnippetStatement css = null;

            css = new CodeSnippetStatement("SqlParameter[] parameters = new SqlParameter[" + (length + 1).ToString() + "];");
            cmm.Statements.Add(css);
            cmm.ReturnType = new CodeTypeReference(typeof(int));
            cmm.Name = "ExecProc_" + spName;
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static;

            int i = 0;
            CodeStatementCollection csc = new CodeStatementCollection();
            foreach (SqlParameter parameter in spa)
            {
                string paramName = "p_" + parameter.ParameterName.Substring(1);

                string paramTypeName = GetTypeName(parameter.SqlDbType);

                CodeTypeReference paramType = new CodeTypeReference(paramTypeName);
                paramType.ToString();

                CodeParameterDeclarationExpression cpde = new CodeParameterDeclarationExpression(paramType, paramName);
                css = new CodeSnippetStatement("parameters[" + i.ToString() + "] = new SqlParameter(\"" + parameter.ParameterName + "\", SqlDbType." + parameter.SqlDbType + ");");
                cmm.Statements.Add(css);

                if (parameter.Direction != ParameterDirection.Input)
                {
                    cpde.Direction = FieldDirection.Out;

                    if (paramTypeName == "System.String")
                    {
                        css = new CodeSnippetStatement("parameters[" + i.ToString() + "].Size = " + parameter.Size + ";");
                        cmm.Statements.Add(css);
                    }


                    css = new CodeSnippetStatement("parameters[" + i.ToString() + "].Direction = ParameterDirection.Output;");
                    cmm.Statements.Add(css);

                    css = new CodeSnippetStatement(paramName + " = null;");
                    csc.Add(css);

                    css = new CodeSnippetStatement("if (parameters[" + i.ToString() + "].Value != System.DBNull.Value)");
                    csc.Add(css);

                    css = new CodeSnippetStatement("{");
                    csc.Add(css);
                    CodeSnippetExpression cse = new CodeSnippetExpression(paramName);
                    CodeCastExpression cce = new CodeCastExpression(paramTypeName, new CodeSnippetExpression("parameters[" + i.ToString() + "].Value"));
                    CodeAssignStatement cas = new CodeAssignStatement(cse, cce);
                    csc.Add(cas);

                    css = new CodeSnippetStatement("}");
                    csc.Add(css);

                }
                else
                {

                    css = new CodeSnippetStatement("parameters[" + i.ToString() + "].Value = " + paramName + ";");
                    cmm.Statements.Add(css);
                }
                cmm.Parameters.Add(cpde);
                i++;

            }

            css = new CodeSnippetStatement("parameters[" + length.ToString() + "] = new SqlParameter(\"@RETURN_VALUE\", SqlDbType.Int);");
            cmm.Statements.Add(css);


            css = new CodeSnippetStatement("parameters[" + length.ToString() + "].Direction = ParameterDirection.ReturnValue;");
            cmm.Statements.Add(css);

            css = new CodeSnippetStatement("SqlConnection connection = new SqlConnection(_ConnectionString);");
            cmm.Statements.Add(css);
            css = new CodeSnippetStatement("SqlHelper.ExecuteNonQuery(connection, CommandType.StoredProcedure, \"" + spName + "\", parameters);");
            cmm.Statements.Add(css);

            for (int j = 0; j < csc.Count; j++)
            {
                cmm.Statements.Add(csc[j]);
            }
            if (includeReturnValueParameter)
            {
                //CodeSnippetExpression cse = new CodeSnippetExpression("p_RETURN_VALUE");
                CodeCastExpression cce = new CodeCastExpression(typeof(int), new CodeSnippetExpression("parameters[" + i.ToString() + "].Value"));
                //CodeAssignStatement cas = new CodeAssignStatement(cse,cce);
                //cmm.Statements.Add(cas);
                cmm.Statements.Add(new CodeMethodReturnStatement(cce));
            }

            ctd.Members.Add(cmm);

            cmm = new CodeMemberMethod();
            cmm.Name = "Main";
            cmm.Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static;

            ctd.Members.Add(cmm);


            cn.Types.Add(ctd);
            ccu.Namespaces.Add(cn);
            using (StreamWriter sw = new StreamWriter(@"test.cs", false))
            {
                provider.GenerateCodeFromCompileUnit(ccu, sw, new CodeGeneratorOptions());
                sw.Close();
            }
            string s = null;
            using (StreamReader sr = new StreamReader(@"test.cs"))
            {
                s = sr.ReadToEnd();
                sr.Close();
            }
            return s;
        }

        public static string GetTypeName(SqlDbType Type)
        {
            string s = "System.String";
            switch (Type)
            {
                case SqlDbType.Int:
                    {
                        s = "System.Int32";
                        break;
                    }
                case SqlDbType.SmallInt:
                    {
                        s = "System.Int32";
                        break;
                    }

                case SqlDbType.TinyInt:
                    {
                        s = "System.Int32";
                        break;
                    }
                case SqlDbType.BigInt:
                    {
                        s = "System.Int64";
                        break;
                    }
                case SqlDbType.Bit:
                    {
                        s = "System.Boolean";
                        break;
                    }
                case SqlDbType.Decimal:
                    {
                        s = "System.Decimal";
                        break;
                    }
                case SqlDbType.Float:
                    {
                        s = "System.Single";
                        break;
                    }

                case SqlDbType.Money:
                    {
                        s = "System.Decimal";
                        break;
                    }
                case SqlDbType.SmallMoney:
                    {
                        s = "System.Decimal";
                        break;
                    }
                case SqlDbType.Real:
                    {
                        s = "System.Single";
                        break;
                    }



                case SqlDbType.DateTime:
                    {
                        s = "DateTime";
                        break;
                    }


                case SqlDbType.SmallDateTime:
                    {
                        s = "DateTime";
                        break;
                    }

                case SqlDbType.UniqueIdentifier:
                    {
                        s = "Guid";
                        break;
                    }

                default:
                    {
                        s = "System.String";
                        break;
                    }

            }
            return s;
        }

    }
}

//================================================================================================================

// 下面是 Microsoft SqlHelper :

//Data Access Application Block 3.1
// http://www.gotdotnet.com/workspaces/workspace.aspx?id=c20d12b0-af52-402b-9b7c-aaeb21d1f431
// SqlHelper.v3.1.cs
//csc.exe SqlHelper.v3.1.cs /t:library /r:C:\WINNT\Microsoft.NET\Framework\v1.1.4322\System.Data.OracleClient.dll

namespace Microsoft.Data
{
    using System;
    using System.IO;
    using System.Data;
    using System.Xml;
    using System.Data.SqlClient;
    using System.Data.Odbc;
    using System.Data.OleDb;
    using System.Data.OracleClient;
    using System.Data.Common;
    using System.Collections;
    using System.Configuration;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Reflection;
    using System.Diagnostics;

    /// <summary>
    /// The AdoHelper class is intended to encapsulate high performance, scalable best practices for 
    /// common data access uses.   It uses the Abstract Factory pattern to be easily extensible
    /// to any ADO.NET provider.  The current implementation provides helpers for SQL Server, ODBC,
    /// OLEDB, and Oracle.
    /// </summary>
    public abstract class AdoHelper
    {
        /// <summary>
        /// This enum is used to indicate whether the connection was provided by the caller, or created by AdoHelper, so that
        /// we can set the appropriate CommandBehavior when calling ExecuteReader()
        /// </summary>
        protected enum AdoConnectionOwnership
        {
            /// <summary>Connection is owned and managed by ADOHelper</summary>
            Internal,
            /// <summary>Connection is owned and managed by the caller</summary>
            External
        }

#region Declare members

        // necessary for handling the general case of needing event handlers for RowUpdating/ed events
        /// <summary>
        /// Internal handler used for bubbling up the event to the user
        /// </summary>
        protected RowUpdatingHandler m_rowUpdating;

        /// <summary>
        /// Internal handler used for bubbling up the event to the user
        /// </summary>
        protected RowUpdatedHandler m_rowUpdated;

#endregion

#region Provider specific abstract methods

        /// <summary>
        /// Returns an IDbConnection object for the given connection string
        /// </summary>
        /// <param name="connectionString">The connection string to be used to create the connection</param>
        /// <returns>An IDbConnection object</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        public abstract IDbConnection GetConnection(string connectionString);

        /// <summary>
        /// Returns an IDbDataAdapter object
        /// </summary>
        /// <returns>The IDbDataAdapter</returns>
        public abstract IDbDataAdapter GetDataAdapter();

        /// <summary>
        /// Calls the CommandBuilder.DeriveParameters method for the specified provider, doing any setup and cleanup necessary
        /// </summary>
        /// <param name="cmd">The IDbCommand referencing the stored procedure from which the parameter information is to be derived. The derived parameters are added to the Parameters collection of the IDbCommand. </param>
        public abstract void DeriveParameters(IDbCommand cmd);

        /// <summary>
        /// Returns an IDataParameter object
        /// </summary>
        /// <returns>The IDataParameter object</returns>
        public abstract IDataParameter GetParameter();

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the provided IDbConnection. 
        /// </summary>
        /// <example>
        /// <code>
        /// XmlReader r = helper.ExecuteXmlReader(command);
        /// </code></example>
        /// <param name="cmd">The IDbCommand to execute</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if command is null.</exception>
        public abstract XmlReader ExecuteXmlReader(IDbCommand cmd);

        /// <summary>
        /// Provider specific code to set up the updating/ed event handlers used by UpdateDataset
        /// </summary>
        /// <param name="dataAdapter">DataAdapter to attach the event handlers to</param>
        /// <param name="rowUpdatingHandler">The handler to be called when a row is updating</param>
        /// <param name="rowUpdatedHandler">The handler to be called when a row is updated</param>
        protected abstract void AddUpdateEventHandlers(IDbDataAdapter dataAdapter, RowUpdatingHandler rowUpdatingHandler, RowUpdatedHandler rowUpdatedHandler);

        /// <summary>
        /// Returns an array of IDataParameters of the specified size
        /// </summary>
        /// <param name="size">size of the array</param>
        /// <returns>The array of IDataParameters</returns>
        protected abstract IDataParameter[] GetDataParameters(int size);

        /// <summary>
        /// Handle any provider-specific issues with BLOBs here by "washing" the IDataParameter and returning a new one that is set up appropriately for the provider.
        /// </summary>
        /// <param name="connection">The IDbConnection to use in cleansing the parameter</param>
        /// <param name="p">The parameter before cleansing</param>
        /// <returns>The parameter after it's been cleansed.</returns>
        protected abstract IDataParameter GetBlobParameter(IDbConnection connection, IDataParameter p);

#endregion

#region Delegates

        // also used in our general case of RowUpdating/ed events
        /// <summary>
        /// Delegate for creating a RowUpdatingEvent handler
        /// </summary>
        /// <param name="obj">The object that published the event</param>
        /// <param name="e">The RowUpdatingEventArgs for the event</param>
        public delegate void RowUpdatingHandler(object obj, RowUpdatingEventArgs e);

        /// <summary>
        /// Delegate for creating a RowUpdatedEvent handler
        /// </summary>
        /// <param name="obj">The object that published the event</param>
        /// <param name="e">The RowUpdatedEventArgs for the event</param>
        public delegate void RowUpdatedHandler(object obj, RowUpdatedEventArgs e);

#endregion

#region Factory

        /// <summary>
        /// Create an AdoHelper for working with a specific provider (i.e. Sql, Odbc, OleDb, Oracle)
        /// </summary>
        /// <param name="providerAssembly">Assembly containing the specified helper subclass</param>
        /// <param name="providerType">Specific type of the provider</param>
        /// <returns>An AdoHelper instance of the specified type</returns>
        /// <example><code>
        /// AdoHelper helper = AdoHelper.CreateHelper("GotDotNet.ApplicationBlocks.Data", "GotDotNet.ApplicationBlocks.Data.OleDb");
        /// </code></example>
        public static AdoHelper CreateHelper(string providerAssembly, string providerType)
        {
            Assembly assembly = Assembly.Load(providerAssembly);
            object provider = assembly.CreateInstance(providerType);
            if (provider is AdoHelper)
            {
                return provider as AdoHelper;
            }
            else
            {
                throw new InvalidOperationException("The provider specified does not extend the AdoHelper abstract class.");
            }
        }


        /// <summary>
        /// Create an AdoHelper instance for working with a specific provider by using a providerAlias specified in the App.Config file.
        /// </summary>
        /// <param name="providerAlias">The alias to look up</param>
        /// <returns>An AdoHelper instance of the specified type</returns>
        /// <example><code>
        /// AdoHelper helper = AdoHelper.CreateHelper("OracleHelper");
        /// </code></example>
        public static AdoHelper CreateHelper(string providerAlias)
        {
            IDictionary dict;
            try
            {
                //dict = ConfigurationSettings.GetConfig("daabProviders") as IDictionary;
                dict = ConfigurationManager.GetSection("daabProviders") as IDictionary;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("If the section is not defined on the configuration file this method can't be used to create an AdoHelper instance.", e);
            }

            ProviderAlias providerConfig = dict[providerAlias] as ProviderAlias;
            string providerAssembly = providerConfig.AssemblyName;
            string providerType = providerConfig.TypeName;

            Assembly assembly = Assembly.Load(providerAssembly);
            object provider = assembly.CreateInstance(providerType);
            if (provider is AdoHelper)
            {
                return provider as AdoHelper;
            }
            else
            {
                throw new InvalidOperationException("The provider specified does not extends the AdoHelper abstract class.");
            }
        }

#endregion

#region GetParameter

        /// <summary>
        /// Get an IDataParameter for use in a SQL command
        /// </summary>
        /// <param name="name">The name of the parameter to create</param>
        /// <param name="value">The value of the specified parameter</param>
        /// <returns>An IDataParameter object</returns>
        public virtual IDataParameter GetParameter(string name, object value)
        {
            IDataParameter parameter = GetParameter();
            parameter.ParameterName = name;
            parameter.Value = value;

            return parameter;
        }

        /// <summary>
        /// Get an IDataParameter for use in a SQL command
        /// </summary>
        /// <param name="name">The name of the parameter to create</param>
        /// <param name="dbType">The System.Data.DbType of the parameter</param>
        /// <param name="size">The size of the parameter</param>
        /// <param name="direction">The System.Data.ParameterDirection of the parameter</param>
        /// <returns>An IDataParameter object</returns>
        public virtual IDataParameter GetParameter(string name, DbType dbType, int size, ParameterDirection direction)
        {
            IDataParameter dataParameter = GetParameter();
            dataParameter.DbType = dbType;
            dataParameter.Direction = direction;
            dataParameter.ParameterName = name;

            if (size > 0 && dataParameter is IDbDataParameter)
            {
                IDbDataParameter dbDataParameter = (IDbDataParameter)dataParameter;
                dbDataParameter.Size = size;
            }
            return dataParameter;
        }

        /// <summary>
        /// Get an IDataParameter for use in a SQL command
        /// </summary>
        /// <param name="name">The name of the parameter to create</param>
        /// <param name="dbType">The System.Data.DbType of the parameter</param>
        /// <param name="size">The size of the parameter</param>
        /// <param name="sourceColumn">The source column of the parameter</param>
        /// <param name="sourceVersion">The System.Data.DataRowVersion of the parameter</param>
        /// <returns>An IDataParameter object</returns>
        public virtual IDataParameter GetParameter(string name, DbType dbType, int size, string sourceColumn, DataRowVersion sourceVersion)
        {
            IDataParameter dataParameter = GetParameter();
            dataParameter.DbType = dbType;
            dataParameter.ParameterName = name;
            dataParameter.SourceColumn = sourceColumn;
            dataParameter.SourceVersion = sourceVersion;

            if (size > 0 && dataParameter is IDbDataParameter)
            {
                IDbDataParameter dbDataParameter = (IDbDataParameter)dataParameter;
                dbDataParameter.Size = size;
            }
            return dataParameter;
        }

#endregion

#region private utility methods

        /// <summary>
        /// This method is used to attach array of IDataParameters to an IDbCommand.
        /// 
        /// This method will assign a value of DbNull to any parameter with a direction of
        /// InputOutput and a value of null.  
        /// 
        /// This behavior will prevent default values from being used, but
        /// this will be the less common case than an intended pure output parameter (derived as InputOutput)
        /// where the user provided no input value.
        /// </summary>
        /// <param name="command">The command to which the parameters will be added</param>
        /// <param name="commandParameters">An array of IDataParameterParameters to be added to command</param>
        /// <exception cref="System.ArgumentNullException">Thrown if command is null.</exception>
        protected virtual void AttachParameters(IDbCommand command, IDataParameter[] commandParameters)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (commandParameters != null)
            {
                foreach (IDataParameter p in commandParameters)
                {
                    if (p != null)
                    {
                        // Check for derived output value with no value assigned
                        if ((p.Direction == ParameterDirection.InputOutput ||
                            p.Direction == ParameterDirection.Input) &&
                            (p.Value == null))
                        {
                            p.Value = DBNull.Value;
                        }
                        if (p.DbType == DbType.Binary)
                        {
                            // special handling for BLOBs
                            command.Parameters.Add(GetBlobParameter(command.Connection, p));
                        }
                        else
                        {
                            command.Parameters.Add(p);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method assigns dataRow column values to an IDataParameterCollection
        /// </summary>
        /// <param name="commandParameters">The IDataParameterCollection to be assigned values</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values</param>
        /// <exception cref="System.InvalidOperationException">Thrown if any of the parameter names are invalid.</exception>
        protected internal void AssignParameterValues(IDataParameterCollection commandParameters, DataRow dataRow)
        {
            if (commandParameters == null || dataRow == null)
            {
                // Do nothing if we get no data
                return;
            }

            DataColumnCollection columns = dataRow.Table.Columns;

            int i = 0;
            // Set the parameters values
            foreach (IDataParameter commandParameter in commandParameters)
            {
                // Check the parameter name
                if (commandParameter.ParameterName == null ||
                    commandParameter.ParameterName.Length <= 1)
                    throw new InvalidOperationException(string.Format(
                        "Please provide a valid parameter name on the parameter #{0}, the ParameterName property has the following value: '{1}'.",
                        i, commandParameter.ParameterName));

                if (columns.Contains(commandParameter.ParameterName))
                    commandParameter.Value = dataRow[commandParameter.ParameterName];
                else if (columns.Contains(commandParameter.ParameterName.Substring(1)))
                    commandParameter.Value = dataRow[commandParameter.ParameterName.Substring(1)];

                i++;
            }
        }

        /// <summary>
        /// This method assigns dataRow column values to an array of IDataParameters
        /// </summary>
        /// <param name="commandParameters">Array of IDataParameters to be assigned values</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values</param>
        /// <exception cref="System.InvalidOperationException">Thrown if any of the parameter names are invalid.</exception>
        protected void AssignParameterValues(IDataParameter[] commandParameters, DataRow dataRow)
        {
            if ((commandParameters == null) || (dataRow == null))
            {
                // Do nothing if we get no data
                return;
            }

            DataColumnCollection columns = dataRow.Table.Columns;

            int i = 0;
            // Set the parameters values
            foreach (IDataParameter commandParameter in commandParameters)
            {
                // Check the parameter name
                if (commandParameter.ParameterName == null ||
                    commandParameter.ParameterName.Length <= 1)
                    throw new InvalidOperationException(string.Format(
                        "Please provide a valid parameter name on the parameter #{0}, the ParameterName property has the following value: '{1}'.",
                        i, commandParameter.ParameterName));

                if (columns.Contains(commandParameter.ParameterName))
                    commandParameter.Value = dataRow[commandParameter.ParameterName];
                else if (columns.Contains(commandParameter.ParameterName.Substring(1)))
                    commandParameter.Value = dataRow[commandParameter.ParameterName.Substring(1)];

                i++;
            }
        }

        /// <summary>
        /// This method assigns an array of values to an array of IDataParameters
        /// </summary>
        /// <param name="commandParameters">Array of IDataParameters to be assigned values</param>
        /// <param name="parameterValues">Array of objects holding the values to be assigned</param>
        /// <exception cref="System.ArgumentException">Thrown if an incorrect number of parameters are passed.</exception>
        protected void AssignParameterValues(IDataParameter[] commandParameters, object[] parameterValues)
        {
            if ((commandParameters == null) || (parameterValues == null))
            {
                // Do nothing if we get no data
                return;
            }

            // We must have the same number of values as we pave parameters to put them in
            if (commandParameters.Length != parameterValues.Length)
            {
                throw new ArgumentException("Parameter count does not match Parameter Value count.");
            }

            // Iterate through the IDataParameters, assigning the values from the corresponding position in the 
            // value array
            for (int i = 0, j = commandParameters.Length, k = 0; i < j; i++)
            {
                if (commandParameters[i].Direction != ParameterDirection.ReturnValue)
                {
                    // If the current array value derives from IDataParameter, then assign its Value property
                    if (parameterValues[k] is IDataParameter)
                    {
                        IDataParameter paramInstance;
                        paramInstance = (IDataParameter)parameterValues[k];
                        if (paramInstance.Direction == ParameterDirection.ReturnValue)
                        {
                            paramInstance = (IDataParameter)parameterValues[++k];
                        }
                        if (paramInstance.Value == null)
                        {
                            commandParameters[i].Value = DBNull.Value;
                        }
                        else
                        {
                            commandParameters[i].Value = paramInstance.Value;
                        }
                    }
                    else if (parameterValues[k] == null)
                    {
                        commandParameters[i].Value = DBNull.Value;
                    }
                    else
                    {
                        commandParameters[i].Value = parameterValues[k];
                    }
                    k++;
                }
            }
        }

        /// <summary>
        /// This method cleans up the parameter syntax for the provider
        /// </summary>
        /// <param name="command">The IDbCommand containing the parameters to clean up.</param>
        public virtual void CleanParameterSyntax(IDbCommand command)
        {
            // do nothing by default
        }

        /// <summary>
        /// This method opens (if necessary) and assigns a connection, transaction, command type and parameters 
        /// to the provided command
        /// </summary>
        /// <param name="command">The IDbCommand to be prepared</param>
        /// <param name="connection">A valid IDbConnection, on which to execute this command</param>
        /// <param name="transaction">A valid IDbTransaction, or 'null'</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters to be associated with the command or 'null' if no parameters are required</param>
        /// <param name="mustCloseConnection"><c>true</c> if the connection was opened by the method, otherwose is false.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if command is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null.</exception>
        protected virtual void PrepareCommand(IDbCommand command, IDbConnection connection, IDbTransaction transaction, CommandType commandType, string commandText, IDataParameter[] commandParameters, out bool mustCloseConnection)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

            // If the provided connection is not open, we will open it
            if (connection.State != ConnectionState.Open)
            {
                mustCloseConnection = true;
                connection.Open();
            }
            else
            {
                mustCloseConnection = false;
            }

            // Associate the connection with the command
            command.Connection = connection;

            // Set the command text (stored procedure name or SQL statement)
            command.CommandText = commandText;

            // If we were provided a transaction, assign it
            if (transaction != null)
            {
                if (transaction.Connection == null) throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
                command.Transaction = transaction;
            }

            // Set the command type
            command.CommandType = commandType;

            // Attach the command parameters if they are provided
            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }
            return;
        }

        /// <summary>
        /// This method clears (if necessary) the connection, transaction, command type and parameters 
        /// from the provided command
        /// </summary>
        /// <remarks>
        /// Not implemented here because the behavior of this method differs on each data provider. 
        /// </remarks>
        /// <param name="command">The IDbCommand to be cleared</param>
        protected virtual void ClearCommand(IDbCommand command)
        {
            // do nothing by default
        }

#endregion private utility methods

#region ExecuteDataset

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <param name="command">The IDbCommand object to use</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if command is null.</exception>
        public virtual DataSet ExecuteDataset(IDbCommand command)
        {
            bool mustCloseConnection = false;

            // Clean Up Parameter Syntax
            CleanParameterSyntax(command);

            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
                mustCloseConnection = true;
            }

            // Create the DataAdapter & DataSet
            IDbDataAdapter da = null;
            try
            {
                da = GetDataAdapter();
                da.SelectCommand = command;

                DataSet ds = new DataSet();

                try
                {
                    // Fill the DataSet using default values for DataTable names, etc
                    da.Fill(ds);
                }
                catch (Exception ex)
                {
                    // Don't just throw ex.  It changes the call stack.  But we want the ex around for debugging, so...
                    Debug.WriteLine(ex);
                    throw;
                }

                // Detach the IDataParameters from the command object, so they can be used again
                // Don't do this...screws up output params -- cjb 
                //command.Parameters.Clear();

                // Return the DataSet
                return ds;
            }
            finally
            {
                if (mustCloseConnection)
                {
                    command.Connection.Close();
                }
                if (da != null)
                {
                    IDisposable id = da as IDisposable;
                    if (id != null)
                        id.Dispose();
                }
            }
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <example>
        /// <code>
        /// DataSet ds = helper.ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders");
        /// </code></example>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        public virtual DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of IDataParameters
            return ExecuteDataset(connectionString, commandType, commandText, (IDataParameter[])null);
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <example>
        /// <code>
        /// DataSet ds = helper.ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new IDbParameter("@prodid", 24));
        /// </code></example>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDbParamters used to execute the command</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        public virtual DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");

            // Create & open an IDbConnection, and dispose of it after we are done
            using (IDbConnection connection = GetConnection(connectionString))
            {
                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                return ExecuteDataset(connection, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// DataSet ds = helper.ExecuteDataset(connString, "GetOrders", 24, 36);
        /// </code></example>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        public virtual DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                IDataParameter[] iDataParameterValues = GetDataParameters(parameterValues.Length);

                // if we've been passed IDataParameters, don't do parameter discovery
                if (AreParameterValuesIDataParameters(parameterValues, iDataParameterValues))
                {
                    return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, iDataParameterValues);
                }
                else
                {
                    // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                    bool includeReturnValue = CheckForReturnValueParameter(parameterValues);
                    IDataParameter[] commandParameters = GetSpParameterSet(connectionString, spName, includeReturnValue);

                    // Assign the provided values to these parameters based on parameter order
                    AssignParameterValues(commandParameters, parameterValues);

                    // Call the overload that takes an array of IDataParameters
                    return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, commandParameters);
                }
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset and takes no parameters) against the provided IDbConnection. 
        /// </summary>
        /// <example>
        /// <code>
        /// DataSet ds = helper.ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual DataSet ExecuteDataset(IDbConnection connection, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of IDataParameters
            return ExecuteDataset(connection, commandType, commandText, (IDataParameter[])null);
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the specified IDbConnection 
        /// using the provided parameters.
        /// </summary>
        /// <example>
        /// <code>
        /// DataSet ds = helper.ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual DataSet ExecuteDataset(IDbConnection connection, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            // Create a command and prepare it for execution
            IDbCommand cmd = connection.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, connection, (IDbTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);
            CleanParameterSyntax(cmd);

            DataSet ds = ExecuteDataset(cmd);

            if (mustCloseConnection)
                connection.Close();

            // Return the DataSet
            return ds;
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified IDbConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// DataSet ds = helper.ExecuteDataset(conn, "GetOrders", 24, 36);
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual DataSet ExecuteDataset(IDbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                IDataParameter[] iDataParameterValues = GetDataParameters(parameterValues.Length);

                // if we've been passed IDataParameters, don't do parameter discovery
                if (AreParameterValuesIDataParameters(parameterValues, iDataParameterValues))
                {
                    return ExecuteDataset(connection, CommandType.StoredProcedure, spName, iDataParameterValues);
                }
                else
                {
                    // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                    bool includeReturnValue = CheckForReturnValueParameter(parameterValues);
                    IDataParameter[] commandParameters = GetSpParameterSet(connection, spName, includeReturnValue);

                    // Assign the provided values to these parameters based on parameter order
                    AssignParameterValues(commandParameters, parameterValues);

                    // Call the overload that takes an array of IDataParameters
                    return ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
                }
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset and takes no parameters) against the provided IDbTransaction. 
        /// </summary>
        /// <example><code>
        ///  DataSet ds = helper.ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual DataSet ExecuteDataset(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of IDataParameters
            return ExecuteDataset(transaction, commandType, commandText, (IDataParameter[])null);
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the specified IDbTransaction
        /// using the provided parameters.
        /// </summary>
        /// <example>
        /// <code>
        /// DataSet ds = helper.ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual DataSet ExecuteDataset(IDbTransaction transaction, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");

            // Create a command and prepare it for execution
            IDbCommand cmd = transaction.Connection.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            CleanParameterSyntax(cmd);

            return ExecuteDataset(cmd);

        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified 
        /// IDbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// DataSet ds = helper.ExecuteDataset(tran, "GetOrders", 24, 36);
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual DataSet ExecuteDataset(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                IDataParameter[] iDataParameterValues = GetDataParameters(parameterValues.Length);

                // if we've been passed IDataParameters, don't do parameter discovery
                if (AreParameterValuesIDataParameters(parameterValues, iDataParameterValues))
                {
                    return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, iDataParameterValues);
                }
                else
                {
                    // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                    bool includeReturnValue = CheckForReturnValueParameter(parameterValues);
                    IDataParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName, includeReturnValue);

                    // Assign the provided values to these parameters based on parameter order
                    AssignParameterValues(commandParameters, parameterValues);

                    // Call the overload that takes an array of IDataParameters
                    return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
                }
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
        }

#endregion ExecuteDataset

#region ExecuteNonQuery

        /// <summary>
        /// Execute an IDbCommand (that returns no resultset) against the database
        /// </summary>
        /// <param name="command">The IDbCommand to execute</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if command is null.</exception>
        public virtual int ExecuteNonQuery(IDbCommand command)
        {
            bool mustCloseConnection = false;

            // Clean Up Parameter Syntax
            CleanParameterSyntax(command);

            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
                mustCloseConnection = true;
            }

            if (command == null) throw new ArgumentNullException("command");

            int returnVal;

            returnVal = command.ExecuteNonQuery();

            if (mustCloseConnection)
            {
                command.Connection.Close();
            }

            return returnVal;
        }

        /// <summary>
        /// Execute an IDbCommand (that returns no resultset and takes no parameters) against the database specified in 
        /// the connection string
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        public virtual int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of IDataParameters
            return ExecuteNonQuery(connectionString, commandType, commandText, (IDataParameter[])null);
        }

        /// <summary>
        /// Execute an IDbCommand (that returns no resultset) against the database specified in the connection string 
        /// using the provided parameters
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        public virtual int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");

            // Create & open an IDbConnection, and dispose of it after we are done
            using (IDbConnection connection = GetConnection(connectionString))
            {
                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                return ExecuteNonQuery(connection, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns no resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// </remarks>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored prcedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        public virtual int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                IDataParameter[] iDataParameterValues = GetDataParameters(parameterValues.Length);

                // if we've been passed IDataParameters, don't do parameter discoveryu
                if (AreParameterValuesIDataParameters(parameterValues, iDataParameterValues))
                {
                    return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, iDataParameterValues);
                }
                else
                {
                    // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                    bool includeReturnValue = CheckForReturnValueParameter(parameterValues);
                    IDataParameter[] commandParameters = GetSpParameterSet(connectionString, spName, includeReturnValue);

                    // Assign the provided values to these parameters based on parameter order
                    AssignParameterValues(commandParameters, parameterValues);

                    // Call the overload that takes an array of IDataParameters
                    return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters);
                }
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute an IDbCommand (that returns no resultset and takes no parameters) against the provided IDbConnection. 
        /// </summary>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual int ExecuteNonQuery(IDbConnection connection, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of IDataParameters
            return ExecuteNonQuery(connection, commandType, commandText, (IDataParameter[])null);
        }

        /// <summary>
        /// Execute an IDbCommand (that returns no resultset) against the specified IDbConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDbParamters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual int ExecuteNonQuery(IDbConnection connection, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            // Create a command and prepare it for execution
            IDbCommand cmd = connection.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, connection, (IDbTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);
            CleanParameterSyntax(cmd);

            // Finally, execute the command
            int retval = ExecuteNonQuery(cmd);

            // Detach the IDataParameters from the command object, so they can be used again
            // don't do this...screws up output parameters -- cjbreisch
            // cmd.Parameters.Clear();
            if (mustCloseConnection)
                connection.Close();
            return retval;
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns no resultset) against the specified IDbConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// </remarks>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual int ExecuteNonQuery(IDbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                IDataParameter[] iDataParameterValues = GetDataParameters(parameterValues.Length);

                // if we've been passed IDataParameters, don't do parameter discoveryu
                if (AreParameterValuesIDataParameters(parameterValues, iDataParameterValues))
                {
                    return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, iDataParameterValues);
                }
                else
                {
                    // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                    bool includeReturnValue = CheckForReturnValueParameter(parameterValues);
                    IDataParameter[] commandParameters = GetSpParameterSet(connection, spName, includeReturnValue);

                    // Assign the provided values to these parameters based on parameter order
                    AssignParameterValues(commandParameters, parameterValues);

                    // Call the overload that takes an array of IDataParameters
                    return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
                }
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute an IDbCommand (that returns no resultset and takes no parameters) against the provided IDbTransaction. 
        /// </summary>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of IDataParameters
            return ExecuteNonQuery(transaction, commandType, commandText, (IDataParameter[])null);
        }

        /// <summary>
        /// Execute an IDbCommand (that returns no resultset) against the specified IDbTransaction
        /// using the provided parameters.
        /// </summary>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");

            // Create a command and prepare it for execution
            IDbCommand cmd = transaction.Connection.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            CleanParameterSyntax(cmd);

            // Finally, execute the command
            int retval = ExecuteNonQuery(cmd);

            // Detach the IDataParameters from the command object, so they can be used again
            // don't do this...screws up output parameters -- cjbreisch
            // cmd.Parameters.Clear();
            return retval;
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns no resultset) against the specified 
        /// IDbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual int ExecuteNonQuery(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                IDataParameter[] iDataParameterValues = GetDataParameters(parameterValues.Length);

                // if we've been passed IDataParameters, don't do parameter discoveryu
                if (AreParameterValuesIDataParameters(parameterValues, iDataParameterValues))
                {
                    return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, iDataParameterValues);
                }
                else
                {
                    // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                    bool includeReturnValue = CheckForReturnValueParameter(parameterValues);
                    IDataParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName, includeReturnValue);

                    // Assign the provided values to these parameters based on parameter order
                    AssignParameterValues(commandParameters, parameterValues);

                    // Call the overload that takes an array of IDbParameters
                    return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
                }
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
            }
        }

#endregion ExecuteNonQuery

#region ExecuteReader

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <param name="command">The IDbCommand object to use</param>
        /// <returns>A IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if command is null.</exception>
        public virtual IDataReader ExecuteReader(IDbCommand command)
        {
            return ExecuteReader(command, AdoConnectionOwnership.External);
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <param name="command">The IDbCommand object to use</param>
        /// <param name="connectionOwnership">Enum indicating whether the connection was created internally or externally.</param>
        /// <returns>A IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if command is null.</exception>
        protected virtual IDataReader ExecuteReader(IDbCommand command, AdoConnectionOwnership connectionOwnership)
        {
            // Clean Up Parameter Syntax
            CleanParameterSyntax(command);

            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
                connectionOwnership = AdoConnectionOwnership.Internal;
            }

            // Create a reader
            IDataReader dataReader;

            // Call ExecuteReader with the appropriate CommandBehavior
            if (connectionOwnership == AdoConnectionOwnership.External)
            {
                dataReader = command.ExecuteReader();
            }
            else
            {
                try
                {
                    dataReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch (Exception ex)
                {
                    // Don't just throw ex.  It changes the call stack.  But we want the ex around for debugging, so...
                    Debug.WriteLine(ex);
                    throw;
                }
            }

            ClearCommand(command);

            return dataReader;
        }

        /// <summary>
        /// Create and prepare an IDbCommand, and call ExecuteReader with the appropriate CommandBehavior.
        /// </summary>
        /// <remarks>
        /// If we created and opened the connection, we want the connection to be closed when the DataReader is closed.
        /// 
        /// If the caller provided the connection, we want to leave it to them to manage.
        /// </remarks>
        /// <param name="connection">A valid IDbConnection, on which to execute this command</param>
        /// <param name="transaction">A valid IDbTransaction, or 'null'</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters to be associated with the command or 'null' if no parameters are required</param>
        /// <param name="connectionOwnership">Indicates whether the connection parameter was provided by the caller, or created by AdoHelper</param>
        /// <returns>IDataReader containing the results of the command</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        private IDataReader ExecuteReader(IDbConnection connection, IDbTransaction transaction, CommandType commandType, string commandText, IDataParameter[] commandParameters, AdoConnectionOwnership connectionOwnership)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            bool mustCloseConnection = false;
            // Create a command and prepare it for execution
            IDbCommand cmd = connection.CreateCommand();
            try
            {
                PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
                CleanParameterSyntax(cmd);

                // override conenctionOwnership if we created the connection in PrepareCommand -- cjbreisch
                if (mustCloseConnection)
                {
                    connectionOwnership = AdoConnectionOwnership.Internal;
                }

                // Create a reader
                IDataReader dataReader;

                dataReader = ExecuteReader(cmd, connectionOwnership);

                ClearCommand(cmd);

                return dataReader;
            }
            catch
            {
                if (mustCloseConnection)
                    connection.Close();
                throw;
            }
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>A IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        public virtual IDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of IDataParameters
            return ExecuteReader(connectionString, commandType, commandText, (IDataParameter[])null);
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <returns>A IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        public virtual IDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            IDbConnection connection = null;
            try
            {
                connection = GetConnection(connectionString);
                connection.Open();

                // Call the private overload that takes an internally owned connection in place of the connection string
                return ExecuteReader(connection, null, commandType, commandText, commandParameters, AdoConnectionOwnership.Internal);
            }
            catch
            {
                // If we fail to return the IDataReader, we need to close the connection ourselves
                if (connection != null) connection.Close();
                throw;
            }

        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// IDataReader dr = helper.ExecuteReader(connString, "GetOrders", 24, 36);
        /// </code></example>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>an IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        public virtual IDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                IDataParameter[] iDataParameterValues = GetDataParameters(parameterValues.Length);

                // if we've been passed IDataParameters, don't do parameter discovery
                if (AreParameterValuesIDataParameters(parameterValues, iDataParameterValues))
                {
                    return ExecuteReader(connectionString, CommandType.StoredProcedure, spName, iDataParameterValues);
                }
                else
                {
                    bool includeReturnValue = CheckForReturnValueParameter(parameterValues);
                    IDataParameter[] commandParameters = GetSpParameterSet(connectionString, spName, includeReturnValue);

                    AssignParameterValues(commandParameters, parameterValues);

                    return ExecuteReader(connectionString, CommandType.StoredProcedure, spName, commandParameters);
                }
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset and takes no parameters) against the provided IDbConnection. 
        /// </summary>
        /// <example>
        /// <code>
        /// IDataReader dr = helper.ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders");
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>an IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        public virtual IDataReader ExecuteReader(IDbConnection connection, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of IDataParameters
            return ExecuteReader(connection, commandType, commandText, (IDataParameter[])null);
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the specified IDbConnection 
        /// using the provided parameters.
        /// </summary>
        /// <example>
        /// <code>
        /// IDataReader dr = helper.ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <returns>an IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual IDataReader ExecuteReader(IDbConnection connection, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            // Pass through the call to the private overload using a null transaction value and an externally owned connection
            return ExecuteReader(connection, (IDbTransaction)null, commandType, commandText, commandParameters, AdoConnectionOwnership.External);
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified IDbConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// IDataReader dr = helper.ExecuteReader(conn, "GetOrders", 24, 36);
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>an IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual IDataReader ExecuteReader(IDbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                IDataParameter[] iDataParameterValues = GetDataParameters(parameterValues.Length);

                // if we've been passed IDataParameters, don't do parameter discovery
                if (AreParameterValuesIDataParameters(parameterValues, iDataParameterValues))
                {
                    return ExecuteReader(connection, CommandType.StoredProcedure, spName, iDataParameterValues);
                }
                else
                {
                    bool includeReturnValue = CheckForReturnValueParameter(parameterValues);
                    IDataParameter[] commandParameters = GetSpParameterSet(connection, spName, includeReturnValue);

                    AssignParameterValues(commandParameters, parameterValues);

                    return ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
                }
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteReader(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset and takes no parameters) against the provided IDbTransaction. 
        /// </summary>
        /// <example><code>
        ///  IDataReader dr = helper.ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>A IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        public virtual IDataReader ExecuteReader(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of IDataParameters
            return ExecuteReader(transaction, commandType, commandText, (IDataParameter[])null);
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the specified IDbTransaction
        /// using the provided parameters.
        /// </summary>
        /// <example>
        /// <code>
        /// IDataReader dr = helper.ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <returns>A IDataReader containing the resultset generated by the command</returns>
        public virtual IDataReader ExecuteReader(IDbTransaction transaction, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");

            // Pass through to private overload, indicating that the connection is owned by the caller
            return ExecuteReader(transaction.Connection, transaction, commandType, commandText, commandParameters, AdoConnectionOwnership.External);
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified
        /// IDbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// IDataReader dr = helper.ExecuteReader(tran, "GetOrders", 24, 36);
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>an IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual IDataReader ExecuteReader(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                IDataParameter[] iDataParameterValues = GetDataParameters(parameterValues.Length);

                // if we've been passed IDataParameters, don't do parameter discovery
                if (AreParameterValuesIDataParameters(parameterValues, iDataParameterValues))
                {
                    return ExecuteReader(transaction, CommandType.StoredProcedure, spName, iDataParameterValues);
                }
                else
                {
                    bool includeReturnValue = CheckForReturnValueParameter(parameterValues);
                    IDataParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName, includeReturnValue);

                    AssignParameterValues(commandParameters, parameterValues);

                    return ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
                }
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteReader(transaction, CommandType.StoredProcedure, spName);
            }
        }

#endregion ExecuteReader

#region ExecuteScalar

        /// <summary>
        /// Execute an IDbCommand (that returns a 1x1 resultset) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <param name="command">The IDbCommand to execute</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if command is null.</exception>
        public virtual object ExecuteScalar(IDbCommand command)
        {
            bool mustCloseConnection = false;

            // Clean Up Parameter Syntax
            CleanParameterSyntax(command);

            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
                mustCloseConnection = true;
            }

            // Execute the command & return the results
            object retval = command.ExecuteScalar();

            // Detach the IDataParameters from the command object, so they can be used again
            // don't do this...screws up output params -- cjbreisch
            // command.Parameters.Clear();

            if (mustCloseConnection)
            {
                command.Connection.Close();
            }

            return retval;
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a 1x1 resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <example>
        /// <code>
        /// int orderCount = (int)helper.ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount");
        /// </code></example>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        public virtual object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of IDataParameters
            return ExecuteScalar(connectionString, commandType, commandText, (IDataParameter[])null);
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a 1x1 resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        public virtual object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            // Create & open an IDbConnection, and dispose of it after we are done
            IDbConnection connection = null;
            try
            {
                connection = GetConnection(connectionString);
                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                return ExecuteScalar(connection, commandType, commandText, commandParameters);
            }
            finally
            {
                IDisposable id = connection as IDisposable;
                if (id != null) id.Dispose();
            }
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a 1x1 resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// int orderCount = (int)helper.ExecuteScalar(connString, "GetOrderCount", 24, 36);
        /// </code></example>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        public virtual object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                IDataParameter[] iDataParameterValues = GetDataParameters(parameterValues.Length);

                // if we've been passed IDataParameters, don't do parameter discovery
                if (AreParameterValuesIDataParameters(parameterValues, iDataParameterValues))
                {
                    return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, iDataParameterValues);
                }
                else
                {
                    // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                    bool includeReturnValue = CheckForReturnValueParameter(parameterValues);
                    IDataParameter[] commandParameters = GetSpParameterSet(connectionString, spName, includeReturnValue);

                    // Assign the provided values to these parameters based on parameter order
                    AssignParameterValues(commandParameters, parameterValues);

                    // Call the overload that takes an array of IDataParameters
                    return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, commandParameters);
                }
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a 1x1 resultset and takes no parameters) against the provided IDbConnection. 
        /// </summary>
        /// <example>
        /// <code>
        /// int orderCount = (int)helper.ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount");
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        public virtual object ExecuteScalar(IDbConnection connection, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of IDbParameters
            return ExecuteScalar(connection, commandType, commandText, (IDataParameter[])null);
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a 1x1 resultset) against the specified IDbConnection 
        /// using the provided parameters.
        /// </summary>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual object ExecuteScalar(IDbConnection connection, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            // Create a command and prepare it for execution
            IDbCommand cmd = connection.CreateCommand();

            bool mustCloseConnection = false;
            PrepareCommand(cmd, connection, (IDbTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);
            CleanParameterSyntax(cmd);

            // Execute the command & return the results
            object retval = ExecuteScalar(cmd);

            // Detach the IDataParameters from the command object, so they can be used again
            // don't do this...screws up output parameters -- cjbreisch
            // cmd.Parameters.Clear();

            if (mustCloseConnection)
                connection.Close();

            return retval;
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a 1x1 resultset) against the specified IDbConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// int orderCount = (int)helper.ExecuteScalar(conn, "GetOrderCount", 24, 36);
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual object ExecuteScalar(IDbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                IDataParameter[] iDataParameterValues = GetDataParameters(parameterValues.Length);

                // if we've been passed IDataParameters, don't do parameter discovery
                if (AreParameterValuesIDataParameters(parameterValues, iDataParameterValues))
                {
                    return ExecuteScalar(connection, CommandType.StoredProcedure, spName, iDataParameterValues);
                }
                else
                {
                    // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                    bool includeReturnValue = CheckForReturnValueParameter(parameterValues);
                    IDataParameter[] commandParameters = GetSpParameterSet(connection, spName, includeReturnValue);

                    // Assign the provided values to these parameters based on parameter order
                    AssignParameterValues(commandParameters, parameterValues);

                    // Call the overload that takes an array of IDataParameters
                    return ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
                }
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteScalar(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a 1x1 resultset and takes no parameters) against the provided IDbTransaction. 
        /// </summary>
        /// <example>
        /// <code>
        /// int orderCount = (int)helper.ExecuteScalar(tran, CommandType.StoredProcedure, "GetOrderCount");
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        public virtual object ExecuteScalar(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of IDataParameters
            return ExecuteScalar(transaction, commandType, commandText, (IDataParameter[])null);
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a 1x1 resultset) against the specified IDbTransaction
        /// using the provided parameters.
        /// </summary>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDbParamters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual object ExecuteScalar(IDbTransaction transaction, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");

            // Create a command and prepare it for execution
            IDbCommand cmd = transaction.Connection.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            CleanParameterSyntax(cmd);

            // Execute the command & return the results
            object retval = ExecuteScalar(cmd);

            // Detach the IDataParameters from the command object, so they can be used again
            // don't do this...screws up output parameters -- cjbreisch
            // cmd.Parameters.Clear();
            return retval;
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a 1x1 resultset) against the specified
        /// IDbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// int orderCount = (int)helper.ExecuteScalar(tran, "GetOrderCount", 24, 36);
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the transaction is rolled back or commmitted</exception>
        public virtual object ExecuteScalar(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                IDataParameter[] iDataParameterValues = GetDataParameters(parameterValues.Length);

                // if we've been passed IDataParameters, don't do parameter discovery
                if (AreParameterValuesIDataParameters(parameterValues, iDataParameterValues))
                {
                    return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, iDataParameterValues);
                }
                else
                {
                    // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                    bool includeReturnValue = CheckForReturnValueParameter(parameterValues);
                    IDataParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName, includeReturnValue);

                    // Assign the provided values to these parameters based on parameter order
                    AssignParameterValues(commandParameters, parameterValues);

                    // Call the overload that takes an array of IDataParameters
                    return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
                }
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
            }
        }

#endregion ExecuteScalar 

#region ExecuteXmlReader

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset and takes no parameters) against the provided IDbConnection. 
        /// </summary>
        /// <example>
        /// <code>
        /// XmlReader r = helper.ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders");
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command using "FOR XML AUTO"</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public XmlReader ExecuteXmlReader(IDbConnection connection, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of IDataParameters
            return ExecuteXmlReader(connection, commandType, commandText, (IDataParameter[])null);
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the specified IDbConnection 
        /// using the provided parameters.
        /// </summary>
        /// <example>
        /// <code>
        /// XmlReader r = helper.ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders", GetParameter("@prodid", 24));
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command using "FOR XML AUTO"</param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public XmlReader ExecuteXmlReader(IDbConnection connection, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            bool mustCloseConnection = false;
            // Create a command and prepare it for execution
            IDbCommand cmd = connection.CreateCommand();
            try
            {
                PrepareCommand(cmd, connection, (IDbTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);
                CleanParameterSyntax(cmd);

                return ExecuteXmlReader(cmd);
            }
            catch (Exception ex)
            {
                if (mustCloseConnection)
                    connection.Close();
                // Don't just throw ex.  It changes the call stack.  But we want the ex around for debugging, so...
                Debug.WriteLine(ex);
                throw;
            }
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified IDbConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// XmlReader r = helper.ExecuteXmlReader(conn, "GetOrders", 24, 36);
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="spName">The name of the stored procedure using "FOR XML AUTO"</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public XmlReader ExecuteXmlReader(IDbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                ArrayList tempParameter = new ArrayList();
                foreach (IDataParameter parameter in GetSpParameterSet(connection, spName))
                {
                    tempParameter.Add(parameter);
                }
                IDataParameter[] commandParameters = (IDataParameter[])tempParameter.ToArray(typeof(IDataParameter));

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of IDataParameters
                return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset and takes no parameters) against the provided IDbTransaction. 
        /// </summary>
        /// <example>
        /// <code>
        /// XmlReader r = helper.ExecuteXmlReader(tran, CommandType.StoredProcedure, "GetOrders");
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command using "FOR XML AUTO"</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public XmlReader ExecuteXmlReader(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of IDataParameters
            return ExecuteXmlReader(transaction, commandType, commandText, (IDataParameter[])null);
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the specified IDbTransaction
        /// using the provided parameters.
        /// </summary>
        /// <example>
        /// <code>
        /// XmlReader r = helper.ExecuteXmlReader(tran, CommandType.StoredProcedure, "GetOrders", GetParameter("@prodid", 24));
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command using "FOR XML AUTO"</param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public XmlReader ExecuteXmlReader(IDbTransaction transaction, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");

            // Create a command and prepare it for execution
            IDbCommand cmd = transaction.Connection.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            CleanParameterSyntax(cmd);

            // Create the DataAdapter & DataSet
            XmlReader retval = ExecuteXmlReader(cmd);

            // Detach the IDataParameters from the command object, so they can be used again
            // don't do this...screws up output params -- cjbreisch
            // cmd.Parameters.Clear();
            return retval;
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified 
        /// IDbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// XmlReader r = helper.ExecuteXmlReader(trans, "GetOrders", 24, 36);
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="spName">The name of the stored procedure using "FOR XML AUTO"</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public XmlReader ExecuteXmlReader(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                ArrayList tempParameter = new ArrayList();
                foreach (IDataParameter parameter in GetSpParameterSet(transaction.Connection, spName))
                {
                    tempParameter.Add(parameter);
                }
                IDataParameter[] commandParameters = (IDataParameter[])tempParameter.ToArray(typeof(IDataParameter));

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of IDataParameters
                return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName);
            }
        }

#endregion ExecuteXmlReader

#region ExecuteXmlReaderTypedParams

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified IDbConnection 
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will assign the parameter values based on parameter order.
        /// </summary>
        /// <param name="command">The IDbCommand to execute</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if command is null.</exception>
        public XmlReader ExecuteXmlReaderTypedParams(IDbCommand command, DataRow dataRow)
        {
            if (command == null) throw new ArgumentNullException("command");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Set the parameters values
                AssignParameterValues(command.Parameters, dataRow);

                return ExecuteXmlReader(command);
            }
            else
            {
                return ExecuteXmlReader(command);
            }
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified IDbConnection 
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public XmlReader ExecuteXmlReaderTypedParams(IDbConnection connection, String spName, DataRow dataRow)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                ArrayList tempParameter = new ArrayList();
                foreach (IDataParameter parameter in GetSpParameterSet(connection, spName))
                {
                    tempParameter.Add(parameter);
                }
                IDataParameter[] commandParameters = (IDataParameter[])tempParameter.ToArray(typeof(IDataParameter));

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified IDbTransaction 
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="transaction">A valid IDbTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public XmlReader ExecuteXmlReaderTypedParams(IDbTransaction transaction, String spName, DataRow dataRow)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                ArrayList tempParameter = new ArrayList();
                foreach (IDataParameter parameter in GetSpParameterSet(transaction.Connection, spName))
                {
                    tempParameter.Add(parameter);
                }
                IDataParameter[] commandParameters = (IDataParameter[])tempParameter.ToArray(typeof(IDataParameter));

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName);
            }
        }

#endregion

#region FillDataset

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <param name="command">The IDbCommand to execute</param>
        /// <param name="dataSet">A DataSet wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)</param>
        /// <exception cref="System.ArgumentNullException">Thrown if command is null.</exception>
        public virtual void FillDataset(IDbCommand command, DataSet dataSet, string[] tableNames)
        {
            bool mustCloseConnection = false;

            // Clean Up Parameter Syntax
            CleanParameterSyntax(command);

            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
                mustCloseConnection = true;
            }

            // Create the DataAdapter & DataSet
            IDbDataAdapter dataAdapter = null;
            try
            {
                dataAdapter = GetDataAdapter();
                dataAdapter.SelectCommand = command;

                // Add the table mappings specified by the user
                if (tableNames != null && tableNames.Length > 0)
                {
                    string tableName = "Table";
                    for (int index = 0; index < tableNames.Length; index++)
                    {
                        if (tableNames[index] == null || tableNames[index].Length == 0)
                            throw new ArgumentException("The tableNames parameter must contain a list of tables, a value was provided as null or empty string.", "tableNames");
                        dataAdapter.TableMappings.Add(
                            tableName + (index == 0 ? "" : index.ToString()),
                            tableNames[index]);
                    }
                }

                // Fill the DataSet using default values for DataTable names, etc
                dataAdapter.Fill(dataSet);

                if (mustCloseConnection)
                {
                    command.Connection.Close();
                }

                // Detach the IDataParameters from the command object, so they can be used again
                // don't do this...screws up output params  --cjb
                // command.Parameters.Clear();
            }
            finally
            {
                IDisposable id = dataAdapter as IDisposable;
                if (id != null) id.Dispose();
            }

        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <example>
        /// <code>
        /// helper.FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </code></example>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="dataSet">A DataSet wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)</param>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        public virtual void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (dataSet == null) throw new ArgumentNullException("dataSet");

            // Create & open an IDbConnection, and dispose of it after we are done
            IDbConnection connection = null;
            try
            {
                connection = GetConnection(connectionString);
                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                FillDataset(connection, commandType, commandText, dataSet, tableNames);
            }
            finally
            {
                IDisposable id = connection as IDisposable;
                if (id != null) id.Dispose();
            }
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <param name="dataSet">A DataSet wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        public virtual void FillDataset(string connectionString, CommandType commandType,
            string commandText, DataSet dataSet, string[] tableNames,
            params IDataParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (dataSet == null) throw new ArgumentNullException("dataSet");
            // Create & open an IDbConnection, and dispose of it after we are done
            IDbConnection connection = null;
            try
            {
                connection = GetConnection(connectionString);
                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                FillDataset(connection, commandType, commandText, dataSet, tableNames, commandParameters);
            }
            finally
            {
                IDisposable id = connection as IDisposable;
                if (id != null) id.Dispose();
            }
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// helper.FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, 24);
        /// </code></example>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>    
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        public virtual void FillDataset(string connectionString, string spName,
            DataSet dataSet, string[] tableNames,
            params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (dataSet == null) throw new ArgumentNullException("dataSet");


            // Create & open an IDbConnection, and dispose of it after we are done
            IDbConnection connection = null;
            try
            {
                connection = GetConnection(connectionString);
                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                FillDataset(connection, spName, dataSet, tableNames, parameterValues);
            }
            finally
            {
                IDisposable id = connection as IDisposable;
                if (id != null) id.Dispose();
            }
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset and takes no parameters) against the provided IDbConnection. 
        /// </summary>
        /// <example>
        /// <code>
        /// helper.FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>    
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual void FillDataset(IDbConnection connection, CommandType commandType,
            string commandText, DataSet dataSet, string[] tableNames)
        {
            FillDataset(connection, commandType, commandText, dataSet, tableNames, null);
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the specified IDbConnection 
        /// using the provided parameters.
        /// </summary>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="dataSet">A DataSet wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual void FillDataset(IDbConnection connection, CommandType commandType,
            string commandText, DataSet dataSet, string[] tableNames,
            params IDataParameter[] commandParameters)
        {
            FillDataset(connection, null, commandType, commandText, dataSet, tableNames, commandParameters);
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified IDbConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// helper.FillDataset(conn, "GetOrders", ds, new string[] {"orders"}, 24, 36);
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual void FillDataset(IDbConnection connection, string spName,
            DataSet dataSet, string[] tableNames,
            params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (dataSet == null) throw new ArgumentNullException("dataSet");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                IDataParameter[] iDataParameterValues = GetDataParameters(parameterValues.Length);

                // if we've been passed IDataParameters, don't do parameter discovery
                if (AreParameterValuesIDataParameters(parameterValues, iDataParameterValues))
                {
                    FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames, iDataParameterValues);
                }
                else
                {
                    // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                    bool includeReturnValue = CheckForReturnValueParameter(parameterValues);
                    IDataParameter[] commandParameters = GetSpParameterSet(connection, spName, includeReturnValue);

                    // Assign the provided values to these parameters based on parameter order
                    AssignParameterValues(commandParameters, parameterValues);

                    // Call the overload that takes an array of IDataParameters
                    FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames, commandParameters);
                }
            }
            else
            {
                // Otherwise we can just call the SP without params
                FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames);
            }
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset and takes no parameters) against the provided IDbTransaction. 
        /// </summary>
        /// <example>
        /// <code>
        /// helper.FillDataset(tran, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>    
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual void FillDataset(IDbTransaction transaction, CommandType commandType,
            string commandText,
            DataSet dataSet, string[] tableNames)
        {
            FillDataset(transaction, commandType, commandText, dataSet, tableNames, null);
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the specified IDbTransaction
        /// using the provided parameters.
        /// </summary>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="dataSet">A DataSet wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual void FillDataset(IDbTransaction transaction, CommandType commandType,
            string commandText, DataSet dataSet, string[] tableNames,
            params IDataParameter[] commandParameters)
        {
            FillDataset(transaction.Connection, transaction, commandType, commandText, dataSet, tableNames, commandParameters);
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified 
        /// IDbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// helper.FillDataset(tran, "GetOrders", ds, new string[] {"orders"}, 24, 36);
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual void FillDataset(IDbTransaction transaction, string spName,
            DataSet dataSet, string[] tableNames,
            params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            if (dataSet == null) throw new ArgumentNullException("dataSet");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                IDataParameter[] iDataParameterValues = GetDataParameters(parameterValues.Length);

                // if we've been passed IDataParameters, don't do parameter discovery
                if (AreParameterValuesIDataParameters(parameterValues, iDataParameterValues))
                {
                    FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames, iDataParameterValues);
                }
                else
                {
                    // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                    bool includeReturnValue = CheckForReturnValueParameter(parameterValues);
                    IDataParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName, includeReturnValue);

                    // Assign the provided values to these parameters based on parameter order
                    AssignParameterValues(commandParameters, parameterValues);

                    // Call the overload that takes an array of IDataParameters
                    FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames, commandParameters);
                }
            }
            else
            {
                // Otherwise we can just call the SP without params
                FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames);
            }
        }

        /// <summary>
        /// Private helper method that execute an IDbCommand (that returns a resultset) against the specified IDbTransaction and IDbConnection
        /// using the provided parameters.
        /// </summary>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="dataSet">A DataSet wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        private void FillDataset(IDbConnection connection, IDbTransaction transaction, CommandType commandType,
            string commandText, DataSet dataSet, string[] tableNames,
            params IDataParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (dataSet == null) throw new ArgumentNullException("dataSet");

            // Create a command and prepare it for execution
            IDbCommand command = connection.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(command, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            CleanParameterSyntax(command);

            FillDataset(command, dataSet, tableNames);

            if (mustCloseConnection)
                connection.Close();
        }

#endregion

#region UpdateDataset

        /// <summary>
        /// This method consumes the RowUpdatingEvent and passes it on to the consumer specifed in the call to UpdateDataset
        /// </summary>
        /// <param name="obj">The object that generated the event</param>
        /// <param name="e">The System.Data.Common.RowUpdatingEventArgs</param>
        protected void RowUpdating(object obj, System.Data.Common.RowUpdatingEventArgs e)
        {
            if (this.m_rowUpdating != null)
                m_rowUpdating(obj, e);
        }

        /// <summary>
        /// This method consumes the RowUpdatedEvent and passes it on to the consumer specifed in the call to UpdateDataset
        /// </summary>
        /// <param name="obj">The object that generated the event</param>
        /// <param name="e">The System.Data.Common.RowUpdatingEventArgs</param>
        protected void RowUpdated(object obj, System.Data.Common.RowUpdatedEventArgs e)
        {
            if (this.m_rowUpdated != null)
                m_rowUpdated(obj, e);
        }

        /// <summary>
        /// Set up a command for updating a DataSet.
        /// </summary>
        /// <param name="command">command object to prepare</param>
        /// <param name="mustCloseConnection">output parameter specifying whether the connection used should be closed by the DAAB</param>
        /// <returns>An IDbCommand object</returns>
        protected virtual IDbCommand SetCommand(IDbCommand command, out bool mustCloseConnection)
        {
            mustCloseConnection = false;
            if (command != null)
            {
                IDataParameter[] commandParameters = new IDataParameter[command.Parameters.Count];
                command.Parameters.CopyTo(commandParameters, 0);
                command.Parameters.Clear();
                this.PrepareCommand(command, command.Connection, null, command.CommandType, command.CommandText, commandParameters, out mustCloseConnection);
                CleanParameterSyntax(command);
            }

            return command;
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <example>
        /// <code>
        /// helper.UpdateDataset(conn, insertCommand, deleteCommand, updateCommand, dataSet, "Order");
        /// </code></example>
        /// <param name="insertCommand">A valid SQL statement or stored procedure to insert new records into the data source</param>
        /// <param name="deleteCommand">A valid SQL statement or stored procedure to delete records from the data source</param>
        /// <param name="updateCommand">A valid SQL statement or stored procedure used to update records in the data source</param>
        /// <param name="dataSet">The DataSet used to update the data source</param>
        /// <param name="tableName">The DataTable used to update the data source.</param>
        public virtual void UpdateDataset(IDbCommand insertCommand, IDbCommand deleteCommand, IDbCommand updateCommand, DataSet dataSet, string tableName)
        {
            UpdateDataset(insertCommand, deleteCommand, updateCommand, dataSet, tableName, null, null);
        }

        /// <summary> 
        /// Executes the IDbCommand for each inserted, updated, or deleted row in the DataSet also implementing RowUpdating and RowUpdated Event Handlers 
        /// </summary> 
        /// <example> 
        /// <code>
        /// RowUpdatingEventHandler rowUpdatingHandler = new RowUpdatingEventHandler( OnRowUpdating );
        /// RowUpdatedEventHandler rowUpdatedHandler = new RowUpdatedEventHandler( OnRowUpdated );
        /// helper.UpdateDataSet(sqlInsertCommand, sqlDeleteCommand, sqlUpdateCommand, dataSet, "Order", rowUpdatingHandler, rowUpdatedHandler);
        /// </code></example> 
        /// <param name="insertCommand">A valid SQL statement or stored procedure to insert new records into the data source</param> 
        /// <param name="deleteCommand">A valid SQL statement or stored procedure to delete records from the data source</param> 
        /// <param name="updateCommand">A valid SQL statement or stored procedure used to update records in the data source</param> 
        /// <param name="dataSet">The DataSet used to update the data source</param> 
        /// <param name="tableName">The DataTable used to update the data source.</param> 
        /// <param name="rowUpdatingHandler">RowUpdatingEventHandler</param> 
        /// <param name="rowUpdatedHandler">RowUpdatedEventHandler</param> 
        public void UpdateDataset(IDbCommand insertCommand, IDbCommand deleteCommand, IDbCommand updateCommand,
            DataSet dataSet, string tableName, RowUpdatingHandler rowUpdatingHandler, RowUpdatedHandler rowUpdatedHandler)
        {
            int rowsAffected = 0;

            if (tableName == null || tableName.Length == 0) throw new ArgumentNullException("tableName");

            // Create an IDbDataAdapter, and dispose of it after we are done
            IDbDataAdapter dataAdapter = null;
            try
            {
                bool mustCloseUpdateConnection = false;
                bool mustCloseInsertConnection = false;
                bool mustCloseDeleteConnection = false;

                dataAdapter = GetDataAdapter();

                // Set the data adapter commands
                dataAdapter.UpdateCommand = SetCommand(updateCommand, out mustCloseUpdateConnection);
                dataAdapter.InsertCommand = SetCommand(insertCommand, out mustCloseInsertConnection);
                dataAdapter.DeleteCommand = SetCommand(deleteCommand, out mustCloseDeleteConnection);

                AddUpdateEventHandlers(dataAdapter, rowUpdatingHandler, rowUpdatedHandler);

                if (dataAdapter is DbDataAdapter)
                {
                    // Update the DataSet changes in the data source
                    try
                    {
                        rowsAffected = ((DbDataAdapter)dataAdapter).Update(dataSet, tableName);
                    }
                    catch (Exception ex)
                    {
                        // Don't just throw ex.  It changes the call stack.  But we want the ex around for debugging, so...
                        Debug.WriteLine(ex);
                        throw;
                    }
                }
                else
                {
                    dataAdapter.TableMappings.Add(tableName, "Table");

                    // Update the DataSet changes in the data source
                    rowsAffected = dataAdapter.Update(dataSet);
                }

                // Commit all the changes made to the DataSet
                dataSet.Tables[tableName].AcceptChanges();

                if (mustCloseUpdateConnection)
                {
                    updateCommand.Connection.Close();
                }
                if (mustCloseInsertConnection)
                {
                    insertCommand.Connection.Close();
                }
                if (mustCloseDeleteConnection)
                {
                    deleteCommand.Connection.Close();
                }
            }
            finally
            {
                IDisposable id = dataAdapter as IDisposable;
                if (id != null) id.Dispose();
            }
        }

#endregion

#region CreateCommand

        /// <summary>
        /// Simplify the creation of an IDbCommand object by allowing
        /// a stored procedure and optional parameters to be provided
        /// </summary>
        /// <example>
        /// <code>
        /// IDbCommand command = helper.CreateCommand(conn, "AddCustomer", "CustomerID", "CustomerName");
        /// </code></example>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="sourceColumns">An array of string to be assigned as the source columns of the stored procedure parameters</param>
        /// <returns>A valid IDbCommand object</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        public virtual IDbCommand CreateCommand(string connectionString, string spName, params string[] sourceColumns)
        {
            return CreateCommand(this.GetConnection(connectionString), spName, sourceColumns);
        }

        /// <summary>
        /// Simplify the creation of an IDbCommand object by allowing
        /// a stored procedure and optional parameters to be provided
        /// </summary>
        /// <example>
        /// <code>
        /// IDbCommand command = helper.CreateCommand(conn, "AddCustomer", "CustomerID", "CustomerName");
        /// </code></example>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="sourceColumns">An array of string to be assigned as the source columns of the stored procedure parameters</param>
        /// <returns>A valid IDbCommand object</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual IDbCommand CreateCommand(IDbConnection connection, string spName, params string[] sourceColumns)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // Create an IDbCommand
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = spName;
            cmd.CommandType = CommandType.StoredProcedure;

            // If we receive parameter values, we need to figure out where they go
            if ((sourceColumns != null) && (sourceColumns.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                IDataParameter[] commandParameters = GetSpParameterSet(connection, spName);

                // Assign the provided source columns to these parameters based on parameter order
                for (int index = 0; index < sourceColumns.Length; index++)
                    if (commandParameters[index].SourceColumn == String.Empty)
                        commandParameters[index].SourceColumn = sourceColumns[index];

                // Attach the discovered parameters to the IDbCommand object
                AttachParameters(cmd, commandParameters);
            }

            return cmd;
        }

        /// <summary>
        /// Simplify the creation of an IDbCommand object by allowing
        /// a stored procedure and optional parameters to be provided
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandText">A valid SQL statement</param>
        /// <param name="commandType">A System.Data.CommandType</param>
        /// <param name="commandParameters">The parameters for the SQL statement</param>
        /// <returns>A valid IDbCommand object</returns>
        public virtual IDbCommand CreateCommand(string connectionString, string commandText, CommandType commandType, params IDataParameter[] commandParameters)
        {
            return CreateCommand(this.GetConnection(connectionString), commandText, commandType, commandParameters);
        }

        /// <summary>
        /// Simplify the creation of an IDbCommand object by allowing
        /// a stored procedure and optional parameters to be provided
        /// </summary>
        /// <example><code>
        /// IDbCommand command = helper.CreateCommand(conn, "AddCustomer", "CustomerID", "CustomerName");
        /// </code></example>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="commandText">A valid SQL statement</param>
        /// <param name="commandType">A System.Data.CommandType</param>
        /// <param name="commandParameters">The parameters for the SQL statement</param>
        /// <returns>A valid IDbCommand object</returns>
        public virtual IDbCommand CreateCommand(IDbConnection connection, string commandText, CommandType commandType, params IDataParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

            // Create an IDbCommand
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = commandText;
            cmd.CommandType = commandType;

            // If we receive parameter values, we need to figure out where they go
            if ((commandParameters != null) && (commandParameters.Length > 0))
            {
                // Assign the provided source columns to these parameters based on parameter order
                for (int index = 0; index < commandParameters.Length; index++)
                    commandParameters[index].SourceColumn = commandParameters[index].ParameterName.TrimStart(new char[] { '@' });

                // Attach the discovered parameters to the IDbCommand object
                AttachParameters(cmd, commandParameters);
            }

            return cmd;
        }

#endregion

#region ExecuteNonQueryTypedParams

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns no resultset) 
        /// against the database specified in the connection string using the 
        /// dataRow column values as the stored procedure's parameters values.
        /// This method will assign the parameter values based on row values.
        /// </summary>
        /// <param name="command">The IDbCommand to execute</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if command is null.</exception>
        public virtual int ExecuteNonQueryTypedParams(IDbCommand command, DataRow dataRow)
        {
            int retVal = 0;

            // Clean Up Parameter Syntax
            CleanParameterSyntax(command);

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Set the parameters values
                AssignParameterValues(command.Parameters, dataRow);

                retVal = ExecuteNonQuery(command);
            }
            else
            {
                retVal = ExecuteNonQuery(command);
            }

            return retVal;
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns no resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        public virtual int ExecuteNonQueryTypedParams(String connectionString, String spName, DataRow dataRow)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                IDataParameter[] commandParameters = GetSpParameterSet(connectionString, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns no resultset) against the specified IDbConnection 
        /// using the dataRow column values as the stored procedure's parameters values.  
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual int ExecuteNonQueryTypedParams(IDbConnection connection, String spName, DataRow dataRow)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                IDataParameter[] commandParameters = GetSpParameterSet(connection, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns no resultset) against the specified
        /// IDbTransaction using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="transaction">A valid IDbTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual int ExecuteNonQueryTypedParams(IDbTransaction transaction, String spName, DataRow dataRow)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // Sf the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                IDataParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
            }
        }

#endregion

#region ExecuteDatasetTypedParams

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will assign the paraemter values based on row values.
        /// </summary>
        /// <param name="command">The IDbCommand to execute</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if command is null.</exception>
        public virtual DataSet ExecuteDatasetTypedParams(IDbCommand command, DataRow dataRow)
        {
            DataSet ds = null;

            // Clean Up Parameter Syntax
            CleanParameterSyntax(command);

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Set the parameters values
                AssignParameterValues(command.Parameters, dataRow);


                ds = ExecuteDataset(command);
            }
            else
            {
                ds = ExecuteDataset(command);
            }

            return ds;
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        public virtual DataSet ExecuteDatasetTypedParams(string connectionString, String spName, DataRow dataRow)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            //If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                IDataParameter[] commandParameters = GetSpParameterSet(connectionString, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified IDbConnection 
        /// using the dataRow column values as the store procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual DataSet ExecuteDatasetTypedParams(IDbConnection connection, String spName, DataRow dataRow)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                IDataParameter[] commandParameters = GetSpParameterSet(connection, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified IDbTransaction 
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="transaction">A valid IDbTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual DataSet ExecuteDatasetTypedParams(IDbTransaction transaction, String spName, DataRow dataRow)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                IDataParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
        }

#endregion

#region ExecuteReaderTypedParams

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will assign the parameter values based on parameter order.
        /// </summary>
        /// <param name="command">The IDbCommand to execute</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if command is null.</exception>
        public virtual IDataReader ExecuteReaderTypedParams(IDbCommand command, DataRow dataRow)
        {
            IDataReader reader = null;

            // Clean Up Parameter Syntax
            CleanParameterSyntax(command);

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Set the parameters values
                AssignParameterValues(command.Parameters, dataRow);

                reader = ExecuteReader(command);
            }
            else
            {
                reader = ExecuteReader(command);
            }

            return reader;
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        public virtual IDataReader ExecuteReaderTypedParams(String connectionString, String spName, DataRow dataRow)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                IDataParameter[] commandParameters = GetSpParameterSet(connectionString, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
            }
        }


        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified IDbConnection 
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual IDataReader ExecuteReaderTypedParams(IDbConnection connection, String spName, DataRow dataRow)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                IDataParameter[] commandParameters = GetSpParameterSet(connection, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteReader(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified IDbTransaction 
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="transaction">A valid IDbTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual IDataReader ExecuteReaderTypedParams(IDbTransaction transaction, String spName, DataRow dataRow)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                IDataParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteReader(transaction, CommandType.StoredProcedure, spName);
            }
        }

#endregion

#region ExecuteScalarTypedParams

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a 1x1 resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will assign the parameter values based on parameter order.
        /// </summary>
        /// <param name="command">The IDbCommand to execute</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if command is null.</exception>
        public virtual object ExecuteScalarTypedParams(IDbCommand command, DataRow dataRow)
        {
            object retVal = null;

            // Clean Up Parameter Syntax
            CleanParameterSyntax(command);

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Set the parameters values
                AssignParameterValues(command.Parameters, dataRow);

                retVal = ExecuteScalar(command);
            }
            else
            {
                retVal = ExecuteScalar(command);
            }

            return retVal;
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a 1x1 resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        public virtual object ExecuteScalarTypedParams(String connectionString, String spName, DataRow dataRow)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                IDataParameter[] commandParameters = GetSpParameterSet(connectionString, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a 1x1 resultset) against the specified IDbConnection 
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual object ExecuteScalarTypedParams(IDbConnection connection, String spName, DataRow dataRow)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                IDataParameter[] commandParameters = GetSpParameterSet(connection, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteScalar(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a 1x1 resultset) against the specified IDbTransaction
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="transaction">A valid IDbTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual object ExecuteScalarTypedParams(IDbTransaction transaction, String spName, DataRow dataRow)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                IDataParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
            }
        }

#endregion

#region Parameter Discovery Functions

        /// <summary>
        /// Checks for the existence of a return value parameter in the parametervalues
        /// </summary>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>true if the parameterValues contains a return value parameter, false otherwise</returns>
        private bool CheckForReturnValueParameter(object[] parameterValues)
        {
            bool hasReturnValue = false;
            foreach (object paramObject in parameterValues)
            {
                if (paramObject is IDataParameter)
                {
                    IDataParameter paramInstance = (IDataParameter)paramObject;
                    if (paramInstance.Direction == ParameterDirection.ReturnValue)
                    {
                        hasReturnValue = true;
                        break;
                    }
                }
            }
            return hasReturnValue;
        }

        /// <summary>
        /// Check to see if the parameter values passed to the helper are, in fact, IDataParameters.
        /// </summary>
        /// <param name="parameterValues">Array of parameter values passed to helper</param>
        /// <param name="iDataParameterValues">new array of IDataParameters built from parameter values</param>
        /// <returns>True if the parameter values are IDataParameters</returns>
        private bool AreParameterValuesIDataParameters(object[] parameterValues, IDataParameter[] iDataParameterValues)
        {
            bool areIDataParameters = true;

            for (int i = 0; i < parameterValues.Length; i++)
            {
                if (!(parameterValues[i] is IDataParameter))
                {
                    areIDataParameters = false;
                    break;
                }
                iDataParameterValues[i] = (IDataParameter)parameterValues[i];
            }
            return areIDataParameters;
        }


        /// <summary>
        /// Retrieves the set of IDataParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <returns>An array of IDataParameterParameters</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        public virtual IDataParameter[] GetSpParameterSet(string connectionString, string spName)
        {
            return GetSpParameterSet(connectionString, spName, false);
        }

        /// <summary>
        /// Retrieves the set of IDataParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of IDataParameters</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        public virtual IDataParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            using (IDbConnection connection = GetConnection(connectionString))
            {
                return GetSpParameterSetInternal(connection, spName, includeReturnValueParameter);
            }
        }

        /// <summary>
        /// Retrieves the set of IDataParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connection">A valid IDataConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <returns>An array of IDataParameters</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual IDataParameter[] GetSpParameterSet(IDbConnection connection, string spName)
        {
            return GetSpParameterSet(connection, spName, false);
        }

        /// <summary>
        /// Retrieves the set of IDataParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of IDataParameters</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual IDataParameter[] GetSpParameterSet(IDbConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (connection as ICloneable == null) throw new ArgumentException("can磘 discover parameters if the connection doesn磘 implement the ICloneable interface", "connection");

            IDbConnection clonedConnection = (IDbConnection)((ICloneable)connection).Clone();
            return GetSpParameterSetInternal(clonedConnection, spName, includeReturnValueParameter);
        }

        /// <summary>
        /// Retrieves the set of IDataParameters appropriate for the stored procedure
        /// </summary>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of IDataParameters</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        private IDataParameter[] GetSpParameterSetInternal(IDbConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // string hashKey = connection.ConnectionString + " :" + spName + (includeReturnValueParameter ? " :include ReturnValue Parameter" :"");

            IDataParameter[] cachedParameters;

            cachedParameters = GetCachedParameterSet(connection,
                spName + (includeReturnValueParameter ? " :include ReturnValue Parameter" : ""));

            if (cachedParameters == null)
            {
                IDataParameter[] spParameters = DiscoverSpParameterSet(connection, spName, includeReturnValueParameter);
                CacheParameterSet(connection,
                    spName + (includeReturnValueParameter ? " :include ReturnValue Parameter" : ""), spParameters);

                cachedParameters = ADOHelperParameterCache.CloneParameters(spParameters);
            }

            return cachedParameters;
        }

        /// <summary>
        /// Retrieve a parameter array from the cache
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>An array of IDataParameters</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        public IDataParameter[] GetCachedParameterSet(string connectionString, string commandText)
        {
            using (IDbConnection connection = GetConnection(connectionString))
            {
                return GetCachedParameterSetInternal(connection, commandText);
            }
        }

        /// <summary>
        /// Retrieve a parameter array from the cache
        /// </summary>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>An array of IDataParameters</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public IDataParameter[] GetCachedParameterSet(IDbConnection connection, string commandText)
        {
            return GetCachedParameterSetInternal(connection, commandText);
        }

        /// <summary>
        /// Retrieve a parameter array from the cache
        /// </summary>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>An array of IDataParameters</returns>
        private IDataParameter[] GetCachedParameterSetInternal(IDbConnection connection, string commandText)
        {
            bool mustCloseConnection = false;
            // this way we control the connection, and therefore the connection string that gets saved as a hash key
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
                mustCloseConnection = true;
            }

            IDataParameter[] parameters = ADOHelperParameterCache.GetCachedParameterSet(connection.ConnectionString, commandText);

            if (mustCloseConnection)
            {
                connection.Close();
            }

            return parameters;
        }

        /// <summary>
        /// Add parameter array to the cache
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters to be cached</param>
        public void CacheParameterSet(string connectionString, string commandText, params IDataParameter[] commandParameters)
        {
            using (IDbConnection connection = GetConnection(connectionString))
            {
                CacheParameterSetInternal(connection, commandText, commandParameters);
            }
        }

        /// <summary>
        /// Add parameter array to the cache
        /// </summary>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters to be cached</param>
        public void CacheParameterSet(IDbConnection connection, string commandText, params IDataParameter[] commandParameters)
        {
            if (connection is ICloneable)
            {
                using (IDbConnection clonedConnection = (IDbConnection)((ICloneable)connection).Clone())
                {
                    CacheParameterSetInternal(clonedConnection, commandText, commandParameters);
                }
            }
            else
            {
                throw new InvalidCastException();
            }
        }

        /// <summary>
        /// Add parameter array to the cache
        /// </summary>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters to be cached</param>
        private void CacheParameterSetInternal(IDbConnection connection, string commandText, params IDataParameter[] commandParameters)
        {
            // this way we control the connection, and therefore the connection string that gets saved as a hask key
            connection.Open();
            ADOHelperParameterCache.CacheParameterSet(connection.ConnectionString, commandText, commandParameters);
            connection.Close();
        }

        /// <summary>
        /// Resolve at run time the appropriate set of IDataParameters for a stored procedure
        /// </summary>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">Whether or not to include their return value parameter</param>
        /// <returns>The parameter array discovered.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        private IDataParameter[] DiscoverSpParameterSet(IDbConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = spName;
            cmd.CommandType = CommandType.StoredProcedure;

            connection.Open();
            DeriveParameters(cmd);
            connection.Close();

            if (!includeReturnValueParameter)
            {
                // not all providers have return value parameters...don't just remove this parameter indiscriminately
                if (cmd.Parameters.Count > 0 && ((IDataParameter)cmd.Parameters[0]).Direction == ParameterDirection.ReturnValue)
                {
                    cmd.Parameters.RemoveAt(0);
                }
            }

            IDataParameter[] discoveredParameters = new IDataParameter[cmd.Parameters.Count];

            cmd.Parameters.CopyTo(discoveredParameters, 0);

            // Init the parameters with a DBNull value
            foreach (IDataParameter discoveredParameter in discoveredParameters)
            {
                discoveredParameter.Value = DBNull.Value;
            }
            return discoveredParameters;
        }

#endregion Parameter Discovery Functions
    }

#region ParameterCache

    /// <summary>
    /// ADOHelperParameterCache provides functions to leverage a static cache of procedure parameters, and the
    /// ability to discover parameters for stored procedures at run-time.
    /// </summary>
    public sealed class ADOHelperParameterCache
    {
        private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// Deep copy of cached IDataParameter array
        /// </summary>
        /// <param name="originalParameters"></param>
        /// <returns></returns>
        internal static IDataParameter[] CloneParameters(IDataParameter[] originalParameters)
        {
            IDataParameter[] clonedParameters = new IDataParameter[originalParameters.Length];

            for (int i = 0, j = originalParameters.Length; i < j; i++)
            {
                clonedParameters[i] = (IDataParameter)((ICloneable)originalParameters[i]).Clone();
            }

            return clonedParameters;
        }

#region caching functions

        /// <summary>
        /// Add parameter array to the cache
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters to be cached</param>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        internal static void CacheParameterSet(string connectionString, string commandText, params IDataParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

            string hashKey = connectionString + " :" + commandText;

            paramCache[hashKey] = commandParameters;
        }

        /// <summary>
        /// Retrieve a parameter array from the cache
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>An array of IDataParameters</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connectionString is null</exception>
        internal static IDataParameter[] GetCachedParameterSet(string connectionString, string commandText)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

            string hashKey = connectionString + " :" + commandText;

            IDataParameter[] cachedParameters = paramCache[hashKey] as IDataParameter[];
            if (cachedParameters == null)
            {
                return null;
            }
            else
            {
                return CloneParameters(cachedParameters);
            }
        }

#endregion caching functions
    }

#endregion

    public class DAABSectionHandler :
        IConfigurationSectionHandler
    {
#region IConfigurationSectionHandler Members

        /// <summary>
        /// Evaluates the given XML section and returns a Hashtable that contains the results of the evaluation.
        /// </summary>
        /// <param name="parent">The configuration settings in a corresponding parent configuration section. </param>
        /// <param name="configContext">An HttpConfigurationContext when Create is called from the ASP.NET configuration system. Otherwise, this parameter is reserved and is a null reference (Nothing in Visual Basic). </param>
        /// <param name="section">The XmlNode that contains the configuration information to be handled. Provides direct access to the XML contents of the configuration section. </param>
        /// <returns>A Hashtable that contains the section's configuration settings.</returns>
        public object Create(object parent, object configContext, XmlNode section)
        {
            Hashtable ht = new Hashtable();
            XmlNodeList list = section.SelectNodes("daabProvider");
            foreach (XmlNode prov in list)
            {
                if (prov.Attributes["alias"] == null)
                    throw new InvalidOperationException("The 'daabProvider' node must contain an attribute named 'alias' with the alias name for the provider.");
                if (prov.Attributes["assembly"] == null)
                    throw new InvalidOperationException("The 'daabProvider' node must contain an attribute named 'assembly' with the name of the assembly containing the provider.");
                if (prov.Attributes["type"] == null)
                    throw new InvalidOperationException("The 'daabProvider' node must contain an attribute named 'type' with the full name of the type for the provider.");

                ht[prov.Attributes["alias"].Value] = new ProviderAlias(prov.Attributes["assembly"].Value, prov.Attributes["type"].Value);
            }
            return ht;
        }

#endregion
    }

    /// <summary>
    /// This class is for reading the 'ProviderAlias' tag from the 'daabProviders' section of the App.Config file
    /// </summary>
    public class ProviderAlias
    {
#region Member variables

        private string _assemblyName;
        private string _typeName;

#endregion

#region Constructor

        /// <summary>
        /// Constructor required by IConfigurationSectionHandler
        /// </summary>
        /// <param name="assemblyName">The Assembly where this provider can be found</param>
        /// <param name="typeName">The type of the provider</param>
        public ProviderAlias(string assemblyName, string typeName)
        {
            _assemblyName = assemblyName;
            _typeName = typeName;
        }

#endregion

#region Properties

        /// <summary>
        /// Returns the Assembly name for this provider
        /// </summary>
        /// <value>The Assembly name for the specified provider</value>
        public string AssemblyName
        {
            get { return _assemblyName; }
        }

        /// <summary>
        /// Returns the type name of this provider
        /// </summary>
        /// <value>The type name of the specified provider</value>
        public string TypeName
        {
            get { return _typeName; }
        }

#endregion
    }

    /// <summary>
    /// The SqlServer class is intended to encapsulate high performance, scalable best practices for 
    /// common uses of the SqlClient ADO.NET provider.  It is created using the abstract factory in AdoHelper.
    /// </summary>
    public class SqlServer : AdoHelper
    {
        /// <summary>
        /// Create a SQL Helper.  Needs to be a default constructor so that the Factory can create it
        /// </summary>
        public SqlServer()
        {
        }

#region Overrides

        /// <summary>
        /// Returns an array of SqlParameters of the specified size
        /// </summary>
        /// <param name="size">size of the array</param>
        /// <returns>The array of SqlParameters</returns>
        protected override IDataParameter[] GetDataParameters(int size)
        {
            return new SqlParameter[size];
        }

        /// <summary>
        /// Returns a SqlConnection object for the given connection string
        /// </summary>
        /// <param name="connectionString">The connection string to be used to create the connection</param>
        /// <returns>A SqlConnection object</returns>
        public override IDbConnection GetConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        /// <summary>
        /// Returns a SqlDataAdapter object
        /// </summary>
        /// <returns>The SqlDataAdapter</returns>
        public override IDbDataAdapter GetDataAdapter()
        {
            return new SqlDataAdapter();
        }

        /// <summary>
        /// Calls the CommandBuilder.DeriveParameters method for the specified provider, doing any setup and cleanup necessary
        /// </summary>
        /// <param name="cmd">The IDbCommand referencing the stored procedure from which the parameter information is to be derived. The derived parameters are added to the Parameters collection of the IDbCommand. </param>
        public override void DeriveParameters(IDbCommand cmd)
        {
            bool mustCloseConnection = false;

            if (!(cmd is SqlCommand))
                throw new ArgumentException("The command provided is not a SqlCommand instance.", "cmd");

            if (cmd.Connection.State != ConnectionState.Open)
            {
                cmd.Connection.Open();
                mustCloseConnection = true;
            }

            SqlDeriveParameters.DeriveParameters((SqlCommand)cmd);

            if (mustCloseConnection)
            {
                cmd.Connection.Close();
            }
        }

        /// <summary>
        /// Returns a SqlParameter object
        /// </summary>
        /// <returns>The SqlParameter object</returns>
        public override IDataParameter GetParameter()
        {
            return new SqlParameter();
        }

        /// <summary>
        /// Detach the IDataParameters from the command object, so they can be used again.
        /// </summary>
        /// <param name="command">command object to clear</param>
        protected override void ClearCommand(IDbCommand command)
        {
            // HACK: There is a problem here, the output parameter values are fletched 
            // when the reader is closed, so if the parameters are detached from the command
            // then the IDataReader can磘 set its values. 
            // When this happen, the parameters can磘 be used again in other command.
            bool canClear = true;

            foreach (IDataParameter commandParameter in command.Parameters)
            {
                if (commandParameter.Direction != ParameterDirection.Input)
                    canClear = false;

            }
            if (canClear)
            {
                command.Parameters.Clear();
            }
        }

        /// <summary>
        /// This cleans up the parameter syntax for an SQL Server call.  This was split out from PrepareCommand so that it could be called independently.
        /// </summary>
        /// <param name="command">An IDbCommand object containing the CommandText to clean.</param>
        public override void CleanParameterSyntax(IDbCommand command)
        {
            // do nothing for SQL
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the provided SqlConnection. 
        /// </summary>
        /// <example>
        /// <code>
        /// XmlReader r = helper.ExecuteXmlReader(command);
        /// </code></example>
        /// <param name="command">The IDbCommand to execute</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public override XmlReader ExecuteXmlReader(IDbCommand command)
        {
            bool mustCloseConnection = false;

            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
                mustCloseConnection = true;
            }

            CleanParameterSyntax(command);
            // Create the DataAdapter & DataSet
            XmlReader retval = ((SqlCommand)command).ExecuteXmlReader();

            // Detach the SqlParameters from the command object, so they can be used again
            // don't do this...screws up output parameters -- cjbreisch
            // cmd.Parameters.Clear();

            if (mustCloseConnection)
            {
                command.Connection.Close();
            }

            return retval;
        }

        /// <summary>
        /// Provider specific code to set up the updating/ed event handlers used by UpdateDataset
        /// </summary>
        /// <param name="dataAdapter">DataAdapter to attach the event handlers to</param>
        /// <param name="rowUpdatingHandler">The handler to be called when a row is updating</param>
        /// <param name="rowUpdatedHandler">The handler to be called when a row is updated</param>
        protected override void AddUpdateEventHandlers(IDbDataAdapter dataAdapter, RowUpdatingHandler rowUpdatingHandler, RowUpdatedHandler rowUpdatedHandler)
        {
            if (rowUpdatingHandler != null)
            {
                this.m_rowUpdating = rowUpdatingHandler;
                ((SqlDataAdapter)dataAdapter).RowUpdating += new SqlRowUpdatingEventHandler(RowUpdating);
            }

            if (rowUpdatedHandler != null)
            {
                this.m_rowUpdated = rowUpdatedHandler;
                ((SqlDataAdapter)dataAdapter).RowUpdated += new SqlRowUpdatedEventHandler(RowUpdated);
            }
        }

        /// <summary>
        /// Handles the RowUpdating event
        /// </summary>
        /// <param name="obj">The object that published the event</param>
        /// <param name="e">The SqlRowUpdatingEventArgs</param>
        protected void RowUpdating(object obj, SqlRowUpdatingEventArgs e)
        {
            base.RowUpdating(obj, e);
        }

        /// <summary>
        /// Handles the RowUpdated event
        /// </summary>
        /// <param name="obj">The object that published the event</param>
        /// <param name="e">The SqlRowUpdatedEventArgs</param>
        protected void RowUpdated(object obj, SqlRowUpdatedEventArgs e)
        {
            base.RowUpdated(obj, e);
        }

        /// <summary>
        /// Handle any provider-specific issues with BLOBs here by "washing" the IDataParameter and returning a new one that is set up appropriately for the provider.
        /// </summary>
        /// <param name="connection">The IDbConnection to use in cleansing the parameter</param>
        /// <param name="p">The parameter before cleansing</param>
        /// <returns>The parameter after it's been cleansed.</returns>
        protected override IDataParameter GetBlobParameter(IDbConnection connection, IDataParameter p)
        {
            // do nothing special for BLOBs...as far as we know now.
            return p;
        }

#endregion
    }

#region Derive Parameters

    // We create our own class to do this because the existing ADO.NET 1.1 implementation is broken.
    internal class SqlDeriveParameters
    {
        internal static void DeriveParameters(SqlCommand cmd)
        {
            string cmdText;
            SqlCommand newCommand;
            SqlDataReader reader;
            ArrayList parameterList;
            SqlParameter sqlParam;
            CommandType cmdType;
            string procedureSchema;
            string procedureName;
            int groupNumber;
            SqlTransaction trnSql = cmd.Transaction;

            cmdType = cmd.CommandType;

            if ((cmdType == CommandType.Text))
            {
                throw new InvalidOperationException();
            }
            else if ((cmdType == CommandType.TableDirect))
            {
                throw new InvalidOperationException();
            }
            else if ((cmdType != CommandType.StoredProcedure))
            {
                throw new InvalidOperationException();
            }

            procedureName = cmd.CommandText;
            string server = null;
            string database = null;
            procedureSchema = null;

            // split out the procedure name to get the server, database, etc.
            GetProcedureTokens(ref procedureName, ref server, ref database, ref procedureSchema);

            // look for group numbers
            groupNumber = ParseGroupNumber(ref procedureName);

            newCommand = null;

            // set up the command string.  We use sp_procuedure_params_rowset to get the parameters
            if (database != null)
            {
                cmdText = string.Concat("[", database, "]..sp_procedure_params_rowset");
                if (server != null)
                {
                    cmdText = string.Concat(server, ".", cmdText);
                }

                // be careful of transactions
                if (trnSql != null)
                {
                    newCommand = new SqlCommand(cmdText, cmd.Connection, trnSql);
                }
                else
                {
                    newCommand = new SqlCommand(cmdText, cmd.Connection);
                }
            }
            else
            {
                // be careful of transactions
                if (trnSql != null)
                {
                    newCommand = new SqlCommand("sp_procedure_params_rowset", cmd.Connection, trnSql);
                }
                else
                {
                    newCommand = new SqlCommand("sp_procedure_params_rowset", cmd.Connection);
                }
            }

            newCommand.CommandType = CommandType.StoredProcedure;
            newCommand.Parameters.Add(new SqlParameter("@procedure_name", SqlDbType.NVarChar, 255));
            newCommand.Parameters[0].Value = procedureName;

            // make sure we specify 
            if (!IsEmptyString(procedureSchema))
            {
                newCommand.Parameters.Add(new SqlParameter("@procedure_schema", SqlDbType.NVarChar, 255));
                newCommand.Parameters[1].Value = procedureSchema;
            }

            // make sure we specify the groupNumber if we were given one
            if (groupNumber != 0)
            {
                newCommand.Parameters.Add(new SqlParameter("@group_number", groupNumber));
            }

            reader = null;
            parameterList = new ArrayList();

            try
            {
                // get a reader full of our params
                reader = newCommand.ExecuteReader();
                sqlParam = null;

                while (reader.Read())
                {
                    // get all the parameter properties that we can get, Name, type, length, direction, precision
                    sqlParam = new SqlParameter();
                    sqlParam.ParameterName = (string)(reader["PARAMETER_NAME"]);
                    sqlParam.SqlDbType = GetSqlDbType((short)(reader["DATA_TYPE"]), (string)(reader["TYPE_NAME"]));

                    if (reader["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value)
                    {
                        sqlParam.Size = (int)(reader["CHARACTER_MAXIMUM_LENGTH"]);
                    }

                    sqlParam.Direction = GetParameterDirection((short)(reader["PARAMETER_TYPE"]));

                    if ((sqlParam.SqlDbType == SqlDbType.Decimal))
                    {
                        sqlParam.Scale = (byte)(((short)(reader["NUMERIC_SCALE"]) & 255));
                        sqlParam.Precision = (byte)(((short)(reader["NUMERIC_PRECISION"]) & 255));
                    }
                    parameterList.Add(sqlParam);
                }
            }
            finally
            {
                // close our reader and connection when we're done
                if (reader != null)
                {
                    reader.Close();
                }
                newCommand.Connection = null;
            }

            // we didn't get any parameters
            if ((parameterList.Count == 0))
            {
                throw new InvalidOperationException();
            }

            cmd.Parameters.Clear();

            // add the parameters to the command object

            foreach (object parameter in parameterList)
            {
                cmd.Parameters.Add(parameter);
            }
        }

        /// <summary>
        /// Checks to see if the stored procedure being called is part of a group, then gets the group number if necessary
        /// </summary>
        /// <param name="procedure">Stored procedure being called.  This method may change this parameter by removing the group number if it exists.</param>
        /// <returns>the group number</returns>
        private static int ParseGroupNumber(ref string procedure)
        {
            string newProcName;
            int groupPos = procedure.IndexOf(';');
            int groupIndex = 0;

            if (groupPos > 0)
            {
                newProcName = procedure.Substring(0, groupPos);
                try
                {
                    groupIndex = int.Parse(procedure.Substring(groupPos + 1));
                }
                catch
                {
                    throw new InvalidOperationException();
                }
            }
            else
            {
                newProcName = procedure;
                groupIndex = 0;
            }

            procedure = newProcName;
            return groupIndex;
        }

        /// <summary>
        /// Tokenize the procedure string
        /// </summary>
        /// <param name="procedure">The procedure name</param>
        /// <param name="server">The server name</param>
        /// <param name="database">The database name</param>
        /// <param name="owner">The owner name</param>
        private static void GetProcedureTokens(ref string procedure, ref string server, ref string database, ref string owner)
        {
            string[] spNameTokens;
            int arrIndex;
            int nextPos;
            int currPos;
            int tokenCount;

            server = null;
            database = null;
            owner = null;

            spNameTokens = new string[4];

            if (!IsEmptyString(procedure))
            {
                arrIndex = 0;
                nextPos = 0;
                currPos = 0;

                while ((arrIndex < 4))
                {
                    currPos = procedure.IndexOf('.', nextPos);
                    if ((-1 == currPos))
                    {
                        spNameTokens[arrIndex] = procedure.Substring(nextPos);
                        break;
                    }
                    spNameTokens[arrIndex] = procedure.Substring(nextPos, (currPos - nextPos));
                    nextPos = (currPos + 1);
                    if ((procedure.Length <= nextPos))
                    {
                        break;
                    }
                    arrIndex = (arrIndex + 1);
                }

                tokenCount = arrIndex + 1;

                // based on how many '.' we found, we know what tokens we found
                switch (tokenCount)
                {
                    case 1:
                        procedure = spNameTokens[0];
                        break;
                    case 2:
                        procedure = spNameTokens[1];
                        owner = spNameTokens[0];
                        break;
                    case 3:
                        procedure = spNameTokens[2];
                        owner = spNameTokens[1];
                        database = spNameTokens[0];
                        break;
                    case 4:
                        procedure = spNameTokens[3];
                        owner = spNameTokens[2];
                        database = spNameTokens[1];
                        server = spNameTokens[0];
                        break;
                }
            }
        }

        /// <summary>
        /// Checks for an empty string
        /// </summary>
        /// <param name="str">String to check</param>
        /// <returns>boolean value indicating whether string is empty</returns>
        private static bool IsEmptyString(string str)
        {
            if (str != null)
            {
                return (0 == str.Length);
            }
            return true;
        }

        /// <summary>
        /// Convert OleDbType to SQlDbType
        /// </summary>
        /// <param name="paramType">The OleDbType to convert</param>
        /// <param name="typeName">The typeName to convert for items such as Money and SmallMoney which both map to OleDbType.Currency</param>
        /// <returns>The converted SqlDbType</returns>
        private static SqlDbType GetSqlDbType(short paramType, string typeName)
        {
            SqlDbType cmdType;
            OleDbType oleDbType;
            cmdType = SqlDbType.Variant;
            oleDbType = (OleDbType)(paramType);

            switch (oleDbType)
            {
                case OleDbType.SmallInt:
                    cmdType = SqlDbType.SmallInt;
                    break;
                case OleDbType.Integer:
                    cmdType = SqlDbType.Int;
                    break;
                case OleDbType.Single:
                    cmdType = SqlDbType.Real;
                    break;
                case OleDbType.Double:
                    cmdType = SqlDbType.Float;
                    break;
                case OleDbType.Currency:
                    cmdType = (typeName == "money") ? SqlDbType.Money : SqlDbType.SmallMoney;
                    break;
                case OleDbType.Date:
                    cmdType = (typeName == "datetime") ? SqlDbType.DateTime : SqlDbType.SmallDateTime;
                    break;
                case OleDbType.BSTR:
                    cmdType = (typeName == "nchar") ? SqlDbType.NChar : SqlDbType.NVarChar;
                    break;
                case OleDbType.Boolean:
                    cmdType = SqlDbType.Bit;
                    break;
                case OleDbType.Variant:
                    cmdType = SqlDbType.Variant;
                    break;
                case OleDbType.Decimal:
                    cmdType = SqlDbType.Decimal;
                    break;
                case OleDbType.TinyInt:
                    cmdType = SqlDbType.TinyInt;
                    break;
                case OleDbType.UnsignedTinyInt:
                    cmdType = SqlDbType.TinyInt;
                    break;
                case OleDbType.UnsignedSmallInt:
                    cmdType = SqlDbType.SmallInt;
                    break;
                case OleDbType.BigInt:
                    cmdType = SqlDbType.BigInt;
                    break;
                case OleDbType.Filetime:
                    cmdType = (typeName == "datetime") ? SqlDbType.DateTime : SqlDbType.SmallDateTime;
                    break;
                case OleDbType.Guid:
                    cmdType = SqlDbType.UniqueIdentifier;
                    break;
                case OleDbType.Binary:
                    cmdType = (typeName == "binary") ? SqlDbType.Binary : SqlDbType.VarBinary;
                    break;
                case OleDbType.Char:
                    cmdType = (typeName == "char") ? SqlDbType.Char : SqlDbType.VarChar;
                    break;
                case OleDbType.WChar:
                    cmdType = (typeName == "nchar") ? SqlDbType.NChar : SqlDbType.NVarChar;
                    break;
                case OleDbType.Numeric:
                    cmdType = SqlDbType.Decimal;
                    break;
                case OleDbType.DBDate:
                    cmdType = (typeName == "datetime") ? SqlDbType.DateTime : SqlDbType.SmallDateTime;
                    break;
                case OleDbType.DBTime:
                    cmdType = (typeName == "datetime") ? SqlDbType.DateTime : SqlDbType.SmallDateTime;
                    break;
                case OleDbType.DBTimeStamp:
                    cmdType = (typeName == "datetime") ? SqlDbType.DateTime : SqlDbType.SmallDateTime;
                    break;
                case OleDbType.VarChar:
                    cmdType = (typeName == "char") ? SqlDbType.Char : SqlDbType.VarChar;
                    break;
                case OleDbType.LongVarChar:
                    cmdType = SqlDbType.Text;
                    break;
                case OleDbType.VarWChar:
                    cmdType = (typeName == "nchar") ? SqlDbType.NChar : SqlDbType.NVarChar;
                    break;
                case OleDbType.LongVarWChar:
                    cmdType = SqlDbType.NText;
                    break;
                case OleDbType.VarBinary:
                    cmdType = (typeName == "binary") ? SqlDbType.Binary : SqlDbType.VarBinary;
                    break;
                case OleDbType.LongVarBinary:
                    cmdType = SqlDbType.Image;
                    break;
            }
            return cmdType;
        }

        /// <summary>
        /// Converts the OleDb parameter direction
        /// </summary>
        /// <param name="oledbDirection">The integer parameter direction</param>
        /// <returns>A ParameterDirection</returns>
        private static ParameterDirection GetParameterDirection(short oledbDirection)
        {
            ParameterDirection pd;
            switch (oledbDirection)
            {
                case 1:
                    pd = ParameterDirection.Input;
                    break;
                case 2: //或者干脆注释掉 case 2 的全部
                    pd = ParameterDirection.Output; //是这里的问题
                    goto default; //我加的这句话
                                  //break; //我注释掉的这句话
                case 4:
                    pd = ParameterDirection.ReturnValue;
                    break;
                default:
                    pd = ParameterDirection.InputOutput;
                    break;
            }
            return pd;
        }
    }

#endregion

    /// <summary>
    /// The SqlHelper class is intended to encapsulate high performance, scalable best practices for 
    /// common uses of SqlClient
    /// </summary>
    public sealed class SqlHelper
    {
        /// <summary>
        /// Calls the SqlCommandBuilder.DeriveParameters, doing any setup and cleanup necessary
        /// </summary>
        /// <param name="cmd">The SqlCommand referencing the stored procedure from which the parameter information is to be derived. The derived parameters are added to the Parameters collection of the SqlCommand. </param>
        public static void DeriveParameters(SqlCommand cmd)
        {
            new SqlServer().DeriveParameters(cmd);
        }

#region Private constructor

        // Since this class provides only static methods, make the default constructor private to prevent 
        // instances from being created with "new SqlHelper()"
        private SqlHelper()
        {
        }

#endregion Private constructor

#region GetParameter

        /// <summary>
        /// Get a SqlParameter for use in a SQL command
        /// </summary>
        /// <param name="name">The name of the parameter to create</param>
        /// <param name="value">The value of the specified parameter</param>
        /// <returns>A SqlParameter object</returns>
        public static SqlParameter GetParameter(string name, object value)
        {
            return (SqlParameter)(new SqlServer().GetParameter(name, value));
        }

        /// <summary>
        /// Get a SqlParameter for use in a SQL command
        /// </summary>
        /// <param name="name">The name of the parameter to create</param>
        /// <param name="dbType">The System.Data.DbType of the parameter</param>
        /// <param name="size">The size of the parameter</param>
        /// <param name="direction">The System.Data.ParameterDirection of the parameter</param>
        /// <returns>A SqlParameter object</returns>
        public static SqlParameter GetParameter(string name, DbType dbType, int size, ParameterDirection direction)
        {
            return (SqlParameter)(new SqlServer().GetParameter(name, dbType, size, direction));
        }

        /// <summary>
        /// Get a SqlParameter for use in a SQL command
        /// </summary>
        /// <param name="name">The name of the parameter to create</param>
        /// <param name="dbType">The System.Data.DbType of the parameter</param>
        /// <param name="size">The size of the parameter</param>
        /// <param name="sourceColumn">The source column of the parameter</param>
        /// <param name="sourceVersion">The System.Data.DataRowVersion of the parameter</param>
        /// <returns>A SqlParameter object</returns>
        public static SqlParameter GetParameter(string name, DbType dbType, int size, string sourceColumn, DataRowVersion sourceVersion)
        {
            return (SqlParameter)new SqlServer().GetParameter(name, dbType, size, sourceColumn, sourceVersion);
        }

#endregion

#region ExecuteNonQuery

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against the database specified in 
        /// the connection string
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(command);
        /// </remarks>
        /// <param name="command">The SqlCommand to execute</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlCommand command)
        {
            return new SqlServer().ExecuteNonQuery(command);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset and takes no parameters) against the database specified in 
        /// the connection string
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            return new SqlServer().ExecuteNonQuery(connectionString, commandType, commandText);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against the database specified in the connection string 
        /// using the provided parameters
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            return new SqlServer().ExecuteNonQuery(connectionString, commandType, commandText, commandParameters);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored prcedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            return new SqlServer().ExecuteNonQuery(connectionString, spName, parameterValues);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset and takes no parameters) against the provided SqlConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText)
        {
            return new SqlServer().ExecuteNonQuery(connection, commandType, commandText);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against the specified SqlConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            return new SqlServer().ExecuteNonQuery(connection, commandType, commandText, commandParameters);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified SqlConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlConnection connection, string spName, params object[] parameterValues)
        {
            return new SqlServer().ExecuteNonQuery(connection, spName, parameterValues);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset and takes no parameters) against the provided SqlTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return new SqlServer().ExecuteNonQuery(transaction, commandType, commandText);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against the specified SqlTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            return new SqlServer().ExecuteNonQuery(transaction, commandType, commandText, commandParameters);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified 
        /// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, trans, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            return new SqlServer().ExecuteNonQuery(transaction, spName, parameterValues);
        }

#endregion ExecuteNonQuery

#region ExecuteDataset

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(command);
        /// </remarks>
        /// <param name="command">The SqlCommand to execute</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(SqlCommand command)
        {
            return new SqlServer().ExecuteDataset(command);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            return new SqlServer().ExecuteDataset(connectionString, commandType, commandText);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            return new SqlServer().ExecuteDataset(connectionString, commandType, commandText, commandParameters);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        {
            return new SqlServer().ExecuteDataset(connectionString, spName, parameterValues);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText)
        {
            return new SqlServer().ExecuteDataset(connection, commandType, commandText);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            return new SqlServer().ExecuteDataset(connection, commandType, commandText, commandParameters);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(SqlConnection connection, string spName, params object[] parameterValues)
        {
            return new SqlServer().ExecuteDataset(connection, spName, parameterValues);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return new SqlServer().ExecuteDataset(transaction, commandType, commandText);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            return new SqlServer().ExecuteDataset(transaction, commandType, commandText, commandParameters);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified 
        /// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            return new SqlServer().ExecuteDataset(transaction, spName, parameterValues);
        }

#endregion ExecuteDataset

#region ExecuteReader

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlDataReader dr = ExecuteReader(command);
        /// </remarks>
        /// <param name="command">The SqlCommand to execute</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReader(SqlCommand command)
        {
            return new SqlServer().ExecuteReader(command) as SqlDataReader;
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            return new SqlServer().ExecuteReader(connectionString, commandType, commandText) as SqlDataReader;
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            return new SqlServer().ExecuteReader(connectionString, commandType, commandText, commandParameters) as SqlDataReader;
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  SqlDataReader dr = ExecuteReader(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        {
            return new SqlServer().ExecuteReader(connectionString, spName, parameterValues) as SqlDataReader;
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReader(SqlConnection connection, CommandType commandType, string commandText)
        {
            return new SqlServer().ExecuteReader(connection, commandType, commandText) as SqlDataReader;
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReader(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            return new SqlServer().ExecuteReader(connection, commandType, commandText, commandParameters) as SqlDataReader;
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  SqlDataReader dr = ExecuteReader(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReader(SqlConnection connection, string spName, params object[] parameterValues)
        {
            return new SqlServer().ExecuteReader(connection, spName, parameterValues) as SqlDataReader;
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReader(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return new SqlServer().ExecuteReader(transaction, commandType, commandText) as SqlDataReader;
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///   SqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReader(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            return new SqlServer().ExecuteReader(transaction, commandType, commandText, commandParameters) as SqlDataReader;
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified
        /// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  SqlDataReader dr = ExecuteReader(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReader(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            return new SqlServer().ExecuteReader(transaction, spName, parameterValues) as SqlDataReader;
        }

#endregion ExecuteReader

#region ExecuteScalar

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(command);
        /// </remarks>
        /// <param name="command">The SqlCommand to execute</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(SqlCommand command)
        {
            // Pass through the call providing null for the set of SqlParameters
            return new SqlServer().ExecuteScalar(command);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return new SqlServer().ExecuteScalar(connectionString, commandType, commandText);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            return new SqlServer().ExecuteScalar(connectionString, commandType, commandText, commandParameters);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(connString, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        {
            return new SqlServer().ExecuteScalar(connectionString, spName, parameterValues);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided SqlConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(SqlConnection connection, CommandType commandType, string commandText)
        {
            return new SqlServer().ExecuteScalar(connection, commandType, commandText);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset) against the specified SqlConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            return new SqlServer().ExecuteScalar(connection, commandType, commandText, commandParameters);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified SqlConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(conn, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(SqlConnection connection, string spName, params object[] parameterValues)
        {
            return new SqlServer().ExecuteScalar(connection, spName, parameterValues);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided SqlTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return new SqlServer().ExecuteScalar(transaction, commandType, commandText);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset) against the specified SqlTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            return new SqlServer().ExecuteScalar(transaction, commandType, commandText, commandParameters);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified
        /// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(trans, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            return new SqlServer().ExecuteScalar(transaction, spName, parameterValues);
        }

#endregion ExecuteScalar 

#region ExecuteXmlReader

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the provided SqlConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(SqlCommand command);
        /// </remarks>
        /// <param name="command">The SqlCommand to execute</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public static XmlReader ExecuteXmlReader(SqlCommand command)
        {
            return new SqlServer().ExecuteXmlReader(command);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO"</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public static XmlReader ExecuteXmlReader(SqlConnection connection, CommandType commandType, string commandText)
        {
            return new SqlServer().ExecuteXmlReader(connection, commandType, commandText);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO"</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public static XmlReader ExecuteXmlReader(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            return new SqlServer().ExecuteXmlReader(connection, commandType, commandText, commandParameters);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="spName">The name of the stored procedure using "FOR XML AUTO"</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public static XmlReader ExecuteXmlReader(SqlConnection connection, string spName, params object[] parameterValues)
        {
            return new SqlServer().ExecuteXmlReader(connection, spName, parameterValues);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO"</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return new SqlServer().ExecuteXmlReader(transaction, commandType, commandText);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO"</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            return new SqlServer().ExecuteXmlReader(transaction, commandType, commandText, commandParameters);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified 
        /// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  XmlReader r = ExecuteXmlReader(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            return new SqlServer().ExecuteXmlReader(transaction, spName, parameterValues);
        }

#endregion ExecuteXmlReader

#region FillDataset

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </remarks>
        /// <param name="command">The SqlCommand to execute</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)</param>
        public static void FillDataset(SqlCommand command, DataSet dataSet, string[] tableNames)
        {
            new SqlServer().FillDataset(command, dataSet, tableNames);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)</param>
        public static void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            new SqlServer().FillDataset(connectionString, commandType, commandText, dataSet, tableNames);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        public static void FillDataset(string connectionString, CommandType commandType,
            string commandText, DataSet dataSet, string[] tableNames,
            params SqlParameter[] commandParameters)
        {
            new SqlServer().FillDataset(connectionString, commandType, commandText, dataSet, tableNames, commandParameters);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, 24);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>    
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        public static void FillDataset(string connectionString, string spName,
            DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            new SqlServer().FillDataset(connectionString, spName, dataSet, tableNames, parameterValues);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>    
        public static void FillDataset(SqlConnection connection, CommandType commandType,
            string commandText, DataSet dataSet, string[] tableNames)
        {
            new SqlServer().FillDataset(connection, commandType, commandText, dataSet, tableNames);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        public static void FillDataset(SqlConnection connection, CommandType commandType,
            string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
        {
            new SqlServer().FillDataset(connection, commandType, commandText, dataSet, tableNames, commandParameters);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  FillDataset(conn, "GetOrders", ds, new string[] {"orders"}, 24, 36);
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        public static void FillDataset(SqlConnection connection, string spName,
            DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            new SqlServer().FillDataset(connection, spName, dataSet, tableNames, parameterValues);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        public static void FillDataset(SqlTransaction transaction, CommandType commandType,
            string commandText, DataSet dataSet, string[] tableNames)
        {
            new SqlServer().FillDataset(transaction, commandType, commandText, dataSet, tableNames);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        public static void FillDataset(SqlTransaction transaction, CommandType commandType,
            string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
        {
            new SqlServer().FillDataset(transaction, commandType, commandText, dataSet, tableNames, commandParameters);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified 
        /// SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  FillDataset(trans, "GetOrders", ds, new string[]{"orders"}, 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        public static void FillDataset(SqlTransaction transaction, string spName,
            DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            new SqlServer().FillDataset(transaction, spName, dataSet, tableNames, parameterValues);
        }

#endregion

#region UpdateDataset

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  UpdateDataset(conn, insertCommand, deleteCommand, updateCommand, dataSet, "Order");
        /// </remarks>
        /// <param name="insertCommand">A valid transact-SQL statement or stored procedure to insert new records into the data source</param>
        /// <param name="deleteCommand">A valid transact-SQL statement or stored procedure to delete records from the data source</param>
        /// <param name="updateCommand">A valid transact-SQL statement or stored procedure used to update records in the data source</param>
        /// <param name="dataSet">The DataSet used to update the data source</param>
        /// <param name="tableName">The DataTable used to update the data source.</param>
        public static void UpdateDataset(SqlCommand insertCommand, SqlCommand deleteCommand, SqlCommand updateCommand, DataSet dataSet, string tableName)
        {
            new SqlServer().UpdateDataset(insertCommand, deleteCommand, updateCommand, dataSet, tableName);
        }

        /// <summary> 
        /// Executes the System.Data.SqlClient.SqlCommand for each inserted, updated, or deleted row in the DataSet also implementing RowUpdating and RowUpdated Event Handlers 
        /// </summary> 
        /// <remarks> 
        /// e.g.:  
        /// SqlRowUpdatingEventHandler rowUpdating = new SqlRowUpdatingEventHandler( OnRowUpdating );
        /// SqlRowUpdatedEventHandler rowUpdated = new SqlRowUpdatedEventHandler( OnRowUpdated );
        /// adoHelper.UpdateDataSet(sqlInsertCommand, sqlDeleteCommand, sqlUpdateCommand, dataSet, "Order", rowUpdating, rowUpdated);
        /// </remarks> 
        /// <param name="insertCommand">A valid transact-SQL statement or stored procedure to insert new records into the data source</param> 
        /// <param name="deleteCommand">A valid transact-SQL statement or stored procedure to delete records from the data source</param> 
        /// <param name="updateCommand">A valid transact-SQL statement or stored procedure used to update records in the data source</param> 
        /// <param name="dataSet">The DataSet used to update the data source</param> 
        /// <param name="tableName">The DataTable used to update the data source.</param> 
        /// <param name="rowUpdating">The AdoHelper.RowUpdatingEventHandler or null</param> 
        /// <param name="rowUpdated">The AdoHelper.RowUpdatedEventHandler or null</param> 
        public static void UpdateDataset(IDbCommand insertCommand, IDbCommand deleteCommand, IDbCommand updateCommand,
            DataSet dataSet, string tableName, AdoHelper.RowUpdatingHandler rowUpdating, AdoHelper.RowUpdatedHandler rowUpdated)
        {
            new SqlServer().UpdateDataset(insertCommand, deleteCommand, updateCommand, dataSet, tableName, rowUpdating, rowUpdated);
        }

#endregion

#region CreateCommand

        /// <summary>
        /// Simplify the creation of a Sql command object by allowing
        /// a stored procedure and optional parameters to be provided
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlCommand command = CreateCommand(connenctionString, "AddCustomer", "CustomerID", "CustomerName");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="sourceColumns">An array of string to be assigned as the source columns of the stored procedure parameters</param>
        /// <returns>A valid SqlCommand object</returns>
        public static SqlCommand CreateCommand(string connectionString, string spName, params string[] sourceColumns)
        {
            return new SqlServer().CreateCommand(connectionString, spName, sourceColumns) as SqlCommand;
        }

        /// <summary>
        /// Simplify the creation of a Sql command object by allowing
        /// a stored procedure and optional parameters to be provided
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlCommand command = CreateCommand(conn, "AddCustomer", "CustomerID", "CustomerName");
        /// </remarks>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="sourceColumns">An array of string to be assigned as the source columns of the stored procedure parameters</param>
        /// <returns>A valid SqlCommand object</returns>
        public static SqlCommand CreateCommand(SqlConnection connection, string spName, params string[] sourceColumns)
        {
            return new SqlServer().CreateCommand(connection, spName, sourceColumns) as SqlCommand;
        }

        /// <summary>
        /// Simplify the creation of a Sql command object by allowing
        /// a stored procedure and optional parameters to be provided
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlCommand command = CreateCommand(connenctionString, "AddCustomer", "CustomerID", "CustomerName");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandText">A valid SQL string to execute</param>
        /// <param name="commandType">The CommandType to execute (i.e. StoredProcedure, Text)</param>
        /// <param name="commandParameters">The SqlParameters to pass to the command</param>
        /// <returns>A valid SqlCommand object</returns>
        public static SqlCommand CreateCommand(string connectionString, string commandText, CommandType commandType, params SqlParameter[] commandParameters)
        {
            return new SqlServer().CreateCommand(connectionString, commandText, commandType, commandParameters) as SqlCommand;
        }

        /// <summary>
        /// Simplify the creation of a Sql command object by allowing
        /// a stored procedure and optional parameters to be provided
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlCommand command = CreateCommand(conn, "AddCustomer", "CustomerID", "CustomerName");
        /// </remarks>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="commandText">A valid SQL string to execute</param>
        /// <param name="commandType">The CommandType to execute (i.e. StoredProcedure, Text)</param>
        /// <param name="commandParameters">The SqlParameters to pass to the command</param>
        /// <returns>A valid SqlCommand object</returns>
        public static SqlCommand CreateCommand(SqlConnection connection, string commandText, CommandType commandType, params SqlParameter[] commandParameters)
        {
            return new SqlServer().CreateCommand(connection, commandText, commandType, commandParameters) as SqlCommand;
        }

#endregion

#region ExecuteNonQueryTypedParams

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will assign the parameter values based on row values.
        /// </summary>
        /// <param name="command">The SqlCommand to execute</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQueryTypedParams(SqlCommand command, DataRow dataRow)
        {
            return new SqlServer().ExecuteNonQueryTypedParams(command, dataRow);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQueryTypedParams(String connectionString, String spName, DataRow dataRow)
        {
            return new SqlServer().ExecuteNonQueryTypedParams(connectionString, spName, dataRow);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified SqlConnection 
        /// using the dataRow column values as the stored procedure's parameters values.  
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQueryTypedParams(SqlConnection connection, String spName, DataRow dataRow)
        {
            return new SqlServer().ExecuteNonQueryTypedParams(connection, spName, dataRow);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified
        /// SqlTransaction using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="transaction">A valid SqlTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQueryTypedParams(SqlTransaction transaction, String spName, DataRow dataRow)
        {
            return new SqlServer().ExecuteNonQueryTypedParams(transaction, spName, dataRow);
        }

#endregion

#region ExecuteDatasetTypedParams

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will assign the parameter values based on row values.
        /// </summary>
        /// <param name="command">The SqlCommand to execute</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDatasetTypedParams(SqlCommand command, DataRow dataRow)
        {
            return new SqlServer().ExecuteDatasetTypedParams(command, dataRow);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDatasetTypedParams(string connectionString, String spName, DataRow dataRow)
        {
            return new SqlServer().ExecuteDatasetTypedParams(connectionString, spName, dataRow);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
        /// using the dataRow column values as the store procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDatasetTypedParams(SqlConnection connection, String spName, DataRow dataRow)
        {
            return new SqlServer().ExecuteDatasetTypedParams(connection, spName, dataRow);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction 
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="transaction">A valid SqlTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDatasetTypedParams(SqlTransaction transaction, String spName, DataRow dataRow)
        {
            return new SqlServer().ExecuteDatasetTypedParams(transaction, spName, dataRow);
        }

#endregion

#region ExecuteReaderTypedParams

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will assign the parameter values based on parameter order.
        /// </summary>
        /// <param name="command">The SqlCommand toe execute</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReaderTypedParams(SqlCommand command, DataRow dataRow)
        {
            return new SqlServer().ExecuteReaderTypedParams(command, dataRow) as SqlDataReader;
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReaderTypedParams(String connectionString, String spName, DataRow dataRow)
        {
            return new SqlServer().ExecuteReaderTypedParams(connectionString, spName, dataRow) as SqlDataReader;
        }


        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReaderTypedParams(SqlConnection connection, String spName, DataRow dataRow)
        {
            return new SqlServer().ExecuteReaderTypedParams(connection, spName, dataRow) as SqlDataReader;
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction 
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="transaction">A valid SqlTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public static SqlDataReader ExecuteReaderTypedParams(SqlTransaction transaction, String spName, DataRow dataRow)
        {
            return new SqlServer().ExecuteReaderTypedParams(transaction, spName, dataRow) as SqlDataReader;
        }

#endregion

#region ExecuteScalarTypedParams

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will assign the parameter values based on parameter order.
        /// </summary>
        /// <param name="command">The SqlCommand to execute</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalarTypedParams(SqlCommand command, DataRow dataRow)
        {
            return new SqlServer().ExecuteScalarTypedParams(command, dataRow);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalarTypedParams(String connectionString, String spName, DataRow dataRow)
        {
            return new SqlServer().ExecuteScalarTypedParams(connectionString, spName, dataRow);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified SqlConnection 
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalarTypedParams(SqlConnection connection, String spName, DataRow dataRow)
        {
            return new SqlServer().ExecuteScalarTypedParams(connection, spName, dataRow);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified SqlTransaction
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="transaction">A valid SqlTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalarTypedParams(SqlTransaction transaction, String spName, DataRow dataRow)
        {
            return new SqlServer().ExecuteScalarTypedParams(transaction, spName, dataRow);
        }

#endregion

#region ExecuteXmlReaderTypedParams

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will assign the parameter values based on parameter order.
        /// </summary>
        /// <param name="command">The SqlCommand to execute</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public static XmlReader ExecuteXmlReaderTypedParams(SqlCommand command, DataRow dataRow)
        {
            return new SqlServer().ExecuteXmlReaderTypedParams(command, dataRow);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public static XmlReader ExecuteXmlReaderTypedParams(SqlConnection connection, String spName, DataRow dataRow)
        {
            return new SqlServer().ExecuteXmlReaderTypedParams(connection, spName, dataRow);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction 
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="transaction">A valid SqlTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public static XmlReader ExecuteXmlReaderTypedParams(SqlTransaction transaction, String spName, DataRow dataRow)
        {
            return new SqlServer().ExecuteXmlReaderTypedParams(transaction, spName, dataRow);
        }

#endregion
    }

    /// <summary>
    /// SqlHelperParameterCache provides functions to leverage a static cache of procedure parameters, and the
    /// ability to discover parameters for stored procedures at run-time.
    /// </summary>
    public sealed class SqlHelperParameterCache
    {
#region private constructor

        //Since this class provides only static methods, make the default constructor private to prevent 
        //instances from being created with "new SqlHelperParameterCache()"
        private SqlHelperParameterCache()
        {
        }

#endregion constructor

#region caching functions

        /// <summary>
        /// Add parameter array to the cache
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters to be cached</param>
        public static void CacheParameterSet(string connectionString, string commandText, params SqlParameter[] commandParameters)
        {
            new SqlServer().CacheParameterSet(connectionString, commandText, commandParameters);
        }

        /// <summary>
        /// Retrieve a parameter array from the cache
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An array of SqlParamters</returns>
        public static SqlParameter[] GetCachedParameterSet(string connectionString, string commandText)
        {
            ArrayList tempValue = new ArrayList();
            IDataParameter[] sqlP = new SqlServer().GetCachedParameterSet(connectionString, commandText);
            foreach (IDataParameter parameter in sqlP)
            {
                tempValue.Add(parameter);
            }
            return (SqlParameter[])tempValue.ToArray(typeof(SqlParameter));
        }

#endregion caching functions

#region Parameter Discovery Functions

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <returns>An array of SqlParameters</returns>
        public static SqlParameter[] GetSpParameterSet(string connectionString, string spName)
        {
            ArrayList tempValue = new ArrayList();
            foreach (IDataParameter parameter in new SqlServer().GetSpParameterSet(connectionString, spName))
            {
                tempValue.Add(parameter);
            }
            return (SqlParameter[])tempValue.ToArray(typeof(SqlParameter));
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of SqlParameters</returns>
        public static SqlParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        {
            ArrayList tempValue = new ArrayList();
            foreach (IDataParameter parameter in new SqlServer().GetSpParameterSet(connectionString, spName, includeReturnValueParameter))
            {
                tempValue.Add(parameter);
            }
            return (SqlParameter[])tempValue.ToArray(typeof(SqlParameter));
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <returns>An array of SqlParameters</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public static SqlParameter[] GetSpParameterSet(IDbConnection connection, string spName)
        {
            return GetSpParameterSet(connection, spName, false);
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of SqlParameters</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if connection is null</exception>
        public static SqlParameter[] GetSpParameterSet(IDbConnection connection, string spName, bool includeReturnValueParameter)
        {
            ArrayList tempValue = new ArrayList();
            foreach (IDataParameter parameter in new SqlServer().GetSpParameterSet(connection, spName, includeReturnValueParameter))
            {
                tempValue.Add(parameter);
            }
            return (SqlParameter[])tempValue.ToArray(typeof(SqlParameter));
        }

#endregion Parameter Discovery Functions
    }

    /// <summary>
    /// The Odbc class is intended to encapsulate high performance, scalable best practices for 
    /// common uses of the Odbc ADO.NET provider.  It is created using the abstract factory in AdoHelper
    /// </summary>
    public class Odbc : AdoHelper
    {
        // used for correcting Call syntax for stored procedures in ODBC
        private static Regex _regExpr = new Regex(@"\{.*call|CALL\s\w+.*}", RegexOptions.Compiled);

        /// <summary>
        /// Create an Odbc Helper.  Needs to be a default constructor so that the Factory can create it
        /// </summary>
        public Odbc()
        {
        }

#region Overrides

        /// <summary>
        /// Returns an array of OdbcParameters of the specified size
        /// </summary>
        /// <param name="size">size of the array</param>
        /// <returns>The array of OdbcParameters</returns>
        protected override IDataParameter[] GetDataParameters(int size)
        {
            return new OdbcParameter[size];
        }

        /// <summary>
        /// Returns an OdbcConnection object for the given connection string
        /// </summary>
        /// <param name="connectionString">The connection string to be used to create the connection</param>
        /// <returns>An OdbcConnection object</returns>
        public override IDbConnection GetConnection(string connectionString)
        {
            return new OdbcConnection(connectionString);
        }

        /// <summary>
        /// Returns an OdbcDataAdapter object
        /// </summary>
        /// <returns>The OdbcDataAdapter</returns>
        public override IDbDataAdapter GetDataAdapter()
        {
            return new OdbcDataAdapter();
        }

        /// <summary>
        /// Calls the CommandBuilder.DeriveParameters method for the specified provider, doing any setup and cleanup necessary
        /// </summary>
        /// <param name="cmd">The IDbCommand referencing the stored procedure from which the parameter information is to be derived. The derived parameters are added to the Parameters collection of the IDbCommand. </param>
        public override void DeriveParameters(IDbCommand cmd)
        {
            bool mustCloseConnection = false;

            if (!(cmd is OdbcCommand))
                throw new ArgumentException("The command provided is not a OdbcCommand instance.", "cmd");
            if (cmd.Connection.State != ConnectionState.Open)
            {
                cmd.Connection.Open();
                mustCloseConnection = true;
            }

            OdbcCommandBuilder.DeriveParameters((OdbcCommand)cmd);

            if (mustCloseConnection)
            {
                cmd.Connection.Close();
            }
        }

        /// <summary>
        /// Returns an OdbcParameter object
        /// </summary>
        /// <returns>The OdbcParameter object</returns>
        public override IDataParameter GetParameter()
        {
            return new OdbcParameter();
        }

        /// <summary>
        /// This cleans up the parameter syntax for an ODBC call.  This was split out from PrepareCommand so that it could be called independently.
        /// </summary>
        /// <param name="command">An IDbCommand object containing the CommandText to clean.</param>
        public override void CleanParameterSyntax(IDbCommand command)
        {
            string call = " call ";

            if (command.CommandType == CommandType.StoredProcedure)
            {
                if (!_regExpr.Match(command.CommandText).Success && // It does not like like {call sp_name() }
                    command.CommandText.Trim().IndexOf(" ") == -1) // If there's only a stored procedure name
                {
                    // If there's only a stored procedure name
                    StringBuilder par = new StringBuilder();
                    if (command.Parameters.Count != 0)
                    {
                        bool isFirst = true;
                        bool hasParameters = false;

                        for (int i = 0; i < command.Parameters.Count; i++)
                        {
                            OdbcParameter p = command.Parameters[i] as OdbcParameter;
                            if (p.Direction != ParameterDirection.ReturnValue)
                            {
                                if (isFirst)
                                {
                                    isFirst = false;
                                    par.Append("(?");
                                }
                                else
                                {
                                    par.Append(",?");
                                }
                                hasParameters = true;
                            }
                            else
                            {
                                call = " ? = call ";
                            }
                        }
                        if (hasParameters)
                        {
                            par.Append(")");
                        }
                    }
                    command.CommandText = "{" + call + command.CommandText + par.ToString() + " }";
                }
            }
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the provided IDbConnection. 
        /// </summary>
        /// <example>
        /// <code>
        /// XmlReader r = helper.ExecuteXmlReader(command);
        /// </code></example>
        /// <param name="command">The IDbCommand to execute</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public override XmlReader ExecuteXmlReader(IDbCommand command)
        {
            bool mustCloseConnection = false;

            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
                mustCloseConnection = true;
            }

            CleanParameterSyntax(command);

            OdbcDataAdapter da = new OdbcDataAdapter((OdbcCommand)command);
            DataSet ds = new DataSet();

            da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            da.Fill(ds);

            StringReader stream = new StringReader(ds.GetXml());

            if (mustCloseConnection)
            {
                command.Connection.Close();
            }

            return new XmlTextReader(stream);
        }

        /// <summary>
        /// Provider specific code to set up the updating/ed event handlers used by UpdateDataset
        /// </summary>
        /// <param name="dataAdapter">DataAdapter to attach the event handlers to</param>
        /// <param name="rowUpdatingHandler">The handler to be called when a row is updating</param>
        /// <param name="rowUpdatedHandler">The handler to be called when a row is updated</param>
        protected override void AddUpdateEventHandlers(IDbDataAdapter dataAdapter, RowUpdatingHandler rowUpdatingHandler, RowUpdatedHandler rowUpdatedHandler)
        {
            if (rowUpdatingHandler != null)
            {
                this.m_rowUpdating = rowUpdatingHandler;
                ((OdbcDataAdapter)dataAdapter).RowUpdating += new OdbcRowUpdatingEventHandler(RowUpdating);
            }

            if (rowUpdatedHandler != null)
            {
                this.m_rowUpdated = rowUpdatedHandler;
                ((OdbcDataAdapter)dataAdapter).RowUpdated += new OdbcRowUpdatedEventHandler(RowUpdated);
            }
        }

        /// <summary>
        /// Handles the RowUpdating event
        /// </summary>
        /// <param name="obj">The object that published the event</param>
        /// <param name="e">The OdbcRowUpdatingEventArgs</param>
        protected void RowUpdating(object obj, OdbcRowUpdatingEventArgs e)
        {
            base.RowUpdating(obj, e);
        }

        /// <summary>
        /// Handles the RowUpdated event
        /// </summary>
        /// <param name="obj">The object that published the event</param>
        /// <param name="e">The OdbcRowUpdatedEventArgs</param>
        protected void RowUpdated(object obj, OdbcRowUpdatedEventArgs e)
        {
            base.RowUpdated(obj, e);
        }

        /// <summary>
        /// Handle any provider-specific issues with BLOBs here by "washing" the IDataParameter and returning a new one that is set up appropriately for the provider.
        /// </summary>
        /// <param name="connection">The IDbConnection to use in cleansing the parameter</param>
        /// <param name="p">The parameter before cleansing</param>
        /// <returns>The parameter after it's been cleansed.</returns>
        protected override IDataParameter GetBlobParameter(IDbConnection connection, IDataParameter p)
        {
            // nothing special needed for ODBC...so far as we know now.
            return p;
        }

#endregion
    }

    /// <summary>
    /// The OleDb class is intended to encapsulate high performance, scalable best practices for 
    /// common uses of the OleDb ADO.NET provider.  It is created using the abstract factory in AdoHelper
    /// </summary>
    public class OleDb : AdoHelper
    {
        /// <summary>
        /// Create an OleDb Helper.  Needs to be a default constructor so that the Factory can create it
        /// </summary>
        public OleDb()
        {
        }

#region Overrides

        /// <summary>
        /// Returns an array of OleDbParameters of the specified size
        /// </summary>
        /// <param name="size">size of the array</param>
        /// <returns>The array of OdbcParameters</returns>
        protected override IDataParameter[] GetDataParameters(int size)
        {
            return new OleDbParameter[size];
        }

        /// <summary>
        /// Returns an OleDbConnection object for the given connection string
        /// </summary>
        /// <param name="connectionString">The connection string to be used to create the connection</param>
        /// <returns>An OleDbConnection object</returns>
        public override IDbConnection GetConnection(string connectionString)
        {
            return new OleDbConnection(connectionString);
        }

        /// <summary>
        /// Returns an OleDbDataAdapter object
        /// </summary>
        /// <returns>The OleDbDataAdapter</returns>
        public override IDbDataAdapter GetDataAdapter()
        {
            return new OleDbDataAdapter();
        }

        /// <summary>
        /// Calls the CommandBuilder.DeriveParameters method for the specified provider, doing any setup and cleanup necessary
        /// </summary>
        /// <param name="cmd">The IDbCommand referencing the stored procedure from which the parameter information is to be derived. The derived parameters are added to the Parameters collection of the IDbCommand. </param>
        public override void DeriveParameters(IDbCommand cmd)
        {
            bool mustCloseConnection = false;

            if (!(cmd is OleDbCommand))
                throw new ArgumentException("The command provided is not a OleDbCommand instance.", "cmd");

            if (cmd.Connection.State != ConnectionState.Open)
            {
                cmd.Connection.Open();
                mustCloseConnection = true;
            }

            OleDbCommandBuilder.DeriveParameters((OleDbCommand)cmd);

            if (mustCloseConnection)
            {
                cmd.Connection.Close();
            }
        }

        /// <summary>
        /// Returns an OleDbParameter object
        /// </summary>
        /// <returns>The OleDbParameter object</returns>
        public override IDataParameter GetParameter()
        {
            return new OleDbParameter();
        }

        /// <summary>
        /// This cleans up the parameter syntax for an OleDb call.  This was split out from PrepareCommand so that it could be called independently.
        /// </summary>
        /// <param name="command">An IDbCommand object containing the CommandText to clean.</param>
        public override void CleanParameterSyntax(IDbCommand command)
        {
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the provided IDbConnection. 
        /// </summary>
        /// <example>
        /// <code>
        /// XmlReader r = helper.ExecuteXmlReader(command);
        /// </code></example>
        /// <param name="command">The IDbCommand to execute</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public override XmlReader ExecuteXmlReader(IDbCommand command)
        {
            bool mustCloseConnection = false;

            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
                mustCloseConnection = true;
            }

            CleanParameterSyntax(command);

            OleDbDataAdapter da = new OleDbDataAdapter((OleDbCommand)command);
            DataSet ds = new DataSet();

            da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            da.Fill(ds);

            StringReader stream = new StringReader(ds.GetXml());
            if (mustCloseConnection)
            {
                command.Connection.Close();
            }

            return new XmlTextReader(stream);
        }

        /// <summary>
        /// Provider specific code to set up the updating/ed event handlers used by UpdateDataset
        /// </summary>
        /// <param name="dataAdapter">DataAdapter to attach the event handlers to</param>
        /// <param name="rowUpdatingHandler">The handler to be called when a row is updating</param>
        /// <param name="rowUpdatedHandler">The handler to be called when a row is updated</param>
        protected override void AddUpdateEventHandlers(IDbDataAdapter dataAdapter, RowUpdatingHandler rowUpdatingHandler, RowUpdatedHandler rowUpdatedHandler)
        {
            if (rowUpdatingHandler != null)
            {
                this.m_rowUpdating = rowUpdatingHandler;
                ((OleDbDataAdapter)dataAdapter).RowUpdating += new OleDbRowUpdatingEventHandler(RowUpdating);
            }

            if (rowUpdatedHandler != null)
            {
                this.m_rowUpdated = rowUpdatedHandler;
                ((OleDbDataAdapter)dataAdapter).RowUpdated += new OleDbRowUpdatedEventHandler(RowUpdated);
            }
        }

        /// <summary>
        /// Handles the RowUpdating event
        /// </summary>
        /// <param name="obj">The object that published the event</param>
        /// <param name="e">The OleDbRowUpdatingEventArgs</param>
        protected void RowUpdating(object obj, OleDbRowUpdatingEventArgs e)
        {
            base.RowUpdating(obj, e);
        }

        /// <summary>
        /// Handles the RowUpdated event
        /// </summary>
        /// <param name="obj">The object that published the event</param>
        /// <param name="e">The OleDbRowUpdatedEventArgs</param>
        protected void RowUpdated(object obj, OleDbRowUpdatedEventArgs e)
        {
            base.RowUpdated(obj, e);
        }

        /// <summary>
        /// Handle any provider-specific issues with BLOBs here by "washing" the IDataParameter and returning a new one that is set up appropriately for the provider.
        /// </summary>
        /// <param name="connection">The IDbConnection to use in cleansing the parameter</param>
        /// <param name="p">The parameter before cleansing</param>
        /// <returns>The parameter after it's been cleansed.</returns>
        protected override IDataParameter GetBlobParameter(IDbConnection connection, IDataParameter p)
        {
            // nothing special needed for OleDb...as far as we know now
            return p;
        }

#endregion
    }

    /// <summary>
    /// The Oracle class is intended to encapsulate high performance, scalable best practices for 
    /// common uses of the Oracle ADO.NET provider.  It is created using the abstract factory in AdoHelper.
    /// </summary>
    public class Oracle : AdoHelper
    {
        /// <summary>
        /// Create an Oracle Helper.  Needs to be a default constructor so that the Factory can create it
        /// </summary>
        public Oracle()
        {
        }

#region Overrides

        /// <summary>
        /// Returns an array of OracleParameters of the specified size
        /// </summary>
        /// <param name="size">size of the array</param>
        /// <returns>The array of OracleParameters</returns>
        protected override IDataParameter[] GetDataParameters(int size)
        {
            return new OracleParameter[size];
        }

        /// <summary>
        /// Returns an OracleConnection object for the given connection string
        /// </summary>
        /// <param name="connectionString">The connection string to be used to create the connection</param>
        /// <returns>An OracleConnection object</returns>
        public override IDbConnection GetConnection(string connectionString)
        {
            return new OracleConnection(connectionString);
        }

        /// <summary>
        /// Returns an OracleDataAdapter object
        /// </summary>
        /// <returns>The OracleDataAdapter</returns>
        public override IDbDataAdapter GetDataAdapter()
        {
            return new OracleDataAdapter();
        }

        /// <summary>
        /// Calls the CommandBuilder.DeriveParameters method for the specified provider, doing any setup and cleanup necessary
        /// </summary>
        /// <param name="cmd">The IDbCommand referencing the stored procedure from which the parameter information is to be derived. The derived parameters are added to the Parameters collection of the IDbCommand. </param>
        public override void DeriveParameters(IDbCommand cmd)
        {
            bool mustCloseConnection = false;

            if (!(cmd is OracleCommand))
                throw new ArgumentException("The command provided is not an OracleCommand instance.", "cmd");

            if (cmd.Connection.State != ConnectionState.Open)
            {
                cmd.Connection.Open();
                mustCloseConnection = true;
            }

            OracleCommandBuilder.DeriveParameters((OracleCommand)cmd);

            if (mustCloseConnection)
            {
                cmd.Connection.Close();
            }
        }

        /// <summary>
        /// Returns an OracleParameter object
        /// </summary>
        /// <returns>The OracleParameter object</returns>
        public override IDataParameter GetParameter()
        {
            OracleParameter parameter = new OracleParameter();
            parameter.Size = 255;
            return parameter;
        }

        /// <summary>
        /// Get an IDataParameter for use in a SQL command
        /// </summary>
        /// <param name="parameterName">The name of the parameter to create</param>
        /// <param name="value">The value of the specified parameter</param>
        /// <returns>An IDataParameter object</returns>
        public override IDataParameter GetParameter(string parameterName, object value)
        {
            OracleParameter parameter = new OracleParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = value;
            parameter.Size = GetParameterSize(parameterName);
            return parameter;
        }

        /// <summary> 
        /// This function will get and assemble the parameter's size dynamically from db or cache 
        /// </summary> 
        /// <param name="name">The parameter name</param> 
        /// <returns>The size</returns> 
        private int GetParameterSize(string name)
        {
            int Size = 255;
            return Size;
        }

        /// <summary>
        /// This cleans up the parameter syntax for an Oracle call.  This was split out from PrepareCommand so that it could be called independently.
        /// </summary>
        /// <param name="command">An IDbCommand object containing the CommandText to clean.</param>
        public override void CleanParameterSyntax(IDbCommand command)
        {
            if (command.CommandType == CommandType.Text)
            {
                command.CommandText = command.CommandText.Replace("@", " :");
            }

            if (command.Parameters.Count > 0)
            {
                foreach (OracleParameter parameter in command.Parameters)
                {
                    parameter.ParameterName = parameter.ParameterName.Replace("@", " :");
                }
            }
        }

        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the provided IDbConnection. 
        /// </summary>
        /// <example>
        /// <code>
        /// XmlReader r = helper.ExecuteXmlReader(command);
        /// </code></example>
        /// <param name="command">The IDbCommand to execute</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public override XmlReader ExecuteXmlReader(IDbCommand command)
        {
            bool mustCloseConnection = false;

            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
                mustCloseConnection = true;
            }

            CleanParameterSyntax(command);

            OracleDataAdapter da = new OracleDataAdapter((OracleCommand)command);
            DataSet ds = new DataSet();

            da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            da.Fill(ds);

            StringReader stream = new StringReader(ds.GetXml());
            if (mustCloseConnection)
            {
                command.Connection.Close();
            }

            return new XmlTextReader(stream);
        }

        /// <summary>
        /// Provider specific code to set up the updating/ed event handlers used by UpdateDataset
        /// </summary>
        /// <param name="dataAdapter">DataAdapter to attach the event handlers to</param>
        /// <param name="rowUpdatingHandler">The handler to be called when a row is updating</param>
        /// <param name="rowUpdatedHandler">The handler to be called when a row is updated</param>
        protected override void AddUpdateEventHandlers(IDbDataAdapter dataAdapter, RowUpdatingHandler rowUpdatingHandler, RowUpdatedHandler rowUpdatedHandler)
        {
            if (rowUpdatingHandler != null)
            {
                this.m_rowUpdating = rowUpdatingHandler;
                ((OracleDataAdapter)dataAdapter).RowUpdating += new OracleRowUpdatingEventHandler(RowUpdating);
            }

            if (rowUpdatedHandler != null)
            {
                this.m_rowUpdated = rowUpdatedHandler;
                ((OracleDataAdapter)dataAdapter).RowUpdated += new OracleRowUpdatedEventHandler(RowUpdated);
            }
        }

        /// <summary>
        /// Handles the RowUpdating event
        /// </summary>
        /// <param name="obj">The object that published the event</param>
        /// <param name="e">The OracleRowUpdatingEventArgs</param>
        protected void RowUpdating(object obj, OracleRowUpdatingEventArgs e)
        {
            base.RowUpdating(obj, e);
        }

        /// <summary>
        /// Handles the RowUpdated event
        /// </summary>
        /// <param name="obj">The object that published the event</param>
        /// <param name="e">The OracleRowUpdatedEventArgs</param>
        protected void RowUpdated(object obj, OracleRowUpdatedEventArgs e)
        {
            base.RowUpdated(obj, e);
        }

        /// <summary>
        /// Handle any provider-specific issues with BLOBs here by "washing" the IDataParameter and returning a new one that is set up appropriately for the provider.
        /// See MS KnowledgeBase article: http://support.microsoft.com/default.aspx?scid=kb;en-us;322796
        /// </summary>
        /// <param name="connection">The IDbConnection to use in cleansing the parameter</param>
        /// <param name="p">The parameter before cleansing</param>
        /// <returns>The parameter after it's been cleansed.</returns>
        protected override IDataParameter GetBlobParameter(IDbConnection connection, IDataParameter p)
        {
            OracleConnection clonedConnection = (OracleConnection)(((ICloneable)connection).Clone());

            clonedConnection.Open();

            OracleCommand cmd = clonedConnection.CreateCommand();
            cmd.CommandText = "declare xx blob;begin dbms_lob.createtemporary(xx, false, 0);:tempblob := xx;end;";
            cmd.Parameters.Add(new OracleParameter("tempblob", OracleType.Blob)).Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();

            OracleLob tempLob;
            tempLob = (OracleLob)(cmd.Parameters[0].Value);
            tempLob.BeginBatch(OracleLobOpenMode.ReadWrite);
            tempLob.Write((byte[])(p.Value), 0, System.Runtime.InteropServices.Marshal.SizeOf(p.Value));
            tempLob.EndBatch();

            OracleParameter op = new OracleParameter(p.ParameterName, OracleType.Blob);
            op.Value = tempLob;

            clonedConnection.Close();

            return op;
        }

#endregion
    }
}
#endif