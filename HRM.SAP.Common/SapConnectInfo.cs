using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Canon.HRM.SAP.Common
{
    public class SapConnectInfo
    {
        public SapConnectInfo() { 
        
        }
        /// <summary>
        /// get myPfrole SAP connect information from config file 
        /// </summary>
        /// <param name="FunctionName"></param>
        public SapConnectInfo(string FunctionName)
        {
            this.Ip = ConfigurationManager.AppSettings["Ip"];
            this.SystemID = ConfigurationManager.AppSettings["SystemID"];
            this.Client = ConfigurationManager.AppSettings["Client"];
            this.UserName = ConfigurationManager.AppSettings["UserName"];
            this.Password = ConfigurationManager.AppSettings["Password"];
            this.FunctionName = FunctionName;
        }
        /// <summary>
        /// Ip
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Ip { get; set; }

        /// <summary>
        /// SystemID
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string SystemID { get; set; }

        /// <summary>
        /// Client
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Client { get; set; }

        /// <summary>
        /// UserName
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string UserName { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Password { get; set; }

        /// <summary>
        /// FunctionName
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string FunctionName { get; set; }

    }
}
