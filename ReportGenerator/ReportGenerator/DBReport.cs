using System;
using System.Configuration;
using System.Collections.Generic;
using System.Text;
using System.Management;

using System.Data;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

using Microsoft.Win32; // for registry access

// For Getting public IP Address
using System.Web;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;

namespace ReportGenerator
{
    /// <summary>
    /// This class will generate a report for the required information
    /// and send it to the database.
    /// </summary>
    public static class DBReport
    {
        public static void ComputerInformation()
        {
            Console.WriteLine("Computer Information Being Generated and Sent to Database");
            MySqlConnection connection = new MySqlConnection(ConfigurationSettings.AppSettings["mysql_db"]);
            MySqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;

            // IP Address Information
            string sHostName = Dns.GetHostName();

            string reportDate = "";

            IPHostEntry ipE = Dns.GetHostEntry(sHostName);
            IPAddress[] ip = ipE.AddressList;
            string ipAddress = ip[0].ToString();            

            ManagementClass os = new ManagementClass("Win32_OperatingSystem");
            
            foreach(ManagementObject obj in os.GetInstances())
            {
                string serialNumber = obj["SerialNumber"].ToString();
                
                // Check to see if this computer already has a record in the 'masterpc_details'
                // table ... if it exists, then nothing will be inserted into the database.
                string cmdString = "SELECT * FROM master_pcdetails WHERE serial_number = '" + serialNumber + "'";
                int computerRecordCount = 0;

                cmd.CommandText = cmdString;
                cmd.Connection.Open();
                
                // I am using a MySqlDataReader object to count the number
                // of rows returned by the SQL command in the cmdString object 
                // above.  If there is at least one record or more, this 
                // method will exit without inserting anything into the database.
                MySqlDataReader reader = cmd.ExecuteReader();

                while(reader.Read())
                {
                    computerRecordCount++;
                }
                              
                reader.Close();
                cmd.Connection.Close();

                //Console.WriteLine("number of records in master_pcdetails: " + computerRecordCount);

                if(computerRecordCount > 0)
                {
                    
                    Console.WriteLine("This computer already has a record in the database.");
                    Console.WriteLine("No data will be inserted into the database.");
                    return;
                }

                string computerName = Environment.MachineName.ToLower();
                string registeredUser = obj["RegisteredUser"].ToString();
                string userName = Environment.UserName.ToLower();
                string osType = obj["Caption"].ToString();
                string servicePack = obj["CSDVersion"].ToString();
                string installDate = ManagementDateTimeConverter.ToDateTime(obj["InstallDate"].ToString()).ToString();
                
                string commandText = "INSERT INTO master_pcdetails(serial_number, computer_name, registered_user, user_name, os_type, service_pack, ip_address, report_date) " +
                                     "VALUES ('" + serialNumber + "','" + computerName + "', '" + registeredUser + "','" + userName + 
                                              "','" + osType + "','"+ servicePack + "','" + ipAddress + "','" + DateTime.Now.ToString() + "')";

                // Insert data on to table
                cmd.Connection.Open();
                cmd.CommandText = commandText;
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();

                // only run thru this loop once
                break;
            }            
        }

        public static void HardwareInformation()
        {
            Console.WriteLine("Hardware Information Being Generated and Sent to Database");

            // all varibles used in the insert statement below
            // I am declaring them here and setting them to a blank string
            // just in case WMI does not retrieve anything.
            string ram = "";
            string manufacturer = "";
            string cpu = "";         
            string manufacturerModel = "";
            string hdd = "";            
            string videoCardname = "";           
            string soundCardName = "";
            string opticalDriveName = "";
            string motherBoard = "";
            string nic = "";
            string serialNumber = "";

            // Serial Number
            ManagementClass os = new ManagementClass("Win32_OperatingSystem");
            foreach(ManagementObject obj in os.GetInstances())
            {
                // store this in the static serialNumber string object to re-use later
                serialNumber = obj["SerialNumber"].ToString();
            }

            // Motherboard Info
            ManagementClass mBoard = new ManagementClass("Win32_BaseBoard");
            foreach(ManagementObject obj in mBoard.GetInstances())
            {
                motherBoard = obj["Manufacturer"].ToString();
                break;
            }

            // Network Information
            IPHostEntry ipE = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] ip = ipE.AddressList;

            // FIX: On a Vista machine that has IPv6 enabled, this is 
            // getting the IPv6 address and not the IPv4 address!
            foreach(IPAddress ipAddress in ip)
            {
                if(!ipAddress.IsIPv6LinkLocal)
                {
                    nic = ipAddress.ToString();
                }                
            }

            MySqlConnection connection = new MySqlConnection(ConfigurationSettings.AppSettings["mysql_db"]);
            MySqlCommand cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.Text;

            ManagementClass csInfo = new ManagementClass("Win32_ComputerSystem");
            foreach(ManagementObject obj in csInfo.GetInstances())
            {
                string tempTotalPhysicalMemory = obj["TotalPhysicalMemory"].ToString();

                // I am using a double to handle those machines
                // with alot of RAM
                double mem = double.Parse(tempTotalPhysicalMemory);
                double memoryResults = (mem / 1024);
                ram = memoryResults.ToString();
                
                manufacturer = obj["Manufacturer"].ToString().Trim();
                manufacturerModel = obj["Model"].ToString().Trim();                
            }

            // Processor Information
            ManagementClass processorInfo = new ManagementClass("Win32_Processor");
            foreach(ManagementObject obj in processorInfo.GetInstances())
            {
                cpu = obj["Name"].ToString().Trim();                
                break;
            }

            // Hard Drive Information
            ManagementClass hdInfo = new ManagementClass("Win32_DiskDrive");
            foreach(ManagementObject obj in hdInfo.GetInstances())
            {
                hdd = obj["Model"].ToString().Trim();
                
                // only get one hard drive ... this is necessary right now
                // because of the current db schema
                break;
            }

            // Video Card Information
            ManagementClass v = new ManagementClass("Win32_VideoController");

            // this will only get the first video card
            foreach(ManagementObject video in v.GetInstances())
            {
                videoCardname = video["Name"].ToString().Trim();
                break;
            }

            // Sound Card Info
            ManagementClass s = new ManagementClass("Win32_SoundDevice");
            foreach(ManagementObject sci in s.GetInstances())
            {
                soundCardName = sci["Manufacturer"].ToString().Trim();
            }

            // Optical Drive Information
            // TODO: There may be more than one in here ... but for now
            // I am only worrying about the first one because of the restrictions
            // on the db schema.
            ManagementClass c = new ManagementClass("Win32_CDROMDrive");
            foreach(ManagementObject cd in c.GetInstances())
            {
                opticalDriveName = cd["Name"].ToString().Trim();
            }

            // Need to remove any single quotes that may give this insert 
            // statement a problem ... there may be more of these things
            // that I need to double check as well! :)
            if(cpu.Contains("'"))
                cpu = cpu.Replace("'", "");

            if(manufacturer.Contains("'"))
                manufacturer = manufacturer.Replace("'", "");

            if(videoCardname.Contains("'"))
                videoCardname = videoCardname.Replace("'", "");

            string commandText = "INSERT INTO hardware_info(serial_number, date_created, manufacturer, manufacturer_model, cpu," +
                                                                "ram, hdd, vdo," +
                                                                "snd, nic," +
                                                                "optical_drive, motherboard) " + 
                                 "VALUES('" + serialNumber + "','" + DateTime.Now.ToString() + "','" + manufacturer + "','" + manufacturerModel + "','" + cpu.Trim() + 
                                        "','" + ram + "','" + hdd + "','" + videoCardname + 
                                        "','" + soundCardName + "','" + nic +                                         
                                        "','" + opticalDriveName + "','" + motherBoard + "')";

            cmd.CommandText = commandText;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();                                        
        }

        /// <summary>
        /// This function will get software information as well as windows
        /// updates and other software updates
        /// </summary>
        public static void SoftwareInformation()
        {
            Console.WriteLine("Software Information Being Generated and Sent to Database");

            MySqlConnection connection = new MySqlConnection(ConfigurationSettings.AppSettings["mysql_db"]);
            string computerName = System.Environment.MachineName;
            string uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

            string serialNumber = "";
            string dateCreated = DateTime.Now.ToString();

            ManagementClass os = new ManagementClass("Win32_OperatingSystem");
            foreach(ManagementObject obj in os.GetInstances())
            {
                // store this in the static serialNumber string object to re-use later
                serialNumber = obj["SerialNumber"].ToString();
            }

            using(RegistryKey rk = Registry.LocalMachine.OpenSubKey(uninstallKey))
            {
                MySqlCommand cmd = connection.CreateCommand();
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Connection.Open();

                foreach(string skName in rk.GetSubKeyNames())
                {
                    using(RegistryKey sk = rk.OpenSubKey(skName))
                    {
                        int isUpdate = 0;

                        // If there is no value in DisplayName, skip this
                        // entry in the registry
                        if(sk.GetValue("DisplayName") == null)
                        {
                            continue;
                        }

                        string displayName = sk.GetValue("DisplayName").ToString();

                        if(displayName.Contains("'"))
                        {
                            displayName = displayName.Replace("'", "");
                        }
                        else
                        {
                            displayName = sk.GetValue("DisplayName").ToString();
                        }

                        string displayVersion = "";
                        string publisher = "";

                        if(sk.GetValue("DisplayVersion") != null)
                        {
                            displayVersion = sk.GetValue("DisplayVersion").ToString();
                        }

                        if(sk.GetValue("Publisher") != null)
                        {
                            publisher = sk.GetValue("Publisher").ToString();
                        }

                        // If the Key has a Value called 'ReleaseType' then this is not an update.
                        // If the 'DisplayName' contains the word 'Hotfix' it's an update
                        // If the 'DisplayName' contains the word 'KB' it's an update (KB = Microsoft KB article)
                        if(sk.GetValue("ReleaseType") != null || 
                           sk.GetValue("DisplayName").ToString().Contains("Hotfix") ||
                           sk.GetValue("DisplayName").ToString().Contains("Update") ||
                           sk.GetValue("DisplayName").ToString().Contains("KB"))
                        {
                            // this entry is an upgrade
                            isUpdate = 1;
                        }
                        
                        string commandText = "INSERT INTO software_info(serial_number, date_created, is_update, app_name, version, publisher) " +
                                             "VALUES ('" + @serialNumber + "','" + @dateCreated + "','" + isUpdate +
                                                      "','" + @displayName + "','" + @displayVersion + "','" + @publisher + "')";

                        cmd.CommandText = commandText;
                        cmd.ExecuteNonQuery();                                              
                    }
                }

                cmd.Connection.Close();
            }
        }

        /// <summary>
        /// This function will get the public IP Address of a machine if it is
        /// unable to ping the local SQL server.
        /// </summary>
        public static void NetworkInformation()
        {
            Console.WriteLine("Network Information Being Generated and Sent to Database");

            MySqlConnection connection;
            MySqlCommand cmd;

            string serialNumber = "";
            string publicIPAddress = "";
            int isStolen = 0;

            ManagementClass os = new ManagementClass("Win32_OperatingSystem");
            foreach(ManagementObject obj in os.GetInstances())
            {
                serialNumber = obj["SerialNumber"].ToString();
            }

            publicIPAddress = GetPulicIPAddress();

            // Check to see if you can ping the local MySQL server
            connection = new MySqlConnection(ConfigurationSettings.AppSettings["mysql_db"]);
            cmd = connection.CreateCommand();
            string sqlServerName = cmd.Connection.DataSource;
            
            Ping pingSender = new Ping();
            PingReply pingReply = pingSender.Send(sqlServerName);

            // If you can't reach the SQL Server, we are assuming that
            // this machine is not in the network and therefore is stolen
            if(pingReply.Status == IPStatus.DestinationHostUnreachable)
            {
                // We need to use the connection string for the public SQL Server so that
                // the machine it can insert the data over the internet
                connection = new MySqlConnection(ConfigurationSettings.AppSettings["public_sql"]);
                cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.Text;

                isStolen = 1;
            }
            else
            {
                Console.WriteLine("Currently in the network ... no Network Info sent to the server.");
                Console.WriteLine("Exiting.");
                return;
            }

            string cmdText = "INSERT INTO network_info(serial_number_id, date_created, " + "ip_address) " + 
                             "VALUES('" + serialNumber + "','" + DateTime.Now.ToString() + "','" + publicIPAddress + "')";

            cmd.CommandText = cmdText;
            cmd.Connection.Open();
            cmd.ExecuteNonQuery();
            cmd.Connection.Close();
        }

       /// <summary>
       /// This method will get the public ip address of the machine
       /// where the application is executed on
       /// </summary>
       /// <returns></returns>
        private static string GetPulicIPAddress()
        {
            string publicIPAddress = "";
            try
            {
                // See the 'public_ip_url' element in the app.config file to see what URL is
                // being used to get the information.
                WebRequest myRequest = WebRequest.Create(ConfigurationSettings.AppSettings["public_ip_url"]);
                myRequest.Credentials = CredentialCache.DefaultCredentials;

                // This request needs the UserAgent filled in
                // See: http://msdn.microsoft.com/en-us/library/456dfw4f(VS.71).aspx
                if(myRequest is HttpWebRequest)
                {
                    ((HttpWebRequest)myRequest).UserAgent = ".NET ReportGenerator";
                }

                // Send request, get response, get IP Address
                using(WebResponse res = myRequest.GetResponse())
                {
                    using(Stream s = res.GetResponseStream())
                    using(StreamReader sr = new StreamReader(s, Encoding.UTF8))
                    {
                        publicIPAddress = sr.ReadToEnd();
                    }

                    return publicIPAddress;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Getting Public IP Address:\n" + ex.Message);
                return publicIPAddress;
            }
        }
    }
}
