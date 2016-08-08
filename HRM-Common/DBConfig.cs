using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Configuration;

namespace Canon.HRM.Common
{
    public static class DBConfig
    {
        /// <summary>
        /// Get database conection string
        /// </summary>
        /// <returns></returns>
        public static string GetConnectionString()
        {
            return ConfigurationManager.AppSettings["HRMSystem"];
           // return ConfigurationManager.ConnectionStrings["HRMSystem"].ConnectionString;
        }
    }
}