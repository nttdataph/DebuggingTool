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
    public partial class BP08
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static string strTrigger = "";
        public List<string> logStr = new List<string>();
        public event EventHandler CompletedEvent;

        private void OnEvent()
        {
            if (CompletedEvent != null)
            {
                CompletedEvent(this, EventArgs.Empty);
            }
        }
        public void BP08Process()
        {
          try
          {
            //ProcessBP08();

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
        void ProcessBP08()
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

        public void batchProcessingForORGCHART(DataTable orgDt)
        {
            try
            {
                //Clear M_ORG_UNIT
                DBHelper.updateDataBySqlText(DBHelper.BP08_DEL_M_ORG_UNIT);

                string strRealo = string.Empty;
                string strDiv = string.Empty;
                string strDesc = string.Empty;
                string strDept = string.Empty;
                string strSec = string.Empty;
                string strCc = string.Empty;

                DataRow[] temp_list_div = orgDt.Select("[PARENT]='0'");

                foreach (var div in temp_list_div)
                {
                    strDiv = div["ID"].ToString();
                    strDesc = div["TEXT4"].ToString();
                    strRealo = div["REALO"].ToString();

                    string sqlStr = String.Format(DBHelper.BP08_INS_M_ORG_UNIT,
                                    strRealo,
                                    strDiv,
                                    0,
                                    0,
                                    0,
                                    StrFilter(strDesc),
                                    "O");
                    DBHelper.updateDataBySqlText(sqlStr);

                    DataRow[] temp_list_dept = orgDt.Select("[PARENT]='" + strDiv + "'");
                    foreach (var dept in temp_list_dept)
                    {
                        strDept = dept["ID"].ToString();
                        strDesc = dept["TEXT4"].ToString();
                        strRealo = dept["REALO"].ToString();

                        sqlStr = String.Format(DBHelper.BP08_INS_M_ORG_UNIT,
                                    strRealo,
                                    strDiv,
                                    strDept,
                                    0,
                                    0,
                                    StrFilter(strDesc),
                                    "O");
                        DBHelper.updateDataBySqlText(sqlStr);

                        DataRow[] temp_list_sect = orgDt.Select("[PARENT]='" + strDept + "'");
                        foreach (var sect in temp_list_sect)
                        {
                            strSec = sect["ID"].ToString();
                            strDesc = sect["TEXT4"].ToString();
                            strRealo = sect["REALO"].ToString();

                            sqlStr = String.Format(DBHelper.BP08_INS_M_ORG_UNIT,
                                    strRealo,
                                    strDiv,
                                    strDept,
                                    strSec,
                                    0,
                                    StrFilter(strDesc),
                                    "O");
                            DBHelper.updateDataBySqlText(sqlStr);

                            DataRow[] temp_list_cc = orgDt.Select("[PARENT]='" + strSec + "'");
                            foreach (var cc in temp_list_cc)
                            {
                                strCc = cc["ID"].ToString();
                                strDesc = cc["TEXT4"].ToString();
                                strRealo = cc["REALO"].ToString();

                                sqlStr = String.Format(DBHelper.BP08_INS_M_ORG_UNIT,
                                    strRealo,
                                    strDiv,
                                    strDept,
                                    strSec,
                                    strCc,
                                    StrFilter(strDesc),
                                    "K");
                                DBHelper.updateDataBySqlText(sqlStr);
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                logStr.Add("batchProcessingForORGCHART");
                if (ex.InnerException != null)
                {
                    logStr.Add(ex.InnerException.Message);
                }
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP08_log.txt", "BP08_log.txt");
            }

        }

        /// <summary>
        /// GM info
        /// </summary>
        /// <param name="bolyDt"></param>
        private void batchProcessingForBOSSONLY(DataTable bolyDt)
        {
            try
            {
                #region Clear M_BOSSONLY
                string sqlStr = DBHelper.BP08_DEL_M_BOSSONLY;
                DBHelper.updateDataBySqlText(sqlStr);
                #endregion

                bolyDt.DefaultView.Sort = "ID asc";

                DataTable temp_orgDt = bolyDt.DefaultView.ToTable();

                if (temp_orgDt != null && temp_orgDt.Rows.Count > 0)
                {
                    int x = 0, y = 0;

                    x = int.Parse(temp_orgDt.Rows[0]["ID"].ToString());
                    y = x + 2;


                    DataRow recordA = null, recordB = null;
                    for (; temp_orgDt.Select(string.Format("ID = '{0}'", x)).Length > 0 && temp_orgDt.Select(string.Format("ID = '{0}'", y)).Length > 0; x += 3, y = x + 2)
                    {
                        recordA = temp_orgDt.Select(string.Format("ID = '{0}'", x))[0];
                        recordB = temp_orgDt.Select(string.Format("ID = '{0}'", y))[0];

                        sqlStr = String.Format(DBHelper.BP08_INS_M_BOSSONLY,
                                    recordA["REALO"],
                                    recordA["TEXT4"],
                                    recordA["SHORT"],
                                    recordB["REALO"],
                                    recordB["TEXT4"]);

                        DBHelper.updateDataBySqlText(sqlStr);

                    }
                }

            }
            catch (System.Exception ex)
            {
                logStr.Add("batchProcessingForBOSSONLY");
                if (ex.InnerException != null)
                {
                    logStr.Add(ex.InnerException.Message);
                }
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP08_log.txt", "BP08_log.txt");
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
                string strBPID = CaptureLog(trigger);
                logStr.Add("BPID:" + strBPID);

                SapProfileResult result = null;
                SapConnectInfo info = new SapConnectInfo(ConfigurationManager.AppSettings["Z_HR_PA_VALTAB"]);
                Hashtable hash = new Hashtable();
                hash.Add(DBHelper.MODE_SAP, "TRANSFER");

                result = SapProfile.getSAPFunData_Z_HR_PA_VALTAB(info, hash);
                //result = CallSapProfileForUT();

                batchProcessingForORGCHART(result.T_ORGCHART);

                // GM info
                batchProcessingForBOSSONLY(result.T_BOSSONLY);

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
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP08_log.txt", "BP08_log.txt");
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
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP08_log.txt", "BP08_log.txt");
            }
        }

        #endregion



        #region DB

        #region batch common process

        /// <summary>
        /// insert content to T_BATCH_LOG
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public static string CaptureLog(string trigger)
        {
            string strBPIDNotFormat = System.DateTime.Now.ToString("yyyyMMddHHmmss").ToString();
            string strBPID = strBPIDNotFormat.Substring(0, strBPIDNotFormat.Length - 1);

            string sqlStr = String.Format(DBHelper.BP08_INS_T_BATCH_LOG,
                                        strBPID,
                                        trigger);

            DBHelper.updateDataBySqlText(sqlStr);

            return strBPID;
        }

        /// <summary>
        /// insert content to T_DOC_ATTACHMENT
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public static DataTable getBatchTimeFromDB()
        {
            string reTime = string.Empty;
            DataTable dt = DBHelper.getData(DBHelper.BP08_GET_BATCHRUNTIME);

            return dt;
        }

        #endregion

        #endregion

        /// <summary>
        /// test:get T_ORGCHART info
        /// </summary>
        /// <returns></returns>
        private SapProfileResult CallSapProfileForUT()
        {
            SapProfileResult result = new SapProfileResult();

            string selectSql = @"SELECT [ID],[PARENT],[TEXT4],[SHORT],[OTYPE],[REALO] FROM [xl_T_ORGCHART]";

            result.T_ORGCHART = DBHelper.getData(selectSql);

            selectSql = @"SELECT [ID],[PARENT],[TEXT4],[SHORT],[OTYPE],[REALO] FROM [xl_T_BOSSONLY]";

            result.T_BOSSONLY = DBHelper.getData(selectSql);

            return result;
        }
    }
}
