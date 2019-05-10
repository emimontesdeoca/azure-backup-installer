using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace AzureBackupInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            WriteHeader();

            #region ASK 

            // COMMANDS TO ASK
            string WEBSITE_NAME = AskForString("WEBSITE_NAME") ?? throw new Exception("Can't be null");
            string WEBSITE_URL = AskForString("WEBSITE_URL") ?? throw new Exception("Can't be null");
            string ZIP_PATH = AskForString("ZIP_PATH") ?? throw new Exception("Can't be null");
            string WEBSITES_PATH = GetSetting("WEBSITES_PATH") ?? throw new Exception("Can't be null");

            #endregion

            #region SQL VARIABLES

            // SQL VARIABLES
            string SQL_SQLPACKAGE = GetSetting("SQL_SQLPACKAGE") ?? throw new Exception("Can't be null");
            string SQL_SEVERNAME = GetSetting("SQL_SEVERNAME") ?? throw new Exception("Can't be null");
            string SQL_DATABASENAME = WEBSITE_NAME ?? throw new Exception("Can't be null");
            string SQL_BACPAC_PATH = string.Empty;
            string SQL_USERNAME = GetSetting("SQL_USERNAME") ?? throw new Exception("Can't be null");
            string SQL_PASSWORD = GetSetting("SQL_PASSWORD") ?? throw new Exception("Can't be null");

            #endregion

            #region CHECKS

            // CONST
            string TEMP_PATH = @"temp\" ?? throw new Exception("Can't be null");

            // Decompress bacpac to temp folder

            Log($"Checking if '{TEMP_PATH}' exists");
            if (!Directory.Exists(TEMP_PATH))
            {
                Log($"Creating '{TEMP_PATH}' folder");
                Directory.CreateDirectory(TEMP_PATH);
            }

            #endregion

            #region WEBSITE

            Log($"Starting website '{WEBSITE_NAME}'");

            #region DECOMPRESSION

            //Decompress folder to C:Websites / WEBSITES_PATH
            try
            {
                string path = Path.Combine(TEMP_PATH, WEBSITE_NAME);
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                Log($"Extracting '{ZIP_PATH}' to '{path}'");
                ZipFile.ExtractToDirectory(ZIP_PATH, path);
                Log($"Extraction finished");
            }
            catch (Exception e)
            {
                throw;
            }

            #endregion

            #region COPY

            // Copy from temp 
            string tempPath = Path.Combine(TEMP_PATH, WEBSITE_NAME);
            string wwwrootPath = Path.Combine(tempPath, @"fs\site\wwwroot\");
            string newFolderName = Path.Combine(WEBSITES_PATH, WEBSITE_NAME);

            try
            {
                Log($"Copying '{tempPath}' to '{newFolderName}'");
                DirectoryCopy(wwwrootPath, newFolderName, true);
                Log($"Copying finished");
            }
            catch (Exception e)
            {
                throw;
            }

            #endregion

            #region WEBCONFIG
            // Replace connectionString in web.config

            try
            {
                Log($"Updating web.config of '{WEBSITE_URL}'");
                string webconfigPath = Path.Combine(newFolderName, "web.config");
                string webconfigInner = File.ReadAllText(webconfigPath);

                //<add name="SiteSqlServer" connectionString="Data Source=hotelequiadev.database.windows.net;Initial Catalog=hotelequia-backend-dev;User ID=hotelequiaadmin;Password=5U9VPMp!QWYS(*+^" providerName="System.Data.SqlClient" />
                //<add name="SiteSqlServer" connectionString="Data Source=DESKTOP-EMI;Initial Catalog=hotelequia-backend-dev_20190510;User ID=emiliano;Password=emiliano" providerName="System.Data.SqlClient" />
                string splitFirst = webconfigInner.Split(new string[] { "<add name=\"SiteSqlServer\" connectionString=\"Data Source=" }, StringSplitOptions.None)[0];
                var splitSecondList = webconfigInner.Split(new string[] { "providerName=\"System.Data.SqlClient\" />" }, StringSplitOptions.None).ToList();
                splitSecondList.RemoveAt(0);
                string splitSecond = string.Join(" ", splitSecondList.ToArray());

                string newConnectionString = $"<add name=\"SiteSqlServer\" connectionString=\"Data Source={SQL_SEVERNAME};Initial Catalog={SQL_DATABASENAME};User ID={SQL_USERNAME};Password={SQL_PASSWORD}\" providerName=\"System.Data.SqlClient\" />";

                string finalInnerWebConfig = $"{splitFirst}{newConnectionString}{splitSecond}";

                File.WriteAllText(webconfigPath,finalInnerWebConfig);

                
                Log($"Updating finished");
            }
            catch (Exception)
            {

                throw;
            }

            #endregion

            #region IIS ADDITION
            // Add to IIS

            try
            {
                Log($"Adding '{WEBSITE_URL}' to IIS");
                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = @"C:\Windows\System32\inetsrv\appcmd.exe",
                        Arguments = $"add site /name:{WEBSITE_NAME} /physicalPath:{newFolderName} /bindings:http/*:80:{WEBSITE_URL}",
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();
                Log($"Addition finished");
            }
            catch (Exception)
            {

                throw;
            }

            #endregion

            #endregion

            #region SQL

            #region IMPORT BACPAC

            // Restore bacpac
            SQL_BACPAC_PATH = Path.Combine(tempPath, @"hotelequia-backend-dev.bacpac");

            // Restore in database
            try
            {
                Log($"Importing '{SQL_BACPAC_PATH}' to server '{SQL_SEVERNAME}' with name '{SQL_DATABASENAME}'");

                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = SQL_SQLPACKAGE,
                        Arguments = $"/a:Import /tsn:{SQL_SEVERNAME} /tdn:{SQL_DATABASENAME} /tu:{SQL_USERNAME} /tp:{SQL_PASSWORD} /sf:{SQL_BACPAC_PATH}",
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();
                Log($"Importing finished");
            }
            catch (Exception e)
            {

                throw;
            }

            #endregion

            #region UPDATE PORTALALIAS

            // Replace PortalAlias
            string queryString = $"UPDATE dbo.PortalAlias SET [HTTPAlias] = replace([HTTPAlias], 'admindev.hotelequia.com', '{WEBSITE_URL}') WHERE [HTTPAlias] LIKE 'admindev.hotelequia.com%'; ";
            string connectionString = $"Server={SQL_SEVERNAME};Database={SQL_DATABASENAME}; User ID={SQL_USERNAME};Password={SQL_PASSWORD};";

            Log($"Connecting to database");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    Log($"Updating 'dbo.PortalAlias' values with new url '{WEBSITE_URL}'");
                    SqlCommand command = new SqlCommand(queryString, connection);
                    connection.Open();
                    command.ExecuteNonQuery();
                    Log($"Updating finished");
                    connection.Close();
                }
                catch (Exception e)
                {
                    connection.Close();
                    throw;
                }
            } 

            #endregion

            #endregion

            #region END

            // Cleanup
            Log($"Deleting temp folder");
            Directory.Delete("temp", true);
            Log($"Deleting finished");
            Log($"Backup installer finished, press enter to exit");

            #endregion

            Console.ReadLine();
        }


        #region UTILS

        private static string AskForString(string message)
        {
            //Console.WriteLine();
            Console.Write($"[{DateTime.Now}] {message}: ");
            return Console.ReadLine();
        }

        private static void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] {message}...");
        }

        private static string GetSetting(string key)
        {
            return System.Configuration.ConfigurationManager.AppSettings[key];
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private static void WriteHeader()
        {
            Console.WriteLine(@" ____             _                  _           _        _ _           ");
            Console.WriteLine(@"|  _ \           | |                (_)         | |      | | |          ");
            Console.WriteLine(@"| |_) | __ _  ___| | ___   _ _ __    _ _ __  ___| |_ __ _| | | ___ _ __ ");
            Console.WriteLine(@"|  _ < / _` |/ __| |/ / | | | '_ \  | | '_ \/ __| __/ _` | | |/ _ \ '__|");
            Console.WriteLine(@"| |_) | (_| | (__|   <| |_| | |_) | | | | | \__ \ || (_| | | |  __/ |   ");
            Console.WriteLine(@"|____/ \__,_|\___|_|\_\\__,_| .__/  |_|_| |_|___/\__\__,_|_|_|\___|_|   ");
            Console.WriteLine(@"                             | |                                         ");
            Console.WriteLine(@"                             |_|                                         ");
            Console.WriteLine();
        }

        #endregion

    }
}
