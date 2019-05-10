using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBackupInstaller.NETFramework
{
    class Program
    {
        private static string WEBSITE_NAME;
        private static string WEBSITE_URL;
        private static string ZIP_PATH;
        private static string WEBSITES_PATH;

        private static string SQL_SQLPACKAGE;
        private static string SQL_SEVERNAME;
        private static string SQL_DATABASENAME;
        private static string SQL_BACPAC_PATH;
        private static string SQL_USERNAME;
        private static string SQL_PASSWORD;
        private static string SQL_REPLACE_WORD;

        private static string TEMP_PATH;

        private static List<Exception> exceptions = new List<Exception>();

        static void Main(string[] args)
        {
            WriteHeader();

            #region ASK 

            WEBSITE_NAME = AskForString("WEBSITE_NAME");
            WEBSITE_URL = AskForString("WEBSITE_URL");
            ZIP_PATH = AskForString("ZIP_PATH");
            WEBSITES_PATH = GetSetting("WEBSITES_PATH");

            #endregion

            #region SQL VARIABLES

            // SQL VARIABLES
            SQL_SQLPACKAGE = GetSetting("SQL_SQLPACKAGE");
            SQL_SEVERNAME = GetSetting("SQL_SEVERNAME");
            SQL_DATABASENAME = WEBSITE_NAME;
            SQL_BACPAC_PATH = string.Empty;
            SQL_USERNAME = GetSetting("SQL_USERNAME");
            SQL_PASSWORD = GetSetting("SQL_PASSWORD");
            SQL_REPLACE_WORD = GetSetting("SQL_REPLACE_WORD");

            TEMP_PATH = @"temp\";

            #endregion

            #region PRE LOGIC

            Log($"Checking all properties");
            Console.WriteLine();

            Check("WEBSITE_NAME");
            Check("WEBSITE_URL");
            Check("ZIP_PATH");
            Check("WEBSITES_PATH");
            Check("SQL_SQLPACKAGE");
            Check("SQL_SEVERNAME");
            Check("SQL_DATABASENAME");
            Check("SQL_BACPAC_PATH");
            Check("SQL_USERNAME");
            Check("SQL_PASSWORD");
            Check("SQL_REPLACE_WORD");
            Check("TEMP_PATH");
            Runner<bool>(() => CheckSQLexplorer(), "Checking if sqlpackage.exe exists");

            #endregion

            #region CHECK IF SQLPACKAGE EXISTS


            #endregion

            #region TEST CONNECTION TO DB

            // Needs work since db doesn't exists

            #endregion

            if (AskToContinue($"There are {exceptions.Count} errors, do you want to continue?"))
            {
                Runner<bool>(() => Cleanup(), "Cleaning temp folder");
                Runner<bool>(() => CheckTempFolder(), "Creating temp folder");

                #region WEBSITE

                Runner<bool>(() => Decompression(), "Extracting zip to temp folder");
                Runner<bool>(() => CopyDecompressedFolder(), "Copying wwwroot folder to IIS websites folder");
                Runner<bool>(() => UpdateWebconfig(), "Updating webconfig");
                Runner<bool>(() => AddToIIS(), "Adding website to IIS");

                #endregion

                #region SQL

                Runner<bool>(() => ImportBacpac(), "Importing bacpac to database");
                Runner<bool>(() => UpdatePortalAlias(), "Updating PortalAlias table");

                #endregion
            }
            else
            {
            }

            // Cleanup
            Runner<bool>(() => Cleanup(), "Cleaning temp folder");

            Log("Press a key to exit");
            Console.ReadLine();
        }

        #region BACKUP FUNCTIONS

        private static bool CheckTempFolder()
        {
            try
            {
                if (!Directory.Exists(TEMP_PATH))
                {
                    Directory.CreateDirectory(TEMP_PATH);
                }

                return true;
            }
            catch (Exception e)
            {
                exceptions.Add(e);
                return false;
            }
        }

        private static bool Decompression()
        {
            //Decompress folder to C:Websites / WEBSITES_PATH
            try
            {
                string path = Path.Combine(TEMP_PATH, WEBSITE_NAME);
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                ZipFile.ExtractToDirectory(ZIP_PATH, path);
                return true;
            }
            catch (Exception e)
            {
                exceptions.Add(e);
                return false;
            }
        }

        private static bool CopyDecompressedFolder()
        {
            try
            {
                // Copy from temp 
                string tempWebsitePath = Path.Combine(TEMP_PATH, WEBSITE_NAME);
                string wwwrootPath = Path.Combine(tempWebsitePath, @"fs\site\wwwroot\");
                string newFolderName = Path.Combine(WEBSITES_PATH, WEBSITE_NAME);
                DirectoryCopy(wwwrootPath, newFolderName, true);
                return true;
            }
            catch (Exception e)
            {
                exceptions.Add(e);
                return false;
            }
        }

        private static bool UpdateWebconfig()
        {
            try
            {
                string newFolderName = Path.Combine(WEBSITES_PATH, WEBSITE_NAME);
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

                File.WriteAllText(webconfigPath, finalInnerWebConfig);

                return true;
            }
            catch (Exception e)
            {
                exceptions.Add(e);
                return false;
            }
        }

        private static bool AddToIIS()
        {
            try
            {
                string newFolderName = Path.Combine(WEBSITES_PATH, WEBSITE_NAME);

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
                return true;
            }
            catch (Exception e)
            {
                exceptions.Add(e);
                return false;
            }
        }

        private static bool ImportBacpac()
        {
            // Restore in database
            try
            {
                // Restore bacpac
                string tempWebsitePath = Path.Combine(TEMP_PATH, WEBSITE_NAME);
                SQL_BACPAC_PATH = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, System.IO.Directory.GetFiles(tempWebsitePath, "*.bacpac")[0]);

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
                return true;
            }
            catch (Exception e)
            {
                exceptions.Add(e);
                return false;
            }
        }

        private static bool UpdatePortalAlias()
        {
            // Replace PortalAlias
            string queryString = $"UPDATE dbo.PortalAlias SET [HTTPAlias] = replace([HTTPAlias], '{SQL_REPLACE_WORD}', '{WEBSITE_URL}') WHERE [HTTPAlias] LIKE '{SQL_REPLACE_WORD}%'; ";
            string connectionString = $"Server={SQL_SEVERNAME};Database={SQL_DATABASENAME}; User ID={SQL_USERNAME};Password={SQL_PASSWORD};";

            //Log($"Connecting to database");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                    return true;
                }
                catch (Exception e)
                {
                    connection.Close();
                    exceptions.Add(e);
                    return false;
                }
            }
        }

        private static bool Cleanup()
        {
            try
            {
                Directory.Delete("temp", true);
                return true;
            }
            catch (Exception e)
            {
                exceptions.Add(e);
                return false;
            }
        }

        private static bool CheckSQLexplorer() {
            return File.Exists(SQL_SQLPACKAGE);
        }

        private static bool CheckSQLConnection() {
            string connectionString = $"Server={SQL_SEVERNAME};Database={SQL_DATABASENAME}; User ID={SQL_USERNAME};Password={SQL_PASSWORD};";

            //Log($"Connecting to database");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    connection.Close();
                    return true;
                }
                catch (Exception e)
                {
                    connection.Close();
                    exceptions.Add(e);
                    return false;
                }
            }
        }
        #endregion

        #region METHODS

        public static void Runner<T>(Func<bool> funcToRun, string message)
        {

            Console.Write($"[{DateTime.Now}] {message}... ");
            var result = funcToRun();

            if (result)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"\t[SUCCESS]");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"\t[ERROR]");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"!");
            }

            Console.WriteLine();
        }

        private static void Check(string property)
        {
            Console.Write($"[{DateTime.Now}] Checking property '{property}'... \t");

            if (String.IsNullOrEmpty(property) || String.IsNullOrWhiteSpace(property))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"[ERROR]");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"[SUCCESS]");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"!");
            }
            Console.WriteLine();
        }

        #endregion

        #region UTILS

        private static string AskForString(string message)
        {
            try
            {
                //Console.WriteLine();
                Console.Write($"[{DateTime.Now}] {message}: ");
                return Console.ReadLine();
            }
            catch (Exception e)
            {
                exceptions.Add(e);
                return null;
            }
        }

        private static bool AskToContinue(string message)
        {
            try
            {
                Console.WriteLine();
                Console.Write($"[{DateTime.Now}] {message} (Y/n): ");
                string res = Console.ReadLine();
                Console.WriteLine();

                switch (res.ToLower())
                {
                    case "y":
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private static void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] {message}...");
        }

        private static string GetSetting(string key)
        {
            try
            {
                string res = System.Configuration.ConfigurationManager.AppSettings[key];

                if (string.IsNullOrEmpty(res))
                {
                    throw new Exception();
                }
                else
                {
                    return res;
                }
            }
            catch (Exception e)
            {
                exceptions.Add(e);
                return null;
            }

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
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine(@" ____             _                  _           _        _ _           ");
            Console.WriteLine(@"|  _ \           | |                (_)         | |      | | |          ");
            Console.WriteLine(@"| |_) | __ _  ___| | ___   _ _ __    _ _ __  ___| |_ __ _| | | ___ _ __ ");
            Console.WriteLine(@"|  _ < / _` |/ __| |/ / | | | '_ \  | | '_ \/ __| __/ _` | | |/ _ \ '__|");
            Console.WriteLine(@"| |_) | (_| | (__|   <| |_| | |_) | | | | | \__ \ || (_| | | |  __/ |   ");
            Console.WriteLine(@"|____/ \__,_|\___|_|\_\\__,_| .__/  |_|_| |_|___/\__\__,_|_|_|\___|_|   ");
            Console.WriteLine(@"                             | |                                         ");
            Console.WriteLine(@"                             |_|                                         ");
            Console.WriteLine();
            Console.WriteLine("Source code: https://github.com/emimontesdeoca/azure-backup-installer");
            Console.WriteLine();
        }

        #endregion

    }
}
