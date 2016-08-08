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
    public partial class BP01
    {
        public static string batchFilePath = "";
        public string strTrigger = "";
        public string strTriggerName = "";
        public List<string> logStr = new List<string>();

        private string strBPID = string.Empty;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public event EventHandler CompletedEvent;

        private void OnEvent()
        {
            if (CompletedEvent != null)
            {
                CompletedEvent(this, EventArgs.Empty);
            }
        }
        public void BP02Process()
        {
            try
            {
                // ProcessBP01();

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
        void ProcessBP01()
        {
            try
            {
               

                string CurrTime = System.DateTime.Now.ToString("HH:mm:ss").ToString();
                //logStr.Add("CurrTime:" + CurrTime);
                DataTable dt = getBatchTimeFromDB();
                bool batchRun = false;
                string mode = string.Empty;
                string time = string.Empty;
                string runHourTime = string.Empty;
                string runMinTime = string.Empty;
                string runSecTime = string.Empty;

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
                    //logStr.Add("batchRun:" + batchRun.ToString());
                    if (batchRun == true)
                    {
                        if (!SapProfile.chkSAPIsConnected())
                        {
                            insT_BATCH_LOG("system");

                        }
                        else
                        {
                            if (!SapProfile.chkSAPIsLocked())
                            {
                                strTrigger = "system";
                                strTriggerName = "System Account";
                                batchProcessingStart(strTrigger, strTriggerName);
                            }
                        }

                    }
                }
            }
            catch (System.Exception ex)
            {
                //logStr.Add("timer_Elapsed");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP01_log.txt", "BP01_log.txt");
            }
        }

        public void createFile(List<string> content, string fullPath, string fileName)
        {
            try
            {
                string strRecord = string.Empty;
                if (File.Exists(fullPath))
                {
                    StreamReader sd = new StreamReader(fullPath);
                    strRecord = sd.ReadToEnd();
                    sd.Close();
                    File.Delete(fullPath);
                }
                using (StreamWriter sw = new StreamWriter(new FileStream(fullPath, FileMode.CreateNew), Encoding.GetEncoding("UTF-8")))
                {

                    sw.WriteLine(strRecord);
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
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP01_log.txt", "BP01_log.txt");
            }
        }


        #region method
        public static void batchProcessing(string fileNamePI, DataTable dt, string strTxtType)
        {
            try
            {
                CreateFileAndFolder(batchFilePath, fileNamePI);
                writeToTxt(batchFilePath + fileNamePI, dt, strTxtType);
                //StatusUpdate(strBPID, strREQ_TYPE);
            }
            catch (System.Exception ex)
            {
            }
        }

        public Boolean batchProcessingStart(string trigger, string triggerName)
        {
            try
            {
                //logStr.Add("batchProcessingStart");


                strTrigger = trigger;
                strTriggerName = triggerName;

                //CAPTURE LOG 13/14 char
                string strBPID = CaptureLog(trigger);
                //logStr.Add("BPID:" + strBPID);
                //SELECT GLOBAL SETTING GET Batch File Path
                //logStr.Add("change1 getBatchFilePathFromDB start:" + System.DateTime.Now .ToString());
                //if (strTrigger.Equals("system"))
                //{
                batchFilePath = getBatchFilePathFromDB(DBHelper.BP00_GET_BATCHFILEPATH);
                //}
                //else
                //{
                //    batchFilePath = getBatchFilePathFromDB(DBHelper.BP01_GET_BATCHFILEPATH);
                //}
                //logStr.Add("change1 getBatchFilePathFromDB end :" + System.DateTime.Now.ToString());


                List<string> listREQ_NO = new List<string>();
                if (!DoMyProfileBatchProcess(strBPID, out listREQ_NO))
                {
                    //logStr.Add("DoMyProfileBatchProcess fail");
                    return false;
                }

                //logStr.Add("change2 ChangeFileName start:" + System.DateTime.Now.ToString());
                //if (!strTrigger.Equals("system"))
                //{
                //    ChangeFileName(batchFilePath);
                //}
                //logStr.Add("change2 ChangeFileName end:" + System.DateTime.Now.ToString());


                //logStr.Add("StatusUpdate End");



                List<string> lREQ_NO = new List<string>();
                if (!DoMyLeaveBatchProcess(strBPID, out lREQ_NO))
                {
                    //logStr.Add("DoMyLeaveBatchProcess fail");
                    return false;
                }



                //logStr.Add("Update T_WF_TRANSACTION Start");
                //Update T_WF_TRANSACTION
                string sqlTransaction = string.Empty;
                string strREQ_NO_HRP = string.Empty;
                string strREQ_NO_HRL = string.Empty;
                if (listREQ_NO.Count + lREQ_NO.Count > 0)
                {
                    if (listREQ_NO.Count > 0)
                    {
                        strREQ_NO_HRP = string.Join<string>(",", listREQ_NO);
                    }
                    if (lREQ_NO.Count > 0)
                    {
                        strREQ_NO_HRL = string.Join<string>(",", lREQ_NO);
                    }

                    //logStr.Add("checkWorkFlow Start");
                    checkWorkFlow(strREQ_NO_HRP, strREQ_NO_HRL);
                    //logStr.Add("checkWorkFlow End");

                    sqlTransaction = strREQ_NO_HRP + "," + strREQ_NO_HRL;
                    sqlTransaction = "(" + sqlTransaction.TrimStart(',').TrimEnd(',') + ")";
                    DBHelper.updateDataBySqlText(string.Format(DBHelper.BP01_UPD_T_WF_TRANSACTION, sqlTransaction));
                    //logStr.Add("Update T_WF_TRANSACTION Start");
                }

                //logStr.Add("StatusUpdate Start");
                if (strTrigger.Equals("system"))
                {
                    //Update status
                    StatusUpdateForHRP(strBPID.Substring(0, 10), "('PAS','EPF','SOC','TAX','SPO','DEP','ACA','INS','EME','PER','PA1')", strREQ_NO_HRP);
                    StatusUpdateForHRL(strBPID.Substring(0, 10), strREQ_NO_HRL);
                }
                else
                {
                    //Update status
                    StatusUpdateForHRP(strBPID.Substring(0, 10), "('PAS','EPF','SOC','TAX','SPO','DEP','ACA','INS','EME','PER','PA1')", strREQ_NO_HRP);
                    StatusUpdateForHRL(strBPID.Substring(0, 10), strREQ_NO_HRL);
                }

                if (strTrigger.Equals("system"))
                {
                    //Update log
                    upDateT_BATCH_LOG(strBPID.Substring(0, 10));
                }
                else
                {
                    //Update log
                    upDateT_BATCH_LOG(strBPID.Substring(0, 10));
                }

                //createFile(logStr, "C:\\NTT\\ErrorLog\\BP01_log.txt", "BP01_log.txt");
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
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP01_log.txt", "BP01_log.txt");
                return false;
            }
        }

        private Boolean DoMyProfileBatchProcess(string strBPID, out List<string> listREQ_NO)
        {
            try
            {
                //get REQ_NO for the update of Status
                DataTable dtPI = RetrievePersonalInfo();
                DataTable dtWD = RetrieveWorkDetailInfo();
                DataTable dtSD = RetrieveSpouseInfo();
                DataTable dtIN = RetrieveInsuranceNomineeInfo();
                DataTable dtED = RetrieveAcademicInfo();
                DataTable dtEC = RetrieveEmergencyInfo();
                listREQ_NO = new List<string>();

                //get REQ_NO for the update of Status     set REQ_NO into a list
                foreach (DataRow dr in dtPI.Rows)
                {
                    if (dtPI.Rows.Count > 0 && !listREQ_NO.Contains("'" + dr["REQ_NO"].ToString() + "'"))
                    {
                        listREQ_NO.Add("'" + dr["REQ_NO"].ToString() + "'");
                    }
                }
                foreach (DataRow dr in dtWD.Rows)
                {
                    if (dtWD.Rows.Count > 0 && !listREQ_NO.Contains("'" + dr["REQ_NO"].ToString() + "'"))
                    {
                        listREQ_NO.Add("'" + dr["REQ_NO"].ToString() + "'");
                    }
                }
                foreach (DataRow dr in dtSD.Rows)
                {
                    if (dtSD.Rows.Count > 0 && !listREQ_NO.Contains("'" + dr["REQ_NO"].ToString() + "'"))
                    {
                        listREQ_NO.Add("'" + dr["REQ_NO"].ToString() + "'");
                    }
                }
                foreach (DataRow dr in dtIN.Rows)
                {
                    if (dtIN.Rows.Count > 0 && !listREQ_NO.Contains("'" + dr["REQ_NO"].ToString() + "'"))
                    {
                        listREQ_NO.Add("'" + dr["REQ_NO"].ToString() + "'");
                    }
                }
                foreach (DataRow dr in dtED.Rows)
                {
                    if (dtED.Rows.Count > 0 && !listREQ_NO.Contains("'" + dr["REQ_NO"].ToString() + "'"))
                    {
                        listREQ_NO.Add("'" + dr["REQ_NO"].ToString() + "'");
                    }
                }
                foreach (DataRow dr in dtEC.Rows)
                {
                    if (dtEC.Rows.Count > 0 && !listREQ_NO.Contains("'" + dr["REQ_NO"].ToString() + "'"))
                    {
                        listREQ_NO.Add("'" + dr["REQ_NO"].ToString() + "'");
                    }
                }

                //1.Retrieve Personal Information
                batchProcessing("eProfile_PI_" + strBPID + ".txt", dtPI, "PI");

                //2.Retrieve Work Detail Information
                batchProcessing("eProfile_WD_" + strBPID + ".txt", dtWD, "WD");

                //3.Retrieve Spouse Information
                batchProcessing("eProfile_SD_" + strBPID + ".txt", dtSD, "SD");

                //4.Retrieve Insurance Nominee Information
                batchProcessing("eProfile_IN_" + strBPID + ".txt", dtIN, "IN");

                //5.Retrieve Academic Information
                batchProcessing("eProfile_ED_" + strBPID + ".txt", dtED, "ED");

                //6.Retrieve Emergency  Information
                batchProcessing("eProfile_EC_" + strBPID + ".txt", dtEC, "EC");

                //logStr.Add("SAPFunction Start");
                //logStr.Add("change3 DoSAPFunction start:" + System.DateTime.Now.ToString());
                //if (strTrigger.Equals("system"))
                //{
                if (!SapProfile.chkSAPIsConnected())
                {
                    upDateT_BATCH_LOG_FOR_SAPERROR(strBPID.Substring(0, 10));
                    return false;
                }
                else
                {
                    SAPHelper.DoSAPFunction();
                }
                //}
                //logStr.Add("change3 DoSAPFunction end:" + System.DateTime.Now.ToString());
                //logStr.Add("SAPFunction End");
                return true;
            }
            catch (System.Exception ex)
            {
                logStr.Add("DoMyProfileBatchProcess");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP01_log.txt", "BP01_log.txt");
                listREQ_NO = new List<string>();
                return false;
            }
        }

        private Boolean DoMyLeaveBatchProcess(string strBPID, out List<string> lREQ_NO)
        {

            try
            {
                //get REQ_NO for the update of Status
                DataTable dtAC = GetAccumulateDataForLAC();
                DataTable dtRE = GetAccumulateDataForLR();
                DataTable dtCN = GetLeaveHistoryData();
                DataTable dtAP = GetLeaveApplyData();
                //get REQ_NO for the update of Status     set REQ_NO into a list
                lREQ_NO = new List<string>();
                foreach (DataRow dr in dtAC.Rows)
                {
                    if (dtAC.Rows.Count > 0 && !lREQ_NO.Contains("'" + dr["REQ_NO"].ToString() + "'"))
                    {
                        lREQ_NO.Add("'" + dr["REQ_NO"].ToString() + "'");
                    }
                }
                foreach (DataRow dr in dtRE.Rows)
                {
                    if (dtRE.Rows.Count > 0 && !lREQ_NO.Contains("'" + dr["REQ_NO"].ToString() + "'"))
                    {
                        lREQ_NO.Add("'" + dr["REQ_NO"].ToString() + "'");
                    }
                }
                foreach (DataRow dr in dtCN.Rows)
                {
                    if (dtCN.Rows.Count > 0 && !lREQ_NO.Contains("'" + dr["REQ_NO"].ToString() + "'"))
                    {
                        lREQ_NO.Add("'" + dr["REQ_NO"].ToString() + "'");
                    }
                }
                foreach (DataRow dr in dtAP.Rows)
                {
                    if (dtAP.Rows.Count > 0 && !lREQ_NO.Contains("'" + dr["REQ_NO"].ToString() + "'"))
                    {
                        lREQ_NO.Add("'" + dr["REQ_NO"].ToString() + "'");
                    }
                }
                //create eLeave_LAC.txt eLeave_LR.txt
                batchProcessing("eLeave_LAC_" + strBPID + ".txt", dtAC, "AC");
                batchProcessing("eLeave_LR_" + strBPID + ".txt", dtRE, "RE");

                //create eLeave_LC.txt
                batchProcessing("eLeave_LC_" + strBPID + ".txt", dtCN, "CN");

                //create RD_eLeave_AP.txt
                batchProcessing("eLeave_LAP_" + strBPID + ".txt", dtAP, "AP");

                //logStr.Add("change4 DoSAPFunction start:" + System.DateTime.Now.ToString());
                //if (!strTrigger.Equals("system"))
                //{
                //    ChangeFileName(batchFilePath);
                //}
                //logStr.Add("change4 ChangeFileName end:" + System.DateTime.Now.ToString());
                //logStr.Add(strTrigger);

                //logStr.Add("change5 DoSAPFunctionAccumulate start:" + System.DateTime.Now.ToString());
                //if (strTrigger.Equals("system"))
                //{

                if (!SapProfile.chkSAPIsConnected())
                {
                    //logStr.Add("UpdateBatchLogForSapError Start");
                    upDateT_BATCH_LOG_FOR_SAPERROR(strBPID.Substring(0, 10));
                    //logStr.Add("UpdateBatchLogForSapError End");
                    return false;
                }
                else
                {
                    //logStr.Add("MyLeaveSAPFunction Start");
                    SAPHelper.DoSAPFunctionAccumulate();
                    SAPHelper.DoSAPFunctionRei();
                    SAPHelper.DoSAPFunctionLeaveHistory();
                    SAPHelper.DoSAPFunctionLeaveApply();
                    //logStr.Add("MyLeaveSAPFunction End");
                }
                //}
                //logStr.Add("change5 DoSAPFunctionAccumulate end:" + System.DateTime.Now.ToString());
                return true;
            }
            catch (System.Exception ex)
            {
                logStr.Add("DoMyLeaveBatchProcess");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP01_log.txt", "BP01_log.txt");
                lREQ_NO = new List<string>();
                return false;
            }

        }

        private DataTable GetAccumulateDataForLAC()
        {
            try
            {
                string strSql = DBHelper.BP01_GET_T_ACCUMULATE_AND_T_REQUEST_HRL1;
                DataTable dt = DBHelper.getData(strSql);

                calculateStartAndEndDate(dt);

                return dt;

            }
            catch (System.Exception ex)
            {
                logStr.Add("GetAccumulateDataForAC");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP01_log.txt", "BP01_log.txt");
                return null;
            }
        }

        private DataTable GetAccumulateDataForLR()
        {
            string strSql = DBHelper.BP01_GET_T_ACCUMULATE_AND_T_REQUEST_HRL2;
            DataTable dt = DBHelper.getData(strSql);

            calculateStartAndEndDate(dt);

            return dt;
        }

        private DataTable GetLeaveHistoryData()
        {
            string strSql = DBHelper.BP01_GET_T_LEAVE_HISTORY;
            return DBHelper.getData(strSql);
        }

        private DataTable GetLeaveApplyData()
        {
            string strSql = DBHelper.BP01_GET_T_LEAVE_APPLY;
            return DBHelper.getData(strSql);
        }

        private void calculateStartAndEndDate(DataTable dt)
        {
            try
            {
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string startDateStr = string.Empty;
                        string endDateStr = string.Empty;

                        DateTime currDate = DateTime.Now;
                        startDateStr += currDate.Year + "-" + currDate.Month + "-01";
                        endDateStr += currDate.AddMonths(12).AddDays(-currDate.Day).ToString("yyyy-MM-dd");

                        dr["BEGDA"] = startDateStr;
                        dr["ENDDA"] = endDateStr;

                    }
                }
            }
            catch (System.Exception ex)
            {
                logStr.Add("calculateStartAndEndDate");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP01_log.txt", "BP01_log.txt");
            }
        }

        public void ChangeFileName(string Path)
        {
            try
            {
                //logStr.Add("ChangeFileName Start");
                //change file name
                if (System.IO.Directory.Exists(Path))
                {
                    DirectoryInfo di = new DirectoryInfo(Path);
                    foreach (FileInfo fi in di.GetFiles())
                    {
                        if (fi.Name.Length == 30)
                        {
                            string newName = fi.Name.Substring(0, 25) + fi.Name.Substring(26, 4);
                            fi.MoveTo(Path + newName);
                        }
                        else if (fi.Name.Length == 29 && fi.Name.Split('_')[1].Length == 3)
                        {
                            //logStr.Add("fi.Name.Length == 29 Start");
                            string newName = fi.Name.Substring(0, 24) + fi.Name.Substring(25, 4);
                            fi.MoveTo(Path + newName);
                            //logStr.Add("fi.Name.Length == 29 End");
                        }
                        else if (fi.Name.Length == 28 && fi.Name.Split('_')[1].Length == 2)
                        {
                            //logStr.Add("fi.Name.Length == 28 Start");
                            string newName = fi.Name.Substring(0, 23) + fi.Name.Substring(24, 4);
                            fi.MoveTo(Path + newName);
                            //logStr.Add("fi.Name.Length == 28 End");
                        }

                    }
                }
                //logStr.Add("ChangeFileName End");
            }
            catch (System.Exception ex)
            {

            }

        }

        //chek file , if not exit ,create it
        public static void CreateFileAndFolder(string filePath, string fileName)
        {
            try
            {
                //create folder
                if (!System.IO.Directory.Exists(filePath))
                {
                    System.IO.Directory.CreateDirectory(filePath);
                }

                //create file
                if (!File.Exists(filePath + fileName))
                {
                    FileStream fs = File.Create(filePath + fileName);
                    fs.Close();
                }
                else
                {
                    //File.WriteAllText(filePath + fileName, "");
                }
            }
            catch (System.Exception ex)
            {

            }
        }

        public void sendMailToAD_BP01(string strSTAGE_APPROVER, string strCCUserID, string subjectStr, string bodyStr, string strADType)
        {
            try
            {
                if (!string.IsNullOrEmpty(ADManager.GetUserMailAddress(strSTAGE_APPROVER, strADType)))
                {
                    List<string> toMailAdrs = new List<string>();
                    toMailAdrs.Add(ADManager.GetUserMailAddress(strSTAGE_APPROVER, strADType));

                    List<string> ccMailAdrs = new List<string>();
                    ccMailAdrs.Add(ADManager.GetUserMailAddress(strCCUserID, strADType));

                    MailCommon.SendMail(subjectStr, bodyStr, toMailAdrs, ccMailAdrs, null, ConfigurationManager.AppSettings["BP07EmailFrom"]);
                }
            }
            catch (System.Exception ex)
            {
                logStr.Add("sendMailToAD");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP01_log.txt", "BP01_log.txt");
            }

        }

        private void checkWorkFlow(string strREQ_NO_HRP, string strREQ_NO_HRL)
        {
            try
            {
                if (!string.IsNullOrEmpty(strREQ_NO_HRP) || !string.IsNullOrEmpty(strREQ_NO_HRL))
                {
                    if (!string.IsNullOrEmpty(strREQ_NO_HRP))
                    {
                        strREQ_NO_HRP = string.Format("AND A.REQ_NO in {0}", "(" + strREQ_NO_HRP + ")");
                    }
                    if (!string.IsNullOrEmpty(strREQ_NO_HRL))
                    {
                        strREQ_NO_HRL = string.Format("AND A.REQ_NO in {0}", "(" + strREQ_NO_HRL + ")");
                    }
                    DataTable dt = getCurrentLevelAndWFID(strREQ_NO_HRP, strREQ_NO_HRL);
                    if (dt == null)
                    {
                        return;
                    }
                    else
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            string strWFID = row["WF_ID"] == null ? string.Empty : row["WF_ID"].ToString().Trim();
                            string strLevel = row["WF_CUR_LEVEL"] == null ? string.Empty : row["WF_CUR_LEVEL"].ToString().Trim();
                            string strReqNo = row["REQ_NO"] == null ? string.Empty : row["REQ_NO"].ToString().Trim();
                            string strModule = row["MODULE"] == null ? string.Empty : row["MODULE"].ToString().Trim();
                            string strEmpId = row["EMP_ID"] == null ? string.Empty : row["EMP_ID"].ToString().Trim();
                            string strEMP_NAME = row["EMP_NAME"] == null ? string.Empty : row["EMP_NAME"].ToString().Trim();

                            try
                            {
                                strLevel = (Int32.Parse(strLevel) + 1).ToString();
                            }
                            catch
                            {
                                strLevel = string.Empty;
                            }
                            if (!string.IsNullOrEmpty(strWFID) && !string.IsNullOrEmpty(strLevel))
                            {
                                DataTable dtNextWorkFlow = getNextWorkFlow(strWFID, strLevel);
                                if (dtNextWorkFlow != null && dtNextWorkFlow.Rows.Count > 0)
                                {
                                    string strStageApprover = string.Empty;
                                    string strStageApproverName = string.Empty;

                                    SapProfileResult result = getSAPData(strEmpId.PadLeft(8, '0').ToString().Trim());
                                    //SapProfileResult result = SapProfileForUT(strEmpId.PadLeft(8, '0').ToString().Trim());

                                    DBManager.GetStageApproverAndStageApproverName(dtNextWorkFlow, strEmpId, result.T_HEADER.Rows[0]["ORGEH"].ToString().Trim(),
                                                                                   out strStageApprover, out strStageApproverName, strModule, strReqNo);
                                    //only for mail cc
                                    string strCC_SETTING = dtNextWorkFlow.Rows[0]["CC_SETTING"].ToString().Trim();
                                    string strCCUserIDFromAD = string.Empty;
                                    DBManager.GetccUserID(strCC_SETTING, strEmpId, result.T_HEADER.Rows[0]["ORGEH"].ToString().Trim(), strModule, strReqNo, out strCCUserIDFromAD);

                                    if (strModule.Equals("HRP"))
                                    {
                                        string sqlstr = string.Empty;
                                        if (!isNextWorkFlowExist(getDBValue(dtNextWorkFlow.Rows[0]["WF_LEVEL"]), strReqNo))
                                        {
                                            //insert T_WF_TRANSACTION
                                            sqlstr = string.Format(DBHelper.BP01_INS_T_WF_TRANSACTION, DBManager.getREQNO("TXN"), getDBValue(dtNextWorkFlow.Rows[0]["WF_ID"]),
                                                getDBValue(dtNextWorkFlow.Rows[0]["WF_LEVEL"]), strStageApprover,
                                                getDBValue(dtNextWorkFlow.Rows[0]["STAGE_NAME"]), "SAP BATCH", strTrigger,
                                                getDBValue(dtNextWorkFlow.Rows[0]["POSITIVE_LBL"]), strReqNo, strModule, strTriggerName, strStageApproverName);
                                            DBHelper.updateDataBySqlText(sqlstr);
                                        }

                                        if (!string.IsNullOrEmpty(dtNextWorkFlow.Rows[0]["EMAIL_NOTICE_FMT"].ToString().Trim()) &&
                                            !string.IsNullOrEmpty(strStageApprover) &&
                                            !"SAP BATCH".Equals(dtNextWorkFlow.Rows[0]["STAGE_NAME"].ToString().Trim()))
                                        {
                                            string sqlMailStr = string.Format(DBManager.MS06_QRY_M_EMAIL_FMT, dtNextWorkFlow.Rows[0]["EMAIL_NOTICE_FMT"]);
                                            DataTable dt_Mail = DBManager.getInfoBySqlText(sqlMailStr);
                                            string tempSubjectStr = dt_Mail.Rows[0]["SUBJECT"] != null ? dt_Mail.Rows[0]["SUBJECT"].ToString().Trim() : null;
                                            string strCurrPICName = getDBValue(dtNextWorkFlow.Rows[0]["STAGE_APPROVER_NAME"]);
                                            string subjectStr = MailCommon.getEProfileBody(tempSubjectStr, strCurrPICName, row["REQ_NO"].ToString(),
                                                strEMP_NAME, "myProfile", dtNextWorkFlow.Rows[0]["STAGE_NAME"].ToString().Trim(), getDBValue(row["REQ_DESC"]), HttpContext.Current.Request.Url.PathAndQuery);
                                            string tempBodyStr = dt_Mail.Rows[0]["CONTENT"] != null ? dt_Mail.Rows[0]["CONTENT"].ToString().Trim() : null;
                                            string bodyStr = MailCommon.getEProfileBody(tempBodyStr, strCurrPICName, row["REQ_NO"].ToString(),
                                                strEMP_NAME, "myProfile", dtNextWorkFlow.Rows[0]["STAGE_NAME"].ToString().Trim(), getDBValue(row["REQ_DESC"]), HttpContext.Current.Request.Url.PathAndQuery);

                                            sendMailToAD_BP01(strStageApprover.Trim(), strCCUserIDFromAD, subjectStr, bodyStr, "Group");
                                            sendMailToAD_BP01(strStageApprover.Trim(), strCCUserIDFromAD, subjectStr, bodyStr, "user");
                                        }
                                    }
                                    else if (strModule.Equals("HRL"))
                                    {


                                        string sqlstr = string.Empty;
                                        if (!isNextWorkFlowExist(getDBValue(dtNextWorkFlow.Rows[0]["WF_LEVEL"]), strReqNo))
                                        {
                                            //insert T_WF_TRANSACTION
                                            sqlstr = string.Format(DBHelper.BP01_INS_T_WF_TRANSACTION, DBManager.getREQNO("TXN"), getDBValue(dtNextWorkFlow.Rows[0]["WF_ID"]),
                                                getDBValue(dtNextWorkFlow.Rows[0]["WF_LEVEL"]), strStageApprover,
                                                getDBValue(dtNextWorkFlow.Rows[0]["STAGE_NAME"]), "SAP BATCH", strTrigger,
                                                getDBValue(dtNextWorkFlow.Rows[0]["POSITIVE_LBL"]), strReqNo, strModule, strTriggerName, strStageApproverName);
                                            DBHelper.updateDataBySqlText(sqlstr);
                                        }

                                        if (!string.IsNullOrEmpty(dtNextWorkFlow.Rows[0]["EMAIL_NOTICE_FMT"].ToString().Trim()) &&
                                            !string.IsNullOrEmpty(strStageApprover) &&
                                            !"SAP BATCH".Equals(dtNextWorkFlow.Rows[0]["STAGE_NAME"].ToString().Trim()))
                                        {
                                            string sqlMailStr = string.Format(DBManager.MS06_QRY_M_EMAIL_FMT, dtNextWorkFlow.Rows[0]["EMAIL_NOTICE_FMT"]);
                                            DataTable dt_Mail = DBManager.getInfoBySqlText(sqlMailStr);
                                            string tempSubjectStr = dt_Mail.Rows[0]["SUBJECT"] != null ? dt_Mail.Rows[0]["SUBJECT"].ToString().Trim() : null;
                                            string strCurrPICName = strStageApproverName;
                                            string subjectStr = MailCommon.getEProfileBody(tempSubjectStr, strCurrPICName, row["REQ_NO"].ToString(),
                                                strEMP_NAME, "myProfile", dtNextWorkFlow.Rows[0]["STAGE_NAME"].ToString().Trim(), getDBValue(row["REQ_DESC"]), HttpContext.Current.Request.Url.PathAndQuery);
                                            string tempBodyStr = dt_Mail.Rows[0]["CONTENT"] != null ? dt_Mail.Rows[0]["CONTENT"].ToString().Trim() : null;
                                            string bodyStr = MailCommon.getEProfileBody(tempBodyStr, strCurrPICName, row["REQ_NO"].ToString(),
                                                strEMP_NAME, "myProfile", dtNextWorkFlow.Rows[0]["STAGE_NAME"].ToString().Trim(), getDBValue(row["REQ_DESC"]), HttpContext.Current.Request.Url.PathAndQuery);

                                            sendMailToAD_BP01(strStageApprover.Trim(), strCCUserIDFromAD, subjectStr, bodyStr, "Group");
                                            sendMailToAD_BP01(strStageApprover.Trim(), strCCUserIDFromAD, subjectStr, bodyStr, "user");
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logStr.Add("checkWorkFlow");
                logStr.Add(ex.Message);
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP01_log.txt", "BP01_log.txt");
            }
        }

        private bool isNextWorkFlowExist(string curLevel, string strReqNo)
        {
            string strSql = string.Format(DBHelper.BP01_GET_T_WF_TRANSACTION, curLevel, strReqNo);
            DataTable dt = DBHelper.getData(strSql);
            if (dt != null && dt.Rows.Count > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>  
        ///   
        /// </summary>  
        public static void writeToTxt(string filePath, DataTable dt, string strTxtType)
        {

            FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            fs.SetLength(0);
            StreamWriter m_streamWriter = new StreamWriter(fs);
            //m_streamWriter.BaseStream.Seek(0, SeekOrigin.End);
            //m_streamWriter.WriteLine(DateTime.Now.ToString() + dt.Rows.Count.ToString() + "--------------------------------------------------------------------------------------------/n");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j + 1 < dt.Columns.Count; j++)
                {
                    if (j == 0)
                    {
                        string EmpID = getDBValue(dt.Rows[i][j]);
                        int length = EmpID.Length;
                        for (int m = 0; m < 8 - length; m++)
                        {
                            EmpID = "0" + EmpID;
                        }
                        dt.Rows[i][j] = EmpID;
                    }
                    string value = string.Empty;
                    switch (strTxtType)
                    {

                        case "PI":

                            if (j == 5)
                            {
                                value = formatDateTime(getDBValue(dt.Rows[i][j])) + "|";
                            }
                            else
                            {
                                value = getDBValue(dt.Rows[i][j]) + "|";
                            }
                            break;
                        case "ED":
                            if (j == 5)
                            {
                                value = formatDateTime(getDBValue(dt.Rows[i][j])) + "|";
                            }
                            else
                            {
                                value = getDBValue(dt.Rows[i][j]) + "|";
                            }

                            break;
                        case "IN":
                            if (j == 5)
                            {
                                value = formatDateTime(getDBValue(dt.Rows[i][j])) + "|";
                            }
                            else
                            {
                                value = getDBValue(dt.Rows[i][j]) + "|";
                            }

                            break;
                        case "SD":
                            if (j == 4 || j == 5 || j == 11 || j == 15)
                            {
                                value = formatDateTime(getDBValue(dt.Rows[i][j])) + "|";
                            }
                            else
                            {
                                value = getDBValue(dt.Rows[i][j]) + "|";
                            }
                            if (j == 3)
                            {
                                if (getDBValue(dt.Rows[i][1]).Trim().Equals("0021"))
                                {
                                    string strSeq = getDBValue(dt.Rows[i][j]);
                                    int length = strSeq.Length;
                                    for (int m = 0; m < 2 - length; m++)
                                    {
                                        strSeq = "0" + strSeq;
                                    }
                                    value = strSeq + "|";
                                }
                            }
                            break;

                        case "EC":
                            if (!string.IsNullOrEmpty(getDBValue(dt.Rows[i][5])) && string.IsNullOrEmpty(getDBValue(dt.Rows[i][7])) && j >= 7)
                            {
                                value = "<blank>" + "|";
                                break;
                            }
                            if (j == 5)
                            {
                                if (string.IsNullOrEmpty(getDBValue(dt.Rows[i][5])))
                                {
                                    value = formatDateTime(DateTime.Now.ToString()) + "|";
                                    break;
                                }
                                value = formatDateTime(getDBValue(dt.Rows[i][j])) + "|";
                            }
                            else
                            {
                                value = getDBValue(dt.Rows[i][j]) + "|";
                            }
                            if (j == 14)
                            {
                                value = getRelationDesc(getDBValue(dt.Rows[i][j])) + "|";
                            }
                            break;

                        case "WD":
                            if (j == 5 || j == 12 || j == 13 || j == 16)
                            {
                                value = formatDateTime(getDBValue(dt.Rows[i][j])) + "|";
                            }
                            else
                            {
                                value = getDBValue(dt.Rows[i][j]) + "|";
                            }
                            break;
                        case "AC":
                            if (j == 4 || j == 5)
                            {
                                value = formatDateTime(getDBValue(dt.Rows[i][j])) + "|";
                            }
                            else
                            {
                                value = getDBValue(dt.Rows[i][j]) + "|";
                            }
                            break;
                        case "RE":
                            if (j == 4 || j == 5)
                            {
                                value = formatDateTime(getDBValue(dt.Rows[i][j])) + "|";
                            }
                            else
                            {
                                value = getDBValue(dt.Rows[i][j]) + "|";
                            }
                            break;
                        case "CN":
                            if (j == 4 || j == 5)
                            {
                                value = formatDateTime(getDBValue(dt.Rows[i][j])) + "|";
                            }
                            else
                            {
                                value = getDBValue(dt.Rows[i][j]) + "|";
                            }
                            break;
                        case "AP":
                            if (j == 4 || j == 5)
                            {
                                value = formatDateTime(getDBValue(dt.Rows[i][j])) + "|";
                            }
                            else
                            {
                                value = getDBValue(dt.Rows[i][j]) + "|";
                            }
                            break;
                        default:
                            break;
                    }
                    if (j == dt.Columns.Count - 2)
                    {
                        value = value.Substring(0, value.Length - 1);
                    }
                    m_streamWriter.Write(value);

                }
                m_streamWriter.Write("\r\n");
            }

            m_streamWriter.Flush();
            m_streamWriter.Close();
            fs.Close();
        }


        private static string getRelationDesc(object strRelation)
        {
            if (strRelation == null)
            {
                return string.Empty;
            }
            else
            {
                if (strRelation.ToString().Length == 2)
                {
                    string strSql = string.Format(DBHelper.QRY_M_RELATION_BY_CODE, strRelation);
                    DataTable dt = DBManager.getInfoBySqlText(strSql);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        return dt.Rows[0]["REL_DESC_EN"].ToString();
                    }
                    else
                    {
                        return string.Empty;
                    }

                }
                else
                {
                    return strRelation.ToString();
                }
            }
        }
        /// <summary>  
        ///   
        /// </summary>  
        public static void writeToTxt_SD(string filePath, DataTable dt)
        {
            FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            fs.SetLength(0);
            StreamWriter m_streamWriter = new StreamWriter(fs);
            //m_streamWriter.BaseStream.Seek(0, SeekOrigin.End);
            //m_streamWriter.WriteLine(DateTime.Now.ToString() + dt.Rows.Count.ToString() + "--------------------------------------------------------------------------------------------/n");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    if (j == 14)
                    {
                        m_streamWriter.Write(formatDateTime(getDBValue(dt.Rows[i][j])) + "\t");
                    }
                    else
                    {
                        m_streamWriter.Write(getDBValue(dt.Rows[i][j]) + "\t");
                    }
                }
                m_streamWriter.Write("\r\n");
            }
            m_streamWriter.Flush();
            m_streamWriter.Close();
            fs.Close();
        }


        public static string formatDateTime(string value)
        {
            try
            {
                DateTime date = DateTime.Parse(value);
                DateTime defualtDate = DateTime.Parse("1900-01-01");
                if (date.Equals(defualtDate))
                {
                    return string.Empty;
                }
                else
                {
                    return string.Format("{0:yyyyMMdd}", date);
                }
            }
            catch
            {
                return value;
            }
        }

        public static string getDBValue(object value)
        {
            if (value == DBNull.Value)
            {
                return string.Empty;
            }
            else
            {
                return value.ToString().Trim();
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

            string sqlStr = String.Format(DBHelper.BP01_INS_T_BATCH_LOG,
                                        strBPIDNotFormat.Substring(0, 10),
                                        trigger);

            DBHelper.updateDataBySqlText(sqlStr);

            return strBPIDNotFormat;

        }

        public static void insT_BATCH_LOG(string trigger)
        {
            string strBPIDNotFormat = System.DateTime.Now.ToString("yyyyMMddHHmmss").ToString();
            string strBPID = strBPIDNotFormat.Substring(0, strBPIDNotFormat.Length - 1);

            string sqlStr = String.Format(DBHelper.INS_T_BATCH_LOG_FOR_SAPERROR,
                                        strBPID.Substring(0, 10), "BP01",
                                        trigger);

            DBHelper.updateDataBySqlText(sqlStr);

        }


        /// <summary>
        /// update T_BATCH_LOG
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public static void upDateT_BATCH_LOG(string strBPID)
        {

            try
            {
                string sqlStr = String.Format(DBHelper.BP01_UPD_T_BATCH_LOG, strBPID);

                DBHelper.updateDataBySqlText(sqlStr);
            }
            catch
            {
            }
        }

        /// <summary>
        /// update T_BATCH_LOG
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public static void upDateT_BATCH_LOG_FOR_SAPERROR(string strBPID)
        {

            try
            {
                string sqlStr = String.Format(DBHelper.UPD_T_BATCH_LOG_FOR_SAPERROR, strBPID);

                DBHelper.updateDataBySqlText(sqlStr);
            }
            catch
            {
            }
        }

        /// <summary>
        /// insert content to T_DOC_ATTACHMENT
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public static DataTable getBatchTimeFromDB()
        {
            string reTime = string.Empty;
            DataTable dt = DBHelper.getData(DBHelper.BP01_GET_BATCHRUNTIME);

            return dt;
        }

        /// <summary>
        /// insert content to T_DOC_ATTACHMENT
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public static string getBatchFilePathFromDB(string strSql)
        {
            string reFilePath = "";
            DataTable dt = DBHelper.getData(strSql);
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
        #endregion

        #region getdata
        /// <summary>
        /// SQL1: Retrieve Personal Information
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns>The result of rows affected</returns>
        public static DataTable RetrievePersonalInfo()
        {
            DataTable dt = DBHelper.getData(DBHelper.BP01_RETRIEVE_PERSONAL_INFO);
            return dt;
        }

        /// <summary>
        /// SQL2: Retrieve Work Detail Information
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns>The result of rows affected</returns>
        public static DataTable RetrieveWorkDetailInfo()
        {
            DataTable dt = DBHelper.getData(DBHelper.BP01_RETRIEVE_WORK_DETAIL_INFO);
            return dt;
        }

        /// <summary>
        /// SQL3: Retrieve Spouse Information
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns>The result of rows affected</returns>
        public static DataTable RetrieveSpouseInfo()
        {
            DataTable dt = DBHelper.getData(DBHelper.BP01_RETRIEVE_SPOUSE_INFO);
            return dt;
        }

        /// <summary>
        /// SQL4: Retrieve Insurance Nominee Information
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns>The result of rows affected</returns>
        public static DataTable RetrieveInsuranceNomineeInfo()
        {
            DataTable dt = DBHelper.getData(DBHelper.BP01_RETRIEVE_INSURANCE_NOMINEE_INFO);
            return dt;
        }

        /// <summary>
        /// SQL5: Retrieve Academic Information
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns>The result of rows affected</returns>
        public static DataTable RetrieveAcademicInfo()
        {
            DataTable dt = DBHelper.getData(DBHelper.BP01_RETRIEVE_ACADEMIC_INFO);
            return dt;
        }

        /// <summary>
        /// SQL6: Retrieve Emergency  Information
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns>The result of rows affected</returns>
        public static DataTable RetrieveEmergencyInfo()
        {
            DataTable dt = DBHelper.getData(DBHelper.BP01_RETRIEVE_EMERGENCY_INFO);
            return dt;
        }


        /// <summary>
        /// SQL11.1
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public static DataTable getNextWorkFlow(string strWFID, string strLevel)
        {
            DataTable dt = DBHelper.getData(string.Format(DBHelper.BP01_GET_NEXT_WORKFLOW, strWFID, strLevel));
            return dt;
        }

        #endregion


        /// <summary>
        /// get Current Level And WFID
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns>The result of rows affected</returns>
        public static DataTable getCurrentLevelAndWFID(string strREQ_NO_HRP, string strREQ_NO_HRL)
        {
            string strSql = string.Empty;
            if (!string.IsNullOrEmpty(strREQ_NO_HRP) && !string.IsNullOrEmpty(strREQ_NO_HRL))
            {
                strSql = string.Format(DBHelper.BP01_GET_CUR_LEVEL_AND_WFID_HRP, strREQ_NO_HRP) + "union " + string.Format(DBHelper.BP01_GET_CUR_LEVEL_AND_WFID_HRL, strREQ_NO_HRL);
            }
            else if (!string.IsNullOrEmpty(strREQ_NO_HRP))
            {
                strSql = string.Format(DBHelper.BP01_GET_CUR_LEVEL_AND_WFID_HRP, strREQ_NO_HRP);
            }
            else if (!string.IsNullOrEmpty(strREQ_NO_HRL))
            {
                strSql = string.Format(DBHelper.BP01_GET_CUR_LEVEL_AND_WFID_HRL, strREQ_NO_HRL);
            }
            DataTable dt = DBHelper.getData(strSql);
            return dt;
        }


        #region update
        /// <summary>
        /// myProfile SQL10:STATUS UPDATE FOR T_REQUEST_HRP
        /// </summary>
        /// <param name="strBP_ID"></param>
        /// <param name="strREQ_TYPE"></param>
        /// <returns></returns>
        private static int StatusUpdateForHRP(string strBP_ID, string strREQ_TYPE, string strREQ_NO)
        {
            string sqlStr = string.Empty;
            if (!string.IsNullOrEmpty(strREQ_NO))
            {
                strREQ_NO = "(" + strREQ_NO.TrimStart(',').TrimEnd(',') + ")";
                sqlStr = String.Format(DBHelper.BP01_UPD_T_REQUEST_HRP, strBP_ID, strREQ_TYPE, strREQ_NO);
                int i = DBHelper.updateDataBySqlText(sqlStr);
                return i;
            }
            return 0;
        }

        /// <summary>
        /// myLeave SQL10:STATUS UPDATE FOR T_REQUEST_HRL
        /// </summary>
        /// <param name="strBP_ID"></param>
        /// <returns></returns>
        private static int StatusUpdateForHRL(string strBP_ID, string strREQ_NO)
        {
            string sqlStr = string.Empty;
            if (!string.IsNullOrEmpty(strREQ_NO))
            {
                strREQ_NO = "(" + strREQ_NO.TrimStart(',').TrimEnd(',') + ")";
                sqlStr = String.Format(DBHelper.BP01_UPD_T_REQUEST_HRL, strBP_ID, strREQ_NO);
                int i = DBHelper.updateDataBySqlText(sqlStr);
                return i;
            }
            return 0;
        }

        #endregion

        /// <summary>
        /// TODO: for UT 
        /// </summary>
        /// <param name="empId"></param>
        /// <returns></returns>
        private SapProfileResult SapProfileForUT(string empId)
        {
            SapProfileResult result = new SapProfileResult();

            string strSQLParr = @"Select *
                            From	wy_T_HEADER where PERNR='{0}'";

            string sqlStr = String.Format(strSQLParr, empId.PadLeft(8, '0'));

            DataTable dt = DBManager.getInfoBySqlText(sqlStr);

            result.T_HEADER = dt;

            return result;
        }

        /// <summary>
        /// get data from SAP
        /// </summary>
        /// <param name="empId"></param>
        /// <returns></returns>
        private SapProfileResult getSAPData(string empId)
        {
            try
            {
                string errorMsg = string.Empty;
                SapConnectInfo info = new SapConnectInfo(ConfigurationManager.AppSettings["Z_HR_PA_ELEAVE"]);
                SapProfileResult result = new SapProfileResult();
                Hashtable hash = new Hashtable();
                hash.Add(DBManager.MODE_SAP, "VIEW");
                hash.Add(DBManager.EMPID_SAP, empId.PadLeft(8, '0'));
                result = SapProfile.getSAPFunData_Z_HR_PA_ELEAVE(info, hash);
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    return null;
                }
                return result;
            }
            catch (Exception ex)
            {
                logStr.Add("getSAPData");
                logStr.Add(ex.Message);
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP01_log.txt", "BP01_log.txt");
                return null;
            }
        }

        #endregion
    }
}
