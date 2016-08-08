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
    public partial class BP03
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string strTrigger = "";
        private List<string> logStr = new List<string>();
        private int maxDay = 0;
        public event EventHandler CompletedEvent;

        private void OnEvent()
        {
            if (CompletedEvent != null)
            {
                CompletedEvent(this, EventArgs.Empty);
            }
        }
        public void BP03Process()
        {
           

            try
            {
                
               // ProcessBP03();

            }
            catch (System.Exception ex)
            {
                //logStr.Add("OnStop");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP03_log.txt", "BP03_log.txt");
            }
            finally
            {
                OnEvent();
            }   

        }
        private static object Locker = new object();
        void ProcessBP03()
        {
            try
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
                        strTrigger = "system";
                        batchProcessingStart(strTrigger);

                    }
                }
            }
            catch (System.Exception ex)
            {
                logStr.Add("timer_Elapsed");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP03_log.txt", "BP03_log.txt");
            }
        }
        



        #region Method

        /// <summary>
        /// batchProcessingStart
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public Boolean batchProcessingStart(string trigger)
        {
            try
            {
                string strBPID = captureLog(trigger);

                if (batchProcessing(trigger))
                {
                    updateLog("0", strBPID);
                }
                else
                {
                    updateLog("1", strBPID);
                }

                return true;
            }
            catch (System.Exception ex)
            {
                logStr.Add("batchProcessingStart");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP03_log.txt", "BP03_log.txt");
                return false;
            }
        }

        public Boolean batchProcessing(string trigger)
        {
            try
            {
                string sqlStr = string.Empty;
                //refer to QA177: get maxday from DB table T_GLOBAL_VAR
                maxDay = int.Parse(DBHelper.getData(DBHelper.BP03_GET_MAXDAY).Rows[0][0].ToString().Trim());

                clearTableData();

                int x = 0;
                while (x != maxDay)
                {
                    DateTime date = DateTime.Now.AddDays(-x);
                    //get data of DB table:T_RAW_EMP_ATT 
                    DataTable rawDt = DBHelper.getData(string.Format(DBHelper.BP03_GET_T_RAW_EMP_ATT, date.ToString("yyyy-MM-dd")));
                    logStr.Add("rawDt");
                    if (rawDt != null && rawDt.Rows.Count > 0)
                    {
                        string cc = string.Empty;
                        string empid = string.Empty;
                        string shift = string.Empty;
                        string day = string.Empty;
                        string status = string.Empty;
                        string empName = string.Empty;
                        string empDept = string.Empty;
                        string dateStr = string.Empty;

                        foreach (DataRow dr in rawDt.Rows)
                        {
                            cc = dr["EMP_COSTCENTER"].ToString();
                            empid = dr["EMP_ID"].ToString();
                            shift = dr["SHIFT_GRP"].ToString();
                            day = dr["DAY"].ToString();
                            status = dr["STATUS"].ToString();
                            empName = dr["EMP_NAME"].ToString();
                            empDept = dr["EMP_DEPT"].ToString();
                            dateStr = dr["ATT_DATE"].ToString();

                            sqlStr = string.Format(DBHelper.BP03_INS_DATA_ATT_DAY,
                                                                                   cc,
                                                                                   empid,
                                                                                   shift,
                                                                                   day,
                                                                                   status,
                                                                                   StrFilter(empName),
                                                                                   empDept,
                                                                                   dateStr);
                            logStr.Add("1");
                            logStr.Add(sqlStr);
                            DBHelper.updateDataBySqlText(sqlStr);
                            logStr.Add("sqlStr");

                        }
                    }

                    x++;
                }

                string startDateStr = DateTime.Now.AddDays(-maxDay).ToString("yyyy-MM-dd");

                // Populate temporary cost center table from attendance
                sqlStr = string.Format(DBHelper.BP03_INS_TEMP_CC, startDateStr);
                logStr.Add("BP03_INS_TEMP_CC");
                logStr.Add(sqlStr);
                DBHelper.updateDataBySqlText(sqlStr);

                // For each cost center, need to caculate daily manpower%
                sqlStr = string.Format(DBHelper.BP03_INS_DATA_ATT_COSTCENTER, startDateStr);
                logStr.Add("BP03_INS_DATA_ATT_COSTCENTER");
                logStr.Add(sqlStr);
                DBHelper.updateDataBySqlText(sqlStr);

                // Populate temporary department and division table from attendance
                sqlStr = DBHelper.BP03_INS_TEMP_DEPT;
                sqlStr += DBHelper.BP03_INS_TEMP_DIV;
                logStr.Add("BP03_INS_TEMP_DIVBP03_INS_TEMP_DEPT");
                logStr.Add(sqlStr);
                DBHelper.updateDataBySqlText(sqlStr);

                // For each department and division, need to caculate daily manpower%
                sqlStr = string.Format(DBHelper.BP03_INS_DATA_ATT_DEPT_DIV, startDateStr);
                logStr.Add("BP03_INS_DATA_ATT_DEPT_DIV");
                logStr.Add(sqlStr);
                DBHelper.updateDataBySqlText(sqlStr);

                // Update current month and previous month statistics
                startDateStr = DateTime.Now.AddMonths(-1).AddDays(-(DateTime.Now.Day - 1)).ToString("yyyy-MM-dd");
                sqlStr = string.Format(DBHelper.BP03_INS_DATA_ATT_DD_MTH, startDateStr);
                logStr.Add("BP03_INS_DATA_ATT_DD_MTH");
                logStr.Add(sqlStr);
                DBHelper.updateDataBySqlText(sqlStr);
                logStr.Add("end ");
                return true;
            }
            catch (System.Exception ex)
            {
                logStr.Add("batchProcessing");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP03_log.txt", "BP03_log.txt");
                return false;
            }
        }

        /// <summary>
        /// Create error log file
        /// </summary>
        /// <param name="content"></param>
        /// <param name="fullPath"></param>
        /// <param name="fileName"></param>
        private void createFile(List<string> content, string fullPath, string fileName)
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
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP03_log.txt", "BP03_log.txt");
            }
        }
        #endregion

        #region DB Method
        /// <summary>
        /// get batch time
        /// </summary>
        /// <returns></returns>
        public DataTable getBatchTimeFromDB()
        {
            try
            {
                string reTime = string.Empty;
                DataTable dt = DBHelper.getData(DBHelper.BP03_GET_BATCHRUNTIME);
                return dt;
            }
            catch (System.Exception ex)
            {
                logStr.Add("getBatchTimeFromDB");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP03_log.txt", "BP03_log.txt");
                return null;
            }
        }
        /// <summary>
        /// insert into T_BATCH_LOG
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public string captureLog(string trigger)
        {
            try
            {
                string strBPIDNotFormat = System.DateTime.Now.ToString("yyyyMMddHHmmss").ToString();
                string strBPID = strBPIDNotFormat.Substring(0, strBPIDNotFormat.Length - 1);

                string sqlStr = String.Format(DBHelper.BP03_INS_T_BATCH_LOG,
                                            strBPID,
                                            trigger);

                DBHelper.updateDataBySqlText(sqlStr);

                return strBPID;
            }
            catch (System.Exception ex)
            {
                logStr.Add("captureLog");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP03_log.txt", "BP03_log.txt");
                return string.Empty;
            }
        }
        /// <summary>
        /// update T_BATCH_LOG
        /// </summary>
        /// <param name="status"></param>
        /// <param name="strBPID"></param>
        public void updateLog(string status, string strBPID)
        {
            try
            {
                string sqlStr = String.Format(DBHelper.BP03_UPD_T_BATCH_LOG,
                                            status,
                                            strBPID);

                DBHelper.updateDataBySqlText(sqlStr);

            }
            catch (System.Exception ex)
            {
                logStr.Add("updateLog");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP03_log.txt", "BP03_log.txt");
            }
        }
        /// <summary>
        /// clear data of tables : DATA_ATT_DAY, DATA_ATT_DEPT_DIV, DATA_ATT_DD_MTH
        /// </summary>
        public void clearTableData()
        {
            try
            {
                string sqlStr = String.Format(DBHelper.BP03_DEL_DATA_ATT_DAY, maxDay);
                sqlStr += String.Format(DBHelper.BP03_DEL_DATA_ATT_COSTCENTER, maxDay);
                sqlStr += String.Format(DBHelper.BP03_DEL_DATA_ATT_DEPT_DIV, maxDay);
                sqlStr += String.Format(DBHelper.BP03_DEL_DATA_ATT_DD_MTH, maxDay); //refer to QA181

                DBHelper.updateDataBySqlText(sqlStr);
            }
            catch (System.Exception ex)
            {
                logStr.Add("clearTableData");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP03_log.txt", "BP03_log.txt");
            }
        }
        #endregion

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
    }
}
