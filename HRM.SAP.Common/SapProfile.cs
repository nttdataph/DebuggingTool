using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
using SAP.Middleware.Connector;
using System.Collections;
using System.IO;

namespace Canon.HRM.SAP.Common
{
    public static class SapProfile
    {
        public static List<string> logStr = new List<string>();
        /// <summary>
        /// MODE from SAP
        /// </summary>
        public const string MODE_SAP = "MODE";

        /// <summary>
        /// employee id column name from SAP
        /// </summary>
        public const string EMPID_SAP = "PERNR";

        /// <summary>
        /// run Z_HR_PA_EPROFILE
        /// T_HEADER
        /// T_WORKDETAIL
        /// T_PERSONALINFO
        /// T_SPOUSEDEPENDENT
        /// T_INSURANCE
        /// T_EDUCATION
        /// T_EMERGENCYCONT
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ParamEMPID"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static SapProfileResult getSAPFunData_Z_HR_PA_EPROFILE(SapConnectInfo info, Hashtable pars)
        {
            logStr.Clear();
            logStr.Add("CallSapProfile  method start****************************************************************");
            SapProfileResult result = new SapProfileResult();
            logStr.Add("new SapProfileResult()");
            result.ErrorMessage = string.Empty;
            logStr.Add("result.ErrorMessage:" + result.ErrorMessage);
            try
            {
                logStr.Add("chkErrorResult:" + "info  IP:" + info.Ip);
                logStr.Add("SystemID:" + info.SystemID);
                logStr.Add("Client:" + info.Client);
                logStr.Add("UserName:" + info.UserName);
                logStr.Add("Password:" + info.Password);
                logStr.Add("FunctionName:" + info.FunctionName);
                //Get Result
                result = chkErrorResult(info);
                if (!String.IsNullOrEmpty(result.ErrorMessage))
                {
                    logStr.Add("return   result.ErrorMessage:" + result.ErrorMessage);
                    return result;
                }
                IRfcFunction myfun = null;
                logStr.Add("IRfcFunction myfun = null;");
                logStr.Add("GetIRfcFunction(info, pars)  start..");
                myfun = GetIRfcFunction(info, pars);
                logStr.Add("GetIRfcFunction(info, pars)  end..");
                logStr.Add("myfun:" + myfun);
                //Get Connect Info
                if (myfun!=null)
                {

                    IRfcTable resultHeader = myfun.GetTable(ConfigurationManager.AppSettings["T_HEADER"]);
                    logStr.Add("ConfigurationManager.AppSettings['T_HEADER']" + ConfigurationManager.AppSettings["T_HEADER"]);
                    logStr.Add("IRfcTable resultHeader = myfun.GetTable(ConfigurationManager.AppSettings['T_HEADER']);");

                    IRfcTable resultWD = myfun.GetTable(ConfigurationManager.AppSettings["ResultWD"]);
                    logStr.Add("ConfigurationManager.AppSettings['ResultWD']" + ConfigurationManager.AppSettings["ResultWD"]);
                    logStr.Add("IRfcTable resultWD = myfun.GetTable(ConfigurationManager.AppSettings['ResultWD']);");

                    IRfcTable resultPI = myfun.GetTable(ConfigurationManager.AppSettings["ResultPI"]);
                    logStr.Add("ConfigurationManager.AppSettings['ResultPI']" + ConfigurationManager.AppSettings["ResultPI"]);
                    logStr.Add("IRfcTable resultPI = myfun.GetTable(ConfigurationManager.AppSettings['ResultPI']);");

                    IRfcTable resultSD = myfun.GetTable(ConfigurationManager.AppSettings["ResultSD"]);
                    logStr.Add("ConfigurationManager.AppSettings['ResultSD']" + ConfigurationManager.AppSettings["ResultSD"]);
                    logStr.Add("IRfcTable resultSD = myfun.GetTable(ConfigurationManager.AppSettings['ResultSD']);");

                    IRfcTable resultIN = myfun.GetTable(ConfigurationManager.AppSettings["ResultIN"]);
                    logStr.Add("ConfigurationManager.AppSettings['ResultIN']" + ConfigurationManager.AppSettings["ResultIN"]);
                    logStr.Add("IRfcTable resultIN = myfun.GetTable(ConfigurationManager.AppSettings['ResultIN']);");

                    IRfcTable resultED = myfun.GetTable(ConfigurationManager.AppSettings["ResultED"]);
                    logStr.Add("ConfigurationManager.AppSettings['ResultED']" + ConfigurationManager.AppSettings["ResultED"]);
                    logStr.Add("IRfcTable resultED = myfun.GetTable(ConfigurationManager.AppSettings['ResultED']);");

                    IRfcTable resultEM = myfun.GetTable(ConfigurationManager.AppSettings["ResultEM"]);
                    logStr.Add("ConfigurationManager.AppSettings['ResultEM']" + ConfigurationManager.AppSettings["ResultEM"]);
                    logStr.Add("IRfcTable resultEM = myfun.GetTable(ConfigurationManager.AppSettings['ResultEM']);");

                    logStr.Add("object imgObj = myfun.GetValue('E_IMAGE');");
                    object imgObj = myfun.GetValue("E_IMAGE");
                    
                   // result.E_IMAGE=
                    logStr.Add("result.E_IMAGE = imgObj != null ? imgObj.ToString() : '';");
                    result.E_IMAGE = (imgObj != null && imgObj!=DBNull.Value) ? imgObj.ToString().Trim() : "";
                    logStr.Add("result.T_HEADER = getDataTableFromIRfcTable(resultHeader);");
                    result.T_HEADER = getDataTableFromIRfcTable(resultHeader);
                    logStr.Add(" result.T_WORKDETAIL = getDataTableFromIRfcTable(resultWD);");
                    result.T_WORKDETAIL = getDataTableFromIRfcTable(resultWD);
                    logStr.Add("result.T_PERSONALINFO = getDataTableFromIRfcTable(resultPI);");
                    result.T_PERSONALINFO = getDataTableFromIRfcTable(resultPI);
                    logStr.Add("result.T_SPOUSEDEPENDENT = getDataTableFromIRfcTable(resultSD);");
                    result.T_SPOUSEDEPENDENT = getDataTableFromIRfcTable(resultSD);
                    logStr.Add(" result.T_INSURANCE = getDataTableFromIRfcTable(resultIN);");
                    result.T_INSURANCE = getDataTableFromIRfcTable(resultIN);
                    logStr.Add("result.T_EDUCATION = getDataTableFromIRfcTable(resultED);");
                    result.T_EDUCATION = getDataTableFromIRfcTable(resultED);
                    logStr.Add("result.T_EMERGENCYCONT = getDataTableFromIRfcTable(resultEM);");
                    result.T_EMERGENCYCONT = getDataTableFromIRfcTable(resultEM);
                    
                }
            }
            catch (RfcCommunicationException ex)
            {
                //Communication Exception
                result.ErrorMessage = ex.Message;
                logStr.Add("result.ErrorMessage RfcCommunicationException:" + result.ErrorMessage);
                createFile(logStr, "C:\\NTT\\ErrorLog\\SAP_PS02_log.txt", "SAP_PS02_log.txt");
            }
            catch (Exception ex)
            {
                logStr.Add("ex:" + ex);
                createFile(logStr, "C:\\NTT\\ErrorLog\\SAP_PS02_log.txt", "SAP_PS02_log.txt");
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// Z_HR_PA_ELEAVE
        /// T_HEADER
        ///T_LEAVESUMMARY
        ///T_LEAVEHISTORY
        ///T_PREVBALANCE
        ///T_ACCUMHISTORY
        /// </summary>
        /// <param name="info"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public static SapProfileResult getSAPFunData_Z_HR_PA_ELEAVE(SapConnectInfo info, Hashtable pars)
        {
            logStr.Add("LS04_GetELeaveData()  start");
            SapProfileResult result = new SapProfileResult();
            result.ErrorMessage = string.Empty;
            try
            {
                //Get Result
                result = chkErrorResult(info);
                if (!String.IsNullOrEmpty(result.ErrorMessage))
                {
                    return result;
                }
                IRfcFunction myfun = null;
                logStr.Add("GetIRfcFunction()  start");
                myfun = GetIRfcFunction(info, pars);
                logStr.Add("GetIRfcFunction()  end");

                logStr.Add("imgObj  start");
                object imgObj = myfun.GetValue("E_IMAGE");
                logStr.Add("imgObj  end");

                logStr.Add("resultHeader  start");
                IRfcTable resultHeader = myfun.GetTable(ConfigurationManager.AppSettings["T_HEADER"]);
                logStr.Add("resultHeader  end");
                logStr.Add("resultLeaveSummary  start");
                IRfcTable resultLeaveSummary = myfun.GetTable(ConfigurationManager.AppSettings["T_LEAVESUMMARY"]);
                logStr.Add("resultLeaveSummary  end");

                logStr.Add("resultLeaveHistory  start");
                IRfcTable resultLeaveHistory = myfun.GetTable(ConfigurationManager.AppSettings["T_LEAVEHISTORY"]);
                logStr.Add("resultLeaveHistory  end");

                logStr.Add("resultPrevBalance  start");
                IRfcTable resultPrevBalance = myfun.GetTable(ConfigurationManager.AppSettings["T_PREVBALANCE"]);
                logStr.Add("resultPrevBalance  end");

                logStr.Add("resultAccumHistory  start");
                IRfcTable resultAccumHistory = myfun.GetTable(ConfigurationManager.AppSettings["T_ACCUMHISTORY"]);
                logStr.Add("resultAccumHistory  end");

                logStr.Add("E_IMAGE  start");
                result.E_IMAGE = (imgObj != null && imgObj != DBNull.Value) ? imgObj.ToString().Trim() : "";
                logStr.Add("E_IMAGE  end");

                logStr.Add("T_HEADER  start");
                result.T_HEADER = getDataTableFromIRfcTable(resultHeader);
                logStr.Add("T_HEADER  end");

                logStr.Add("T_LEAVESUMMARY  start");
                result.T_LEAVESUMMARY = getDataTableFromIRfcTable(resultLeaveSummary);
                logStr.Add("T_LEAVESUMMARY  end");

                logStr.Add("T_LEAVEHISTORY  start");
                result.T_LEAVEHISTORY = getDataTableFromIRfcTable(resultLeaveHistory);
                logStr.Add("T_LEAVEHISTORY  end");

                logStr.Add("T_PREVBALANCE  start");
                result.T_PREVBALANCE = getDataTableFromIRfcTable(resultPrevBalance);
                logStr.Add("T_PREVBALANCE  end");

                logStr.Add("T_ACCUMHISTORY  start");
                result.T_ACCUMHISTORY = getDataTableFromIRfcTable(resultAccumHistory);
                logStr.Add("T_ACCUMHISTORY  end");
            }
            catch (RfcCommunicationException ex)
            {
                //Communication Exception
                result.ErrorMessage = ex.Message;
                logStr.Add("RfcCommunicationException" + ex.Message);
                createFile(logStr, "C:\\NTT\\ErrorLog\\LS04_GetELeaveData.txt", "LS04_GetELeaveData.txt");
            }
            catch (Exception ex)
            {
                logStr.Add("Exception");
                createFile(logStr, "C:\\NTT\\ErrorLog\\LS04_GetELeaveData.txt", "LS04_GetELeaveData.txt");
                throw ex;

            }

            return result;
        }

        /// <summary>
        /// BP08 run Z_HR_PA_VALTAB only use below table,so only return 2 table
        /// T_ORGCHART
        /// T_BOSSONLY
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ParamEMPID"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static SapProfileResult getSAPFunData_Z_HR_PA_VALTAB(SapConnectInfo info, Hashtable pars)
        {
            logStr.Clear();
            logStr.Add("CallSapMyleave  method start****************************************************************");
            SapProfileResult result = new SapProfileResult();
            logStr.Add("new SapProfileResult()");
            result.ErrorMessage = string.Empty;
            logStr.Add("result.ErrorMessage:" + result.ErrorMessage);
            try
            {
                logStr.Add("chkErrorResult:" + "info  IP:" + info.Ip);
                logStr.Add("SystemID:" + info.SystemID);
                logStr.Add("Client:" + info.Client);
                logStr.Add("UserName:" + info.UserName);
                logStr.Add("Password:" + info.Password);
                logStr.Add("FunctionName:" + info.FunctionName);
                //Get Result
                result = chkErrorResult(info);
                if (!String.IsNullOrEmpty(result.ErrorMessage))
                {
                    logStr.Add("return   result.ErrorMessage:" + result.ErrorMessage);
                    return result;
                }
                IRfcFunction myfun = null;
                logStr.Add("IRfcFunction myfun = null;");
                logStr.Add("GetIRfcFunction(info, pars)  start..");
                myfun = GetIRfcFunction(info, pars);
                logStr.Add("GetIRfcFunction(info, pars)  end..");
                logStr.Add("myfun:" + myfun);
                //Get Connect Info
                if (myfun != null)
                {

                    IRfcTable resultHeader = myfun.GetTable(ConfigurationManager.AppSettings["T_HEADER"]);
                    logStr.Add("ConfigurationManager.AppSettings['T_HEADER']" + ConfigurationManager.AppSettings["T_HEADER"]);
                    logStr.Add("IRfcTable resultHeader = myfun.GetTable(ConfigurationManager.AppSettings['T_HEADER']);");

                    IRfcTable resultOR = myfun.GetTable(ConfigurationManager.AppSettings["ResultOR"]);
                    logStr.Add("ConfigurationManager.AppSettings['ResultOR']" + ConfigurationManager.AppSettings["ResultOR"]);
                    logStr.Add("IRfcTable resultOR = myfun.GetTable(ConfigurationManager.AppSettings['ResultOR']);");

                    IRfcTable T_BOSSONLY = myfun.GetTable(ConfigurationManager.AppSettings["T_BOSSONLY"]);

                    logStr.Add("object imgObj = myfun.GetValue('E_IMAGE');");
                    object imgObj = myfun.GetValue("E_IMAGE");

                    // result.E_IMAGE=
                    logStr.Add("result.T_ORGCHART = getDataTableFromIRfcTable(resultOR);");
                    result.T_ORGCHART = getDataTableFromIRfcTable(resultOR);
                    result.T_BOSSONLY = getDataTableFromIRfcTable(T_BOSSONLY);
                }
            }
            catch (RfcCommunicationException ex)
            {
                //Communication Exception
                result.ErrorMessage = ex.Message;
                logStr.Add("result.ErrorMessage RfcCommunicationException:" + result.ErrorMessage);
                createFile(logStr, "C:\\NTT\\ErrorLog\\SAP_PS02_log.txt", "SAP_PS02_log.txt");
            }
            catch (Exception ex)
            {
                logStr.Add("ex:" + ex);
                createFile(logStr, "C:\\NTT\\ErrorLog\\SAP_PS02_log.txt", "SAP_PS02_log.txt");
                throw ex;
            }
            return result;
        }
        
        /// <summary>
        /// return table 
        /// GT_WS
        /// </summary>
        /// <param name="info"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public static SapProfileResult getSAPFunData_Z_HR_PA_ATTEND_VIEW_talbe(SapConnectInfo info, Hashtable pars)
        {
            SapProfileResult result = new SapProfileResult();
            result.ErrorMessage = string.Empty;

            try
            {
                result = chkErrorResult(info);
                if (!String.IsNullOrEmpty(result.ErrorMessage))
                {
                    return result;
                }

                IRfcFunction myfun = GetIRfcFunction(info, pars);
                IRfcTable resultGT_WS = myfun.GetTable(ConfigurationManager.AppSettings["GT_WS"]);
                result.GT_WS = getDataTableFromIRfcTable(resultGT_WS);
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                logStr.Add("Message:" + ex.Message);
                logStr.Add("LS02_CallSapAttend");
                createFile(logStr, "C:\\NTT\\ErrorLog\\LS02_CallSapAttend.txt", "LS02_CallSapAttend.txt");               
            }

            return result;
        }

        public static SapProfileResult getSAPFunData_Z_HR_PA_ELEAVE_only_HEADER_TB(SapConnectInfo info, Hashtable pars)
        {
            SapProfileResult result = new SapProfileResult();

            logStr.Add("FunctionName==" + info.FunctionName);
            logStr.Add("SapProfileResult result = new SapProfileResult();");
            result.ErrorMessage = string.Empty;
            try
            {
                //Get Result
                result = chkErrorResult(info);
                if (!String.IsNullOrEmpty(result.ErrorMessage))
                {
                    return result;
                }
                IRfcFunction myfun = null;
                logStr.Add("myfun = GetIRfcFunction(info, pars); start ");
                myfun = GetIRfcFunction(info, pars);
                logStr.Add("myfun = " + myfun);
                logStr.Add("myfun = GetIRfcFunction(info, pars); end ");
                //Get Connect Info
                logStr.Add("IRfcTable resultHeader = myfun.GetTable(ConfigurationManager.AppSettings['T_HEADER']);");
                IRfcTable resultHeader = myfun.GetTable(ConfigurationManager.AppSettings["T_HEADER"]);
                logStr.Add("result.T_HEADER = getDataTableFromIRfcTable(resultHeader);");
                result.T_HEADER = getDataTableFromIRfcTable(resultHeader);
                logStr.Add("result.T_HEADER = " + result.T_HEADER);
            }
            catch (RfcCommunicationException ex)
            {
                //Communication Exception
                result.ErrorMessage = ex.Message;
                logStr.Add("result.ErrorMessage :" + result.ErrorMessage);
            }
            catch (Exception ex)
            {
                logStr.Add("ex.message:" + ex.Message.ToString());
            }
            logStr.Add("getSAPFunData_Z_HR_PA_ELEAVE_only_HEADER_TB()  end");
            //createFile(logStr, "C:\\NTT\\ErrorLog\\SAP_MS05_CallSap_log.txt", "SAP_MS05_CallSap_log.txt");
            return result;
        }
        
        public static string getLockStatus(SapConnectInfo info, Hashtable pars)
        {
            SapProfileResult result = new SapProfileResult();
            result.ErrorMessage = string.Empty;
            try
            {
                logStr.Add("chkErrorResult:" + "info  IP:" + info.Ip);
                logStr.Add("SystemID:" + info.SystemID);
                logStr.Add("Client:" + info.Client);
                logStr.Add("UserName:" + info.UserName);
                logStr.Add("Password:" + info.Password);
                logStr.Add("FunctionName:" + info.FunctionName);
                //createFile(logStr, "C:\\NTT\\ErrorLog\\getLockStatus_log.txt", "getLockStatus_log.txt");
                //Get Result
                result = chkErrorResult(info);
                if (!String.IsNullOrEmpty(result.ErrorMessage))
                {
                    logStr.Add("return   result.ErrorMessage:" + result.ErrorMessage);
                    return "ERROR";
                }
                IRfcFunction myfun = null;
                logStr.Add("IRfcFunction myfun = null;");
                logStr.Add("getLockStatus(info, pars)  start..");
                myfun = GetIRfcFunction(info, pars);
                logStr.Add("getLockStatus(info, pars)  end..");
                logStr.Add("myfun:" + myfun);
                //Get Connect Info
                if (myfun != null)
                {
                    object imgObj = myfun.GetValue("E_STATUS");
                    if(imgObj != null)
                    {
                        return imgObj.ToString();
                    }
                }
                return string.Empty;
            }
            catch (RfcCommunicationException ex)
            {
                //Communication Exception
                result.ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return string.Empty;
        }

        public static string getConnectStatus(SapConnectInfo info, Hashtable pars)
        {
            SapProfileResult result = new SapProfileResult();
            result.ErrorMessage = string.Empty;
            try
            {
                logStr.Add("chkErrorResult:" + "info  IP:" + info.Ip);
                logStr.Add("SystemID:" + info.SystemID);
                logStr.Add("Client:" + info.Client);
                logStr.Add("UserName:" + info.UserName);
                logStr.Add("Password:" + info.Password);
                logStr.Add("FunctionName:" + info.FunctionName);
                //Get Result
                result = chkErrorResult(info);
                if (!String.IsNullOrEmpty(result.ErrorMessage))
                {
                    return result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return string.Empty;
        }

        /// <summary>
        /// true:LOCKED
        /// </summary>
        /// <returns></returns>
        public static bool chkSAPIsLocked()
        {
            Hashtable hash = new Hashtable();
            SapConnectInfo info = new SapConnectInfo(ConfigurationManager.AppSettings["Z_HR_PA_PAYROLL_CHK"]);
            string returnvalue = getLockStatus(info, hash);
            if (returnvalue.Contains("LOCKED") && !returnvalue.Equals("UNLOCKED"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool chkSAPIsConnected()
        {
            Hashtable hash = new Hashtable();
            SapConnectInfo info = new SapConnectInfo(ConfigurationManager.AppSettings["Z_HR_PA_PAYROLL_CHK"]);
            string returnValue = getConnectStatus(info, hash);

            if (string.IsNullOrEmpty(returnValue))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// check error 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static SapProfileResult chkErrorResult(SapConnectInfo info)
        {
            logStr.Add("SapProfileResult result = new SapProfileResult()");
            SapProfileResult result = new SapProfileResult();
            string myIp = info.Ip;
            string mySystemID = info.SystemID;
            string myClient = info.Client;
            string userName = info.UserName;
            string password = info.Password;
            string refFunctionName = info.FunctionName;
            logStr.Add("myIp:" + myIp);
            logStr.Add("mySystemID:" + mySystemID);
            logStr.Add("myClient:" + myClient);
            logStr.Add("userName:" + userName);
            logStr.Add("password:" + password);
            logStr.Add("refFunctionName:" + refFunctionName);
            if (string.IsNullOrEmpty(myIp))
            {
                result.ErrorMessage = "CallSapProfile Error: Ip is empty.";
                return result;
            }
            if (string.IsNullOrEmpty(myClient))
            {
                result.ErrorMessage = "CallSapProfile Error: Client is empty.";
                return result;
            }
            if (string.IsNullOrEmpty(mySystemID))
            {
                result.ErrorMessage = "CallSapProfile Error: SystemID is empty.";
                return result;
            }
            if (string.IsNullOrEmpty(userName))
            {
                result.ErrorMessage = "CallSapProfile Error: UserName is empty.";
                return result;
            }
            if (string.IsNullOrEmpty(password))
            {
                result.ErrorMessage = "CallSapProfile Error: Password is empty.";
                return result;
            }
            if (string.IsNullOrEmpty(refFunctionName))
            {
                result.ErrorMessage = "CallSapProfile Error: RefFunctionName is empty.";
                return result;
            }
            logStr.Add("result.ErrorMessage:" + result.ErrorMessage);
            logStr.Add("chkErrorResult method end..");
            return result;
        }

        /// <summary>
        /// get function
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ParamEMPID"></param>
        /// <param name="ModeVal"></param>
        /// <returns></returns>
        public static IRfcFunction GetIRfcFunction(SapConnectInfo info, Hashtable pars)
        {
            //try
            //{
                SapProfileResult result = new SapProfileResult();
                logStr.Add("SapProfileResult result = new SapProfileResult();");
                string myIp = info.Ip;
                string mySystemID = info.SystemID;
                string myClient = info.Client;
                string userName = info.UserName;
                string password = info.Password;
                string refFunctionName = info.FunctionName;
                logStr.Add("myIp:" + myIp);
                logStr.Add("mySystemID:" + mySystemID);
                logStr.Add("myClient:" + myClient);
                logStr.Add("userName:" + userName);
                logStr.Add("password:" + password);
                logStr.Add("refFunctionName:" + refFunctionName);
                RfcConfigParameters rfc = new RfcConfigParameters();
                logStr.Add("RfcConfigParameters rfc = new RfcConfigParameters();");
                rfc.Add(RfcConfigParameters.Name, "ANY");
                logStr.Add(" rfc.Add(RfcConfigParameters.Name, 'ANY');");
                rfc.Add(RfcConfigParameters.AppServerHost, myIp);
                logStr.Add(" rfc.Add(RfcConfigParameters.AppServerHost, myIp);");
                rfc.Add(RfcConfigParameters.Client, myClient);
                logStr.Add("rfc.Add(RfcConfigParameters.Client, myClient);");
                //rfc.Add(RfcConfigParameters.SAPRouter, myRouter)
                rfc.Add(RfcConfigParameters.User, userName);
                logStr.Add("rfc.Add(RfcConfigParameters.User, userName);");
                rfc.Add(RfcConfigParameters.Password, password);
                logStr.Add(" rfc.Add(RfcConfigParameters.Password, password);");
                rfc.Add(RfcConfigParameters.SystemID, mySystemID);
                logStr.Add("rfc.Add(RfcConfigParameters.SystemID, mySystemID);");

                RfcDestination rfcdest = RfcDestinationManager.GetDestination(rfc);
                logStr.Add("RfcDestination rfcdest = RfcDestinationManager.GetDestination(rfc);");
                RfcRepository rfcrep = rfcdest.Repository;
                logStr.Add(" RfcRepository rfcrep = rfcdest.Repository;");
                //Set Params for SAP query
                //Value depends on function call

                IRfcFunction myfun = null;
                logStr.Add("IRfcFunction myfun = null;");
                myfun = rfcrep.CreateFunction(refFunctionName);
                logStr.Add("myfun = rfcrep.CreateFunction(refFunctionName);");
                logStr.Add("myfun.SetValue(key, pars[key]);");
                foreach (string key in pars.Keys)
                {
                    myfun.SetValue(key, pars[key]);
                    logStr.Add("key:" + key + ";   pars[key]" + pars[key]);
                }
                //myfun.SetValue("MODE", ModeVal);
                //myfun.SetValue("PERNR", ParamEMPID);
                //Run
                myfun.Invoke(rfcdest);
                logStr.Add("myfun.Invoke(rfcdest)");
                logStr.Add("myfun:" + myfun);
                //createFile(logStr, "C:\\NTT\\ErrorLog\\GetIRfcFunction_log.txt", "SAP_MS02b_CallSapHeader.txt");
                return myfun;

            //}
            //catch (RfcCommunicationException ex)
            //{
            //    //Communication Exception

            //    logStr.Add("result.ErrorMessage :" + ex.Message);
            //    createFile(logStr, "C:\\NTT\\ErrorLog\\SapProfile.txt", "SapProfile.txt");
            //    return null;
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}

        }

        /// <summary>
        /// get DataTable Structure Data from IRfcTable
        /// </summary>
        /// <param name="rTable"></param>
        /// <returns></returns>
        public static DataTable getDataTableFromIRfcTable(IRfcTable rTable)
        {
            DataTable dtRes = null;
            if (rTable != null)
            {
                //get table
                dtRes = new DataTable();
                logStr.Add("rTable.ElementCount:" + rTable.ElementCount);
                for (int j = 0; j <= rTable.ElementCount - 1; j++)
                {
                    dtRes.Columns.Add(rTable.GetElementMetadata(j).Name);
                    logStr.Add("rTable.GetElementMetadata(" + j + ").Name  :" + rTable.GetElementMetadata(j).Name);
                }

                for (int j = 0; j <= rTable.RowCount - 1; j++)
                {
                    rTable.CurrentIndex = j;
                    Object[] values = new object[rTable.ElementCount];
                    logStr.Add("values " + values);
                    for (int k = 0; k <= rTable.ElementCount - 1; k++)
                    {
                        values[k] = rTable.CurrentRow.GetValue(k);
                        logStr.Add("rTable.CurrentRow.GetValue(" + k + ")  :" + rTable.CurrentRow.GetValue(k));
                    }
                    dtRes.Rows.Add(values);
                }

            }
            return dtRes;

        }

        /// <summary>
        /// get data from SAP
        /// </summary>
        /// <param name="empId"></param>
        /// <returns></returns>
        public static SapProfileResult GetSAPDataForHelpDesk(string empId,string strSAPFunName)
        {
            SapProfileResult result = null;
            try
            {
                string errorMsg = string.Empty;
                SapConnectInfo info = new SapConnectInfo(ConfigurationManager.AppSettings[strSAPFunName]);
                result = new SapProfileResult();
                Hashtable hash = new Hashtable();
                hash.Add(MODE_SAP, "VIEW");
                hash.Add(EMPID_SAP, empId.PadLeft(8, '0'));
                if (strSAPFunName.Equals("Z_HR_PA_EPROFILE"))
                {
                    result = getSAPFunData_Z_HR_PA_EPROFILE(info, hash);
                }
                else if (strSAPFunName.Equals("Z_HR_PA_ELEAVE"))
                {
                    result = getSAPFunData_Z_HR_PA_ELEAVE(info, hash);
                }

            }
            catch (Exception ex)
            {
                logStr.Add("ex.Message:" + ex.Message.ToString());
                logStr.Add("Method Name:" + "GetSAPData");
                logStr.Add("date time:" + DateTime.Now.ToString());
                createFile(logStr, "C:\\NTT\\ErrorLog\\SapProfile.txt", "SapProfile.txt");

            }
            return result;
        }

        #region Write Log
        /// <summary>
        /// 逐行写入到文件中
        /// </summary>
        /// <param name="content">要写入的内容</param>
        /// <param name="fullPath">文件的完整路径</param>
        /// <param name="fileName">文件名</param>
        public static void createFile(List<string> content, string fullPath, string fileName)
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
        #endregion
    }
}
