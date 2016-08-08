using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Canon.HRM.SAP.Common;
using System.Configuration;
using System.Collections;
using Canon.HRM.Common;
using System.DirectoryServices;
using System.Web;
using SAP.Middleware.Connector;
using NLog;
using System.Timers;
using System.Data;
 

namespace CoreProcess.Threads
{
    public partial class BP04
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static string strTrigger = "";
        public List<string> logStr = new List<string>();
        public string strBPID = "";
        public event EventHandler CompletedEvent;

        private void OnEvent()
        {
            if (CompletedEvent != null)
            {
                CompletedEvent(this, EventArgs.Empty);
            }
        }
        public void BP04Process()
        {
            try
            {
            //ProcessBP04();

            }
            catch (System.Exception ex)
            {
                //logStr.Add("OnStart");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP00_log.txt", "BP00_log.txt");
            }
            finally
            {
                OnEvent();
            }     
           
        }
        private static object Locker = new object();
        void ProcessBP04()
        {
            string CurrTime = System.DateTime.Now.ToString("HH:mm:ss").ToString();
            DataTable dt = getBatchTimeFromDB();
            bool batchRun = false;
            string mode = string.Empty;
            string time = string.Empty;

            lock (Locker)
            {
                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        mode = row["MODE"].ToString().Trim();
                        time = row["SCH_TIME"].ToString().Trim();
                        if (mode.Equals("1"))
                        {
                            //logStr.Add("mode:" + mode);
                            if (CurrTime.Equals(time))
                            {
                                batchRun = true;
                                break;
                            }
                        }
                        else if (mode.Equals("2"))
                        {
                            //logStr.Add("mode:" + mode);
                            batchRun = false;
                        }
                        else if (mode.Equals("0"))
                        {
                            //logStr.Add("mode:" + mode);
                            //logStr.Add(CurrTime.Substring(3, 5));
                            if (CurrTime.Substring(3, 5).Equals("00:00"))
                            {
                                batchRun = true;
                                break;
                            }
                            else
                            {
                                batchRun = false;
                            }
                        }
                    }
                }

                if (batchRun == true)
                {
                    //TODO
                    //strTrigger = "current user";
                    batchProcessingStart(strTrigger);

                }
            }
        }
       


        #region method

        /// <summary>
        /// SQL Format
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string StrFilter(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            if (str.Contains("'"))
            {
                str = str.Replace("'", "''");
                return str;
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        /// batchProcessing
        /// </summary>
        public void batchProcessing(DataTable dt)
        {
            try
            {
                string empId = string.Empty;
                string attDt = string.Empty;
                string empName = string.Empty;
                string empDept = string.Empty;
                string empCc = string.Empty;
                string shift = string.Empty;
                string day = string.Empty;
                string status = string.Empty;

                foreach (DataRow dr in dt.Rows)
                {

                    empId = dr["PERNR"].ToString();
                    attDt = dr["DATUM"].ToString();
                    empName = StrFilter(dr["NAME"].ToString());
                    empDept = dr["ORGEH"].ToString();
                    empCc = dr["KOSTL"].ToString();
                    shift = dr["SCHKZ"].ToString();
                    day = dr["KURZT"].ToString();
                    status = dr["TPROG"].ToString();

                    string sqlStrUpd = string.Format(DBHelper.BP04_INS_T_RAW_EMP_ATT, empId, attDt, empName, empDept,
                                                 empCc, shift, day, status);
                    DBHelper.updateDataBySqlText(sqlStrUpd);

                }

                updCaptureLog("0");
            }
            catch (System.Exception ex)
            {
                updCaptureLog("1");

                logStr.Add("batchProcessing");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP04_log.txt", "BP04_log.txt");
            }
        }

        /// <summary>
        /// batchProcessingStart
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public Boolean batchProcessingStart(string trigger)
        {
            try
            {
                logStr.Add("batchProcessingStart");

                //CAPTURE LOG
                strBPID = CaptureLog(trigger);
                logStr.Add("BPID:" + strBPID);

                //Clear old records
                DBHelper.updateDataBySqlText(DBHelper.BP04_DEL_T_RAW_EMP_ATT1);

                //Clear records before getting updated ones
                double maxdays = double.Parse(DBHelper.getData(DBHelper.BP04_GET_MAXDAY).Rows[0][0].ToString());
                DBHelper.updateDataBySqlText(string.Format(DBHelper.BP04_DEL_T_RAW_EMP_ATT2, -maxdays));

                SapProfileResult result = new SapProfileResult();
                SapConnectInfo info = new SapConnectInfo(ConfigurationManager.AppSettings["Z_HR_PA_ATTEND_VIEW"]);
                Hashtable hash = new Hashtable();
                hash.Add(DBHelper.BEGINDATE_SAP, DateTime.Now.AddDays(-maxdays).ToString("yyyyMMdd"));
                hash.Add(DBHelper.ENDDATE_SAP, DateTime.Now.ToString("yyyyMMdd"));
                hash.Add(DBHelper.PERNR_SAP, string.Empty);

                if (SAPHelper.getSAPFunData_Z_HR_PA_ATTEND_VIEW_File(info, hash))
                {
                    batchProcessing(changeFileToDataTable());
                }

                return true;
            }
            catch (System.Exception ex)
            {
                logStr.Add("batchProcessingStart");
                if (ex.InnerException != null)
                {
                    logStr.Add(ex.InnerException.Message);
                }
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP04_log.txt", "BP04_log.txt");
                return false;
            }
        }

        public void createFile(List<string> content, string fullPath, string fileName)
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
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP04_log.txt", "BP04_log.txt");
            }
        }


        private DataTable changeFileToDataTable()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("PERNR", typeof(String));
                dt.Columns.Add("NAME", typeof(String));
                dt.Columns.Add("SCHKZ", typeof(String));
                dt.Columns.Add("DATUM", typeof(String));
                dt.Columns.Add("KURZT", typeof(String));
                dt.Columns.Add("TPROG", typeof(String));
                dt.Columns.Add("KOSTL", typeof(String));
                dt.Columns.Add("ORGEH", typeof(String));

                //get file
                string fullPath = DBHelper.getData(DBHelper.BP04_GET_SAP_FOLDER).Rows[0][0].ToString();
                string fileName = "eLeave_AO.txt";//QA

                //find folder
                if (!System.IO.Directory.Exists(fullPath))
                {
                    logStr.Add("There is no have this path:" + fullPath);
                }
                else
                {
                    //find file
                    if (!File.Exists(fullPath + fileName))
                    {
                        logStr.Add("There is no have this path:" + fullPath);
                    }
                    else
                    {
                        using (StreamReader sr = new StreamReader(new FileStream(fullPath + fileName, FileMode.Open), Encoding.GetEncoding("UTF-8")))
                        {
                            while (!sr.EndOfStream)
                            {
                                string line = sr.ReadLine();
                                string[] line_items = line.Split('|');

                                DataRow dr = dt.NewRow();
                                dr[0] = line_items[0];
                                dr[1] = line_items[1];
                                dr[2] = line_items[2];
                                dr[3] = line_items[3];
                                dr[4] = line_items[4];
                                dr[5] = line_items[5];
                                dr[6] = line_items[6];
                                dr[7] = line_items[7];

                                dt.Rows.Add(dr);
                            }
                            sr.Close();
                        }
                    }
                }
                return dt;
            }
            catch (Exception ex)
            {
                logStr.Add("changeFileToDataTable");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP04_log.txt", "BP04_log.txt");

                return null;
            }


        }

        #endregion

        #region DB

        #region batch common process

        public static string CaptureLog(string trigger)
        {
            string strBPIDNotFormat = System.DateTime.Now.ToString("yyyyMMddHHmmss").ToString();
            string strBPID = strBPIDNotFormat.Substring(0, strBPIDNotFormat.Length - 1);

            string sqlStr = String.Format(DBHelper.BP04_INS_T_BATCH_LOG,
                                        strBPID,
                                        trigger);

            DBHelper.updateDataBySqlText(sqlStr);

            //if (trigger.Equals("system"))
            //{
            //    return strBPID;
            //}
            //else
            //{
            //    return strBPIDNotFormat;
            //}

            return strBPID;
        }

        public void updCaptureLog(string status)
        {
            string sqlStr = String.Format(DBHelper.BP04_UPD_T_BATCH_LOG,
                                        status,
                                        strBPID);

            DBHelper.updateDataBySqlText(sqlStr);
        }

        /// <summary>
        /// getBatchTimeFromDB
        /// </summary>
        /// <returns></returns>
        public static DataTable getBatchTimeFromDB()
        {
            string reTime = string.Empty;
            DataTable dt = DBHelper.getData(DBHelper.BP04_GET_BATCHRUNTIME);

            return dt;
        }

        #endregion

        #endregion
    }
}
