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
    public partial class BP07
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public ArrayList BPIDList = null;
        public string strREFBPID = "";
        public string strEurBPID = string.Empty;
        public bool errorFlg = false;
        public bool isAnyArchiveFileExist = false;
        public bool isAnyErrorFileExist = false;
        public string strTrigger = "";
        public List<string> logStr = new List<string>();
        public event EventHandler CompletedEvent;

        private void OnEvent()
        {
            if (CompletedEvent != null)
            {
                CompletedEvent(this, EventArgs.Empty);
            }
        }
        public void BP07Process()
        {
            try
            {
                //ProcessBP07();

            }
            catch (System.Exception ex)
            {
                //logStr.Add("OnStart");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP07_log.txt", "BP07_log.txt");
            }
            finally
            {
                OnEvent();
            }     
        }
        private static object Locker = new object();
        void ProcessBP07()
        {
            try
            {
                string CurrTime = System.DateTime.Now.ToString("HH:mm:ss").ToString();
                DataTable dt = getBatchTimeFromDB();
                bool batchRun = false;
                string mode = string.Empty;
                string time = string.Empty;
                string runHourTime = string.Empty;
                string runMinTime = string.Empty;
                string runSecTime = string.Empty;

                //logStr.Add("time:" + time);
                lock (Locker)
                {
                    if (dt != null)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            mode = row["MODE"].ToString().Trim();

                            if (row["HOUR_TIME"].ToString().Length < 2)
                            {
                                runHourTime = row["HOUR_TIME"].ToString().PadLeft(2, '0');
                            }
                            else
                            {
                                runHourTime = row["HOUR_TIME"].ToString();
                            }

                            if (row["MIN_TIME"].ToString().Length < 2)
                            {
                                runMinTime = row["MIN_TIME"].ToString().PadLeft(2, '0');
                            }
                            else
                            {
                                runMinTime = row["MIN_TIME"].ToString();
                            }

                            if (row["SEC_TIME"].ToString().Length < 2)
                            {
                                runSecTime = row["SEC_TIME"].ToString().PadLeft(2, '0');
                            }
                            else
                            {
                                runSecTime = row["SEC_TIME"].ToString();
                            }

                            if (mode.Equals("1"))
                            {
                                //hh:mm:ss begin run
                                //logStr.Add("mode:" + mode);
                                time = runHourTime + ":" + runMinTime + ":" + runSecTime;
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
                                //support up to Minute only,need not think over 60min
                                //mm:ss begin run
                                string strRealRunTime = runMinTime + ":00";

                                if (CurrTime.Substring(3, 5).Equals(strRealRunTime))
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
            logStr.Add(ex.Message.ToString());
            createFile(logStr, "C:\\NTT\\ErrorLog\\BP07_log.txt", "BP07_log.txt");
        }
    
    }   
        #region method

        public void batchProcessing(string fileNamePI, string strBPID)
        {
            StatusUpdate(strBPID);


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
                BPIDList = new ArrayList();
                string batchErrorFilePath = getBatchErrorFilePathFromDB();

                //batchArchiveFilePath = getBatchArchiveFilePathFromDB();
                string isError = string.Empty; ;

                if (!GetBPIDList(batchErrorFilePath))
                {
                    return false;
                }
                strEurBPID = GetBPID();
                InsertStatusUpdate(strEurBPID, "1", trigger, "");

                string strGetReqNoByBPID = string.Empty;

                foreach (string strREFBPID in BPIDList)
                {

                    if (IsFileExist(batchErrorFilePath, "eProfile_WD_" + strREFBPID + "_ERROR.txt"))
                    {
                        doProcess(batchErrorFilePath, "eProfile_WD_" + strREFBPID + "_ERROR.txt", "WD", strREFBPID, trigger);
                    }

                    if (IsFileExist(batchErrorFilePath, "eProfile_PI_" + strREFBPID + "_ERROR.txt"))
                    {
                        doProcess(batchErrorFilePath, "eProfile_PI_" + strREFBPID + "_ERROR.txt", "PI", strREFBPID, trigger);
                    }

                    if (IsFileExist(batchErrorFilePath, "eProfile_SD_" + strREFBPID + "_ERROR.txt"))
                    {
                        doProcess(batchErrorFilePath, "eProfile_SD_" + strREFBPID + "_ERROR.txt", "SD", strREFBPID, trigger);
                    }

                    if (IsFileExist(batchErrorFilePath, "eProfile_IN_" + strREFBPID + "_ERROR.txt"))
                    {
                        doProcess(batchErrorFilePath, "eProfile_IN_" + strREFBPID + "_ERROR.txt", "IN", strREFBPID, trigger);
                    }

                    if (IsFileExist(batchErrorFilePath, "eProfile_ED_" + strREFBPID + "_ERROR.txt"))
                    {
                        doProcess(batchErrorFilePath, "eProfile_ED_" + strREFBPID + "_ERROR.txt", "ED", strREFBPID, trigger);
                    }

                    if (IsFileExist(batchErrorFilePath, "eProfile_EC_" + strREFBPID + "_ERROR.txt"))
                    {
                        //refer to  QA144
                        doProcess(batchErrorFilePath, "eProfile_EC_" + strREFBPID + "_ERROR.txt", "EC", strREFBPID, trigger);
                    }

                    if (IsFileExist(batchErrorFilePath, "eLeave_LAC_" + strREFBPID + "_ERROR.txt"))
                    {
                        doProcessForMyLeave(batchErrorFilePath, "eLeave_LAC_" + strREFBPID + "_ERROR.txt", "LAC", strREFBPID, trigger);
                    }

                    if (IsFileExist(batchErrorFilePath, "eLeave_LR_" + strREFBPID + "_ERROR.txt"))
                    {
                        doProcessForMyLeave(batchErrorFilePath, "eLeave_LR_" + strREFBPID + "_ERROR.txt", "LR", strREFBPID, trigger);
                    }

                    if (IsFileExist(batchErrorFilePath, "eLeave_LC_" + strREFBPID + "_ERROR.txt"))
                    {
                        doProcessForMyLeave(batchErrorFilePath, "eLeave_LC_" + strREFBPID + "_ERROR.txt", "LC", strREFBPID, trigger);
                    }

                    if (IsFileExist(batchErrorFilePath, "eLeave_LAP_" + strREFBPID + "_ERROR.txt"))
                    {
                        doProcessForMyLeave(batchErrorFilePath, "eLeave_LAP_" + strREFBPID + "_ERROR.txt", "LAP", strREFBPID, trigger);
                    }

                    strGetReqNoByBPID = "'" + strREFBPID.Substring(0, 10) + "'," + strGetReqNoByBPID;
                }



                Update_T_BATCH_LOG_Status(strEurBPID);
                //logStr.Add("strGetReqNoByBPID" + strGetReqNoByBPID);
                if (!string.IsNullOrEmpty(strGetReqNoByBPID))
                {
                    SendMail(strGetReqNoByBPID.Substring(0, strGetReqNoByBPID.Length - 1));
                }
                else
                {
                    SendMail(strGetReqNoByBPID);
                }

                return true;
            }
            catch (Exception ex)
            {
                logStr.Add("batchProcessingStart");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP07_log.txt", "BP07_log.txt");
                return false;
            }
        }

        /// <summary>  
        /// check file  
        /// </summary> 
        public bool IsFileExist(string filePath, string fileName)
        {
            if (!File.Exists(filePath + fileName))
            {
                return false;
            }
            else
            {
                return true;
            }

        }


        private void doProcess(string filePath, string fileName, string type, string strBPID, string trriger)
        {
            StreamReader sr = new StreamReader(filePath + fileName);
            try
            {
                strBPID = strBPID.Substring(0, 10);
                string strRecord = string.Empty;

                while (!sr.EndOfStream)
                {
                    strRecord = sr.ReadLine();
                    string empid = strRecord.Split('|')[0].Substring(3, 5);
                    string subty = strRecord.Split('|')[2];
                    string infoty = strRecord.Split('|')[1];
                    string objps = strRecord.Split('|')[3];
                    string seq = strRecord.Split('|')[6];
                    string begda = strRecord.Split('|')[5];
                    string error = string.Empty;
                    string reqtype = string.Empty;
                    DataTable dt = null;
                    switch (type)
                    {
                        case "WD":
                            string epfno = strRecord.Split('|')[7].Trim();
                            string socso = strRecord.Split('|')[8].Trim();
                            string taxno = strRecord.Split('|')[9].Trim();
                            string passport = strRecord.Split('|')[11].Trim();

                            if (!string.IsNullOrEmpty(epfno))
                            {
                                reqtype = "EPF";
                                dt = getT_REQUEST_HRP(empid, strBPID, "'" + reqtype + "'");
                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    updataErrorInfo(strBPID, dt.Rows[0]["REQ_NO"].ToString(), empid, dt.Rows[0]["EMP_NAME"].ToString(),
                                        reqtype, error, trriger, infoty, subty, seq, objps, fileName);
                                }
                            }

                            if (!string.IsNullOrEmpty(socso))
                            {
                                reqtype = "SOC";
                                dt = getT_REQUEST_HRP(empid, strBPID, "'" + reqtype + "'");
                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    updataErrorInfo(strBPID, dt.Rows[0]["REQ_NO"].ToString(), empid, dt.Rows[0]["EMP_NAME"].ToString(),
                                        reqtype, error, trriger, infoty, subty, seq, objps, fileName);
                                }
                            }

                            if (!string.IsNullOrEmpty(taxno))
                            {
                                reqtype = "TAX";
                                dt = getT_REQUEST_HRP(empid, strBPID, "'" + reqtype + "'");
                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    updataErrorInfo(strBPID, dt.Rows[0]["REQ_NO"].ToString(), empid, dt.Rows[0]["EMP_NAME"].ToString(),
                                        reqtype, error, trriger, infoty, subty, seq, objps, fileName);
                                }
                            }

                            if (!string.IsNullOrEmpty(passport))
                            {
                                reqtype = "'PA1','PAS'";
                                dt = getT_REQUEST_HRP(empid, strBPID, reqtype);
                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    updataErrorInfo(strBPID, dt.Rows[0]["REQ_NO"].ToString(), empid, dt.Rows[0]["EMP_NAME"].ToString(),
                                        dt.Rows[0]["REQ_TYPE"].ToString(), error, trriger, infoty, subty, seq, objps, fileName);
                                }
                            }

                            break;
                        case "PI":
                            reqtype = "PER";
                            dt = getT_REQUEST_HRP(empid, strBPID, "'" + reqtype + "'");
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                updataErrorInfo(strBPID, dt.Rows[0]["REQ_NO"].ToString(), empid, dt.Rows[0]["EMP_NAME"].ToString(),
                                    reqtype, error, trriger, infoty, subty, seq, objps, fileName);
                            }
                            break;
                        case "SD":
                            error = strRecord.Split('|')[22].Trim();

                            if (subty.Equals("1"))
                            {
                                reqtype = "SPO";
                                dt = DBHelper.getData(string.Format(DBHelper.BP07_GET_T_REQUEST_HRP_AND_T_SP_LIST, empid, strBPID, reqtype, objps, seq));

                            }
                            else
                            {
                                reqtype = "DEP";
                                dt = DBHelper.getData(string.Format(DBHelper.BP07_GET_T_REQUEST_HRP_AND_T_SP_DEPENDANT, empid, strBPID, reqtype, objps, seq));
                            }

                            if (dt != null && dt.Rows.Count > 0)
                            {
                                updataErrorInfo(strBPID, dt.Rows[0]["REQ_NO"].ToString(), empid, dt.Rows[0]["EMP_NAME"].ToString(),
                                    reqtype, error, trriger, infoty, subty, seq, objps, fileName);
                            }
                            break;

                        case "IN":

                            reqtype = "INS";
                            dt = DBHelper.getData(string.Format(DBHelper.BP07_GET_T_REQUEST_HRP_AND_T_INS_NOMINEE, empid, strBPID, reqtype, subty));
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                updataErrorInfo(strBPID, dt.Rows[0]["REQ_NO"].ToString(), empid, dt.Rows[0]["EMP_NAME"].ToString(),
                                    reqtype, error, trriger, infoty, subty, seq, objps, fileName);
                            }
                            break;
                        case "ED":
                            reqtype = "ACA";
                            string strEDU_CERT = strRecord.Split('|')[11].Trim();
                            dt = DBHelper.getData(string.Format(DBHelper.BP07_GET_T_REQUEST_HRP_AND_T_ACADEMIC, empid, strBPID, reqtype, begda.Substring(0, 4) + "-" + begda.Substring(4, 2) + "-" + begda.Substring(6, 2), strEDU_CERT));
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                updataErrorInfo(strBPID, dt.Rows[0]["REQ_NO"].ToString(), empid, dt.Rows[0]["EMP_NAME"].ToString(),
                                    reqtype, error, trriger, infoty, subty, seq, objps, fileName);
                            }
                            break;
                        case "EC":
                            //refer to QA139
                            reqtype = "EME";
                            dt = DBHelper.getData(string.Format(DBHelper.BP07_GET_T_REQUEST_HRP_AND_T_EME_CTC_LIST, empid, strBPID, reqtype, seq));
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                updataErrorInfo(strBPID, dt.Rows[0]["REQ_NO"].ToString(), empid, dt.Rows[0]["EMP_NAME"].ToString(),
                                    reqtype, error, trriger, infoty, subty, seq, objps, fileName);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                logStr.Add("doProcess");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP07_log.txt", "BP07_log.txt");

            }
            finally
            {
                sr.Close();
            }


        }

        private void doProcessForMyLeave(string filePath, string fileName, string type, string strBPID, string trriger)
        {
            StreamReader sr = new StreamReader(filePath + fileName);
            try
            {
                strBPID = strBPID.Substring(0, 10);
                string strRecord = string.Empty;

                while (!sr.EndOfStream)
                {
                    strRecord = sr.ReadLine();
                    string empid = strRecord.Split('|')[0].Substring(3, 5);
                    string sapLty = strRecord.Split('|')[2];
                    string leaveEn = convertDateFormat(strRecord.Split('|')[4]);
                    string leaveSt = convertDateFormat(strRecord.Split('|')[5]);
                    //string quota = strRecord.Split('|')[7];
                    string cancel = string.Empty;
                    string error = string.Empty;

                    DataTable dt = null;
                    switch (type)
                    {
                        case "LAC":
                        case "LR":
                            dt = DBHelper.getData(string.Format(DBHelper.BP07_GET_T_REQUEST_HRL_T_ACCUMULATE, empid, strBPID));
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                updataErrorInfo2ForMyLeave(strBPID, dt.Rows[0]["REQ_NO"].ToString(), empid, dt.Rows[0]["EMP_NAME"].ToString(),
                                    dt.Rows[0]["REQ_TYPE"].ToString(), error, trriger, fileName, leaveSt, leaveEn);
                            }
                            break;
                        case "LC":
                            cancel = "X";
                            dt = DBHelper.getData(string.Format(DBHelper.BP07_GET_T_REQUEST_HRL_T_LEAVE_DET, empid, strBPID, cancel, sapLty, leaveSt, leaveEn));
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                updataErrorInfo1ForMyLeave(strBPID, dt.Rows[0]["REQ_NO"].ToString(), empid, dt.Rows[0]["EMP_NAME"].ToString(),
                                    dt.Rows[0]["REQ_TYPE"].ToString(), error, trriger, fileName, leaveSt, leaveEn, cancel);
                            }
                            break;
                        case "LAP":
                            dt = DBHelper.getData(string.Format(DBHelper.BP07_GET_T_REQUEST_HRL_T_LEAVE_DET, empid, strBPID, cancel, sapLty, leaveSt, leaveEn));
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                updataErrorInfo1ForMyLeave(strBPID, dt.Rows[0]["REQ_NO"].ToString(), empid, dt.Rows[0]["EMP_NAME"].ToString(),
                                    dt.Rows[0]["REQ_TYPE"].ToString(), error, trriger, fileName, leaveSt, leaveEn, cancel);
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                logStr.Add("doProcessForMyLeave");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP07_log.txt", "BP07_log.txt");
            }
            finally
            {
                sr.Close();
            }
        }


        /// <summary>  
        /// send mail  
        /// </summary> 
        public void SendMail(string strBPID)
        {
            DataTable dt_Mail = DBHelper.getData(string.Format(DBHelper.BP07_QRY_M_EMAIL_FMT, ConfigurationManager.AppSettings["BP07NotifyEmailFormatID"]));
            if (dt_Mail.Rows.Count == 0)
            {
                return;
            }

            string strMailErrorReqNO = string.Empty;
            if (!string.IsNullOrEmpty(strBPID))
            {
                DataTable dt_REQNO_OFBATCH = DBHelper.getData(string.Format(DBHelper.BP07_QRY_REQNO_LIST_BY_ERRORFILEBPID, strBPID));

                if (dt_REQNO_OFBATCH != null && dt_REQNO_OFBATCH.Rows.Count > 0)
                {
                    for (int i = 0; i < dt_REQNO_OFBATCH.Rows.Count; i++)
                    {
                        strMailErrorReqNO = dt_REQNO_OFBATCH.Rows[i]["req_no"].ToString() + " - " + dt_REQNO_OFBATCH.Rows[i]["req_desc"].ToString() + "<br/>" + strMailErrorReqNO;
                    }
                }
            }

            //mean <ListofRequestInfo> have not infromation,need not send mail
            if (!string.IsNullOrEmpty(strMailErrorReqNO))
            {
                string tempSubjectStr = dt_Mail.Rows[0]["SUBJECT"] != null ? dt_Mail.Rows[0]["SUBJECT"].ToString() : null;
                string tempBodyStr = dt_Mail.Rows[0]["CONTENT"] != null ? dt_Mail.Rows[0]["CONTENT"].ToString() : null;

                string subjectStr = MailCommon.getEProfileBody_BP07(tempSubjectStr, System.DateTime.Now.ToString(), strMailErrorReqNO);
                string bodyStr = MailCommon.getEProfileBody_BP07(tempBodyStr, System.DateTime.Now.ToString(), strMailErrorReqNO);

                string groupName = ConfigurationManager.AppSettings["BP07NotifyEmailTo"];

                if (!string.IsNullOrEmpty(ADManager.GetUserMailAddress(groupName, "Group")))
                {
                    sendMailToAD_BP07(groupName, subjectStr, bodyStr, "Group");
                }
                if (!string.IsNullOrEmpty(ADManager.GetUserMailAddress(groupName, "user")))
                {
                    sendMailToAD_BP07(groupName, subjectStr, bodyStr, "user");
                }
            }
        }


        public void sendMailToAD_BP07(string strSTAGE_APPROVER, string subjectStr, string bodyStr, string strADType)
        {
            try
            {
                if (!string.IsNullOrEmpty(ADManager.GetUserMailAddress(strSTAGE_APPROVER, strADType)))
                {
                    List<string> toMailAdrs = new List<string>();

                    toMailAdrs.Add(ADManager.GetUserMailAddress(strSTAGE_APPROVER, strADType));
                    MailCommon.SendMail(subjectStr, bodyStr, toMailAdrs, null, null, ConfigurationManager.AppSettings["BP07EmailFrom"]);
                }
            }
            catch (System.Exception ex)
            {
                logStr.Add("sendMailToAD");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP07_log.txt", "BP07_log.txt");
            }

        }


        /// <summary>
        /// get BP_ID
        /// </summary>
        /// <returns></returns>
        public string GetBPID()
        {
            string strBPID = System.DateTime.Now.ToString("yyyyMMddHH").ToString();
            strBPID = strBPID.Substring(0, strBPID.Length);

            return strBPID;
        }

        /// <summary>
        /// convert date format from yyyyMMdd to yyyy-MM-dd
        /// </summary>
        /// <returns></returns>
        private string convertDateFormat(string strDate)
        {
            string returnDate = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(strDate))
                {
                    returnDate = strDate.Substring(0, 4) + "-" + strDate.Substring(4, 2) + "-" + strDate.Substring(6, 2);
                }
            }
            catch (System.Exception ex)
            {
                logStr.Add("convertDateFormat");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP07_log.txt", "BP07_log.txt");
            }
            return returnDate;
        }

        /// <summary>
        /// get BPID List
        /// </summary>
        /// <returns></returns>
        public bool GetBPIDList(string filePath)
        {
            try
            {
                if (!System.IO.Directory.Exists(filePath))
                {
                    return false;
                }
                else
                {
                    DirectoryInfo strFolder = new DirectoryInfo(filePath);
                    foreach (FileInfo file in strFolder.GetFiles())
                    {
                        string BPID = string.Empty;
                        if (file.Name.Split('_')[2].ToString().Length == 14)
                        {
                            BPID = file.Name.Split('_')[2].Substring(0, 14);
                        }

                        //if (file.Name.Split('_')[0].Equals("eLeave"))
                        //{
                        //    BPID = file.Name.Split('_')[2].Substring(0, 14);
                        //}
                        //else
                        //{
                        //    BPID = file.Name.Split('_')[2].Substring(0, 14);
                        //}

                        if (!BPIDList.Contains(BPID) && !string.IsNullOrEmpty(BPID))
                        {
                            BPIDList.Add(BPID);
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }

        }

        #endregion

        #region DB

        #region batch common process

        /// <summary>
        /// insert content to T_DOC_ATTACHMENT
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public DataTable getBatchTimeFromDB()
        {
            string reTime = string.Empty;
            DataTable dt = DBHelper.getData(DBHelper.BP07_GET_BATCHRUNTIME);
            return dt;
        }

        /// <summary>
        /// SQL1: SEARCH FOLDER
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
        /// SEARCH FOLDER of ArchiveFile
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public string getBatchArchiveFilePathFromDB()
        {
            string reFilePath = "";
            DataTable dt = DBHelper.getData(DBHelper.BP07_GET_ARCHIVEBATCHFILEPATH);
            if (dt.Rows.Count > 0)
            {
                reFilePath = dt.Rows[0]["VALUE"].ToString();
            }
            return reFilePath;
        }

        /// <summary>
        /// SQL4.1: T_REQUEST_HRP
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public DataTable getT_REQUEST_HRP(string empid, string strBPID, string reqtype)
        {
            string strSql = string.Format(DBHelper.BP07_GET_T_REQUEST_HRP, empid, strBPID, reqtype);
            return DBHelper.getData(strSql);
        }

        #endregion

        #region update
        /// <summary>
        /// STATUS UPDATE
        /// </summary>
        /// <param name="strBPID">BP_ID</param>
        private int StatusUpdate(string strBPID)
        {
            string sqlStr = String.Format(DBHelper.BP07_UPD_T_REQUEST_HRP,
                strBPID);
            sqlStr += String.Format(DBHelper.BP07_UPD_T_REQUEST_HRL,
                strBPID);

            int i = DBHelper.updateDataBySqlText(sqlStr);
            return i;
        }

        private void updataErrorInfo(string bpid, string reqno, string empid, string empName, string reqtype, string error, string trigger,
            string infotype, string subtype, string seq, string objps, string filename)
        {
            try
            {
                string strSql = string.Format(DBHelper.BP07_UPD_T_REQUEST_HRP_BY_REQ_NO, reqno);
                strSql += string.Format(DBHelper.BP07_INS_T_BATCH_ERROR, bpid, reqno, empid, empName, reqtype, error,
                    trigger, infotype, subtype, seq, objps, filename);

                strSql += string.Format(DBHelper.BP07_UPD_T_BATCH_ERROR, bpid, reqno, empid);

                DBHelper.updateDataBySqlText(strSql);
            }
            catch (Exception ex)
            {
                logStr.Add("updataErrorInfo");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP07_log.txt", "BP07_log.txt");
            }
        }

        private void updataErrorInfo1ForMyLeave(string bpid, string reqno, string empid, string empName, string reqtype, string error, string trigger,
             string filename, string leaveSt, string leaveEn, string cancel)
        {
            try
            {
                string strSql = string.Empty;

                strSql += string.Format(DBHelper.BP07_INS_T_BATCH_ERROR2, bpid, reqno, empid, empName, reqtype, error,
                    trigger, filename, leaveSt, leaveEn);

                if ("X".Equals(cancel))
                {
                    strSql += string.Format(DBHelper.BP07_UPD_T_WF_TRANSACTION2, reqno);
                }
                else
                {
                    strSql += string.Format(DBHelper.BP07_UPD_T_WF_TRANSACTION1, reqno);
                }

                string strSqlGet = string.Format(DBHelper.BP07_QRY_T_WF_TRANSACTION2, reqno);
                DataTable dt = DBHelper.getData(strSqlGet);
                if (hasNextWorkFlow(dt, reqno))
                {
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        int wf_level = int.Parse(dt.Rows[0]["WF_CUR_LEVEL"].ToString()) + 1;
                        strSql += string.Format(DBHelper.BP07_UPD_T_WF_TRANSACTION3, reqno, wf_level);
                    }
                }

                strSql += string.Format(DBHelper.BP07_UPD_T_REQUEST_HRL_BY_REQ_NO, reqno);

                strSql += string.Format(DBHelper.BP07_UPD_T_BATCH_ERROR, bpid, reqno, empid);

                DBHelper.updateDataBySqlText(strSql);

                insertT_WF_TRANSACTION(cancel, reqno);
            }
            catch (Exception ex)
            {
                logStr.Add("updataErrorInfoForMyLeave");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP07_log.txt", "BP07_log.txt");
            }
        }

        private void updataErrorInfo2ForMyLeave(string bpid, string reqno, string empid, string empName, string reqtype, string error, string trigger,
             string filename, string leaveSt, string leaveEn)
        {
            try
            {
                string strSql = string.Empty;

                strSql += string.Format(DBHelper.BP07_INS_T_BATCH_ERROR2, bpid, reqno, empid, empName, reqtype, error,
                    trigger, filename, leaveSt, leaveEn);

                strSql += string.Format(DBHelper.BP07_UPD_T_WF_TRANSACTION1, reqno);

                strSql += string.Format(DBHelper.BP07_UPD_T_REQUEST_HRL_BY_REQ_NO, reqno);

                strSql += string.Format(DBHelper.BP07_UPD_T_BATCH_ERROR, bpid, reqno, empid);

                DBHelper.updateDataBySqlText(strSql);

                insertT_WF_TRANSACTION(string.Empty, reqno);
            }
            catch (Exception ex)
            {
                logStr.Add("updataErrorInfoForMyLeave");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP07_log.txt", "BP07_log.txt");
            }
        }

        private void insertT_WF_TRANSACTION(string cancel, string reqno)
        {
            try
            {
                DataTable dt = null;
                string strSql = string.Empty;
                string strWF_ID = string.Empty;
                string strWF_CUR_LEVEL = string.Empty;

                if ("X".Equals(cancel))
                {
                    strSql = string.Format(DBHelper.BP07_QRY_T_WF_TRANSACTION2, reqno);
                }
                else
                {
                    strSql = string.Format(DBHelper.BP07_QRY_T_WF_TRANSACTION1, reqno);
                }

                dt = DBHelper.getData(strSql);

                if (dt != null && dt.Rows.Count > 0)
                {
                    strWF_ID = dt.Rows[0]["WF_ID"].ToString();
                    strWF_CUR_LEVEL = dt.Rows[0]["WF_CUR_LEVEL"].ToString();
                }

                strSql = string.Format(DBHelper.BP07_INS_T_WF_TRANSACTION,
                                       DBManager.getREQNO("TXN"),
                                       reqno,
                                       "HRL",
                                       strWF_ID,
                                       strWF_CUR_LEVEL,
                                       "NORMAL BATCH",
                                       "NORMAL BATCH",
                                       "1",
                                       "SAP BATCH",
                                       "RERUN BATCH PROCESS",
                                       "",
                                       "system",
                                       "system",
                                       "COMPLETED");

                DBHelper.updateDataBySqlText(strSql);
            }
            catch (Exception ex)
            {
                logStr.Add("insertT_WF_TRANSACTION");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP07_log.txt", "BP07_log.txt");
            }
        }

        private Boolean hasNextWorkFlow(DataTable dt, string reqno)
        {
            Boolean flag = false;
            try
            {
                if (dt != null && dt.Rows.Count > 0)
                {
                    int curLevel = int.Parse(dt.Rows[0]["WF_CUR_LEVEL"].ToString()) + 1;
                    string strSql = string.Format(DBHelper.BP07_GET_T_WF_TRANSACTION, curLevel, reqno);
                    DataTable dt1 = DBHelper.getData(strSql);

                    if (dt1 != null && dt1.Rows.Count > 0)
                    {
                        flag = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logStr.Add("hasNextWorkFlow");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP07_log.txt", "BP07_log.txt");
            }
            return flag;
        }

        #endregion

        #region insert
        /// <summary>
        /// SQL2: STATUS UPDATE
        /// </summary>
        /// <param name="strBP_ID">Employee ID</param>
        private int InsertStatusUpdate(string strBP_ID, string strErrorFlg, string trigger, string strREFBP_ID)
        {
            string sqlStr = String.Format(DBHelper.BP07_INS_T_BATCH_LOG,
                strBP_ID,
                trigger,
                strErrorFlg,
                strREFBP_ID);

            int i = DBHelper.updateDataBySqlText(sqlStr);
            return i;
        }


        /// <summary>
        /// SQL2: STATUS UPDATE
        /// </summary>
        /// <param name="strBP_ID">Employee ID</param>
        private int Update_T_BATCH_LOG_Status(string strBP_ID)
        {
            string sqlStr = String.Format(DBHelper.BP07_UPD_T_BATCH_LOG,
                strBP_ID);

            int i = DBHelper.updateDataBySqlText(sqlStr);
            return i;
        }

        #endregion

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
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP07_log.txt", "BP07_log.txt");
            }
        }

        #endregion



    }
}
