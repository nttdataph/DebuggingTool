using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using Canon.HRM.SAP.Common;
using System.Collections;
using Canon.HRM.Common;
using System.IO;
using System.Configuration;
using SAP.Middleware.Connector;

namespace CoreProcess
{
    public static class SAPHelper
    {
        public static List<string> logStr = new List<string>();
        /// <summary>
        /// SAP FUNCTION RUN
        /// </summary>
        /// <returns></returns>
        public static void DoSAPFunction()
        {
            Hashtable hash = new Hashtable();
            string empName = string.Empty;
            hash.Add(DBManager.MODE_SAP, "UPDATE");
            string errorMsg = string.Empty;
            SapConnectInfo info = new SapConnectInfo(ConfigurationManager.AppSettings["Z_HR_PA_EPROFILE"]);
            logStr.Add("AppSettings:" + ConfigurationManager.AppSettings["Z_HR_PA_EPROFILE"]);
            //createFile(logStr, "C:\\NTT\\ErrorLog\\BATCH_SAPHelper_log.txt", "BATCH_SAPHelper_log.txt");
            SapProfile.GetIRfcFunction(info, hash);
        }

        public static void DoSAPFunctionAccumulate()
        {
            Hashtable hash = new Hashtable();
            string empName = string.Empty;
            hash.Add(DBManager.MODE_SAP, "UPDATE_ACCUM");
            string errorMsg = string.Empty;
            SapConnectInfo info = new SapConnectInfo(ConfigurationManager.AppSettings["Z_HR_PA_LV_REIM_ACCU"]);
            logStr.Add("AppSettings:" + ConfigurationManager.AppSettings["Z_HR_PA_LV_REIM_ACCU"]);
            //createFile(logStr, "C:\\NTT\\ErrorLog\\BATCH_SAPHelper_log.txt", "BATCH_SAPHelper_log.txt");
            SapProfile.GetIRfcFunction(info, hash);
        }

        public static void DoSAPFunctionRei()
        {
            Hashtable hash = new Hashtable();
            string empName = string.Empty;
            hash.Add(DBManager.MODE_SAP, "UPDATE_REIMB");
            string errorMsg = string.Empty;
            SapConnectInfo info = new SapConnectInfo(ConfigurationManager.AppSettings["Z_HR_PA_LV_REIM_ACCU"]);
            logStr.Add("AppSettings:" + ConfigurationManager.AppSettings["Z_HR_PA_LV_REIM_ACCU"]);
            //createFile(logStr, "C:\\NTT\\ErrorLog\\BATCH_SAPHelper_log.txt", "BATCH_SAPHelper_log.txt");
            SapProfile.GetIRfcFunction(info, hash);
        }

        public static void DoSAPFunctionLeaveHistory()
        {
            Hashtable hash = new Hashtable();
            string empName = string.Empty;
            hash.Add(DBManager.MODE_SAP, "UPDATE");
            string errorMsg = string.Empty;
            SapConnectInfo info = new SapConnectInfo(ConfigurationManager.AppSettings["Z_HR_PA_LV_HISTORY"]);
            logStr.Add("AppSettings:" + ConfigurationManager.AppSettings["Z_HR_PA_LV_HISTORY"]);
            //createFile(logStr, "C:\\NTT\\ErrorLog\\BATCH_SAPHelper_log.txt", "BATCH_SAPHelper_log.txt");
            SapProfile.GetIRfcFunction(info, hash);
        }

        public static void DoSAPFunctionLeaveApply()
        {
            Hashtable hash = new Hashtable();
            string empName = string.Empty;
            hash.Add(DBManager.MODE_SAP, "UPDATE");
            string errorMsg = string.Empty;
            SapConnectInfo info = new SapConnectInfo(ConfigurationManager.AppSettings["Z_HR_PA_LV_APPLY"]);
            logStr.Add("AppSettings:" + ConfigurationManager.AppSettings["Z_HR_PA_LV_APPLY"]);
            //createFile(logStr, "C:\\NTT\\ErrorLog\\BATCH_SAPHelper_log.txt", "BATCH_SAPHelper_log.txt");
            SapProfile.GetIRfcFunction(info, hash);
        }

        public static bool getSAPFunData_Z_HR_PA_ATTEND_VIEW_File(SapConnectInfo info, Hashtable pars)
        {
            logStr.Add("getSAPFunData_Z_HR_PA_ATTEND_VIEW_File()  start");
            SapProfileResult result = new SapProfileResult();
            result.ErrorMessage = string.Empty;
            try
            {
                //Get Result
                result = SapProfile.chkErrorResult(info);
                if (!String.IsNullOrEmpty(result.ErrorMessage))
                {
                    logStr.Add("SAP error :" + result.ErrorMessage);
                    return false;
                }

                SapProfile.GetIRfcFunction(info, pars);
                return true;
            }
            catch (RfcCommunicationException ex)
            {
                //Communication Exception
                result.ErrorMessage = ex.Message;
                logStr.Add("ex.message:" + ex.Message.ToString());
                return false;
            }
            catch (Exception ex)
            {
                logStr.Add("ex.message:" + ex.Message.ToString());
                return false;
            }
            logStr.Add("getSAPFunData_Z_HR_PA_ATTEND_VIEW_File()  end");
            //createFile(logStr, "C:\\NTT\\ErrorLog\\getSAPFunData_Z_HR_PA_ATTEND_VIEW_File.txt", "getSAPFunData_Z_HR_PA_ATTEND_VIEW_File.txt");       
        }

        public static void createFile(List<string> content, string fullPath, string fileName)
        {
            try
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                using (StreamWriter sw = new StreamWriter(new FileStream(fullPath, FileMode.CreateNew), Encoding.GetEncoding("UTF-8")))
                {
                    foreach (string value in content)
                    {
                        sw.WriteLine(value);
                    }
                    sw.Close();
                }
            }
            catch (System.Exception ex)
            {
                logStr.Add("createFile");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BATCH_SAPHelper_log.txt", "BATCH_SAPHelper_log.txt");
            }
        }

    }
}
