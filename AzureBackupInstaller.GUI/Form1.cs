using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;

namespace AzureBackupInstaller.GUI
{
    public partial class Form1 : Form
    {

        private static string TEMP_PATH;
        private static List<Exception> exceptions = new List<Exception>();

        public Form1()
        {
            InitializeComponent();
        }

        private void btn_install_Click(object sender, EventArgs e)
        {

        }

        private void btn_zip_browse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    txt_zip.Text = fbd.SelectedPath.ToString();
                }
            }
        }

        private void btn_iisfolder_browse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    txt_iisfolder.Text = fbd.SelectedPath.ToString();
                }
            }
        }

        private void btn_package_location_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    txt_sqlpackage.Text = fbd.SelectedPath.ToString();
                }
            }
        }

        #region Functions

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
                string path = Path.Combine(TEMP_PATH, txt_websitename.Text);
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                ZipFile.ExtractToDirectory(txt_zip.Text, path);
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
                string tempWebsitePath = Path.Combine(TEMP_PATH, txt_websitename.Text);
                string wwwrootPath = Path.Combine(tempWebsitePath, @"fs\site\wwwroot\");
                string newFolderName = Path.Combine(txt_iisfolder.Text, txt_websitename.Text);
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
                string newFolderName = Path.Combine(WEBSITES_PATH, txt_websitename.Text);
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
                string newFolderName = Path.Combine(WEBSITES_PATH, txt_websitename.Text);

                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = @"C:\Windows\System32\inetsrv\appcmd.exe",
                        Arguments = $"add site /name:{txt_websitename.Text} /physicalPath:{newFolderName} /bindings:http/*:80:{WEBSITE_URL}",
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
                string tempWebsitePath = Path.Combine(TEMP_PATH, txt_websitename.Text);
                SQL_BACPAC_PATH = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, System.IO.Directory.GetFiles(tempWebsitePath, "*.bacpac")[0]);

                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = SQL_SQLPACKAGE,
                        Arguments = $"/a:Import /tsn:{SQL_SEVERNAME} /tdn:{SQL_DATABASENAME} /tu:{SQL_USERNAME} /tp:{SQL_PASSWORD} /sf:{SQL_BACPAC_PATH}",
                        CreateNoWindow = false
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
                if (Directory.Exists(TEMP_PATH))
                {
                    Directory.Delete(TEMP_PATH, true);
                }

                return true;
            }
            catch (Exception e)
            {
                exceptions.Add(e);
                return false;
            }
        }

        private static bool CheckSQLexplorer()
        {
            return File.Exists(SQL_SQLPACKAGE);
        }

        private static bool CheckSQLConnection()
        {
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

        #region Methods

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


        #region Utils

        private static void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] {message}...");
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


        #endregion
    }
}
