using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Security.Principal;

namespace AzureBackupInstaller.Desktop
{
    public partial class Form1 : Form
    {
        private string TEMP_PATH = $"temp";
        private List<Exception> exceptions = new List<Exception>();

        public Form1()
        {
            var isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

            if (isAdmin)
            {
                InitializeComponent();

                WelcomeMessages();
                CheckSQLPackage();
                CheckWebsitesFolder();
                SetServerName();
                Spacer();
            }
            else
            {
                MessageBox.Show("Azure DNN Backup Installer must be run in privileged mode", "Azure DNN Backup Installer",
        MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }

        #region EVENTS

        private void btn_install_Click(object sender, EventArgs e)
        {
            WriteLogRunMessage("Backup installer started");
            WriteLogRunResult(true);

            btn_install.Invoke((Action)delegate
            {
                btn_install.Enabled = false;
            });

            btn_cancel.Invoke((Action)delegate
            {
                btn_cancel.Enabled = false;
            });

            progressBar.Maximum = 10;
            progressBar.Value = 0;

            var noErrors = true;

            Task.Run(() =>
            {
                try
                {
                    WriteLogRunMessage("Checking temp folder");
                    var checkTempFolder = CheckTempFolder();
                    WriteLogRunResult(checkTempFolder);
                    UpdateProgressBar();

                    WriteLogRunMessage("Unziping backup");
                    var decompression = Decompression();
                    WriteLogRunResult(decompression);
                    UpdateProgressBar();

                    WriteLogRunMessage("Copying files to temp folder");
                    var copy = CopyDecompressedFolder();
                    WriteLogRunResult(copy);
                    UpdateProgressBar();

                    WriteLogRunMessage("Updating website .config");
                    var webconfig = UpdateWebconfig();
                    WriteLogRunResult(webconfig);
                    UpdateProgressBar();

                    WriteLogRunMessage("Adding site to IIS");
                    var iis = AddToIIS();
                    WriteLogRunResult(iis);
                    UpdateProgressBar();

                    WriteLogRunMessage("Importing bacpac to SQL");
                    var import = ImportBacpac();
                    WriteLogRunResult(import);
                    UpdateProgressBar();

                    if (chk_portalalias.Checked)
                    {
                        WriteLogRunMessage("Updating portal alias");
                        var portalAlias = UpdatePortalAlias();
                        WriteLogRunResult(portalAlias);
                    }

                    UpdateProgressBar();
                }
                catch (Exception e)
                {
                    WriteLogRunMessage($"Exception: {e.Message}");
                    noErrors = false;
                    WriteLogRunResult(noErrors);
                    UpdateProgressBar();
                }

                WriteLogRunMessage("Cleaning");
                var cleanup = Cleanup();
                WriteLogRunResult(cleanup);
                UpdateProgressBar();

                EndProgressBar();

                Spacer();
                WriteLogRunMessage("Backup installer finished");
                WriteLogRunResult(noErrors);


                groupBox1.Invoke((Action)delegate
                {
                    groupBox1.Enabled = true;
                });

                groupBox2.Invoke((Action)delegate
                {
                    groupBox2.Enabled = true;
                });

                button1.Invoke((Action)delegate
                {
                    button1.Enabled = true;
                });

                btn_install.Invoke((Action)delegate
                {
                    btn_install.Enabled = false;
                });

                btn_cancel.Invoke((Action)delegate
                {
                    btn_cancel.Enabled = true;
                });

            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            progressBar.Maximum = 30;
            progressBar.Value = 0;

            groupBox1.Invoke((Action)delegate
            {
                groupBox1.Enabled = false;
            });

            groupBox2.Invoke((Action)delegate
            {
                groupBox2.Enabled = false;
            });

            button1.Invoke((Action)delegate
            {
                button1.Enabled = false;
            });

            btn_cancel.Invoke((Action)delegate
            {
                btn_cancel.Enabled = false;
            });

            btn_install.Invoke((Action)delegate
            {
                btn_install.Enabled = false;
            });

            Task.Run(() =>
            {
                var firstCheck = CheckFilesAndFolders();
                var secondCheck = CheckSQL();

                if (firstCheck && secondCheck)
                {
                    groupBox1.Invoke((Action)delegate
                    {
                        groupBox1.Enabled = false;
                    });

                    groupBox2.Invoke((Action)delegate
                    {
                        groupBox2.Enabled = false;
                    });

                    button1.Invoke((Action)delegate
                    {
                        button1.Enabled = false;
                    });


                    btn_install.Invoke((Action)delegate
                    {
                        btn_install.Enabled = true;
                    });

                    WriteLogSystemFromThread("Check finished successfully, controls are locked.", true);
                    WriteLogSystemFromThread("Click 'Cancel' to enable controls and make changes or 'Install' to start the installation.", true);
                    Spacer();
                }

                btn_cancel.Invoke((Action)delegate
                {
                    btn_cancel.Enabled = true;
                });

                EndProgressBar();

                // To end
                //rtb_logs.ScrollToCaret();
            });
        }

        public void ToggleGroupAndButtons()
        {
            groupBox1.Invoke((Action)delegate
            {
                groupBox1.Enabled = !groupBox1.Enabled;
            });

            groupBox2.Invoke((Action)delegate
            {
                groupBox2.Enabled = !groupBox2.Enabled;
            });

            button1.Invoke((Action)delegate
            {
                button1.Enabled = !button1.Enabled;
            });

            btn_install.Invoke((Action)delegate
            {
                btn_install.Enabled = !btn_install.Enabled;
            });
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            groupBox1.Enabled = true;
            groupBox2.Enabled = true;
            btn_install.Enabled = false;
            button1.Enabled = true;
        }

        private void btn_zip_browse_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Zip Files|*.zip";
                ofd.Multiselect = false;

                DialogResult result = ofd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(ofd.FileName))
                {
                    txt_zip.Text = ofd.FileName.ToString();

                    var filename = ofd.FileName.Split('\\').Last().Replace(".zip", "");

                    txt_websitename.Text = filename;
                    txt_websiteurl.Text = $"{filename}.dnndev.me";
                    txt_databasename.Text = $"{filename}";
                    txt_portalalias.Text = $"{filename}.dnndev.me";
                    chk_portalalias.Checked = true;
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
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Exe files|*.exe";
                ofd.Multiselect = false;

                DialogResult result = ofd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(ofd.FileName))
                {
                    txt_sqlpackage.Text = ofd.FileName.ToString();
                }
            }
        }

        #endregion

        #region Functions

        private bool CheckTempFolder()
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

        private bool Decompression()
        {
            //Decompress folder to C:Websites / txt_iisfolder.Text
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

        private bool CopyDecompressedFolder()
        {
            try
            {
                if (!Directory.Exists(txt_iisfolder.Text))
                {
                    Directory.CreateDirectory(txt_iisfolder.Text);
                }

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

        private bool UpdateWebconfig()
        {
            try
            {
                string newFolderName = Path.Combine(txt_iisfolder.Text, txt_websitename.Text);
                string webconfigPath = Path.Combine(newFolderName, "web.config");
                string webconfigInner = File.ReadAllText(webconfigPath);

                //<add name="SiteSqlServer" connectionString="Data Source=hotelequiadev.database.windows.net;Initial Catalog=hotelequia-backend-dev;User ID=hotelequiaadmin;Password=5U9VPMp!QWYS(*+^" providerName="System.Data.SqlClient" />
                //<add name="SiteSqlServer" connectionString="Data Source=DESKTOP-EMI;Initial Catalog=hotelequia-backend-dev_20190510;User ID=emiliano;Password=emiliano" providerName="System.Data.SqlClient" />
                string splitFirst = webconfigInner.Split(new string[] { "<add name=\"SiteSqlServer\" connectionString=\"Data Source=" }, StringSplitOptions.None)[0];
                var splitSecondList = webconfigInner.Split(new string[] { "providerName=\"System.Data.SqlClient\" />" }, StringSplitOptions.None).ToList();
                splitSecondList.RemoveAt(0);
                string splitSecond = string.Join(" ", splitSecondList.ToArray());

                string newConnectionString = $"<add name=\"SiteSqlServer\" connectionString=\"Data Source={txt_servername.Text};Initial Catalog={txt_databasename.Text};User ID={txt_serverusername.Text};Password={txt_serverpassword.Text}\" providerName=\"System.Data.SqlClient\" />";

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

        private bool AddToIIS()
        {
            try
            {
                string newFolderName = Path.Combine(txt_iisfolder.Text, txt_websitename.Text);

                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = @"C:\Windows\System32\inetsrv\appcmd.exe",
                        Arguments = $"add site /name:{txt_websitename.Text} /physicalPath:{newFolderName} /bindings:http/*:80:{txt_websiteurl.Text}",
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

        private bool ImportBacpac()
        {
            // Restore in database
            try
            {
                // Restore bacpac
                string tempWebsitePath = Path.Combine(TEMP_PATH, txt_websitename.Text);
                var SQL_BACPAC_PATH = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, System.IO.Directory.GetFiles(tempWebsitePath, "*.bacpac")[0]);

                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                        FileName = txt_sqlpackage.Text,
                        Arguments = $"/a:Import /tsn:{txt_servername.Text} /tdn:{txt_databasename.Text} /tu:{txt_serverusername.Text} /tp:{txt_serverpassword.Text} /sf:{SQL_BACPAC_PATH}",
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

        private bool UpdatePortalAlias()
        {
            // Replace PortalAlias
            //string queryString = $"UPDATE dbo.PortalAlias SET [HTTPAlias] = replace([HTTPAlias], '{txt_portalalias.Text}', '{txt_websiteurl.Text}') WHERE [HTTPAlias] LIKE '{txt_portalalias.Text}%'; ";
            string queryString = $"UPDATE dbo.PortalAlias SET [HTTPAlias] = '{txt_websiteurl.Text}' WHERE [HTTPAlias] LIKE '{txt_portalalias.Text}%'; ";
            string connectionString = $"Server={txt_servername.Text};Database={txt_databasename.Text}; User ID={txt_serverusername.Text};Password={txt_serverpassword.Text};";

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

        private bool Cleanup()
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

        #endregion

        #region Checks

        private bool CheckFilesAndFolders()
        {
            // Starting
            WriteLogCheckMessage($"Starting file and folders check");
            WriteLogCheckResult(true);

            // Zip package¡
            var zipPackageExists = !String.IsNullOrEmpty(txt_zip.Text) && File.Exists(txt_zip.Text);
            WriteLogCheckMessage($"Checking Zip package");
            WriteLogCheckResult(zipPackageExists);
            UpdateProgressBar();

            // website name
            var websiteNameExists = !String.IsNullOrEmpty(txt_websitename.Text);
            WriteLogCheckMessage($"Checking website name");
            WriteLogCheckResult(websiteNameExists);
            UpdateProgressBar();

            // Website url
            var websiteUrlExists = !String.IsNullOrEmpty(txt_websiteurl.Text);
            WriteLogCheckMessage($"Checking website url");
            WriteLogCheckResult(websiteUrlExists);
            UpdateProgressBar();

            // IIS folder
            var issFolderExists = !String.IsNullOrEmpty(txt_iisfolder.Text) && Directory.Exists(txt_iisfolder.Text);
            WriteLogCheckMessage($"Checking IIS folder");
            WriteLogCheckResult(issFolderExists);
            UpdateProgressBar();

            var result = zipPackageExists && websiteNameExists && websiteUrlExists && issFolderExists;
            WriteLogCheckMessage($"Finished file and folders check");
            WriteLogCheckResult(result);

            Spacer();
            UpdateProgressBar();

            return result;
        }

        private bool CheckSQL()
        {
            // Starting
            WriteLogCheckMessage($"Starting SQL check");
            WriteLogCheckResult(true);

            // SQL Package
            WriteLogCheckMessage($"Checking SqlPackacge.exe");
            var zipPackageExists = !String.IsNullOrEmpty(txt_zip.Text) && File.Exists(txt_zip.Text);
            WriteLogCheckResult(zipPackageExists);
            UpdateProgressBar();

            // Server
            WriteLogCheckMessage($"Checking SQL server name");
            var sqlServer = !String.IsNullOrEmpty(txt_servername.Text);
            WriteLogCheckResult(sqlServer);
            UpdateProgressBar();

            // Database
            WriteLogCheckMessage($"Checking SQL database name");
            var database = !String.IsNullOrEmpty(txt_databasename.Text);
            WriteLogCheckResult(database);
            UpdateProgressBar();

            // Username
            WriteLogCheckMessage($"Checking SQL username");
            var username = !String.IsNullOrEmpty(txt_serverusername.Text);
            WriteLogCheckResult(username);
            UpdateProgressBar();

            // Password
            WriteLogCheckMessage($"Checking SQL password");
            var password = !String.IsNullOrEmpty(txt_serverpassword.Text);
            WriteLogCheckResult(password);
            UpdateProgressBar();

            // Check connection
            WriteLogCheckMessage($"Checking SQL connection");
            var sqlCheck = CheckSQLConnection();
            WriteLogCheckResult(sqlCheck);

            var result = zipPackageExists && sqlServer && database && username && password && sqlCheck;
            UpdateProgressBar();

            WriteLogCheckMessage($"Finished SQL check");
            WriteLogCheckResult(result);
            Spacer();

            return result;
        }

        private bool CheckSQLConnection()
        {
            string connectionString = $"Server={txt_servername.Text};Database=master; User ID={txt_serverusername.Text};Password={txt_serverpassword.Text};";

            //Log($"Connecting to database");
            using SqlConnection connection = new SqlConnection(connectionString);
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

        private void CheckSQLPackage()
        {
            // Find SQLPackage
            var sqlpackageLocation = @"C:\Program Files\Microsoft SQL Server\150\DAC\bin\sqlpackage.exe";

            if (File.Exists(sqlpackageLocation))
            {
                txt_sqlpackage.Text = sqlpackageLocation;
                WriteLogSystem("Found sqlpackage.exe", true);
            }
            else
            {
                WriteLogSystem("Found sqlpackage.exe", false);
            }
        }

        private void SetServerName()
        {
            txt_servername.Text = Environment.MachineName;
            WriteLogSystem("Set SQL server name", true);
        }

        private void CheckWebsitesFolder()
        {
            // Find SQLPackage
            var websitesFolder = @"C:\Websites";
            txt_iisfolder.Text = websitesFolder;
            WriteLogSystem("Set default website folder", true);
        }

        #endregion

        #region Methods

        private void WelcomeMessages()
        {
            rtb_logs.Text += $"[{DateTime.Now}] - [SYSTEM] - Welcome to Azure DNN Backup Installer. {Environment.NewLine}";
            rtb_logs.Text += $"[{DateTime.Now}] - [SYSTEM] - This tool was created intentionally for restoring DNN Azure backups in local environments. {Environment.NewLine}";
            rtb_logs.Text += $"[{DateTime.Now}] - [SYSTEM] - SqlPackage must be installed or have a portable executable. {Environment.NewLine}";
            rtb_logs.Text += $"[{DateTime.Now}] - [SYSTEM] - This must be used with administrator privileges in order to deploy to the local IIS. {Environment.NewLine}";
            rtb_logs.Text += $"[{DateTime.Now}] - [SYSTEM] - Use at your own risk. {Environment.NewLine}";
            rtb_logs.Text += $"{Environment.NewLine}";

            rtb_logs.SelectionStart = rtb_logs.Text.Length;
            rtb_logs.ScrollToCaret();
        }

        private void WriteLogRunMessage(string message)
        {
            rtb_logs.Invoke((Action)delegate
            {
                rtb_logs.Text += $"[{DateTime.Now}] - [RUNNING] - {message} - ";
                rtb_logs.SelectionStart = rtb_logs.Text.Length;
                rtb_logs.ScrollToCaret();
            });
        }

        private void WriteLogRunResult(bool result)
        {
            rtb_logs.Invoke((Action)delegate
            {
                var resultString = result ? "OK" : "FAIL";
                rtb_logs.Text += $"{resultString}. {Environment.NewLine}";
                rtb_logs.SelectionStart = rtb_logs.Text.Length;
                rtb_logs.ScrollToCaret();
            });
        }

        private void Spacer()
        {
            rtb_logs.Invoke((Action)delegate
            {
                rtb_logs.Text += $"{Environment.NewLine}";
                rtb_logs.SelectionStart = rtb_logs.Text.Length;
                rtb_logs.ScrollToCaret();
            });

        }

        private void WriteLogCheckResult(bool result)
        {
            rtb_logs.Invoke((Action)delegate
            {
                var resultString = result ? "OK" : "FAIL";
                rtb_logs.Text += $"{resultString}. {Environment.NewLine}";
                rtb_logs.SelectionStart = rtb_logs.Text.Length;
                rtb_logs.ScrollToCaret();
            });
        }

        private void WriteLogCheckMessage(string message)
        {
            rtb_logs.Invoke((Action)delegate
            {
                rtb_logs.Text += $"[{DateTime.Now}] - [CHECKING] - {message} - ";
                rtb_logs.SelectionStart = rtb_logs.Text.Length;
                rtb_logs.ScrollToCaret();
            });
        }


        private void WriteLogSystem(string message, bool result)
        {
            var resultString = result ? "OK" : "FAIL";
            rtb_logs.Text += $"[{DateTime.Now}] - [SYSTEM] - {message} - {resultString}. {Environment.NewLine}";
            rtb_logs.SelectionStart = rtb_logs.Text.Length;
            rtb_logs.ScrollToCaret();
        }

        private void WriteLogSystemFromThread(string message, bool result)
        {
            rtb_logs.Invoke((Action)delegate
            {
                var resultString = result ? "OK" : "FAIL";
                rtb_logs.Text += $"[{DateTime.Now}] - [SYSTEM] - {message} - {resultString}. {Environment.NewLine}";
                rtb_logs.SelectionStart = rtb_logs.Text.Length;
                rtb_logs.ScrollToCaret();
            });
        }

        private void UpdateProgressBar()
        {
            progressBar.Invoke((Action)delegate
            {
                progressBar.Value++;
            });
        }

        private void EndProgressBar()
        {
            progressBar.Invoke((Action)delegate
            {
                progressBar.Value = progressBar.Maximum;
            });
        }

        #endregion

        #region Utils

        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
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
