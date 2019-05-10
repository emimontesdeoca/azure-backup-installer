# Azure-backup-installer

Small tool that giving a .zip backup from Azure restores the bacpac and installs the website in IIS.

## App.config

Before using you need to add an app.config with the name `AzureBackupInstaller.dll.config`.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="WEBSITES_PATH" value="C:\Websites"/>
    <add key="SQL_SQLPACKAGE" value="C:\Program Files (x86)\Microsoft SQL Server\140\DAC\bin\sqlpackage.exe"/>
    <add key="SQL_SEVERNAME" value="LAPTOP-EMI"/>
    <add key="SQL_USERNAME" value="emiliano"/>
    <add key="SQL_PASSWORD" value="emiliano"/>
  </appSettings>
</configuration>
```

 - `WEBSITES_PATH` is the path to the IIS websites folder
 - `SQL_SQLPACKAGE` is the path to the sqlpackage.exe
 - `SQL_SEVERNAME` is the login name for the server
 - `SQL_USERNAME` is sql username
 - `SQL_PASSWORD` is sql passowrd
 
 [![Image from Gyazo](https://i.gyazo.com/78e7fe38db16bc7ce36ab30789571d23.jpg)](https://gyazo.com/78e7fe38db16bc7ce36ab30789571d23)
