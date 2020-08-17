using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.IO;
using NLog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;


namespace MantisAlarmSys
{
    
    class Program
    {
        private static readonly Logger SLog = LogManager.GetCurrentClassLogger();
        private static IConfiguration config;
        private static IConfigurationBuilder builder;
        private static string path;
        static void Main(string[] args)
        {

            path = System.AppContext.BaseDirectory;


            builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($@"{path}\appsettings.json");

             config = new ConfigurationBuilder()
                .AddJsonFile($@"{path}\appsettings.json", true, true)
                .Build();

            MantisAlarm();
        }
        public static DateTime ConvertToDateTimeFromUnix(long unixTime)
        { 
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTime).ToLocalTime();
            return dtDateTime;
        }
        static void MantisAlarm()
        {



            path = System.AppContext.BaseDirectory;
            SLog.Info($"cesta {path}");

            PropertiesUserList properties = new PropertiesUserList();
            PropertiesIssueList propertiesIssue = new PropertiesIssueList();
            EmailProperties emailProperties = new EmailProperties();
            try 
            {
                Console.WriteLine($" Hello { config["name"] } !");
                string URL1 = $"{config["#"]}DBMantis/";
                string URL2 = $"{config["#"]}DBMantis1/";
            
                int count = 0;

                var IssueList = "";
                var UserList = "";


                string htmlString = "";


                SLog.Info($"Start|{DateTime.Now.ToString()}");
                

                using (WebClient wc = new WebClient())
                {
                    SLog.Info($"Stahovani|{URL2}|{DateTime.Now.ToString()}");                    
                    IssueList = wc.DownloadString(URL2);
                    SLog.Info($"Stahovani|{URL1}|{DateTime.Now.ToString()}");                   
                    UserList = wc.DownloadString(URL1);

                    List<PropertiesIssueList> ProIssueList = JsonConvert.DeserializeObject<List<PropertiesIssueList>>(IssueList.ToString());
                    List<PropertiesUserList> ProUserList = JsonConvert.DeserializeObject<List<PropertiesUserList>>(UserList.ToString());

                    var group = ProUserList.GroupBy(user => user.Id);
                
                
                    foreach(var item in ProIssueList)
                    {
                    
                        propertiesIssue.handlerId = item.handlerId;
                        propertiesIssue.bugId = item.bugId;
                        propertiesIssue.fieldName = item.fieldName;
                        propertiesIssue.newValue = item.newValue;
                        propertiesIssue.user_id = item.user_id;
                        emailProperties.IssueID = item.bugId;

                        emailProperties.Date = ConvertToDateTimeFromUnix(propertiesIssue.fieldName);
                        foreach (var Item in group)
                        {
                        
                            if (Item.Key == propertiesIssue.handlerId)
                            {
                                foreach(var user in Item)
                                {
                                
                                    if (user.Id == propertiesIssue.handlerId)
                                    {
                                        emailProperties.handlerName = user.username;
                                        emailProperties.email = user.email;
                                    }
                                    if(user.Id == propertiesIssue.user_id)
                                    {
                                        emailProperties.contractingAuthority = user.username;
                                    }
                                   
                                    if (emailProperties.Date < DateTime.Now) 
                                    { 
                                        htmlString = $"Úkol s ID: {propertiesIssue.bugId}, který má předpokládané datum splnění: {emailProperties.Date} nebyl splněn. <br/> Úkol, prosím, vyřeště nebo změňte předpokládané datum splnění!<br/>= = = = = = = = = = = = = = " +
                                                        $"<br/>Zadavatel: {emailProperties.contractingAuthority} <br/> Řešitel: {emailProperties.handlerName} <br/>Odkaz do Matis: #} <br/>= = = = = = = = = = = = = =<br/>Email byl odeslán automaticky.";
                                        Console.WriteLine(htmlString);
                                        Console.WriteLine(emailProperties.email);
                                    }
                                }
                            }
                        }  
                        count++; 
                    }

                
                }
                SLog.Info($"Pocet nalezenych problemu {count}");
                SLog.Info($"END.");
            }
            catch(Exception e)
            {
                SLog.Error($"{e.ToString()}");
            }
        }
        public static void Email(string htmlString, EmailProperties emailProperties)
        {
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtpClient = new SmtpClient();
                message.From = new MailAddress(config["SmtpFrom"]);
                message.To.Add(new MailAddress(emailProperties.email));
                message.Subject = $"Upozornění na nesplněný úkol {emailProperties.IssueID} | { emailProperties.Date}";
                message.IsBodyHtml = true;
                message.Body = htmlString;

                smtpClient.Port = int.Parse(config["SmtpPort"]);
                smtpClient.Host = config["SmtpServer"];
                smtpClient.EnableSsl = false;
                smtpClient.Credentials = new NetworkCredential(config["SmtpUser"], config["SmtpPassword"], config["SmtpServer"]);
                smtpClient.UseDefaultCredentials = false;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Send(message);

            }
            catch (Exception e)
            {
                SLog.Error($"{e.ToString()}");
            }
        }
    }
}
