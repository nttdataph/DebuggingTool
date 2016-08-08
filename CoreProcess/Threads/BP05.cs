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
    public partial class BP05
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public string strTrigger = string.Empty;
        private DataTable dtHeader = null;
        SapProfileResult SAPDATA = null;
        public string strTriggerName = "";  
        public List<string> logStr = new List<string>();
        public event EventHandler CompletedEvent;

        private void OnEvent()
        {
            if (CompletedEvent != null)
            {
                CompletedEvent(this, EventArgs.Empty);
            }
        }
        public void BP05Process()
        {
           try
           {
                //ProcessBP05();

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
        void ProcessBP05()
        {
            try
            {
                //logStr.Add("timer check start");

                string CurrTime = System.DateTime.Now.ToString("HH:mm:ss").ToString();
                //logStr.Add("CurrTime:"+CurrTime);
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
                }
            }
            catch (System.Exception ex)
            {
                logStr.Add("timer_Elapsed");
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP05_log.txt", "BP05_log.txt");
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
                strTrigger = trigger;
                string strBPID = CaptureLog(strTrigger);
                #region loop1
                DataTable dtSupervisor = getM_DELEGATION();
                if (dtSupervisor != null && dtSupervisor.Rows.Count > 0)
                {
                    foreach (DataRow row in dtSupervisor.Rows)
                    {
                        string strCostCenter = string.Empty;
                        strCostCenter = getTableValue(row["SUPER_COSTCENTER"]).Trim();
                        string strSupervisor = getTableValue(row["SUPERVISOR"]).Trim();
                        DateTime endDate = getTableDate(row["DATE_END"]);
                        //SAP
                        getSAPData(strSupervisor);
                        //UT
                        //SapProfileForUT(strSupervisor);
                        if (dtHeader != null && dtHeader.Rows.Count > 0)
                        {
                            //$cc = T_HEADER.KOSTL
                            if (getTableValue(dtHeader.Rows[0]["KOSTL"]).Trim().Equals(strCostCenter))
                            {
                                if (endDate < DateTime.Now)
                                {
                                    updateM_DELEGATION(trigger, strSupervisor, strCostCenter);
                                }
                            }
                            else
                            {
                                updateM_DELEGATION(trigger, strSupervisor, strCostCenter);
                                DataTable dtTRANSACTION = getT_WF_TRANSACTION(strSupervisor.TrimStart('0').PadLeft(5, '0'));
                                if (dtTRANSACTION != null && dtTRANSACTION.Rows.Count > 0)
                                {
                                    foreach (DataRow rowWorkFlow in dtTRANSACTION.Rows)
                                    {
                                        string txnID = getTableValue(rowWorkFlow["WF_TXN_ID"]);
                                        updateT_WF_TRANSACTION(trigger, txnID);
                                        string newTxnID = DBManager.getREQNO("TXN");
                                        string strSql = string.Format(DBHelper.BP05_INS_T_WF_TRANSACTION1, getTableValue(row["MANAGER"]).Trim().TrimStart('0').PadLeft(5, '0'),
                                            getTableValue(row["MGR_NAME"]), newTxnID, getTableValue(rowWorkFlow["REQ_NO"]), getTableValue(rowWorkFlow["MODULE"]),
                                            getTableValue(rowWorkFlow["WF_ID"]), getTableValue(rowWorkFlow["WF_CUR_LEVEL"]), getTableValue(rowWorkFlow["WF_STAGENAME"]),
                                            getTableValue(rowWorkFlow["POSITIVE_LBL"]));
                                        DBHelper.updateDataBySqlText(strSql);

                                        //send email
                                        SendMail(getTableValue(row["MGR_NAME"]));
                                    }
                                }
                            }
                        }


                    }
                }
                #endregion
                #region loop2
                DataTable dtSupervisorMem = getM_DELEGATION_MEM();
                if (dtSupervisorMem != null && dtSupervisorMem.Rows.Count > 0)
                {
                    string empID = string.Empty;
                    string emp_cc = string.Empty;
                    foreach (DataRow row in dtSupervisorMem.Rows)
                    {
                        empID = getTableValue(row["EMP_ID"]);
                        emp_cc = getTableValue(row["EMP_COSTCENTER"]);
                        //SAP
                        getSAPData(empID);
                        //UT
                        //SapProfileForUT(empID);

                        if (dtHeader != null && dtHeader.Rows.Count > 0)
                        {
                            //$emp_cc = T_HEADER.KOSTL
                            if (!getTableValue(dtHeader.Rows[0]["KOSTL"]).Trim().Equals(emp_cc))
                            {
                                updateM_DELEGATION_MEM(trigger, empID);
                                DataTable dtTRANSACTION = getT_WF_TRANSACTION_ALL(empID.TrimStart('0').PadLeft(5, '0'));
                                if (dtTRANSACTION != null && dtTRANSACTION.Rows.Count > 0)
                                {
                                    foreach (DataRow rowWorkFlow in dtTRANSACTION.Rows)
                                    {
                                        string txnID = getTableValue(rowWorkFlow["WF_TXN_ID"]);
                                        updateT_WF_TRANSACTION(trigger, txnID);


                                        string newTxnID = DBManager.getREQNO("TXN");
                                        string ADmgr = string.Empty;
                                        string ADmgrName = string.Empty;

                                        //get HOD info from AD
                                        ADManager.GetSuperiorInfo(empID, out ADmgr, out ADmgrName);

                                        string strSql = string.Format(DBHelper.BP05_INS_T_WF_TRANSACTION1, ADmgr,
                                            ADmgrName, newTxnID, getTableValue(rowWorkFlow["REQ_NO"]), getTableValue(rowWorkFlow["MODULE"]),
                                            getTableValue(rowWorkFlow["WF_ID"]), getTableValue(rowWorkFlow["WF_CUR_LEVEL"]), getTableValue(rowWorkFlow["WF_STAGENAME"]),
                                            getTableValue(rowWorkFlow["POSITIVE_LBL"]));
                                        DBHelper.updateDataBySqlText(strSql);

                                        //TODO
                                        //send email
                                        SendMail(ADmgrName);
                                    }
                                }
                            }
                        }
                    }


                }
                #endregion

                forward2Days();

                return true;
            }
            catch (Exception ex)
            {
                logStr.Add("batchProcessingStart");
                if (ex.InnerException != null)
                {
                    logStr.Add(ex.InnerException.Message);
                }
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP05_log.txt", "BP05_log.txt");
                return false;
            }
        }

        /// <summary>
        /// forward 2 days process
        /// </summary>
        private void forward2Days()
        {
            try
            {
                insertTEMP_NEW_TRANS(getT_WF_TRANSACTION_NEW_LEAVE());

                string strSql = string.Empty;

                // get reassign time
                int reassign = int.Parse(DBHelper.getData(DBHelper.BP01_GET_REASSIGNTIME).Rows[0]["VALUE"].ToString());

                DataTable dt = getTEMP_NEW_TRANS();

                foreach (DataRow dr in dt.Rows)
                {
                    DateTime dataCreated = DateTime.Parse(DateTime.Parse(getTableValue(dr["DATE_CREATED"])).ToString("yyyy-MM-dd"));
                    DateTime dateCurrent = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));

                    dataCreated = dataCreated.AddHours(reassign);

                    if (dataCreated > dateCurrent)
                    {
                        if (!checkIsGM(getTableValue(dr["WF_CUR_PIC"])))
                        {
                            string curPIC = string.Empty;
                            string curPICname = string.Empty;

                            ADManager.GetSuperiorInfo(getTableValue(dr["WF_CUR_PIC"]), out curPIC, out curPICname);

                            updateT_WF_TRANSACTION("system", getTableValue(dr["WF_TXN_ID"]));

                            strSql = string.Format(DBHelper.BP05_INS_T_WF_TRANSACTION2, DBManager.getREQNO("TXN"), getTableValue(dr["REQ_NO"]), getTableValue(dr["MODULE"]),
                                           getTableValue(dr["WF_ID"]), getTableValue(dr["WF_CUR_LEVEL"]), curPIC, curPICname, "0", getTableValue(dr["WF_STAGENAME"]),
                                           getTableValue(dr["CURR_COMMENT"]), getTableValue(dr["PREV_COMMENT"]), "system", getTableValue(dr["POSITIVE_LBL"]));
                            DBHelper.updateDataBySqlText(strSql);
                        }
                    }

                    // delete record from temp table
                    strSql = string.Format(DBHelper.BP05_DEL_TEMP_NEW_TRANS, getTableValue(dr["WF_TXN_ID"]));
                    DBHelper.updateDataBySqlText(strSql);

                }

            }
            catch (Exception ex)
            {
                logStr.Add("forward2Days");
                if (ex.InnerException != null)
                {
                    logStr.Add(ex.InnerException.Message);
                }
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP05_log.txt", "BP05_log.txt");
            }
        }

        /// <summary>
        /// insert content to T_DOC_ATTACHMENT
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        private DataTable getBatchTimeFromDB()
        {
            string reTime = string.Empty;
            DataTable dt = DBHelper.getData(DBHelper.BP01_GET_BATCHRUNTIME);

            return dt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private void getSAPData(string strSupervisor)
        {
            //SAP DATA
            //TODO function name? table?
            SapConnectInfo info = new SapConnectInfo(ConfigurationManager.AppSettings["Z_HR_PA_ELEAVE"]);
            Hashtable hashPars = new Hashtable();
            hashPars.Add(DBHelper.MODE_SAP, "VIEW");
            hashPars.Add("PERNR", strSupervisor);
            SapProfileResult result = SapProfile.getSAPFunData_Z_HR_PA_ELEAVE(info, hashPars);
            SAPDATA = result;
            dtHeader = result.T_HEADER;

        }

        private DataTable getHeaderData(string strSupervisor)
        {

            string sqlStr = String.Format("select * from wy_T_HEADER where PERNR = '{0}'", strSupervisor);
            DataTable dt = DBHelper.getData(sqlStr);
            return dt;
        }

        private void SapProfileForUT(string strSupervisor)
        {
            dtHeader = getHeaderData(strSupervisor);

        }

        /// <summary>
        /// insert content to T_BATCH_LOG
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        private string CaptureLog(string trigger)
        {
            string strBPIDNotFormat = System.DateTime.Now.ToString("yyyyMMddHHmmss").ToString();
            string strBPID = strBPIDNotFormat.Substring(0, strBPIDNotFormat.Length - 1);

            string sqlStr = String.Format(DBHelper.BP05_INS_T_BATCH_LOG,
                                        strBPID,
                                        trigger);

            DBHelper.updateDataBySqlText(sqlStr);

            return strBPID;

        }

        /// <summary>
        /// get TEMP_NEW_TRANS data
        /// </summary>
        /// <param name="emplyeeId"></param>
        private DataTable getTEMP_NEW_TRANS()
        {
            DataTable dt = null;
            try
            {
                dt = DBHelper.getData(DBHelper.BP05_GET_TEMP_NEW_TRANS);
                return dt;
            }
            catch (Exception ex)
            {
                logStr.Add("getTEMP_NEW_TRANS");
                if (ex.InnerException != null)
                {
                    logStr.Add(ex.InnerException.Message);
                }
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP05_log.txt", "BP05_log.txt");
            }
            return dt;
        }

        /// <summary>
        /// get M_DELEGATION data
        /// </summary>
        /// <param name="emplyeeId"></param>
        private DataTable getM_DELEGATION()
        {
            DataTable dt = DBHelper.getData(DBHelper.BP05_GET_M_DELEGATION);
            return dt;
        }

        /// <summary>
        /// get M_DELEGATION data
        /// </summary>
        /// <param name="emplyeeId"></param>
        private DataTable getM_DELEGATION_MEM()
        {
            //TODO
            //SQL right? is from M_DELEGATION_MEM only?
            DataTable dt = DBHelper.getData(DBHelper.BP05_GET_M_DELEGATION_MEM);
            return dt;
        }

        /// <summary>
        /// get T_WF_TRANSACTION data
        /// </summary>
        /// <returns></returns>
        private DataTable getT_WF_TRANSACTION_NEW_LEAVE()
        {
            DataTable dt = null;
            try
            {
                dt = DBHelper.getData(DBHelper.BP05_GET_T_WF_TRANSACTION_NEW_LEAVE);
            }
            catch (Exception ex)
            {
                logStr.Add("getT_WF_TRANSACTION");
                if (ex.InnerException != null)
                {
                    logStr.Add(ex.InnerException.Message);
                }
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP05_log.txt", "BP05_log.txt");
            }
            return dt;
        }

        /// <summary>
        /// insert TEMP_NEW_TRANS data
        /// </summary>
        /// <param name="dt"></param>
        private void insertTEMP_NEW_TRANS(DataTable dt)
        {
            try
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string strSql = string.Format(DBHelper.BP05_INS_TEMP_NEW_TRANS,
                                     dr["WF_TXN_ID"],
                                     dr["REQ_NO"],
                                     dr["MODULE"],
                                     dr["WF_ID"],
                                     dr["WF_CUR_LEVEL"],
                                     dr["WF_CUR_PIC"],
                                     dr["WF_CUR_PIC_NAME"],
                                     dr["WF_STAGENAME"],
                                     dr["CURR_COMMENT"],
                                     dr["PREV_COMMENT"],
                                     DateTime.Parse(dr["DATE_CREATED"].ToString()).ToString("yyyy-MM-dd"),
                                     dr["POSITIVE_LBL"]);

                    DBHelper.updateDataBySqlText(strSql);
                }
            }
            catch (Exception ex)
            {
                logStr.Add("insertTEMP_NEW_TRANS");
                if (ex.InnerException != null)
                {
                    logStr.Add(ex.InnerException.Message);
                }
                logStr.Add(ex.Message.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP05_log.txt", "BP05_log.txt");
            }
        }


        /// <summary>
        /// update M_DELEGATION data
        /// </summary>
        /// <param name="emplyeeId"></param>
        private void updateM_DELEGATION(string trigger, string strSupervisor, string strCostCenter)
        {
            string strSql = string.Format(DBHelper.BP05_UPD_M_DELEGATION, trigger, strSupervisor, strCostCenter);
            DBHelper.updateDataBySqlText(strSql);
        }

        /// <summary>
        /// update M_DELEGATION_MEM data
        /// </summary>
        /// <param name="emplyeeId"></param>
        private void updateM_DELEGATION_MEM(string trigger, string empID)
        {
            string strSql = string.Format(DBHelper.BP05_UPD_M_DELEGATION_MEM, trigger, empID);
            DBHelper.updateDataBySqlText(strSql);
        }

        /// <summary>
        /// get T_WF_TRANSACTION data
        /// </summary>
        /// <param name="emplyeeId"></param>
        private DataTable getT_WF_TRANSACTION(string strSupervisor)
        {
            DataTable dt = DBHelper.getData(string.Format(DBHelper.BP05_GET_T_WF_TRANSACTION, strSupervisor));
            return dt;
        }

        /// <summary>
        /// get T_WF_TRANSACTION, T_REQUEST_HRP,T_REQUEST_HRL,T_REQUEST_HRH data
        /// </summary>
        /// <param name="emplyeeId"></param>
        private DataTable getT_WF_TRANSACTION_ALL(string empID)
        {
            DataTable dt = DBHelper.getData(string.Format(DBHelper.BP05_GET_T_WF_TRANSACTION_ALL, empID));
            return dt;
        }


        /// <summary>
        /// update T_WF_TRANSACTION data
        /// </summary>
        /// <param name="emplyeeId"></param>
        private void updateT_WF_TRANSACTION(string trigger, string txnID)
        {
            string strSql = string.Format(DBHelper.BP05_UPD_T_WF_TRANSACTION, trigger, txnID);
            DBHelper.updateDataBySqlText(strSql);
        }

        private string getTableValue(object value)
        {
            if (value == DBNull.Value || value == null)
            {
                return string.Empty;
            }
            else
            {
                return value.ToString();
            }
        }

        private DateTime getTableDate(object value)
        {
            if (DBNull.Value == value)
            {
                //TODO
                return DateTime.MinValue;
            }
            else
            {
                return DateTime.Parse(value.ToString());
            }
        }

        private void getSAPLeaveData(string strPERNR)
        {
            SapConnectInfo info = new SapConnectInfo(ConfigurationManager.AppSettings["Z_HR_PA_ELEAVE"]);
            Hashtable hashPars = new Hashtable();
            hashPars.Add(DBHelper.MODE_SAP, "VIEW");

            hashPars.Add("PERNR", strPERNR);
            SapProfileResult result = SapProfile.getSAPFunData_Z_HR_PA_ELEAVE(info, hashPars);
            dtHeader = result.T_HEADER;

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
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP05_log.txt", "BP05_log.txt");
            }
        }

        /// <summary>  
        /// send mail  
        /// </summary> 
        public void SendMail(string groupName)
        {
            DataTable dt_Mail = DBHelper.getData(string.Format(DBHelper.BP07_QRY_M_EMAIL_FMT, ConfigurationManager.AppSettings["BP05NotifyEmailFormatID"]));
            if (dt_Mail.Rows.Count == 0)
            {
                return;
            }


            string subjectStr = dt_Mail.Rows[0]["SUBJECT"] != null ? dt_Mail.Rows[0]["SUBJECT"].ToString() : null;
            string tempBodyStr = dt_Mail.Rows[0]["CONTENT"] != null ? dt_Mail.Rows[0]["CONTENT"].ToString() : null;

            sendMailToAD_BP05(groupName, subjectStr, tempBodyStr, "Group");
            sendMailToAD_BP05(groupName, subjectStr, tempBodyStr, "user");
        }


        public void sendMailToAD_BP05(string strSTAGE_APPROVER, string subjectStr, string bodyStr, string strADType)
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
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP05_log.txt", "BP01_log.txt");
            }

        }

        /// <summary>
        /// check emp is whether general manager
        /// </summary>
        /// <param name="empId"></param>
        /// <returns></returns>
        private Boolean checkIsGM(string empId)
        {
            Boolean flag = false;

            try
            {
                string sqlStr = string.Format(DBManager.COM_CHECK_USER_IS_GM, empId.Trim());
                DataTable dt = DBManager.getInfoBySqlText(sqlStr);
                if (dt != null && dt.Rows.Count > 0)
                {
                    flag = true;
                }
            }
            catch (Exception ex)
            {
                logStr.Add("ex.Message:" + ex.Message.ToString());
                logStr.Add("Event Name:" + "checkIsGM");
                logStr.Add("date time:" + DateTime.Now.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\BP05_log.txt", "BP01_log.txt");
            }
            return flag;
        }
    }
}
