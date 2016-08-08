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
using System.Data;

namespace CoreProcess.Threads
{
    public partial class BP00
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public List<string> logStr = new List<string>();
        string batchFilePath = string.Empty;
        string tempPath = string.Empty;
        string strErrorFilePath = string.Empty;
        string strErrorBackUpPath = string.Empty;

        public event EventHandler CompletedEvent;
        
        private void OnEvent()
        {
            if (CompletedEvent != null)
            {
                CompletedEvent(this, EventArgs.Empty);
            }
        }

        public void BP00Process()
        {
            try
            {
                //SELECT GLOBAL SETTING GET Batch File Path
                batchFilePath = getBatchFilePathFromDB();
                //logStr.Add("batchFilePath");
                tempPath = getTempPathFromDB();
                //logStr.Add("tempPath");
                strErrorFilePath = getBatchErrorFilePathFromDB();
                strErrorBackUpPath = getErrorFileBackUpFolder();

                //ProcessBP00();
          
            }
            catch (System.Exception ex)
            {
                logger.Error(ex.Message);
            }
            finally
            {
                OnEvent();
            }            
        }

        private static object Locker = new object();

        void ProcessBP00()
        {
            try
            {
                bool isFilemoved = false;
                lock (Locker)
                {
                    if (!System.IO.Directory.Exists(batchFilePath))
                    {
                        System.IO.Directory.CreateDirectory(batchFilePath);
                    }
                    
                    moveFileForBP07();
                    //move file
                    if (System.IO.Directory.Exists(tempPath))
                    {
                        DirectoryInfo di = new DirectoryInfo(tempPath);
                        foreach (FileInfo fi in di.GetFiles())
                        {
                            if ((fi.Name.StartsWith("eProfile") && fi.Name.Length == 29) ||
                                (fi.Name.StartsWith("eLeave") && (fi.Name.Length == 27 || fi.Name.Length == 28)))
                            {
                                fi.MoveTo(batchFilePath + fi.Name);
                                isFilemoved = true;
                            }
                        }
                    }
                }
                //logStr.Add("isFilemoved:" + isFilemoved.ToString());
                if (isFilemoved)
                {
                    if (!SapProfile.chkSAPIsConnected())
                    {
                        insT_BATCH_LOG("system");
                    }
                    else
                    {
                        if (!SapProfile.chkSAPIsLocked())
                        {
                            SAPHelper.DoSAPFunction();
                            SAPHelper.DoSAPFunctionAccumulate();
                            SAPHelper.DoSAPFunctionRei();
                            SAPHelper.DoSAPFunctionLeaveHistory();
                            SAPHelper.DoSAPFunctionLeaveApply();
                        }
                    }
                }
                //createFile(logStr, "C:\\NTT\\ErrorLog\\BP00_log.txt", "BP00_log.txt");
            }
            catch (System.Exception ex)
            {
                logger.Error(ex.Message);
            }
        }
        
        /// <summary>
        /// write log
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
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
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP00_log.txt", "BP00_log.txt");
            }
        }

        /// <summary>
        /// insert content to T_DOC_ATTACHMENT
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public static string getBatchFilePathFromDB()
        {
            string reFilePath = "";
            DataTable dt = DBHelper.getData(DBHelper.BP00_GET_BATCHFILEPATH);
            if (dt.Rows.Count > 0)
            {
                reFilePath = dt.Rows[0]["VALUE"].ToString();
                if (reFilePath.Substring(reFilePath.Length - 1, 1) != "\\")
                {
                    reFilePath = reFilePath + "\\";
                }
            }
            return reFilePath;
        }

        /// <summary>
        /// insert content to T_DOC_ATTACHMENT
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public static string getTempPathFromDB()
        {
            string reFilePath = "";
            DataTable dt = DBHelper.getData(DBHelper.BP01_GET_BATCHFILEPATH);
            if (dt.Rows.Count > 0)
            {
                reFilePath = dt.Rows[0]["VALUE"].ToString();
                if (reFilePath.Substring(reFilePath.Length - 1, 1) != "\\")
                {
                    reFilePath = reFilePath + "\\";
                }
            }
            return reFilePath;
        }

        public static void insT_BATCH_LOG(string trigger)
        {
            string strBPIDNotFormat = System.DateTime.Now.ToString("yyyyMMddHHmmss").ToString();
            string strBPID = strBPIDNotFormat.Substring(0, strBPIDNotFormat.Length - 1);

            string sqlStr = String.Format(DBHelper.INS_T_BATCH_LOG_FOR_SAPERROR,
                                        strBPID, "BP00",
                                        trigger);

            DBHelper.updateDataBySqlText(sqlStr);

        }

        public void moveFileForBP07()
        {


            //get all BP07 run success error file name in T_BATCH_ERROR
            DataTable dt = DBHelper.getData(DBHelper.BP01_QRY_T_BATCH_ERROR);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //move file
                if (System.IO.Directory.Exists(strErrorFilePath))
                {
                    DirectoryInfo di = new DirectoryInfo(strErrorFilePath);
                    foreach (FileInfo fi in di.GetFiles())
                    {
                        if (fi.Name.Contains(dt.Rows[i]["FILENAME"].ToString()))
                        {
                            fi.MoveTo(strErrorBackUpPath + fi.Name);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// SQL1: SEARCH ERROR FOLDER PATH
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public string getBatchErrorFilePathFromDB()
        {
            string reFilePath = "";
            DataTable dt = DBHelper.getData(DBHelper.BP07_GET_BATCHFILEPATH);
            if (dt.Rows.Count > 0)
            {
                reFilePath = dt.Rows[0]["VALUE"].ToString();
            }
            return reFilePath;
        }

        /// <summary>
        /// error file back up folder path
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public static string getErrorFileBackUpFolder()
        {
            string reFilePath = "";
            DataTable dt = DBHelper.getData(DBHelper.BP01_GET_ERROR_FILE_BACKUP_FOLDER);
            if (dt.Rows.Count > 0)
            {
                reFilePath = dt.Rows[0]["VALUE"].ToString();
                if (reFilePath.Substring(reFilePath.Length - 1, 1) != "\\")
                {
                    reFilePath = reFilePath + "\\";
                }
            }
            return reFilePath;
        }

    }
}
