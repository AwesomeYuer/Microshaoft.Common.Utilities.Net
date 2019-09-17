Oracle.ManagedDataAccess NuGet Package 18.6.0 README
====================================================

Release Notes: Oracle Data Provider for .NET, Managed Driver

March 2019

This document provides information that supplements the Oracle Data Provider for .NET (ODP.NET) documentation. 
You have downloaded Oracle Data Provider for .NET from Oracle, the license agreement to which is available at 
http://www.oracle.com/technetwork/licenses/distribution-license-152002.html

TABLE OF CONTENTS
*New Features
*Bug Fixes
*Installation and Configuration Steps
*Installation Changes
*Documentation Corrections and Additions
*ODP.NET, Managed Driver Tips, Limitations, and Known Issues

Note: The 32-bit "Oracle Developer Tools for Visual Studio" download from https://otn.oracle.com/dotnet is 
required for Entity Framework design-time features and for other Visual Studio designers such as the 
TableAdapter Wizard. This NuGet download does not enable design-time tools; it only provides run-time support. 
This version of ODP.NET supports Oracle Database version 11.2 and higher.


New Features since Oracle.ManagedDataAccess NuGet Package 18.3.0
================================================================
None


Bug Fixes since Oracle.ManagedDataAccess NuGet Package 18.3.0
==============================================================
Bug 29242017 - PARAMETER IN NVL FUNCTION RESULTS IN READONLY COLUMNS IN DATASET
Bug 29412269 - ODP.NET DRIVER DOES NOT SHARE CURSORS FOR EXECUTIONS WITH NULL / NON-NULL PARAMETER VALUES
Bug 29314539 - ODP.NET ORACLE CONNECTION FAILS IF TABS EXIST BEFORE / AFTER USER ID
Bug 28728040 - ODP.NET MANAGED DRIVER FAILED TO SWITCH OVER TO STANDBY DB IN DATAGUARD ENVIRONMENT
Bug 28632559 - CONNECTION TO STANDBY DB CAUSES ORA-01219 
Bug 29033896 - SYSTEM.IO.IOEXCEPTION IS ENCOUNTERED FOR ONS-BASED HA/RLB REGISTRATIONS
Bug 28747690 - SSL CONNECTIONS RESULTS IN ORA-00542: FAILURE DURING SSL HANDSHAKE
Bug 28085865 - PLSQLASSOCIATIVEARRAY : PRECISION NOT APPLIED TO ALL THE ELEMENTS IN THE ARRAY BIND

Installation and Configuration Steps
====================================
The downloads are NuGet packages that can be installed with the NuGet Package Manager. These instructions apply 
to install ODP.NET, Managed Driver.

1. Un-GAC and un-configure any existing assembly (i.e. Oracle.ManagedDataAccess.dll) and policy DLL 
(i.e. Policy.4.122.Oracle.ManagedDataAccess.dll) for the ODP.NET, Managed Driver, version 4.122.18.3
that exist in the GAC. Remove all references of Oracle.ManagedDataAccess from machine.config file, if any exists.

2. In Visual Studio, open NuGet Package Manager from an existing Visual Studio project. 

3. Install the NuGet package from an OTN-downloaded local package source or from nuget.org.


   From Local Package Source
   -------------------------
   A. Click on the Settings button in the lower left of the dialog box.

   B. Click the "+" button to add a package source. In the Source field, enter in the directory location where the 
   NuGet package(s) were downloaded to. Click the Update button, then the Ok button.

   C. On the left side, under the Online root node, select the package source you just created. The ODP.NET NuGet 
   packages will appear.


   From Nuget.org
   --------------
   A. In the Search box in the upper right, search for the package with id, "Oracle.ManagedDataAccess". Verify 
   that the package uses this unique ID to ensure it is the official Oracle Data Provider for .NET, Managed Driver 
   download.

   B. Select the package you wish to install.


4. Click on the Install button to select the desired NuGet package(s) to include with the project. Accept the 
license agreement and Visual Studio will continue the setup.

5. Open the app/web.config file to configure the ODP.NET connection string and connect descriptors.
Below is an example of configuring the net service aliases and connect descriptors parameters:

  <oracle.manageddataaccess.client>
    <version number="*">
      <dataSources>
        <!-- Customize these connection alias settings to connect to Oracle DB -->
        <dataSource alias="MyDataSource" descriptor="(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ORCL))) " />
      </dataSources>
    </version>
  </oracle.manageddataaccess.client>

After following these instructions, ODP.NET is now configured and ready to use.

IMPORTANT: Oracle recommends configuring net service aliases and connect descriptors in a .NET config file to 
have the application configuration be self-contained rather than using tnsnames.ora or TNS_ADMIN. 

NOTE: ODP.NET, Managed Driver comes with one set of platform specific assemblies for Kerberos support: Oracle.ManagedDataAccessIOP.dll.

The Oracle.ManagedDataAccessIOP.dll assembly is ONLY needed if you are using Kerberos5 based external 
authentication. Kerberos5 users will need to download MIT Kerberos for Windows version 4.0.1 from 
	https://web.mit.edu/kerberos/dist/
to utilize ODP.NET, Managed Driver's support of Kerberos5.

The asssemblies are located under
      packages\Oracle.ManagedDataAccess.<version>\bin\x64
and
      packages\Oracle.ManagedDataAccess.<version>\bin\x86
depending on the platform.

If these assemblies are required by your application, your Visual Studio project requires additional changes.

Use the following steps for your application to use the 64-bit version of Oracle.ManagedDataAccessIOP.dll:

1. Right click on the Visual Studio project.
2. Select Add -> New Folder.
3. Name the folder x64.
4. Right click on the newly created x64 folder.
5. Select Add -> Existing Item.
6. Browse to packages\Oracle.ManagedDataAccess.<version>\bin\x64 under your project solution directory.
7. Choose Oracle.ManagedDataAccessIOP.dll.
8. Click the 'Add' button.
9. Left click the newly added Oracle.ManagedDataAccessIOP.dll in the x64 folder.
10. In the properties window, set 'Copy To Output Directory' to 'Copy Always'.

For x86 targeted applications, name the folder x86 and add assemblies from the 
packages\Oracle.ManagedDataAccess.<version>\bin\x86 folder.

To make your application platform independent even if it depends on Oracle.ManagedDataAccessIOP.dll, create both x64 and x86 folders with the necessary assemblies added to them.


Installation Changes
====================
The following app/web.config entries are added by including the "Official Oracle ODP.NET, Managed Driver" NuGet package 
to your application:

1) Configuration Section Handler

The following entry is added to the app/web.config to enable applications to add an <oracle.manageddataaccess.client> 
section for ODP.NET, Managed Driver-specific configuration:

<configuration>
  <configSections>
    <section name="oracle.manageddataaccess.client" type="OracleInternal.Common.ODPMSectionHandler, Oracle.ManagedDataAccess, Version=4.122.18.3, Culture=neutral, PublicKeyToken=89b483f429c47342" />
  </configSections>
</configuration>

Note: If your application is a web application and the above entry was added to a web.config and the same config 
section handler for "oracle.manageddataaccess.client" also exists in machine.config but the "Version" attribute values 
are different, an error message of "There is a duplicate 'oracle.manageddataaccess.client' section defined." may be 
observed at runtime.  If so, the config section handler entry in the machine.config for 
"oracle.manageddataaccess.client" has to be removed from the machine.config for the web application to not encounter 
this error.  But given that there may be other applications on the machine that depended on this entry in the 
machine.config, this config section handler entry may need to be moved to all of the application's .NET config file on 
that machine that depend on it.

2) DbProviderFactories

The following entry is added for applications that use DbProviderFactories and DbProviderFactory classes. Also, any 
DbProviderFactories entry for "Oracle.ManagedDataAccess.Client" in the machine.config will be ignored with the following 
entry:

<configuration>
  <system.data>
    <DbProviderFactories>
      <remove invariant="Oracle.ManagedDataAccess.Client" />
      <add name="ODP.NET, Managed Driver" invariant="Oracle.ManagedDataAccess.Client" description="Oracle Data Provider for .NET, Managed Driver" type="Oracle.ManagedDataAccess.Client.OracleClientFactory, Oracle.ManagedDataAccess, Version=4.122.18.3, Culture=neutral, PublicKeyToken=89b483f429c47342" />
    </DbProviderFactories>
  </system.data>
</configuration>

3) Dependent Assembly

The following entry is created to ignore policy DLLs for Oracle.ManagedDataAccess.dll and always use the 
Oracle.ManagedDataAccess.dll version that is specified by the newVersion attribute in the <bindingRedirect> element.  
The newVersion attribute corresponds to the Oracle.ManagedDataAccess.dll version which came with the NuGet package 
associated with the application.

<configuration>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <publisherPolicy apply="no" />
        <assemblyIdentity name="Oracle.ManagedDataAccess" publicKeyToken="89b483f429c47342" culture="neutral" />
        <bindingRedirect oldVersion="4.122.0.0 - 4.65535.65535.65535" newVersion="4.122.18.3" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>

4) Data Sources

The following entry is added to provide a template on how a data source can be configured in the app/web.config. 
Simply rename "MyDataSource" to an alias of your liking and modify the PROTOCOL, HOST, PORT, SERVICE_NAME as required 
and un-comment the <dataSource> element. Once that is done, the alias can be used as the "data source" attribute in 
your connection string when connecting to an Oracle Database through ODP.NET, Managed Driver.

<configuration>
  <oracle.manageddataaccess.client>
    <version number="*">
      <dataSources>
        <dataSource alias="SampleDataSource" descriptor="(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ORCL))) " />
      </dataSources>
    </version>
  </oracle.manageddataaccess.client>
</configuration>


Documentation Corrections and Additions
=======================================
None


ODP.NET, Managed Driver Tips, Limitations, and Known Issues
===========================================================
1) ODP.NET 12c and 18c PL/SQL CHAR Binding Error "ORA-12899: Value Too Large for Column" When Inserting into a Table

Issue:
After patching managed Oracle Data Provider for .NET (ODP.NET) 12.x / 18.x or ODP.NET Core version 18.x, customers may encounter the "ORA-12899: Value Too Large for Column" error when binding CHAR parameters in PL/SQL which in turn inserts that value to a character based column in a database, that uses a multi-byte character set, such as AL32UTF8 where one character can take up as much as 4 bytes.

Explanation:
Due to a fix to a bug that forces ODP.NET to bind data with the "max byte size", the PL/SQL layer will create a blank-padded value that is beyond the size that the column can accept, since ODP.NET is required to provide the size of the parameter in terms of characters multiplied by the character expansion ratio.

Imagine that you have the following table created in a database with the AL32UTF8 mult-byte character set:
create table testchar_tab (char_column char(40));

And let us assume that you have the following PL/SQL stored procedure:
CREATE OR REPLACE PROCEDURE insert_row(param1 IN CHAR) AS
BEGIN
  INSERT INTO testchar_tab (CHAR_COLUMN) VALUES(param1);
END;
/

With the previous version of managed ODP.NET 12.x / 18.x  or ODP.NET Core 18.x, the application would have been able to insert character based data up to 40 characters.  However, starting with this patch, you will not able to insert more than 10 characters in this particular example, since PL/SQL layer will blank pad the data to 10 (characters) * 4 (character expansion ratio), which is 40 bytes, the max length of the CHAR column.

Resolution:
One workaround for this issue is to change the PL/SQL parameter type from CHAR to VARCHAR2, which eliminates the blank padding in the PL/SQL layer:

CREATE OR REPLACE PROCEDURE insert_row(param1 IN VARCHAR2) AS
BEGIN
  INSERT INTO testchar_tab (CHAR_COLUMN) VALUES(param1);
END;
/

The alternative approach is to bypass PL/SQL and insert directly into the table using SQL to avoid the blank padding by the PL/SQL layer.




Copyright (c) 2019, Oracle and/or its affiliates. All rights reserved.
