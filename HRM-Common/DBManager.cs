using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace Canon.HRM.Common
{
    public static class DBManager
    {

        #region PS02
        #region SQL
        public const string test_QRY_PER = "select ATTACH_ITEM,ATTACH_FILENAME from T_DOC_ATTACHMENT where ATTACH_ID='2'";
        
        /// <summary>
        ///get request information from  T_REQUEST_HRP by employeeId
        /// </summary>
        public const string QRY_T_REQUEST_HRP = @"select EMP_ID,REQ_NO,EMP_NAME,COST_CENTER,POSITION,JOINED_DT,REQ_TYPE,IC_NO,REQ_CODE,WF_STATUS,BR_STATUS,Gender ,RELIGION  from T_REQUEST_HRP  where EMP_ID = '{0}' and BR_STATUS < 3 ;";
        /// <summary>
        ///  get request information from  T_WD_NO_WF by employeeId
        ///  and (r.REQ_TYPE='EPF' or r.REQ_TYPE='SOC' or r.REQ_TYPE='TAX')
        /// </summary>
        public const string QRY_T_WD_NO_WF = @"select  A.FLD_TYPE,A.ATTACH_ID,A.VALUE ,d.ATTACH_FILENAME,d.ATTACH_ITEM
                                                from(
                                                select  w.FLD_TYPE,w.ATTACH_ID,w.VALUE
                                                from T_WD_NO_WF w ,T_REQUEST_HRP r 
                                                where w.FLD_TYPE='EPF'
                                                 and w.REQ_NO=r.REQ_NO and  r.EMP_ID = '{0}'           
                                                 and w.EMP_ID =r.EMP_ID and r.BR_STATUS < 3 
                                               ) as A left join T_DOC_ATTACHMENT d on A.ATTACH_ID=d.ATTACH_ID
                                                  union all 
                                                select  A.FLD_TYPE,A.ATTACH_ID,A.VALUE ,d.ATTACH_FILENAME,d.ATTACH_ITEM
                                                from(
                                                select  w.FLD_TYPE,w.ATTACH_ID,w.VALUE
                                                from T_WD_NO_WF w ,T_REQUEST_HRP r 
                                                where w.FLD_TYPE='SOC'
                                                 and w.REQ_NO=r.REQ_NO and  r.EMP_ID = '{0}'           
                                                 and w.EMP_ID =r.EMP_ID and r.BR_STATUS < 3 
                                              ) as A left join T_DOC_ATTACHMENT d on A.ATTACH_ID=d.ATTACH_ID
                                                 union all
                                                   select  A.FLD_TYPE,A.ATTACH_ID,A.VALUE ,d.ATTACH_FILENAME,d.ATTACH_ITEM
                                                from(
                                                select  w.FLD_TYPE,w.ATTACH_ID,w.VALUE
                                                from T_WD_NO_WF w ,T_REQUEST_HRP r 
                                                where w.FLD_TYPE='TAX'
                                                 and w.REQ_NO=r.REQ_NO and  r.EMP_ID = '{0}'           
                                                 and w.EMP_ID =r.EMP_ID and r.BR_STATUS < 3 
                                              ) as A left join T_DOC_ATTACHMENT d on A.ATTACH_ID=d.ATTACH_ID";

        /// <summary>
        /// get request information from  T_WD_WF by employeeId
        /// </summary>
        public const string QRY_T_WD_WF = @" SELECT  T_WD_WF.PASSPORT_NO, T_WD_WF.PASSPORT_EXP, T_WD_WF.PASSPORT_ISSUE_DT, T_WD_WF.PASSPORT_ISSUE_PL, T_WD_WF.PERMIT_NO, 
                                                              T_WD_WF.PERMIT_EXP, T_WD_WF.REQ_NO, T_WD_WF.ATTACH_ID, REQ_HRP.WF_STATUS
                                                      FROM    T_WD_WF INNER JOIN
                                                                  (SELECT  TOP (1) REQ_NO,GRP_REQNO, WF_STATUS
                                                                    FROM   T_REQUEST_HRP
                                                                    WHERE  (EMP_ID = '{0}') AND (REQ_TYPE = 'PAS')
                                                                    ORDER BY USER_CREATED) AS REQ_HRP  ON T_WD_WF.GRP_REQNO = REQ_HRP.GRP_REQNO";
        /// <summary>
        ///   get request information from  T_PERSONAL by employeeId
        /// </summary>
        public const string QRY_T_PERSONAL = @" select r.REQ_NO, p.CUR_ADD1,p.CUR_ADD2,p.CUR_ADD3,p.CUR_POSTCODE,p.CUR_CITY,p.CUR_STATE,p.PHONE_HOME,p.PHONE_HP,
                                                p.PER_ADD1,p.PER_ADD2,p.PER_ADD3,p.PER_POSTCODE,p.PER_CITY,p.PER_STATE,p.RELIGION,p.BUS_ROUTE
                                                from T_REQUEST_HRP r,T_PERSONAL p where r.EMP_ID = p.EMP_ID
									            and r.REQ_NO=p.REQ_NO and r.REQ_CODE='PD' and r.BR_STATUS < 3   and  r.EMP_ID  = '{0}'";
        /// <summary>
        ///  get request information from  T_SP_LIST by employeeId
        /// </summary>
        public const string QRY_T_SP_LIST = @"select ROW_NUMBER() over(order by sp.SP_SEQ) as NUM, sp.MARRIAGE_STS,sp.SP_WORKING_STS,sp.SP_NAME,sp.SP_ICNo,
                                              		sp.SP_DOB,sp.SP_OCCUPATION,sp.ATTACH_ID, sp.WF_STATUS , t.ATTACH_FILENAME,t.ATTACH_ITEM,sp.DEL_FLAG
                                              from(
                                                  select  s.MARRIAGE_STS,s.SP_WORKING_STS,s.SP_NAME,s.SP_ICNo,s.SP_DOB,s.SP_OCCUPATION,s.SP_SEQ,
                                              			  s.DATE_MODIFIED, s.ATTACH_ID,r.WF_STATUS ,s.DEL_FLAG
                                                  from T_REQUEST_HRP r left join T_SP_LIST s on r.GRP_REQNO=s.GRP_REQNO
                                                  where r.EMP_ID = s.EMP_ID  and s.REQ_NO=r.REQ_NO
                                                       and r.REQ_CODE='SD' and r.REQ_TYPE='SPO' and  r.EMP_ID = '{0}' and r.BR_STATUS < 3 
                                                  )as sp  LEFT OUTER JOIN  T_DOC_ATTACHMENT t ON sp.ATTACH_ID = t.ATTACH_ID 
                                              order by sp.SP_SEQ,sp.DATE_MODIFIED;";
        /// <summary>
        /// get request information from  T_SP_DEPENDANT by employeeId
        /// </summary>
        public const string QRY_T_SP_DEPENDANT = @" select  ROW_NUMBER() over(order by spd.DEP_SEQ) as NUM, spd.DEP_NAME,spd.DEP_ICNO,spd.DEP_DOB,spd.DEP_ADOPTED,spd.DEP_HC,
                                                               spd.DEP_NEWBORN,spd.ATTACH_ID,spd.WF_STATUS,t.ATTACH_FILENAME,t.ATTACH_ITEM,spd.DEL_FLAG
                                                     from(
                                                          select s.DEP_NAME,s.DEP_ICNO,s.DEP_DOB,s.DEP_ADOPTED,s.DEP_HC,s.DEP_NEWBORN,s.ATTACH_ID ,r.WF_STATUS,s.DEP_SEQ,
                                                          r.DATE_MODIFIED,s.DEL_FLAG
                                                          from T_REQUEST_HRP  r left join T_SP_DEPENDANT s on r.GRP_REQNO=s.GRP_REQNO
                                                          where r.EMP_ID = '{0}' 
                                                          and r.BR_STATUS < 3 
                                                          and r.REQ_TYPE='DEP'
                                                          and r.EMP_ID=s.EMP_ID ) as spd   LEFT OUTER JOIN  T_DOC_ATTACHMENT t
                                                      ON spd.ATTACH_ID = t.ATTACH_ID   order by spd.DEP_SEQ, spd.DATE_MODIFIED;";
        /// <summary>
        ///  get request information from  T_INS_NOMINEE by employeeId
        /// </summary>
        public const string QRY_T_INS_NOMINEE = @" select ROW_NUMBER() over(order by A.NOM_SEQ) as NUM ,A.WF_STATUS,A.ATTACH_ID,A.DEL_FLAG,
                                                          A.NOM_NAME,A.NOM_RELATION,A.NOM_ICNO,A.NOM_BIRTH_CERT,A.DATE_MODIFIED,A.NOM_SEQ,t.ATTACH_FILENAME,t.ATTACH_ITEM
                                                   from(
                                                      select r.WF_STATUS,ins.ATTACH_ID,ins.NOM_NAME,ins.NOM_RELATION,ins.NOM_ICNO,ins.NOM_BIRTH_CERT,r.DATE_MODIFIED,ins.NOM_SEQ,ins.DEL_FLAG
                                                      from T_REQUEST_HRP r,T_INS_NOMINEE ins  
                                                      where r.EMP_ID = ins.EMP_ID and r.BR_STATUS < 3 
                                                         and ins.REQ_NO=r.REQ_NO and r.REQ_CODE='IN' and r.REQ_TYPE='INS'  and  r.EMP_ID = '{0}'
                                                      ) as A LEFT OUTER JOIN  T_DOC_ATTACHMENT t
                                                     ON A.ATTACH_ID = t.ATTACH_ID order by A.NOM_SEQ, A.DATE_MODIFIED;";
        /// <summary>
        /// get request information from  T_SP_LIST and  T_SP_DEPENDANT by employeeId
        /// </summary>
        public const string QRY_IN_DEPENDANT = @" select ROW_NUMBER() over(order by A.SEQ) as NUM ,A.NAME,A.RELATIONSHIP,A.ICNO ,A.DOB ,A.SEQ,A.MARRIAGE_DATE,
                                                          A.MARRIAGE_STS,A.ADOPTED, A.WF_STATUS,A.ATTACH_ID,t.ATTACH_FILENAME,t.ATTACH_ITEM,A.DATE_MODIFIED
                                                          from(
                                                   select d.DEP_NAME as NAME,'Child' as RELATIONSHIP, d.DEP_ICNO as ICNO ,d.DEP_DOB as DOB ,d.DEP_SEQ as SEQ ,convert(datetime, null, 103) as MARRIAGE_DATE,
                                                          '-' as MARRIAGE_STS,d.DEP_ADOPTED as ADOPTED, r.WF_STATUS as WF_STATUS,d.ATTACH_ID as ATTACH_ID,r.DATE_MODIFIED
                                                   from  T_SP_DEPENDANT d left join T_REQUEST_HRP r on r.GRP_REQNO=d.GRP_REQNO 
                                                    where  r.EMP_ID=d.EMP_ID
                                                         and  r.REQ_TYPE='DEP' and r.BR_STATUS < 3 
                                                         and r.EMP_ID = '{0}'  ) as A LEFT OUTER JOIN  T_DOC_ATTACHMENT t
                                                   ON A.ATTACH_ID = t.ATTACH_ID order by A.SEQ,A.DATE_MODIFIED;";
        /// <summary> 
        /// get request information from  T_SP_LIST and  T_SP_DEPENDANT by employeeId
        /// </summary>
        public const string QRY_IN_SP_LIST = @" select ROW_NUMBER() over(order by A.SEQ) as NUM ,A.NAME,A.RELATIONSHIP,A.ICNO ,A.DOB ,A.SEQ,A.MARRIAGE_DATE,
                                                        A.MARRIAGE_STS,A.ADOPTED, A.WF_STATUS,A.ATTACH_ID,t.ATTACH_FILENAME,t.ATTACH_ITEM,A.DATE_MODIFIED
                                                from(
                                                     select  s.SP_NAME as NAME,'Spouse' as RELATIONSHIP, s.SP_ICNO as ICNO,s.SP_DOB as DOB ,s.SP_SEQ as SEQ ,s.MARRIAGE_DATE as MARRIAGE_DATE,
                                                        s.MARRIAGE_STS as MARRIAGE_STS,'0' as ADOPTED, r.WF_STATUS as WF_STATUS,s.ATTACH_ID as ATTACH_ID,r.DATE_MODIFIED
                                                     from  T_SP_LIST s left join T_REQUEST_HRP r on r.GRP_REQNO=s.GRP_REQNO  
                                                     where r.EMP_ID=s.EMP_ID and r.BR_STATUS < 3 
                                                       and  r.REQ_TYPE='SPO'  and r.EMP_ID = '{0}') as A LEFT OUTER JOIN  T_DOC_ATTACHMENT t
                                                   ON A.ATTACH_ID = t.ATTACH_ID order by A.SEQ,A.DATE_MODIFIED;";

        /// <summary>
        ///  get request information from T_ACADEMIC by employeeId
        /// </summary>
        public const string QRY_T_ACADEMIC = @"  select  ROW_NUMBER() over(order by B.EDU_SEQ) as NUM , B.EDU_LEVEL,B.EDU_INSTITUTION,B.EDU_YEAR,
                                                         B.WF_STATUS,B.ATTACH_ID,t.ATTACH_FILENAME,t.ATTACH_ITEM
                                                 from(
                                                 select  a.EDU_LEVEL,a.EDU_INSTITUTION,a.EDU_YEAR,r.WF_STATUS,a.ATTACH_ID,a.EDU_SEQ,r.DATE_MODIFIED
                                                 from T_REQUEST_HRP r, T_ACADEMIC a,T_DOC_ATTACHMENT d
                                                 where r.EMP_ID=a.EMP_ID and r.REQ_NO=a.REQ_NO and r.REQ_CODE='AC' and r.BR_STATUS < 3 
                                                	 and r.REQ_TYPE='ACA' and a.ATTACH_ID=d.ATTACH_ID and r.EMP_ID  = '{0}') as B 
                                                	 LEFT OUTER JOIN  T_DOC_ATTACHMENT t
                                                 ON B.ATTACH_ID = t.ATTACH_ID order by B.EDU_SEQ,B.DATE_MODIFIED;";
        /// <summary>
        /// get request information from T_EME_CTC_LIST by employeeId
        /// </summary>
        public const string QRY_T_EME_CTC_LIST = @" select e.EME_NAME,e.EME_ADD1,e.EME_ADD2,e.EME_ADD3,e.EME_POSTCODE,
                                                    e.EME_CITY,e.EME_STATE,e.EME_PHONE_HOME,e.EME_HPHONE,e.EME_RELATION
                                                    from T_REQUEST_HRP r,T_EME_CTC_LIST e  where r.EMP_ID=e.EMP_ID and r.BR_STATUS < 3 
                                                    and r.REQ_NO=e.REQ_NO and r.REQ_CODE='EM' and r.REQ_TYPE='EME' and r.EMP_ID = '{0}'";

        #endregion
        #region method
        public static int insT_DOC_ATTACHMENT_PS02(string strFileName, Byte[] imgBytes, string strREQ_TYPE, string strReqID, string strEMP_ID, string strAttachID)
        {
            int result = 0;
            using (SqlConnection connection = new SqlConnection(DBConfig.GetConnectionString()))
            {
                try
                {
                    connection.Open();

                    DataSet tempDataSet = new DataSet();

                    SqlDataAdapter tempAdapter = new SqlDataAdapter("SELECT * FROM T_DOC_ATTACHMENT", connection);

                    SqlCommandBuilder tempBuilder = new SqlCommandBuilder(tempAdapter);

                    tempAdapter.Fill(tempDataSet);
                    DataRow tempDataRow = tempDataSet.Tables[0].NewRow();

                    tempDataRow["ATTACH_ID"] = strAttachID;//TODO
                    tempDataRow["REQ_NO"] = strReqID;
                    tempDataRow["REQ_TYPE"] = strREQ_TYPE;
                    tempDataRow["ATTACH_DESC"] = "3";//TODO
                    tempDataRow["ATTACH_ITEM"] = imgBytes;
                    tempDataRow["ATTACH_FILENAME"] = strFileName;
                    tempDataRow["USER_CREATED"] = strEMP_ID;
                    tempDataRow["DATE_CREATED"] = System.DateTime.Now;
                    tempDataRow["USER_UPDATED"] = strEMP_ID;
                    tempDataRow["DATE_MODIFIED"] = System.DateTime.Now;

                    tempDataSet.Tables[0].Rows.Add(tempDataRow);

                    tempAdapter.Update(tempDataSet);

                }
                catch (Exception e)
                {
                    result = -1;
                    //Rollback();
                    Console.Write(e.Message.ToString());
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                        connection.Dispose();
                    }
                }
            }
            return result;
        }

        #endregion
        #endregion

        #region PS03
        #region SQL
        /// <summary>
        /// get RELATION information from RESOURCESMAINT
        /// </summary>
        public const string QRY_M_RELATION = " select RESOURCE_ID as REL_CODE, DESC_BM as REL_DESC_BM,DESC_EN as REL_DESC_EN from RESOURCESMAINT where CATEGORY='RELIGION' and IS_ACTIVE='1'";
        /// <summary>
        /// get RELATIONSHIP information from RESOURCESMAINT
        /// </summary>
        public const string QRY_M_RELATIONSHIP = " select RESOURCE_ID as REL_CODE, DESC_BM as REL_DESC_BM,DESC_EN as REL_DESC_EN from RESOURCESMAINT where CATEGORY='RELATIONSP' and IS_ACTIVE='1'";
        /// <summary>
        /// get STATE information from RESOURCESMAINT
        /// </summary>
        public const string QRY_M_STATE = " select RESOURCE_ID as STATE_CODE,DESC_BM as STATE_DESC_BM, DESC_EN as STATE_DESC_EN from RESOURCESMAINT where CATEGORY='STATE_CODE' and IS_ACTIVE='1' ";
        /// <summary>
        ///  get CITY information from RESOURCESMAINT
        /// </summary>
        public const string QRY_M_CITY = " select RESOURCE_ID as CITY_CODE,DESC_BM as CITY_DESC_BM ,DESC_EN as CITY_DESC_EN from RESOURCESMAINT where CATEGORY='CITY_CODE' and IS_ACTIVE='1' ";
        /// <summary>
        /// get EDU information from RESOURCESMAINT
        /// </summary>
        public const string QRY_M_EDU_LVL = " select RESOURCE_ID as EDU_CODE,DESC_BM as EDU_DESC_BM,DESC_EN as EDU_DESC_EN from RESOURCESMAINT where CATEGORY='EDUCATE_LV' and IS_ACTIVE='1'";      
          /// <summary>
        /// get MARITAL_ST information from RESOURCESMAINT
        /// </summary>
        public const string QRY_M_MARITAL_ST = "select RESOURCE_ID as MAR_CODE,DESC_BM as MAR_DESC_BM,DESC_EN as MAR_DESC_EN from RESOURCESMAINT where CATEGORY='MARITAL_ST' and IS_ACTIVE='1'";
       
        /// <summary>
        /// get info from M_REQ_TYPE
         /// </summary>
        public const string QRY_M_REQ_TYPE = "select REQ_TYPE,REQ_DESC_BM,REQ_DESC_EN from M_REQ_TYPE";
        /// <summary>
        /// Insert data in T_REQUEST_HRP
        /// </summary>
        public const string INS_T_REQUEST_HRP = @" declare @OutputTbl table (ID INT,REQ_ID char(18))
                                                   declare @GRP_REQNO int
                                                   declare @REQ_NO as char(18)
                                            insert into T_REQUEST_HRP (REQ_NO,EMP_ID,BR_STATUS,WF_STATUS,REQ_CODE,REQ_TYPE,REQ_DESC,EMP_NAME,COST_CENTER,
                                                                      POSITION,JOINED_DT,KEY_VALUE,USER_CREATED,DATE_CREATED,USER_MODIFIED,DATE_MODIFIED, WF_PROC_ID,GENDER,RELIGION)
                                             output INSERTED.GRP_REQNO,INSERTED.REQ_NO INTO @OutputTbl(ID,REQ_ID)
                                             values('{0}','{1}','{2}','{3}','{4}', '{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}',GETDATE(),'{13}',GETDATE(),'{14}','{15}','{16}')
                                                SELECT @GRP_REQNO = ID FROM @OutputTbl
                                                SELECT @REQ_NO = REQ_ID FROM @OutputTbl";
        /// <summary>
        /// update T_REQUEST_HRP
        /// </summary>
        public const string UPD_T_REQUEST_HRP = @"update T_REQUEST_HRP set WF_STATUS='{0}', USER_MODIFIED='{1}', DATE_MODIFIED=GETDATE()  where REQ_NO='{2};'";
        /// <summary>
        /// Insert data in T_WD_NO_WF
        /// </summary>
        public const string INS_T_WD_NO_WF = @" insert into T_WD_NO_WF(EMP_ID,GRP_REQNO,FLD_TYPE,VALUE,ATTACH_ID,USER_CREATED,DATE_CREATED,USER_MODIFIED,DATE_MODIFIED,REQ_NO) 
                                              values('{0}',@GRP_REQNO,'{1}','{2}','{3}','{4}',GETDATE(),'{5}',GETDATE(),'{6}');";
       /// <summary>
       /// 
       /// </summary>
        public const string UPD_T_WD_NO_WF_A = @"update T_WD_NO_WF set VALUE='{0}', ATTACH_ID='{1}', USER_MODIFIED='{2}', DATE_MODIFIED=GETDATE() where REQ_NO='{3}'";
       /// <summary>
       /// 
       /// </summary>
        public const string UPD_T_WD_NO_WF = @"update T_WD_NO_WF set VALUE='{0}', USER_MODIFIED='{1}', DATE_MODIFIED=GETDATE() where REQ_NO='{2}'";
        /// <summary>
        /// Insert data in T_WD_WF
        /// </summary>
        public const string INS_T_WD_WF = @"insert into T_WD_WF(EMP_ID,REQ_NO,PASSPORT_NO,PASSPORT_EXP,PASSPORT_ISSUE_DT,PASSPORT_ISSUE_PL,PERMIT_NO,PERMIT_EXP,
                                             ATTACH_ID,USER_CREATED,DATE_CREATED,USER_MODIFIED,DATE_MODIFIED) 
                                            values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}',GETDATE(),'{5}',GETDATE());;";
        /// <summary>
        /// Insert data in T_PERSONAL
        /// </summary>
        public const string INS_T_PERSONAL = @" insert into T_PERSONAL( EMP_ID,REQ_NO,GRP_REQNO,CUR_ADD1,CUR_ADD2,CUR_ADD3,CUR_POSTCODE,CUR_CITY,CUR_STATE,PER_ADD1,PER_ADD2,PER_ADD3,
                                                     PER_POSTCODE,PER_CITY,PER_STATE,PHONE_HOME,PHONE_HP,RELIGION,BUS_ROUTE,USER_CREATED,DATE_CREATED,USER_MODIFIED,DATE_MODIFIED)
                                                values('{0}',@REQ_NO,@GRP_REQNO,'{1}','{2}','{3}','{4}', '{5}','{6}','{7}','{8}','{9}','{10}','{11}',
                                                      '{12}','{13}','{14}','{15}','{16}','{17}',GETDATE(),'{18}',GETDATE())";
       /// <summary>
        /// update T_PERSONAL
       /// </summary>
        public const string UPD_T_PERSONAL = @"update T_PERSONAL 
                                              set CUR_ADD1 = '{0}',CUR_ADD2='{1}',CUR_ADD3='{2}',CUR_POSTCODE='{3}',CUR_CITY='{4}',CUR_STATE='{5}',
                                                  PER_ADD1='{6}',PER_ADD2='{7}',PER_ADD3='{8}',PER_POSTCODE='{9}',PER_CITY='{10}',PER_STATE='{11}',PHONE_HOME='{12}',PHONE_HP='{13}',
                                               	RELIGION='{14}',USER_MODIFIED='{15}',DATE_MODIFIED=GETDATE()  where REQ_NO='{16}'; ";
        /// <summary>
        /// Insert data in T_SP_LIST
        /// </summary>
        public const string INS_T_SP_LIST = @" insert into T_SP_LIST( EMP_ID,GRP_REQNO,SP_SEQ,MARRIAGE_STS,SP_WORKING_STS,SP_EMPID,SP_NAME,SP_ICNO,SP_DOB,SP_OCCUPATION,
                                                ATTACH_ID,DEL_FLAG,USER_CREATED,DATE_CREATED,USER_MODIFIED,DATE_MODIFIED,REQ_NO)
                                                values('{0}',@GRP_REQNO,'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}',GETDATE(),'{12}',GETDATE(),'{13}');";
        /// <summary>
        /// Insert data in T_SP_DEPENDANT
        /// </summary>
        public const string INS_T_SP_DEPENDANT = @" insert into T_SP_DEPENDANT values('{0}','{1}','{2}','{3}','{4}', '{5}','{6}','{7}','{8}','{9}',
                                                 '{10}','{11}','{12}','{13}','{14}','{15}');";
        /// <summary>
        /// Insert data in T_INS_NOMINEE
        /// </summary>
        public const string INS_T_INS_NOMINEE = @" insert into T_INS_NOMINEE values('{0}','{1}','{2}','{3}','{4}', '{5}','{6}','{7}','{8}','{9}',
                                                 '{10}','{11}','{12}');";
        /// <summary>
        /// Insert data in T_ACADEMIC
        /// </summary>
        public const string INS_T_ACADEMIC = @" insert into T_ACADEMIC values('{0}','{1}','{2}',{3},'{4}', '{5}','{6}','{7}','{8}','{9}',
                                                 '{10}','{11}','{12}','{13}');";
        /// <summary>
        /// Insert data in T_EME_CTC_LIST
        /// </summary>
        public const string INS_T_EME_CTC_LIST = @" insert into T_EME_CTC_LIST values('{0}','{1}','{2}','{3}','{4}', '{5}','{6}','{7}','{8}','{9}',
                                                 '{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}');";
       /// <summary>
        /// Retrieve WorkFlow Configuration – passing parameter $ReqType
       /// </summary>
        public const string QRY_CONFIG_WF = " select G.*,H.* from M_WF_MAP G,M_WF_INFO H  where G.BUSS_PROC='{0}'  and  G.WF_ID=H.WF_ID  and  H.WF_LEVEL='1';";
        /// <summary>
        /// insert T_DOC_ATTCHMENT table
        /// </summary>
        public const string INS_T_DOC_ATTCHMENT = @"insert into T_DOC_ATTACHMENT(ATTACH_ID,REQ_NO,REQ_TYPE,ATTACH_DESC,ATTACH_ITEM,ATTACH_FILENAME,USER_CREATED,DATE_CREATED,USER_MODIFIED,DATE_MODIFIED)
                                                    values('{0}',@REQ_NO,'{1}','{2}','{3}','{4}','{5}',GETDATE(),'{6}',GETDATE());";
       /// <summary>
        /// update T_DOC_ATTCHMENT table
       /// </summary>
        public const string UPD_T_DOC_ATTCHMENT = @"update T_DOC_ATTACHMENT set ATTACH_ITEM='{0}', ATTACH_FILENAME='{1}', USER_MODIFIED='{2}', DATE_MODIFIED=GETDATE()  where REQ_NO='{3}'";
       
        /// <summary>
        ///  Insert WorkFlow Transaction – workflow status = Submitted record
        /// </summary>
        public const string INS_T_WF_TRANSACTION_S = @"insert into T_WF_TRANSACTION (WF_TXN_ID,REQ_NO,MODULE,WF_ID,WF_CUR_LEVEL,WF_CUR_PIC,WF_CUR_PIC_NAME,WF_TXN_STATUS,WF_STAGENAME,
							                        	USER_CREATED,NAME_CREATED,DATE_CREATED,USER_MODIFIED,DATE_MODIFIED,POSITIVE_LBL)
                                                       values('{0}',@REQ_NO,'HRP','{1}',0,'{2}','{3}','4','NEW','{4}','{5}',GETDATE(),'{6}',GETDATE(),'');";
       /// <summary>
        /// Insert WorkFlow Transaction – workflow status = New record
       /// </summary>
        public const string INS_T_WF_TRANSACTION_N = @"insert into T_WF_TRANSACTION (WF_TXN_ID,REQ_NO,MODULE,WF_ID,WF_CUR_LEVEL,WF_CUR_PIC,WF_CUR_PIC_NAME,WF_TXN_STATUS,WF_STAGENAME,
							                        	USER_CREATED,NAME_CREATED,DATE_CREATED,USER_MODIFIED,DATE_MODIFIED,POSITIVE_LBL)
                                                        values('{0}',@REQ_NO,'HRP','{1}','{2}','{3}','{4}','0','{5}','{6}','{7}',GETDATE(),'{8}',GETDATE(),'{9}');";
        /// <summary>
        /// update T_WF_TRANSACTION
        /// </summary>
        public const string UPD_T_WF_TRANSACTION = @"update T_WF_TRANSACTION set WF_TXN_STATUS='{0}', USER_MODIFIED='{1}', DATE_MODIFIED=GETDATE() where REQ_NO='{2}' and WF_CUR_LEVEL=1;";
        #endregion
        #region method
       /// <summary>
       /// 
       /// </summary>
       /// <param name="attachId"></param>
       /// <param name="wf_txn_id"></param>
       /// <param name="reqType"></param>
       /// <param name="reqDesc"></param>
       /// <param name="attacInfo"></param>
       /// <param name="strEMP_ID"></param>
       /// <param name="strEMP_NAME"></param>
       /// <returns></returns>
        public static string PS03_GetInsertAttWFSqlStr(string attachId, string wf_txn_id,string reqType, string reqDesc,
                              FileUploadInfo attacInfo, string strEMP_ID, string strEMP_NAME)
        {
            string attach_WF_sqlStr = string.Empty;
            //SQL4.1b
            // INS_T_DOC_ATTCHMENT
            if (attacInfo != null)
            {
                string attachSql = string.Format(DBManager.INS_T_DOC_ATTCHMENT, attachId, reqType, reqDesc, attacInfo.fileData, attacInfo.strFileName,
                                strEMP_ID, strEMP_ID);
                attach_WF_sqlStr += attachSql;
            }
            //SQL2  SQL 4.1d SQL 4.1e
            attach_WF_sqlStr += PS03_GetInsertWFSqlStr(wf_txn_id, reqType, strEMP_ID, strEMP_NAME);
           
            return attach_WF_sqlStr;

        }

        public static string PS03_GetInsertWFSqlStr( string wf_txn_id, string reqType, string strEMP_ID, string strEMP_NAME)
        {
            string WF_sqlStr = string.Empty;
            //SQL2 
            string queryWFStr = string.Format(DBManager.QRY_CONFIG_WF, reqType);
            DataTable H1 = DBManager.getInfoBySqlText(queryWFStr);
            if (H1 != null && H1.Rows.Count > 0)
            {//TODO not record
                //SQL 4.1d
                string insWFS = string.Format(DBManager.INS_T_WF_TRANSACTION_S, wf_txn_id, H1.Rows[0]["WF_ID"], strEMP_ID, strEMP_NAME, strEMP_ID, strEMP_NAME, strEMP_ID);
                //SQL 4.1e
                string insWFN = string.Format(DBManager.INS_T_WF_TRANSACTION_S, wf_txn_id, H1.Rows[0]["WF_ID"], H1.Rows[0]["WF_LEVEL"], H1.Rows[0]["STAGE_APPROVAL"],
                                 H1.Rows[0]["STAGE_APPROVAL_NAME"], H1.Rows[0]["STAGE_NAME"], strEMP_ID, strEMP_NAME, strEMP_ID, H1.Rows[0]["POSITIVE_LBL"]);

                WF_sqlStr += insWFS;
                WF_sqlStr += insWFN;
            }
            return WF_sqlStr;

        }
        #endregion
        #endregion

        #region MS05
        /// <summary>
        /// department column name from SAP
        /// </summary>
        public const string DEPARTMENT_SAP = "ORGEH";
        /// <summary>
        /// cost center column name from SAP
        /// </summary>
        public const string COSTCENTER_SAP = "KOSTL";
        /// <summary>
        /// employee id column name from SAP
        /// </summary>
        public const string EMPID_SAP = "PERNR";
        /// <summary>
        /// employee name column name from SAP 
        /// </summary>
        public const string EMPNAME_SAP = "SNAME";
        /// <summary>
        /// MODE from SAP
        /// </summary>
        public const string MODE_SAP = "MODE";
        ///// <summary>
        ///// get request information from  M_DEPARTMENT
        ///// </summary>
        //public const string QRY_M_DEPARTMENT = " select ID_COSTCTR,ID_DEPART,COST_CENTER_NAME, DEPARTMENT_NAME  from M_COST_CENTER";

        /// <summary>
        /// get request information from  M_COSTCENTER 
        /// </summary>
        public const string QRY_M_COSTCENTER = " select ID_COSTCTR,ID_DEPART,COST_CENTER_NAME, DEPARTMENT_NAME  from M_COST_CENTER";
        /// <summary>
        /// get request information from T_Employee 
        /// </summary>
        public const string QRY_T_Employee = " select EmpID,Department,EmpName,CostCenter from T_Employee where 1=1 {0}";

        #endregion

        #region MS06
        /// <summary>
        /// get request information from T_WF_TRANSACTION
        /// </summary>
        public const string MS06_QRY_T_WF_TRANSACTION = " select REQ_NO, MODULE, WF_ID,WF_CUR_PIC_NAME,WF_TXN_STATUS, WF_CUR_LEVEL, WF_STAGENAME, CURR_COMMENT, PREV_COMMENT, POSITIVE_LBL from T_WF_TRANSACTION where REQ_NO = '{0}' and WF_CUR_PIC='{1}' order by WF_CUR_LEVEL desc ;";
        /// <summary>
        /// Update transaction status 
        /// </summary>
        public const string MS06_UPD_T_WF_TRANSACTION = " update T_WF_TRANSACTION set WF_TXN_STATUS = '{0}',USER_MODIFIED = '{1}',DATE_MODIFIED = GETDATE() where REQ_NO= '{2}' and WF_CUR_LEVEL = {3} ;";
        /// <summary>
        /// Insert transaction status 
        /// </summary>
        public const string MS06_INS_T_WF_TRANSACTION = @" insert into T_WF_TRANSACTION (WF_TXN_ID,REQ_NO,MODULE,WF_ID,WF_CUR_LEVEL,WF_CUR_PIC,WF_CUR_PIC_NAME,WF_TXN_STATUS,WF_STAGENAME,
                                                            CURR_COMMENT,PREV_COMMENT,POSITIVE_LBL,USER_CREATED,DATE_CREATED,NAME_CREATED,USER_MODIFIED,DATE_MODIFIED) 
                                                            values('{0}','{1}','{2}','{3}',{4},'{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}',GETDATE(),'{13}','{14}',GETDATE()) ;";
        /// <summary>
        ///  get STAGE_NAME, EMAIL_NOTICE_FMT from M_WF_INFO
        /// </summary>
        public const string MS06_QRY_M_WF_INFO = " select STAGE_NAME, EMAIL_NOTICE_FMT from M_WF_INFO where REQ_NO = '{0}' and WF_CUR_LEVEL = {1} ;";
        /// <summary>
        ///  get request information from M_EMAIL_FMT
        /// </summary>
        public const string MS06_QRY_M_EMAIL_FMT = " select EMAIL_FMT,EMAIL_DESC,EMAIL_MODULE,SUBJECT,CONTENT,WF_LEVEL,WF_ID from M_EMAIL_FMT where EMAIL_FMT = '{0}'";

        #endregion

        #region CS05
        /// <summary>
        /// get request information from T_REQUEST_HRP
        /// </summary>
        public const string CS05_QRY_HRP_TASKLIST = @" select ROW_NUMBER() over(order by A.REQ_NO) as NUM,A.REQ_NO, A.REQ_DESC, A.EMP_ID, A.EMP_NAME, A.WF_STATUS, A.DATE_CREATED  
                                                        from T_REQUEST_HRP A, T_WF_TRANSACTION C where A.REQ_NO=C.REQ_NO {0}";
        /// <summary>
        /// get request information from T_REQUEST_HRL
        /// </summary>
        public const string CS05_QRY_HRL_TASKLIST = @" select ROW_NUMBER() over(order by A.REQ_NO) as NUM,A.REQ_NO, A.REQ_DESC, A.EMP_ID, A.EMP_NAME,A.WF_STATUS, A.DATE_CREATED  
                                                        from T_REQUEST_HRL A, T_WF_TRANSACTION C where A.REQ_NO=C.REQ_NO {0}";
        /// <summary>
        ///  get request information from M_REQ_TYPE
        /// </summary>
        public const string CS05_QRY_M_REQ_TYPE = @"select REQ_TYPE,REQ_DESC_BM,REQ_DESC_EN  from M_REQ_TYPE ";
        #endregion

        #region CS06

        /// <summary>
        /// get MAX LEVEL workflow  information from T_WF_TRANSACTION
        /// </summary>
        public const string CS06_QRY_T_WF_TRANSACTION_MAX_WF_CUR_LEVEL = @"select 
                                                                        max(WF_CUR_LEVEL) as WF_CUR_LEVEL 
                                                                        from T_WF_TRANSACTION 
                                                                        where REQ_NO='{0}' group by REQ_NO";
        
        /// <summary>
        /// get workflow  information from T_WF_TRANSACTION by level
        /// </summary>
        public const string CS06_QRY_T_WF_TRANSACTION_BY_LEVEL = @"select 
                                                                MODULE,
                                                                WF_STAGENAME,
                                                                CURR_COMMENT,
                                                                PREV_COMMENT,
                                                                WF_CUR_PIC,
                                                                WF_CUR_PIC_NAME,
                                                                WF_TXN_ID,
                                                                POSITIVE_LBL,
                                                                WF_ID 
                                                                from T_WF_TRANSACTION where WF_CUR_LEVEL ='{0}' and  REQ_NO ='{1}'";
        
        /// <summary>
        /// get workflow  information from T_WF_TRANSACTION by status=0
        /// </summary>
        public const string CS06_QRY_T_WF_TRANSACTION_BY_STATUS0 = @"select 
                                                                    MODULE,
                                                                    WF_STAGENAME,
                                                                    CURR_COMMENT,
                                                                    PREV_COMMENT,
                                                                    WF_CUR_PIC,
                                                                    WF_CUR_PIC_NAME,
                                                                    WF_TXN_ID,
                                                                    POSITIVE_LBL,
                                                                    WF_ID 
                                                                    from T_WF_TRANSACTION where WF_TXN_STATUS ='0' and  REQ_NO ='{0}'";

        /// <summary>
        /// get workflow history information from T_WF_TRANSACTION
        /// </summary>
        public const string CS06_QRY_T_WF_TRANSACTION_HISTORY = @"SELECT   
                                                                WF_CUR_LEVEL,
                                                                WF_STAGENAME, 
                                                                WF_CUR_PIC, 
                                                                WF_CUR_PIC_NAME, 
                                                                WF_TXN_STATUS, 
                                                                DATE_CREATED
                                                                FROM T_WF_TRANSACTION where REQ_NO='{0}'";

        /// <summary>
        /// insert into T_WF_TRANSACTION information
        /// </summary>
        public const string CS06_INS_T_WF_TRANSACTION = @"insert into T_WF_TRANSACTION values(
                                                        '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}',
                                                        '{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}')";

        /// <summary>
        /// update T_WF_TRANSACTION information by WF_TXN_ID
        /// </summary>
        public const string CS06_UPD_T_WF_TRANSACTION = @"update T_WF_TRANSACTION set  
                                                        WF_TXN_STATUS='{0}',
                                                        CURR_COMMENT='{1}',
                                                        SLA_COMPLETED='{5}',
                                                        USER_MODIFIED='{2}',
                                                        DATE_MODIFIED='{3}'
                                                        where WF_TXN_ID ='{4}'";

        /// <summary>
        /// update T_WF_TRANSACTION information by F_TXN_STATUS='0'
        /// </summary>
        public const string CS06_UPD_T_WF_TRANSACTION_STATUS = @"update T_WF_TRANSACTION set  
                                                                WF_CUR_PIC='{0}', 
                                                                WF_CUR_PIC_NAME='{1}', 
                                                                WF_TXN_STATUS='1',
                                                                SLA_PICKUP_DT='{2}',
                                                                USER_MODIFIED='{4}',
                                                                DATE_MODIFIED='{5}'
                                                                where REQ_NO ='{3}' and  WF_TXN_STATUS='0'";

        
        /// <summary>
        /// update T_REQUEST_HRP STATUS
        /// </summary>
        public const string CS06_UPD_T_REQUEST_STATUS = @"update {5} set  
                                                                WF_STATUS='{0}', 
                                                                BR_STATUS='{1}',
                                                                USER_MODIFIED='{3}',
                                                                DATE_MODIFIED='{4}'
                                                                where REQ_NO ='{2}'";

        /// <summary>
        /// get request information from T_REQUEST_HRP
        /// </summary>
        public const string CS06_QRY_REQUEST_INFORMATION_BY_MODULE = @"select 
                                                        REQ_DESC, 
                                                        REQ_TYPE,
                                                        KEY_VALUE,
                                                        DATE_CREATED,
                                                        EMP_ID,
                                                        EMP_NAME,
                                                        COST_CENTER,
                                                        WF_STATUS
                                                        from {0} 
                                                        where  REQ_NO ='{1}'";

        /// <summary>
        /// get attachment information from T_DOC_ATTACHMENT
        /// </summary>
        public const string CS06_QRY_T_DOC_ATTACHMENT_IN_REQUEST = @"select 
                                                                    ATTACH_ITEM,
                                                                    ATTACH_FILENAME 
                                                                    from T_DOC_ATTACHMENT where ATTACH_ID = 
                                                                    (select ATTACH_ID from {0} where REQ_NO ='{1}')";

        /// <summary>
        /// get attachment information from T_DOC_ATTACHMENT
        /// </summary>
        public const string CS06_QRY_T_DOC_ATTACHMENT_BASE_OTHER = @"select 
                                                                    ATTACH_ITEM,
                                                                    ATTACH_FILENAME 
                                                                    from T_DOC_ATTACHMENT where REQ_NO ='{0}' AND 
                                                                    ATTACH_ID not in
                                                                    (select ATTACH_ID from T_LEAVE where REQ_NO='{0}')";


        /// <summary>
        /// get next workflow level information
        /// </summary>
        public const string CS06_QRY_M_WF_INFO = @"select * from M_WF_INFO where WF_LEVEL ='{0}' and WF_ID ='{1}'";

        /// <summary>
        /// insert content to T_DOC_ATTACHMENT
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public static int COMMON_INS_T_DOC_ATTACHMENT(string strAttachID, string strFileName, Byte[] imgBytes, string strREQ_TYPE, string strReqID, string strEMP_ID, string strKEY_VALUE)
        {
            int result = 0;
            using (SqlConnection connection = new SqlConnection(DBConfig.GetConnectionString()))
            {
                try
                {
                    connection.Open();

                    DataSet tempDataSet = new DataSet();

                    SqlDataAdapter tempAdapter = new SqlDataAdapter("SELECT * FROM T_DOC_ATTACHMENT", connection);

                    SqlCommandBuilder tempBuilder = new SqlCommandBuilder(tempAdapter);

                    tempAdapter.Fill(tempDataSet);
                    DataRow tempDataRow = tempDataSet.Tables[0].NewRow();

                    tempDataRow["ATTACH_ID"] = strAttachID;
                    tempDataRow["REQ_NO"] = strReqID;
                    tempDataRow["REQ_TYPE"] = strREQ_TYPE;
                    tempDataRow["ATTACH_DESC"] = strKEY_VALUE;
                    tempDataRow["ATTACH_ITEM"] = imgBytes;
                    tempDataRow["ATTACH_FILENAME"] = strFileName;
                    tempDataRow["USER_CREATED"] = strEMP_ID;
                    tempDataRow["DATE_CREATED"] = System.DateTime.Now;
                    tempDataRow["USER_MODIFIED"] = strEMP_ID;
                    tempDataRow["DATE_MODIFIED"] = System.DateTime.Now;

                    tempDataSet.Tables[0].Rows.Add(tempDataRow);

                    tempAdapter.Update(tempDataSet);

                }
                catch (Exception e)
                {
                    result = -1;
                    //Rollback();
                    Console.Write(e.Message.ToString());
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                        connection.Dispose();
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// insert content to T_DOC_ATTACHMENT
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public static int CS06_UPD_T_DOC_ATTACHMENT(string strFileName, Byte[] imgBytes, string strREQ_TYPE, string strReqID, string strEMP_ID)
        {
            int result = 0;
            using (SqlConnection connection = new SqlConnection(DBConfig.GetConnectionString()))
            {
                try
                {
                    connection.Open();

                    DataSet tempDataSet = new DataSet();

                    string strSQL = "SELECT * FROM T_DOC_ATTACHMENT where REQ_NO='" + strReqID + "' and REQ_TYPE='" + strREQ_TYPE + "'";
                    SqlDataAdapter tempAdapter = new SqlDataAdapter(strSQL, connection);

                    SqlCommandBuilder tempBuilder = new SqlCommandBuilder(tempAdapter);

                    tempAdapter.Fill(tempDataSet);

                    if (tempDataSet.Tables[0].Rows.Count > 0)
                    {
                        tempDataSet.Tables[0].Rows[0]["ATTACH_ITEM"] = imgBytes;
                        tempDataSet.Tables[0].Rows[0]["ATTACH_FILENAME"] = strFileName;
                        tempDataSet.Tables[0].Rows[0]["USER_MODIFIED"] = strEMP_ID;
                        tempDataSet.Tables[0].Rows[0]["DATE_MODIFIED"] = System.DateTime.Now;
                    }

                    tempAdapter.Update(tempDataSet);
                }
                catch (Exception e)
                {
                    result = -1;
                    //Rollback();
                    Console.Write(e.Message.ToString());
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                        connection.Dispose();
                    }
                }
            }
            return result;
        }
        #endregion

        #region MS12

        /// <summary>
        /// get request information from T_REQUEST_HRP
        /// </summary>
        public const string MS12_QRY_T_REQUEST = @"select 
                                                A.REQ_NO,
                                                WF.WF_CUR_LEVEL,
                                                A.REQ_DESC,
                                                A.REQ_TYPE,
                                                A.EMP_ID,
                                                A.EMP_NAME 
                                                from
                                                (select REQ_NO,REQ_DESC,REQ_TYPE,EMP_ID,EMP_NAME from T_REQUEST_HRP where BR_STATUS ='2'
                                                union 
                                                select REQ_NO,REQ_DESC,REQ_TYPE,EMP_ID,EMP_NAME from T_REQUEST_HRL where BR_STATUS ='2'
                                                ) A 
                                                left join  
                                                (select REQ_NO,max(WF_CUR_LEVEL)as WF_CUR_LEVEL from T_WF_TRANSACTION group by REQ_NO) WF
                                                on 
                                                A.REQ_NO =WF.REQ_NO ";
                
        #endregion

        #region MS09

        /// <summary>
        /// get request information from T_REQUEST_HRP
        /// </summary>
        public const string MS09_QRY_REQUEST_HRP_ATTCHMENT = @"select                                                      
                                                        EMP_ID, 
                                                        REQ_NO,
                                                        GRP_REQNO,
                                                        EMP_NAME, 
                                                        REQ_DESC,
                                                        REQ_TYPE,
                                                        DATE_CREATED, 
                                                        COST_CENTER,
                                                        KEY_VALUE from  T_REQUEST_HRP  where REQ_NO in ({0})";

        /// <summary>
        /// get request information from T_REQUEST_HRL
        /// </summary>
        public const string MS09_QRY_REQUEST_HRL_ATTCHMENT = @"select                                                      
                                                        EMP_ID, 
                                                        REQ_NO,
                                                        null as GRP_REQNO,
                                                        EMP_NAME, 
                                                        REQ_DESC,
                                                        REQ_TYPE,
                                                        DATE_CREATED, 
                                                        COST_CENTER,
                                                        KEY_VALUE from  T_REQUEST_HRL  where REQ_NO in ({0})";

        /// <summary>
        /// get request information from T_REQUEST_HRP
        /// </summary>
        public const string MS09_QRY_HRP_REQUEST_RELATION_TABLE = @"select  
                                                                    REQ_NO
                                                                    from  T_WD_NO_WF where  ATTACH_ID is null 
                                                                    union
                                                                    select  
                                                                    REQ_NO
                                                                    from  T_SP_LIST where  ATTACH_ID is null 
                                                                    union
                                                                    select  
                                                                    REQ_NO
                                                                    from  T_SP_DEPENDANT where  ATTACH_ID is null 
                                                                    union
                                                                    select  
                                                                    REQ_NO
                                                                    from  T_ACADEMIC where  ATTACH_ID is null ";

        /// <summary>
        /// get request information from T_REQUEST_HRP
        /// </summary>
        public const string MS09_QRY_HRL_REQUEST_RELATION_TABLE = @"select  
                                                                    REQ_NO
                                                                    from  T_LEAVE where  ATTACH_ID is null";

        /// <summary>
        /// 
        /// </summary>
        public const string MS09_UPD_T_REQUEST_RELEVANCE_TABLES = @"UPDATE {0} set 
                                                        ATTACH_ID ='{6}',
                                                        USER_MODIFIED = '{1}',
                                                        DATE_MODIFIED ='{2}'
                                                        where 
                                                        GRP_REQNO ='{4}'
                                                        and EMP_ID = '{5}'
                                                        and REQ_NO ='{3}'
";



        #endregion

        #region MS11

        /// <summary>
        /// 
        /// </summary>
        public const string MS11_QRY_M_BATCH_SCH = @"SELECT BATCH_CODE,BATCH_DESC FROM M_BATCH_SCH";

        /// <summary>
        /// 
        /// </summary>
        public const string MS11_QRY_BATCH_LASTRUN = @"SELECT max(TIMERUN) as TIMERUN
                                                    FROM  T_BATCH_LOG 
                                                    WHERE BATCH_CODE = '{0}' group by BATCH_CODE";

        #endregion

        #region DB
        /// <summary>
        /// Get information by sql text
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns>The result of rows affected</returns>
        public static DataTable getInfoBySqlText(string sqlStr)
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(DBConfig.GetConnectionString()))
            {
                try
                {
                    connection.Open();
                    DataSet ds = new DataSet();
                    SqlCommand cmd = new SqlCommand(sqlStr, connection);
                    SqlDataAdapter dp = new SqlDataAdapter(cmd);
                    //Commit();
                    dp.Fill(ds);
                    dt = ds.Tables[0];
                }
                catch (Exception e)
                {
                    //Rollback();
                    Console.Write(e.Message.ToString());
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                        connection.Dispose();
                    }
                }
            }
            return dt;
        }

        /// <summary>
        /// update content by sql text
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public static int updateDataBySqlText(string sqlStr)
        {
            int result = 0;
            using (SqlConnection connection = new SqlConnection(DBConfig.GetConnectionString()))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(sqlStr, connection);
                    result = cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    result = -1;
                    //Rollback();
                    Console.Write(e.Message.ToString());
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                        connection.Dispose();
                    }
                }
            }
            return result;
        }

        public static string getREQNO(string flgStr)
        {
            string reqNO = string.Empty;
            string sqlStr = String.Format(@"DECLARE @new_id VARCHAR(20) 
                                              EXECUTE hrm_get_new_id '{0}', @new_id OUTPUT
                                               SELECT @new_id
                                              GO ", flgStr);
            using (SqlConnection connection = new SqlConnection(DBConfig.GetConnectionString()))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(sqlStr, connection);
                  var reqNoStr=  cmd.ExecuteScalar();
                  reqNO = reqNoStr.ToString();
                }
                catch (Exception e)
                {
                    //Rollback();
                    Console.Write(e.Message.ToString());
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Close();
                        connection.Dispose();
                    }
                }
            }
            return reqNO;
        }
        #endregion
    }
}