using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.IO;

namespace CoreProcess
{
    public class DBHelper
    {

        /// <summary>
        /// PERNR from SAP
        /// </summary>
        public const string PERNR_SAP = "PERNR";

        /// <summary>
        /// MODE from SAP
        /// </summary>
        public const string MODE_SAP = "MODE";

        public const string BEGINDATE_SAP = "BEGDA";
        /// <summary>
        /// end date column name from SAP
        /// </summary>
        public const string ENDDATE_SAP = "ENDDA";

        //get connection string
        public static string ConnectionString = ConfigurationManager.AppSettings["HRMSystem"];
        #region SQL strings

        //jba

        public const string GET_ISNOTRUNNING = @"SELECT * FROM M_BATCH_LIST WHERE IS_RUNNING=0";

        public const string UPDATE_ISNOTRUNNING = @"UPDATE M_BATCH_LIST SET IS_RUNNING=1, LAST_PROCESSED=getdate() WHERE BATCH_CODE=";
        public const string UPDATE_ISRUNNING_ORIG = @"UPDATE M_BATCH_LIST SET IS_RUNNING=0 WHERE BATCH_CODE=";

        

        public const string BP01_RETRIEVE_PERSONAL_INFO = @"select
                                    A.EMP_ID,
                                    '0006',
                                    '2',
                                    NULL,
                                    '99991231',
                                    B.CUR_BEGDA as BEGDA,
                                    '0',
                                    B.CUR_ADD1,
                                    B.CUR_ADD2,
                                    B.CUR_CITY,
                                    B.CUR_POSTCODE,
                                    B.CUR_ADD3,
                                    B.CUR_STATE,
                                    B.BUS_ROUTE,
                                    B.PHONE_HOME,
                                    B.PHONE_HP,
                                    B.RELIGION,
                                    B.REQ_NO
                                    from 
                                    T_REQUEST_HRP A, T_PERSONAL B
                                    where 
                                    A.REQ_NO = B.REQ_NO AND
                                    A.BR_STATUS ='2' AND
                                    A.REQ_TYPE = 'PER' 
                                    union
                                    select
                                    A.EMP_ID,
                                    '0006',
                                    '1',
                                    NULL,
                                    '99991231',
                                    B.PER_BEGDA as BEGDA,
                                    '0',
                                    B.PER_ADD1,
                                    B.PER_ADD2,
                                    B.PER_CITY,
                                    B.PER_POSTCODE,
                                    B.PER_ADD3,
                                    B.PER_STATE,
                                    NULL as BUS_ROUTE,
                                    B.PER_PHONEHOME as PHONE_HOME,
                                    B.PER_PHONEHP as PHONE_HP,
                                    B.RELIGION,
                                    B.REQ_NO
                                    from 
                                    T_REQUEST_HRP A, T_PERSONAL B
                                    where 
                                    A.REQ_NO = B.REQ_NO AND
                                    A.BR_STATUS ='2' AND
                                    A.REQ_TYPE = 'PER' AND
                                    B.INCL_PERM = '1'
                                    order by EMP_ID
                                    ";

        public const string BP01_RETRIEVE_WORK_DETAIL_INFO = @"select
                                    T0.EMP_ID,
                                    '0196' as INFTY,
                                    NULL as SUBTY,
                                    NULL as OBJPS,
                                    '99991231' as ENDDA,
                                    CASE WHEN T1.BEGDA is not NULL
                                    then T1.BEGDA
                                    when T2.BEGDA is not NULL
                                    then T2.BEGDA
                                    when T3.BEGDA is not NULL
                                    then T3.BEGDA
                                    when T4.BEGDA is not NULL
                                    then T4.BEGDA
                                    ELSE '' END as BEGDA,
                                    '0' as SEQNR,
                                    T2.VALUE as EPF,
                                    T3.VALUE as SOC,
                                    T4.VALUE as TAX,
                                    T1.IC_NO,
                                    T1.PASSPORT_NO,
                                    T1.PASSPORT_EXP,
                                    T1.PASSPORT_ISSUE_DT,
                                    T1.PASSPORT_ISSUE_PL,
                                    T1.PERMIT_NO,
                                    T1.PERMIT_EXP,
                                    T0.REQ_NO
                                    from 
                                    (select
                                    DISTINCT(EMP_ID),
                                    REQ_NO 
                                    from 
                                    T_REQUEST_HRP
                                    where
                                    BR_STATUS ='2' AND
                                    REQ_TYPE in('PAS','EPF','SOC','TAX','PA1')) T0                                   
                                    left join
                                    (select
                                    A.EMP_ID,
                                    A.IC_NO,
                                    B.GRP_REQNO,
                                    B.PASSPORT_NO,
                                    B.PASSPORT_EXP,
                                    B.PASSPORT_ISSUE_DT,
                                    B.PASSPORT_ISSUE_PL,
                                    B.PERMIT_NO,
                                    B.PERMIT_EXP,
                                    B.BEGDA
                                     from 
                                    T_REQUEST_HRP A,
                                    T_WD_WF B
                                    where 
                                    A.REQ_NO = B.REQ_NO AND
                                    A.BR_STATUS ='2' AND
                                    A.REQ_TYPE in ('PAS','PA1')) T1
                                    on T0.EMP_ID = T1.EMP_ID
                                    left join
                                    (select
                                    A.EMP_ID,
                                    A.IC_NO,
                                    B.GRP_REQNO, 
                                    B.VALUE,
                                    B.BEGDA
                                     from 
                                    T_REQUEST_HRP A,
                                    T_WD_NO_WF B
                                    where 
                                    A.REQ_NO = B.REQ_NO AND
                                    A.BR_STATUS ='2' AND
                                    A.REQ_TYPE = B.FLD_TYPE AND
                                    A.REQ_TYPE = 'EPF') T2
                                    on T0.EMP_ID = T2.EMP_ID
                                    left join
                                    (select
                                    A.EMP_ID,
                                    A.IC_NO,
                                    B.GRP_REQNO,
                                    B.VALUE,
                                    B.BEGDA
                                     from 
                                    T_REQUEST_HRP A,
                                    T_WD_NO_WF B
                                    where 
                                    A.REQ_NO = B.REQ_NO AND
                                    A.BR_STATUS ='2' AND
                                    A.REQ_TYPE = B.FLD_TYPE AND
                                    A.REQ_TYPE = 'SOC') T3
                                    on T0.EMP_ID = T3.EMP_ID
                                    left join
                                     (select
                                    A.EMP_ID,
                                    A.IC_NO,
                                    B.GRP_REQNO,
                                    B.VALUE,
                                    B.BEGDA
                                     from 
                                    T_REQUEST_HRP A,
                                    T_WD_NO_WF B
                                    where 
                                    A.REQ_NO = B.REQ_NO AND
                                    A.BR_STATUS ='2' AND
                                    A.REQ_TYPE = B.FLD_TYPE AND
                                    A.REQ_TYPE = 'TAX') T4
                                    on T0.EMP_ID = T4.EMP_ID                                   
                                    order by EMP_ID";

        public const string BP01_RETRIEVE_SPOUSE_INFO = @"select
                                    A.EMP_ID,
                                    '0002' as INFTY,
                                    NULL as SUBTY,
                                    NULL as OBJPS,
                                    case when B.ENDDA is null or B.ENDDA = '' 
                                         then '99991231' 
                                    else B.ENDDA end as ENDDA,
                                    B.BEGDA,
                                    '0' as SEQ,
                                    B.MARRIAGE_STS,
                                    NULL as SP_WORKING_STS,
                                    NULL as SP_NAME,
                                    NULL as SP_ICNO,
                                    NULL as SP_DOB,
                                    NULL as SP_OCCUPATION,
                                    NULL as DEP_NAME,
                                    NULL as DEP_ICNO,
                                    NULL as DEP_DOB,
                                    NULL as DEP_ADOPTED,
                                    NULL as DEP_HC,
                                    B.SP_SEX,
                                    NULL as DEL_FLAG,
                                    NULL as DEP_NEWBORN,
                                    NULL as NEWBORN_CLAIM,
                                    NULL as CLAIM,
                                    A.REQ_NO
                                    from 
                                    T_REQUEST_HRP A, T_SP_LIST B
                                    where 
                                    A.REQ_NO = B.REQ_NO AND
                                    A.BR_STATUS ='2' AND
                                    A.REQ_TYPE = 'SPO' AND
                                    B.DEL_FLAG <> 'C'
                                    union
                                    select
                                    A.EMP_ID,
                                    '0021' as INFTY,
                                    '1' as SUBTY,
                                    B.OBJPS as OBJPS,
                                    case when B.ENDDA is null or B.ENDDA = '' 
                                         then '99991231' 
                                    else B.ENDDA end as ENDDA,
                                    B.BEGDA,
                                    B.SEQ as SEQ,
                                    B.MARRIAGE_STS,
                                    case B.SP_WORKING_STS 
                                    when 'C'
                                    then 'X'
                                    else B.SP_WORKING_STS 
                                    end as SP_WORKING_STS,
                                    B.SP_NAME,
                                    B.SP_ICNO,
                                    B.SP_DOB,
                                    B.SP_OCCUPATION,
                                    NULL as DEP_NAME,
                                    NULL as DEP_ICNO,
                                    NULL as DEP_DOB,
                                    NULL as DEP_ADOPTED,
                                    NULL as DEP_HC,
                                    B.SP_SEX,
                                    B.DEL_FLAG,
                                    NULL as DEP_NEWBORN,
                                    NULL as NEWBORN_CLAIM,
                                    NULL as CLAIM,
                                    A.REQ_NO
                                    from 
                                    T_REQUEST_HRP A, T_SP_LIST B
                                    where 
                                    A.REQ_NO = B.REQ_NO AND
                                    A.BR_STATUS ='2' AND
                                    A.REQ_TYPE = 'SPO' AND
                                    B.DEL_FLAG <> 'C'
                                    union
                                    select
                                    A.EMP_ID,
                                    '0021' as INFTY,
                                    '2' as SUBTY,
                                    C.OBJPS as OBJPS,
                                    '99991231' as ENDDA,
                                    C.BEGDA,
                                    C.SEQ as SEQ,
                                    NULL as MARRIAGE_STS,
                                    NULL as SP_WORKING_STS,
                                    NULL as SP_NAME,
                                    NULL as SP_ICNO,
                                    NULL as SP_DOB,
                                    NULL as SP_OCCUPATION,
                                    C.DEP_NAME,
                                    C.DEP_ICNO,
                                    C.DEP_DOB as DEP_DOB,
                                    C.DEP_ADOPTED,
                                    C.DEP_HC,
                                    C.DEP_SEX,
                                    C.DEL_FLAG,
                                    CASE C.DEP_NEWBORN
                                    WHEN '1'
                                    THEN 'X'
                                    ELSE NULL
                                    END as DEP_NEWBORN,
                                    CASE C.DEP_NEWBORN
                                    WHEN '1'
                                    THEN D.VALUE
                                    ELSE NULL
                                    END as NEWBORN_CLAIM,
                                    CASE C.DEP_NEWBORN
                                    WHEN '1'
                                    THEN 'NEWBORN CLAIM'
                                    ELSE NULL
                                    END as CLAIM,
                                    A.REQ_NO
                                    from 
                                    T_REQUEST_HRP A, T_SP_DEPENDANT C,
                                    (select VALUE from T_GLOBAL_VAR where GVAR = 'NEWBORN_CLAIM') D
                                    where 
                                    A.REQ_NO = C.REQ_NO AND
                                    A.BR_STATUS ='2' AND
                                    A.REQ_TYPE = 'DEP' AND 
                                    C.DEL_FLAG <> 'C'
                                    order by EMP_ID";

        public const string BP01_RETRIEVE_INSURANCE_NOMINEE_INFO = @"select
                                    A.EMP_ID,
                                    '0021' as INFTY,
                                    B.NOM_SEQ as SUBTY,
                                    NULL as OBJPS,
                                    '99991231' as ENDDA,
                                    B.BEGDA,
                                    '0' as SEQ,
                                    B.NOM_NAME,
                                    B.NOM_ICNO,
                                    B.NOM_BIRTH_CERT,
                                    B.NOM_RELATION,   
                                    B.NOM_GENDER,                            
                                    B.DEL_FLAG,
                                    A.REQ_NO                                
                                    from 
                                    T_REQUEST_HRP A, T_INS_NOMINEE B
                                    where 
                                    A.REQ_NO = B.REQ_NO AND
                                    A.GRP_REQNO = B.GRP_REQNO AND
                                    A.BR_STATUS ='2' AND
                                    A.REQ_TYPE = 'INS' AND
                                    B.DEL_FLAG <> 'C'";

        public const string BP01_RETRIEVE_ACADEMIC_INFO = @"select
                                    A.EMP_ID,
                                    '0022' as INFTY,
                                    '02' as SUBTY,
                                    NULL as OBJPS,
                                    '99991231' as ENDDA,
                                    case B.EDU_YEAR 
                                    when ''
                                    then '99991231'
                                    else B.EDU_YEAR + '0101' 
                                    end as BEGDA,
                                    '0' as SEQ,
                                    B.EDU_LEVEL,
                                    B.EDU_COUNTRY, 
                                    B.EDU_CERT,
                                    B.EDU_RESULT,
                                    B.EDU_INSTITUTION,
                                    A.REQ_NO                                  
                                    from 
                                    T_REQUEST_HRP A, T_ACADEMIC B
                                    where 
                                    A.REQ_NO = B.REQ_NO AND
                                    A.BR_STATUS ='2' AND
                                    A.REQ_TYPE = 'ACA';";

        public const string BP01_RETRIEVE_EMERGENCY_INFO = @"select
                                    A.EMP_ID,
                                    '0006' as INFTY,
                                    '3' as SUBTY,
                                    NULL as OBJPS,
                                    '99991231' as ENDDA,
                                    B.BEGDA,
                                    B.SEQ,
                                    B.EME_NAME,
                                    B.EME_ADD1,
                                    B.EME_ADD2,
                                    B.EME_CITY,                                    
                                    B.EME_POSTCODE,
                                    B.EME_ADD3,
                                    B.EME_STATE,
                                    B.EME_RELATION,
                                    B.EME_PHONE_HOME,
                                    B.EME_HPHONE,
                                    A.REQ_NO  
                                    from 
                                    T_REQUEST_HRP A, T_EME_CTC_LIST B
                                    where 
                                    A.REQ_NO = B.REQ_NO AND
                                    A.GRP_REQNO = B.GRP_REQNO AND
                                    A.BR_STATUS ='2' AND
                                    A.REQ_TYPE = 'EME' AND
                                    (B.BEGDA is not NULL OR
                                     B.EME_NAME <> '')";

        public const string BP01_GET_CUR_LEVEL_AND_WFID_HRP = @"select 
                                    A.EMP_ID,
                                    A.EMP_NAME,
                                    A.REQ_NO,
                                    A.REQ_TYPE,
                                    A.REQ_DESC,
                                    B.WF_CUR_LEVEL,
                                    B.WF_ID,
                                    B.MODULE
                                    from 
                                    T_REQUEST_HRP A,
                                    (select
                                    C.REQ_NO,
                                    C.WF_ID,
                                    C.MODULE,
                                    max(WF_CUR_LEVEL) as WF_CUR_LEVEL
                                    from
                                    T_WF_TRANSACTION C
                                    group by REQ_NO,WF_ID,MODULE) B
                                    where 
                                    A.REQ_NO=B.REQ_NO AND
                                    A.BR_STATUS ='2' AND
                                    A.REQ_TYPE in('PAS','EPF','SOC','TAX','SPO','DEP','ACA','INS','EME','PER','PA1')
                                    {0} ";

        public const string BP01_GET_CUR_LEVEL_AND_WFID_HRL = @"select
                                    A.EMP_ID,
                                    A.EMP_NAME,
                                    A.REQ_NO,
                                    A.REQ_TYPE,
                                    A.REQ_DESC,
                                    B.WF_CUR_LEVEL,
                                    B.WF_ID,
                                    B.MODULE
                                    from 
                                    T_REQUEST_HRL A,
                                    (select
                                    C.REQ_NO,
                                    C.WF_ID,
                                    C.MODULE,
                                    max(WF_CUR_LEVEL) as WF_CUR_LEVEL
                                    from
                                    T_WF_TRANSACTION C
                                    group by REQ_NO,WF_ID,MODULE) B
                                    where 
                                    A.REQ_NO=B.REQ_NO AND
                                    A.BR_STATUS ='2' AND
                                    A.REQ_TYPE in('L01','L02', 'L03', 'L04', 'L05', 'L06', 'L07', 'L08', 'L09', 'L10', 'L11', 'L12', 'L13', 'L14', 'L15', 'L16', 'L18',
                                                  'L19', 'L20', 'L21', 'L22', 'L23', 'L24', 'L25', 'L26', 'L27', 'L28','L29', 'L30', 'L31', 'L32', 'L33', 'L34', 'L35',
                                                  'L36', 'L37','L38', 'L39', 'L40', 'L41','L42', 'L43', 'L44', 'L45', 'L46', 'L47', 'L48','L49', 'L50', 'L51', 'L52', 'L53', 'L54', 'L55',
                                                                                                           'L56', 'L57')
                                    {0} ;";

        public const string BP01_GET_NEXT_WORKFLOW = @"select 
                                    F.WF_ID,F.WF_LEVEL, F.STAGE_NAME, F.STAGE_APPROVER, F.EMAIL_NOTICE_FMT, F.CC_SETTING, F.POSITIVE_LBL,F.STAGE_APPROVER_NAME 
                                    from 
                                    M_WF_INFO F 
                                    where 
                                    F.WF_ID = '{0}'  and
                                    F.WF_LEVEL = '{1}';";

        public const string BP01_GET_T_WF_TRANSACTION = @"select * from T_WF_TRANSACTION
                                    where WF_CUR_LEVEL='{0}' and REQ_NO='{1}';";

        public const string BP00_GET_BATCHFILEPATH = @"select 
                                    VALUE 
                                    from 
                                    T_GLOBAL_VAR 
                                    where 
                                    GVAR='SAP_UPLOAD_FOLDER'";

        public const string BP01_GET_BATCHFILEPATH = @"select 
                                    VALUE 
                                    from 
                                    T_GLOBAL_VAR 
                                    where 
                                    GVAR='BATCH_OUTPUT_FOLDER'";

        public const string BP01_GET_REASSIGNTIME = @"select 
                                    VALUE 
                                    from 
                                    T_GLOBAL_VAR 
                                    where 
                                    GVAR='reassignTime' and
                                    IS_ACTIVE = '1';";

        public const string BP01_GET_BATCHRUNTIME = @"select 
                                    SCH_TIME,
                                    datepart(hour,[SCH_TIME]) + datepart(hour,[DELAY_TIME]) as HOUR_TIME,
                                    datepart(minute,[SCH_TIME]) + datepart(minute,[DELAY_TIME]) as MIN_TIME,
                                    datepart(second,[SCH_TIME]) + datepart(second,[DELAY_TIME]) as SEC_TIME,
                                    MODE 
                                    from 
                                    M_BATCH_SCH 
                                    where 
                                    BATCH_CODE='BP01'";

        public const string BP01_UPD_T_WF_TRANSACTION = @"update T_WF_TRANSACTION set  
                                    WF_TXN_STATUS ='2',DATE_MODIFIED = GETDATE() 
                                    where WF_STAGENAME='SAP BATCH' and WF_TXN_STATUS<>'5' and WF_TXN_STATUS<>'2' 
                                    and REQ_NO in 
                                    (select REQ_NO from T_REQUEST_HRP where BR_STATUS='2' and REQ_TYPE in ('PAS','EPF','SOC','TAX','SPO','DEP','ACA','INS','EME','PER','PA1')
                                     union 
                                     select REQ_NO from T_REQUEST_HRL where BR_STATUS='2' and REQ_TYPE in ('L01','L02', 'L03', 'L04', 'L05', 'L06', 'L07', 'L08', 'L09', 'L10', 'L11', 'L12', 'L13', 'L14', 'L15', 'L16', 'L18',
                                                                                                           'L19', 'L20', 'L21', 'L22', 'L23', 'L24', 'L25', 'L26', 'L27', 'L28','L29', 'L30', 'L31', 'L32', 'L33', 'L34', 'L35',
                                                                                                           'L36', 'L37','L38', 'L39', 'L40', 'L41', 'L42', 'L43', 'L44', 'L45', 'L46', 'L47', 'L48','L49', 'L50', 'L51', 'L52', 'L53', 'L54', 'L55',
                                                                                                           'L56', 'L57'))
                                    and REQ_NO in {0}";

        public const string BP01_UPD_T_REQUEST_HRP = @"update T_REQUEST_HRP set  
                                   BR_STATUS ='3' ,
                                    BP_ID='{0}'
                                    where BR_STATUS ='2' and REQ_TYPE in {1} and REQ_NO in {2}";
        //manda add DATE_MODIFIED,USER_MODIFIED
        public const string BP01_INS_T_WF_TRANSACTION = @"insert into T_WF_TRANSACTION 
                                    (WF_TXN_STATUS,
                                    WF_TXN_ID,
                                    WF_ID,
                                    WF_CUR_LEVEL,
                                    WF_CUR_PIC,
                                    WF_STAGENAME,
                                    PREV_COMMENT,
                                    USER_CREATED,
                                    DATE_CREATED,
                                    POSITIVE_LBL,
                                    REQ_NO,
                                    MODULE,
                                    NAME_CREATED,
                                    WF_CUR_PIC_NAME,
                                    DATE_MODIFIED,
                                    USER_MODIFIED)
                                    values(
                                    '0',
                                    '{0}',
                                    '{1}',
                                    '{2}',
                                    '{3}',
                                    '{4}',
                                    '{5}',
                                    '{6}',
                                    GETDATE(),
                                    '{7}',
                                    '{8}',
                                    '{9}',
                                    '{10}',
                                    '{11}',
                                    GETDATE(),
                                    '{6}');";

        public const string BP01_INS_T_BATCH_LOG = @"insert into T_BATCH_LOG values(
                                    '{0}',
                                    'BP01',
                                    GETDATE(),
                                    '{1}',
                                    '',
                                    '3',
                                    '{0}')";

        public const string BP01_QRY_T_BATCH_ERROR = @"select FILENAME from  T_BATCH_ERROR where IS_SUCCEED='1'";

        public const string BP01_GET_ERROR_FILE_BACKUP_FOLDER = @"select 
                                    VALUE
                                    from 
                                    T_GLOBAL_VAR 
                                    where 
                                    GVAR='SAP_FOLDER_ERROR_BK'";

        public const string INS_T_BATCH_LOG_FOR_SAPERROR = @"insert into T_BATCH_LOG values(
                                    '{0}',
                                    '{1}',
                                    GETDATE(),
                                    '{2}',
                                    'No SAP connection. Action aborted',
                                    '1',
                                    '{0}')";

        public const string BP01_UPD_T_BATCH_LOG = @"update T_BATCH_LOG set
                                    STATUS = '0'
                                    where 
                                    BP_ID = '{0}';";


        public const string BP01_GET_T_ACCUMULATE_AND_T_REQUEST_HRL1 = @"select
                                    A.EMP_ID as PERNR,
                                    '2006' as INFTY,
                                    '02' as SUBTY,
                                    NULL as OBJPS,
                                    '' as ENDDA,
                                    '' as BEGDA,
                                    NULL as SEQNR,
                                    B.LE_ACCUM_AN as ANZHL,
                                    A.REQ_NO
                                    from
                                    T_REQUEST_HRL A, T_ACCUMULATE B
                                    where
                                    A.REQ_NO = B.REQ_NO	and
                                    A.BR_STATUS = '2'	and
                                    A.REQ_TYPE = 'L01'  and
                                    B.LE_ACCUM_AN <> '0'
                                    union 
                                    select 
                                    A.EMP_ID as PERNR,
                                    '2006' as INFTY,
                                    '03' as SUBTY,
                                    NULL as OBJPS,
                                    '' as ENDDA,
                                    '' as BEGDA,
                                    NULL as SEQNR,
                                    B.LE_ACCUM_AH as ANZHL,
                                    A.REQ_NO
                                    from
                                    T_REQUEST_HRL A, T_ACCUMULATE B
                                    where
                                    A.REQ_NO = B.REQ_NO	and
                                    A.BR_STATUS = '2'	and
                                    A.REQ_TYPE = 'L01'  and
                                    B.LE_ACCUM_AH <> '0'
                                    order by PERNR;";

        public const string BP01_GET_T_ACCUMULATE_AND_T_REQUEST_HRL2 = @"select
                                    A.EMP_ID as PERNR,
                                    '0416' as INFTY,
                                    'Z001' as SUBTY,
                                    NULL as OBJPS,
                                    '' as ENDDA,
                                    '' as BEGDA,
                                    NULL as SEQNR,
                                    B.LE_REIMBURSE as NUMBR,
                                    '' as BALANCE,
                                    A.REQ_NO
                                    from
                                    T_REQUEST_HRL A, T_ACCUMULATE B
                                    where
                                    A.REQ_NO = B.REQ_NO and
                                    A.BR_STATUS = '2' and
                                    B.LE_REIMBURSE <>0 and
                                    A.REQ_TYPE = 'L01'
                                    union
                                    select
                                    A.EMP_ID as PERNR,
                                    '0416' as INFTY,
                                    'Z001' as SUBTY,
                                    NULL as OBJPS,
                                    '' as ENDDA,
                                    '' as BEGDA,
                                    NULL as SEQNR,
                                    B.BALANCE as NUMBR,
                                    'X' as BALANCE,
                                    A.REQ_NO
                                    from
                                    T_REQUEST_HRL A, T_ACCUMULATE B
                                    where
                                    A.REQ_NO = B.REQ_NO and
                                    A.BR_STATUS = '2' and
                                    A.REQ_TYPE = 'L01' and 
                                    ((B.LE_REIMBURSE <>0 and B.BALANCE<>0) or (B.LE_REIMBURSE <>0 and B.LE_ACCUM_AH<>0) or (B.LE_REIMBURSE<>0 and B.LE_ACCUM_AN<>0) or (B.LE_REIMBURSE=0 and (B.LE_ACCUM_AH<>0 or B.LE_ACCUM_AN<>0 or B.BALANCE<>0)))
                                   order by PERNR, BALANCE asc;";

        public const string BP01_GET_T_LEAVE_HISTORY = @"select
                                    A.EMP_ID as PERNR,
                                    '2001' as INFTY,
                                    B.SAP_LEAVETYPE as SUBTY,
                                    NULL as OBJPS,
                                    B.LEAVE_END as ENDDA,
                                    B.LEAVE_START as BEGDA,
                                    NULL as SEQNR,
                                    NULL as AEDTM,
                                    B.NO_OF_DAYS as ABWTG,
                                    B.BALANCE as QUOTANUM,
                                    B.[DELETE] as DEL,
                                    A.REQ_NO
                                    from
                                    T_REQUEST_HRL A, T_LEAVE_DET B
                                    where
                                    A.REQ_NO = B.REQ_NO	and
                                    A.BR_STATUS = '2'	and
                                    A.REQ_TYPE in ('L18', 'L29', 'L30', 'L31', 'L32', 'L33', 'L34', 'L35', 'L36', 'L37','L38', 'L39', 'L40', 'L41', 'L50','L51', 'L52', 'L53', 'L54', 'L55', 'L56', 'L57') and
                                    B.[DELETE] = 'X';";

        public const string BP01_GET_T_LEAVE_APPLY = @"select
                                    A.EMP_ID as PERNR,
                                    '2001' as INFTY,
                                    B.SAP_LEAVETYPE as SUBTY,
                                    NULL as OBJPS,
                                    B.LEAVE_END as ENDDA,
                                    B.LEAVE_START as BEGDA,
                                    NULL as SEQNR,
                                    A.REQ_NO
                                    from
                                    T_REQUEST_HRL A, T_LEAVE_DET B
                                    where
                                    A.REQ_NO = B.REQ_NO	and
                                    A.BR_STATUS = '2'	and
                                    A.REQ_TYPE in ('L02', 'L03', 'L04', 'L05', 'L06', 'L07', 'L08', 'L09', 'L10', 'L11', 'L12', 'L13', 'L14', 'L15', 'L16',
                                                'L19', 'L20', 'L21', 'L22', 'L23', 'L24', 'L25', 'L26', 'L27', 'L28','L42', 'L43', 'L44', 'L45','L46', 'L47','L48', 'L49') and
                                    B.[DELETE] <> 'X';";



        public const string BP01_UPD_T_REQUEST_HRL = @"update T_REQUEST_HRL set
                                    BR_STATUS = '3',
                                    WF_STATUS ='3',
                                    BP_ID = '{0}'
                                    where 
                                    BR_STATUS = '2' and
                                    (
                                    (REQ_TYPE = 'L01' and REQ_CODE = 'RE')  or 
                                    (REQ_TYPE in ('L18', 'L29', 'L30', 'L31', 'L32', 'L33', 'L34', 'L35', 'L36', 'L37','L38', 'L39', 'L40', 'L41','L42', 'L43', 'L44', 'L45','L46', 'L47','L48', 'L49', 'L50','L51', 'L52', 'L53', 'L54', 'L55', 'L56', 'L57') and REQ_CODE = 'LE' ) or
                                    (REQ_TYPE in ('L02', 'L03', 'L04', 'L05', 'L06', 'L07', 'L08', 'L09', 'L10', 'L11', 'L12', 'L13', 'L14', 'L15', 'L16', 
                                                  'L19', 'L20', 'L21', 'L22', 'L23', 'L24', 'L25', 'L26', 'L27', 'L28') and REQ_CODE = 'LE')
                                    ) and REQ_NO in {1};";










        public const string UPD_T_BATCH_LOG_FOR_SAPERROR = @"update T_BATCH_LOG set
                                    STATUS = '1',
                                    NOTES = 'No SAP connection. Action aborted'
                                    where 
                                    BP_ID = '{0}';";


        public const string BP08_GET_BATCHRUNTIME = @"select 
                                    SCH_TIME,
                                    MODE 
                                    from 
                                    M_BATCH_SCH 
                                    where 
                                    BATCH_CODE='BP08'";
        public const string BP08_INS_T_BATCH_LOG = @"insert into T_BATCH_LOG values(
                                    '{0}',
                                    'BP08',
                                    GETDATE(),
                                    '{1}',
                                    '',
                                    '3',
                                    '{0}')";
        public const string BP08_DEL_M_ORG_UNIT = @"delete from M_ORG_UNIT";
        public const string BP08_INS_M_ORG_UNIT = @"insert into M_ORG_UNIT(
                                     ORG_UNIT
                                    ,DIV_GRP
                                    ,DEPT_GRP
                                    ,SEC_GRP
                                    ,COST_CTR
                                    ,DESCRIPTION                                   
                                    ,CC_FLAG)
                                    values(
                                    '{0}',
                                    {1},
                                    {2},
                                    {3},
                                    {4},
                                    '{5}',
                                    '{6}')";
        public const string BP08_DEL_M_BOSSONLY = @"delete from M_BOSSONLY";
        public const string BP08_INS_M_BOSSONLY = @"insert into M_BOSSONLY(
                                    ORG_UNIT
                                   ,DIV_NAME
                                   ,DIV_NAME_SH
                                   ,GM_EMPID
                                   ,GM_NAME)
                                   values(
                                    '{0}',
                                    '{1}',
                                    '{2}',
                                    '{3}',
                                    '{4}'); ";


        public const string BP05_INS_T_BATCH_LOG = @"insert into T_BATCH_LOG values(
                                    '{0}',
                                    'BP05',
                                    GETDATE(),
                                    '{1}',
                                    '',
                                    '3',
                                    '{0}')";

        public const string BP05_UPD_M_DELEGATION = @"update M_DELEGATION set
                                    DATE_END = GETDATE(),
                                    IS_ACTIVE = '0',
                                    DATE_MODIFIED = GETDATE(),
                                    USER_MODIFIED = '{0}'
                                    where
                                    SUPERVISOR = '{1}' and
                                    SUPER_COSTCENTER = '{2}';";

        public const string BP05_UPD_M_DELEGATION_MEM = @"update M_DELEGATION_MEM set
                                    IS_ACTIVE = '0',
                                    DATE_MODIFIED = GETDATE(),
                                    USER_MODIFIED = '{0}'
                                    where
                                    EMP_ID = '{1}' and
                                    IS_ACTIVE = '1';";

        public const string BP05_GET_TEMP_NEW_TRANS = @"select * from TEMP_NEW_TRANS order by WF_TXN_ID asc";

        public const string BP05_GET_M_DELEGATION = @"select 
                                    SUPERVISOR,
                                    SUPER_NAME,
                                    SUPER_DEPT,
                                    SUPER_COSTCENTER,
                                    MANAGER,
                                    MGR_NAME,
                                    COSTCENTER,
                                    IS_ACTIVE,
                                    DATE_START,
                                    DATE_END
                                    from
                                    M_DELEGATION
                                    where 
                                    IS_ACTIVE='1' and
                                    DATE_START <= GETDATE() and
                                    DATE_END >= GETDATE()";

        public const string BP05_GET_M_DELEGATION_MEM = @"select 
                                    SUPERVISOR,
                                    EMP_ID,
                                    EMP_NAME,
                                    EMP_COSTCENTER,
                                    SUP_EMAIL,
                                    IS_ACTIVE
                                    from
                                    M_DELEGATION_MEM
                                    where 
                                    IS_ACTIVE='1';";


        public const string BP05_GET_T_WF_TRANSACTION = @"select 
                                    REQ_NO,
                                    WF_TXN_ID,
                                    MODULE,
                                    WF_ID,
                                    WF_CUR_LEVEL,
                                    WF_CUR_PIC,
                                    WF_CUR_PIC_NAME,
                                    WF_TXN_STATUS,
                                    WF_STAGENAME,
                                    POSITIVE_LBL
                                    from
                                    T_WF_TRANSACTION
                                    where 
                                    WF_CUR_PIC ='{0}' and
                                    WF_TXN_STATUS = '0';";

        public const string BP05_GET_T_WF_TRANSACTION_ALL = @"select 
                                    B.REQ_NO,
                                    B.WF_TXN_ID,
                                    B.MODULE,
                                    B.WF_ID,
                                    B.WF_CUR_LEVEL,
                                    B.WF_CUR_PIC,
                                    B.WF_CUR_PIC_NAME,
                                    B.WF_TXN_STATUS,
                                    B.WF_STAGENAME,
                                    B.POSITIVE_LBL,
                                    D.EMP_ID
                                    from
                                    T_WF_TRANSACTION B,T_REQUEST_HRP D
                                    where 
                                    D.EMP_ID ='{0}' and
                                    B.WF_TXN_STATUS = '0' and
                                    B.REQ_NO = D.REQ_NO
                                    union
                                    select 
                                    B.REQ_NO,
                                    B.WF_TXN_ID,
                                    B.MODULE,
                                    B.WF_ID,
                                    B.WF_CUR_LEVEL,
                                    B.WF_CUR_PIC,
                                    B.WF_CUR_PIC_NAME,
                                    B.WF_TXN_STATUS,
                                    B.WF_STAGENAME,
                                    B.POSITIVE_LBL,
                                    D.EMP_ID
                                    from
                                    T_WF_TRANSACTION B,T_REQUEST_HRL D
                                    where 
                                    D.EMP_ID ='{0}' and
                                    B.WF_TXN_STATUS = '0' and
                                    B.REQ_NO = D.REQ_NO
                                    union
                                    select 
                                    B.REQ_NO,
                                    B.WF_TXN_ID,
                                    B.MODULE,
                                    B.WF_ID,
                                    B.WF_CUR_LEVEL,
                                    B.WF_CUR_PIC,
                                    B.WF_CUR_PIC_NAME,
                                    B.WF_TXN_STATUS,
                                    B.WF_STAGENAME,
                                    B.POSITIVE_LBL,
                                    D.EMP_ID
                                    from
                                    T_WF_TRANSACTION B,T_REQUEST_HRH D
                                    where 
                                    D.EMP_ID ='{0}' and
                                    B.WF_TXN_STATUS = '0' and
                                    B.REQ_NO = D.REQ_NO;";

        public const string BP05_GET_T_WF_TRANSACTION_NEW_LEAVE = @"select A.*, B.REQ_CODE, B.REQ_TYPE from T_WF_TRANSACTION A, T_REQUEST_HRL B
                                    where A.MODULE = 'HRL' and 
                                    A.WF_TXN_STATUS = '0' and
                                    B.REQ_NO = A.REQ_NO and 
                                    B.REQ_CODE = 'LE' and 
                                    B.REQ_TYPE <> 'L01' and 
                                    A.WF_STAGENAME not in ('SAP BATCH', 'NEW', '');";


        public const string BP05_UPD_T_WF_TRANSACTION = @"update T_WF_TRANSACTION set
                                    WF_TXN_STATUS = '9',
                                    DATE_MODIFIED = GETDATE(),
                                    USER_MODIFIED = '{0}'
                                    where
                                    WF_TXN_ID = '{1}';";

        public const string BP05_INS_TEMP_NEW_TRANS = @"insert into TEMP_NEW_TRANS
                                    (WF_TXN_ID,
                                    REQ_NO,
                                    MODULE,
                                    WF_ID,
                                    WF_CUR_LEVEL,
                                    WF_CUR_PIC,
                                    WF_CUR_PIC_NAME,
                                    WF_STAGENAME,
                                    CURR_COMMENT,
                                    PREV_COMMENT,
                                    DATE_CREATED,
                                    POSITIVE_LBL)
                                    values(
                                    '{0}',
                                    '{1}',
                                    '{2}',
                                    '{3}',
                                    '{4}',
                                    '{5}',
                                    '{6}',
                                    '{7}',
                                    '{8}',
                                    '{9}',
                                    '{10}',                                   
                                    '{11}');";

        public const string BP05_INS_T_WF_TRANSACTION1 = @"insert into T_WF_TRANSACTION 
                                    (WF_CUR_PIC,
                                    WF_CUR_PIC_NAME,
                                    WF_TXN_STATUS,
                                    WF_TXN_ID,
                                    REQ_NO,
                                    MODULE,
                                    WF_ID,
                                    WF_CUR_LEVEL,
                                    WF_STAGENAME,
                                    USER_CREATED,
                                    NAME_CREATED,
                                    DATE_CREATED,
                                    POSITIVE_LBL)
                                    values(
                                    '{0}',
                                    '{1}',
                                    '0',
                                    '{2}',
                                    '{3}',
                                    '{4}',
                                    '{5}',
                                    '{6}',
                                    '{7}',
                                    'BP05',
                                    'BP05',
                                    GETDATE(),
                                    '{8}');";

        public const string BP05_INS_T_WF_TRANSACTION2 = @"insert into T_WF_TRANSACTION 
                                    (WF_TXN_ID,
                                    REQ_NO,
                                    MODULE,
                                    WF_ID,
                                    WF_CUR_LEVEL,
                                    WF_CUR_PIC,
                                    WF_CUR_PIC_NAME,
                                    WF_TXN_STATUS,
                                    WF_STAGENAME,
                                    CURR_COMMENT,
                                    PREV_COMMENT,
                                    USER_CREATED,
                                    DATE_CREATED,
                                    USER_MODIFIED,
                                    DATE_MODIFIED,
                                    POSITIVE_LBL)
                                    values(
                                    '{0}',
                                    '{1}',                                   
                                    '{2}',
                                    '{3}',
                                    '{4}',
                                    '{5}',
                                    '{6}',
                                    '{7}',
                                    '{8}',
                                    '{9}',
                                    '{10}',
                                    '{11}',
                                    GETDATE(),
                                    '{11}',
                                    GETDATE(),
                                    '{12}');";

        public const string BP05_DEL_TEMP_NEW_TRANS = @"delete from TEMP_NEW_TRANS where WF_TXN_ID = '{0}';";

        public const string BP07_GET_T_REQUEST_HRP = @"select 
                                    REQ_NO,
                                    GRP_REQNO,
                                    EMP_ID,
                                    EMP_NAME,
                                    BP_ID,
                                    BR_STATUS,
                                    REQ_TYPE
                                    from
                                    T_REQUEST_HRP
                                    where 
                                    EMP_ID ='{0}' and
                                    BP_ID = '{1}' and
                                    REQ_TYPE in ({2});";


        public const string BP07_GET_T_REQUEST_HRP_AND_T_SP_LIST = @"select 
                                    A.REQ_NO,
                                    A.GRP_REQNO,
                                    A.EMP_ID,
                                    A.EMP_NAME,
                                    A.BP_ID,
                                    A.BR_STATUS,
                                    B.SEQ,
                                    B.OBJPS
                                    from
                                    T_REQUEST_HRP A,T_SP_LIST B
                                    where 
                                    A.EMP_ID ='{0}' and
                                    A.BP_ID = '{1}' and
                                    A.REQ_TYPE = '{2}' and
                                    B.OBJPS = '{3}' and
                                    B.SEQ = '{4}' and
                                    A.REQ_NO = B.REQ_NO;";

        public const string BP07_GET_T_REQUEST_HRP_AND_T_SP_DEPENDANT = @"select 
                                    A.REQ_NO,
                                    A.GRP_REQNO,
                                    A.EMP_ID,
                                    A.EMP_NAME,
                                    A.BP_ID,
                                    A.BR_STATUS,
                                    B.SEQ,
                                    B.OBJPS
                                    from
                                    T_REQUEST_HRP A,T_SP_DEPENDANT B
                                    where 
                                    A.EMP_ID ='{0}' and
                                    A.BP_ID = '{1}' and
                                    A.REQ_TYPE = '{2}' and
                                    B.OBJPS = '{3}' and
                                    B.SEQ = '{4}' and
                                    A.REQ_NO = B.REQ_NO;";

        public const string BP07_GET_T_REQUEST_HRP_AND_T_INS_NOMINEE = @"select 
                                    A.REQ_NO,
                                    A.GRP_REQNO,
                                    A.EMP_ID,
                                    A.EMP_NAME,
                                    A.BP_ID,
                                    A.BR_STATUS,
                                    B.NOM_SEQ
                                    from
                                    T_REQUEST_HRP A,T_INS_NOMINEE  B
                                    where 
                                    A.EMP_ID ='{0}' and
                                    A.BP_ID = '{1}' and
                                    A.REQ_TYPE = '{2}' and
                                    B.NOM_SEQ = '{3}' and
                                    A.REQ_NO = B.REQ_NO;";

        public const string BP07_GET_T_REQUEST_HRP_AND_T_ACADEMIC = @"select 
                                    A.REQ_NO,
                                    A.GRP_REQNO,
                                    A.EMP_ID,
                                    A.EMP_NAME,
                                    A.BP_ID,
                                    A.BR_STATUS,
                                    B.BEGDA
                                    from
                                    T_REQUEST_HRP A,T_ACADEMIC B
                                    where 
                                    A.EMP_ID ='{0}' and
                                    A.BP_ID = '{1}' and
                                    A.REQ_TYPE = '{2}' and
                                    B.BEGDA = '{3}' and
                                    B.EDU_INSTITUTION='{4}' and
                                    A.REQ_NO = B.REQ_NO;";

        public const string BP07_GET_T_REQUEST_HRP_AND_T_EME_CTC_LIST = @"select 
                                    A.REQ_NO,
                                    A.GRP_REQNO,
                                    A.EMP_ID,
                                    A.EMP_NAME,
                                    A.BP_ID,
                                    A.BR_STATUS,
                                    B.SEQ                                   
                                    from
                                    T_REQUEST_HRP A,T_EME_CTC_LIST B
                                    where 
                                    A.EMP_ID ='{0}' and
                                    A.BP_ID = '{1}' and
                                    A.REQ_TYPE = '{2}' and                                   
                                    B.SEQ = '{3}' and
                                    A.REQ_NO = B.REQ_NO;";

        public const string BP07_GET_T_REQUEST_HRL_T_LEAVE_DET = @"select 
                                    A.REQ_NO,
                                    A.REQ_TYPE,
                                    A.EMP_ID,
                                    A.EMP_NAME,
                                    A.BP_ID,
                                    A.BR_STATUS,
                                    B.LEAVE_START,
                                    B.LEAVE_END
                                    from
                                    T_REQUEST_HRL A, T_LEAVE_DET B
                                    where 
                                    A.EMP_ID ='{0}' and
                                    A.BP_ID = '{1}' and
                                    B.[DELETE] = '{2}' and
                                    A.REQ_NO = B.REQ_NO and
                                    B.SAP_LEAVETYPE = '{3}' and
                                    B.LEAVE_START = '{4}' and
                                    B.LEAVE_END = '{5}';";

        public const string BP07_GET_T_REQUEST_HRL_T_ACCUMULATE = @"select 
                                    A.REQ_NO,
                                    A.REQ_TYPE,
                                    A.EMP_ID,
                                    A.EMP_NAME,
                                    A.BP_ID,
                                    A.BR_STATUS,
                                    B.LE_REIMBURSE,
                                    B.LE_ACCUM_AN,
                                    B.LE_ACCUM_AH
                                    from
                                    T_REQUEST_HRL A, T_ACCUMULATE B
                                    where 
                                    A.REQ_NO = B.REQ_NO and
                                    A.EMP_ID = '{0}' and
                                    A.BP_ID = '{1}';";



        public const string BP07_UPD_T_REQUEST_HRP_BY_REQ_NO = @"update 
                                    T_REQUEST_HRP
                                    set
                                    BR_STATUS = '4'
                                    where 
                                    REQ_NO ='{0}';";

        public const string BP07_UPD_T_REQUEST_HRL_BY_REQ_NO = @"update 
                                    T_REQUEST_HRL
                                    set
                                    BR_STATUS = '2'
                                    where 
                                    REQ_NO ='{0}';";

        public const string BP07_UPD_T_WF_TRANSACTION1 = @"update 
                                    T_WF_TRANSACTION
                                    set
                                    WF_TXN_STATUS = '5'
                                    where 
                                    REQ_NO ='{0}' and
                                    WF_STAGENAME = 'SAP BATCH' and
                                    WF_ID not in ('WF90','WF91','WF92','WF93','WF94','WF95','WF96','WF97','WF98','WF99');";

        public const string BP07_UPD_T_WF_TRANSACTION2 = @"update 
                                    T_WF_TRANSACTION
                                    set
                                    WF_TXN_STATUS = '5'
                                    where 
                                    REQ_NO ='{0}' and
                                    WF_STAGENAME = 'SAP BATCH' and
                                    WF_ID in ('WF90','WF91','WF92','WF93','WF94','WF95','WF96','WF97','WF98','WF99');";

        public const string BP07_UPD_T_WF_TRANSACTION3 = @"update 
                                    T_WF_TRANSACTION
                                    set
                                    WF_TXN_STATUS = '5'
                                    where 
                                    REQ_NO ='{0}' and
                                    WF_CUR_LEVEL = '{1}' and
                                    WF_ID not in ('WF90','WF91','WF92','WF93','WF94','WF95','WF96','WF97','WF98','WF99');";

        public const string BP07_INS_T_BATCH_ERROR = @"insert into T_BATCH_ERROR 
                                    (BP_ID,
                                    REQ_NO,
                                    EMP_ID,
                                    EMP_NAME,
                                    REQ_TYPE,
                                    BR_STATUS,
                                    ERROR,
                                    TRIGGER_TIME,
                                    TRIGGER_BY,
                                    INFOTY,
                                    SUBTY,
                                    SEQNO,
                                    OBJPS,
                                    FILENAME,
                                    IS_SUCCEED)
                                    values
                                    ('{0}',
                                    '{1}',
                                    '{2}',
                                    '{3}',
                                    '{4}',
                                    '4',
                                    '{5}',
                                    GETDATE(),
                                    '{6}',
                                    '{7}',
                                    '{8}',
                                    '{9}',
                                    '{10}',
                                    '{11}',
                                    '0');";

        public const string BP07_INS_T_BATCH_ERROR2 = @"insert into T_BATCH_ERROR 
                                    (BP_ID,
                                    REQ_NO,
                                    EMP_ID,
                                    EMP_NAME,
                                    REQ_TYPE,
                                    BR_STATUS,
                                    ERROR,
                                    TRIGGER_TIME,
                                    TRIGGER_BY,
                                    FILENAME,
                                    LEAVE_START,
                                    LEAVE_END,
                                    IS_SUCCEED)
                                    values
                                    ('{0}',
                                    '{1}',
                                    '{2}',
                                    '{3}',
                                    '{4}',
                                    '4',
                                    '{5}',
                                    GETDATE(),
                                    '{6}',
                                    '{7}',
                                    '{8}',
                                    '{9}',
                                    '0');";

        public const string BP07_UPD_T_BATCH_ERROR = @"update T_BATCH_ERROR set IS_SUCCEED='1' where BP_ID='{0}' and REQ_NO='{1}' and EMP_ID='{2}'";

        public const string BP07_GET_ARCHIVEBATCHFILEPATH = @"select 
                                    VALUE 
                                    from 
                                    T_GLOBAL_VAR 
                                    where 
                                    GVAR='SAP_FOLDER_ARCHIVE'";

        public const string BP07_GET_BATCHFILEPATH = @"select 
                                    VALUE 
                                    from 
                                    T_GLOBAL_VAR 
                                    where 
                                    GVAR='SAP_FOLDER_ERROR'";

        public const string BP07_GET_T_WF_TRANSACTION = @"select * from T_WF_TRANSACTION
                                    where WF_CUR_LEVEL='{0}' and REQ_NO='{1}';";

        public const string BP07_INS_T_BATCH_LOG = @"insert into T_BATCH_LOG values(
                                    '{0}',
                                    'BP07',
                                    GETDATE(),
                                    '{1}',
                                    '',
                                    '{2}',
                                    '{3}')";

        public const string BP07_UPD_T_BATCH_LOG = @"update T_BATCH_LOG set
                                    STATUS = '0'
                                    where 
                                    BP_ID = '{0}';";

        public const string BP07_INS_T_WF_TRANSACTION = @"insert into 
                                                    T_WF_TRANSACTION (
                                                    WF_TXN_ID,
                                                    REQ_NO,
                                                    MODULE,
                                                    WF_ID,
                                                    WF_CUR_LEVEL,
                                                    WF_CUR_PIC,
                                                    WF_CUR_PIC_NAME,
                                                    WF_TXN_STATUS,
                                                    WF_STAGENAME,
                                                    CURR_COMMENT,
                                                    PREV_COMMENT,
                                                    USER_CREATED,                                                    
                                                    NAME_CREATED,
                                                    DATE_CREATED,
                                                    USER_MODIFIED,
                                                    DATE_MODIFIED,
                                                    POSITIVE_LBL)
                                                    values(
                                                    '{0}',
                                                    '{1}',
                                                    '{2}',
                                                    '{3}',
                                                    '{4}',
                                                    '{5}',
                                                    '{6}',
                                                    '{7}',
                                                    '{8}',
                                                    '{9}',
                                                    '{10}',
                                                    '{11}',
                                                    '{12}',                                                   
                                                    GETDATE(),
                                                    '{11}',
                                                    GETDATE(),
                                                    '{13}');";

        public const string BP07_UPD_T_REQUEST_HRP = @"update T_REQUEST_HRP set  
                                    BR_STATUS='4'
                                    where BP_ID ='{0}';";

        public const string BP07_UPD_T_REQUEST_HRL = @"update T_REQUEST_HRL set  
                                    BR_STATUS='4'
                                    where BP_ID ='{0}';";

        public const string BP07_GET_BATCHRUNTIME = @"select 
                                    SCH_TIME,
                                    datepart(hour,[SCH_TIME]) + datepart(hour,[DELAY_TIME]) as HOUR_TIME,
                                    datepart(minute,[SCH_TIME]) + datepart(minute,[DELAY_TIME]) as MIN_TIME,
                                    datepart(second,[SCH_TIME]) + datepart(second,[DELAY_TIME]) as SEC_TIME,
                                    MODE 
                                    from 
                                    M_BATCH_SCH 
                                    where 
                                    BATCH_CODE='BP07'";
        public const string BP07_QRY_M_EMAIL_FMT = @" select 
                                    EMAIL_FMT,
                                    EMAIL_DESC,
                                    EMAIL_MODULE,
                                    SUBJECT,
                                    CONTENT,
                                    WF_LEVEL,
                                    WF_ID 
                                    from 
                                    M_EMAIL_FMT 
                                    where 
                                    EMAIL_FMT = '{0}'";

        public const string BP07_QRY_REQNO_LIST_BY_ERRORFILEBPID = @"select distinct r.req_no, r.req_desc
                                from T_REQUEST_HRL r, T_BATCH_LOG l
                                where r.BP_ID = l.BP_ID
                                and BATCH_CODE ='BP07' 
                                and l.BP_ID IN 
                                ({0})  
                                union 
                                select distinct r.req_no, r.req_desc 
                                from T_REQUEST_HRP r, T_BATCH_LOG l 
                                where r.BP_ID = l.BP_ID
                                and BATCH_CODE ='BP07' 
                                and l.BP_ID IN 
                                ({0}) ";

        public const string BP07_QRY_T_WF_TRANSACTION1 = @"select WF_TXN_ID, WF_CUR_LEVEL, WF_TXN_STATUS, WF_ID, MODULE from T_WF_TRANSACTION
                                                        where WF_ID not in ('WF90','WF91','WF92','WF93','WF94','WF95','WF96','WF97','WF98','WF99') and 
                                                        WF_STAGENAME = 'SAP BATCH' and REQ_NO = '{0}';";

        public const string BP07_QRY_T_WF_TRANSACTION2 = @"select WF_TXN_ID, WF_CUR_LEVEL, WF_TXN_STATUS, WF_ID, MODULE from T_WF_TRANSACTION
                                                        where WF_ID in ('WF90','WF91','WF92','WF93','WF94','WF95','WF96','WF97','WF98','WF99') and 
                                                        WF_STAGENAME = 'SAP BATCH' and REQ_NO = '{0}';";

        public const string BP04_GET_BATCHRUNTIME = @"select 
                                    SCH_TIME,
                                    MODE 
                                    from 
                                    M_BATCH_SCH 
                                    where 
                                    BATCH_CODE='BP04'";
        public const string BP04_GET_MAXDAY = @"select 
                                    VALUE
                                    from 
                                    T_GLOBAL_VAR 
                                    where 
                                    GVAR = 'MAXDAY'";
        public const string BP04_GET_SAP_FOLDER = @"select 
                                    VALUE 
                                    from 
                                    T_GLOBAL_VAR 
                                    where 
                                    GVAR='SAP_FOLDER'";
        public const string BP04_INS_T_BATCH_LOG = @"insert into T_BATCH_LOG values(
                                    '{0}',
                                    'BP04',
                                    GETDATE(),
                                    '{1}',
                                    '',
                                    '3',
                                    '{0}')";
        public const string BP04_UPD_T_BATCH_LOG = @"update T_BATCH_LOG set
                                    STATUS = '{0}'
                                    where 
                                    BP_ID = '{1}'
                                    and BATCH_CODE = 'BP04' ";
        public const string BP04_DEL_T_RAW_EMP_ATT1 = @"delete from T_RAW_EMP_ATT where ATT_DATE <= DATEADD(mm,-3,GETDATE())";
        public const string BP04_DEL_T_RAW_EMP_ATT2 = @"delete from T_RAW_EMP_ATT where ATT_DATE >= DATEADD(dd,{0},GETDATE())
                                    and ATT_DATE <= GETDATE()";
        public const string BP04_INS_T_RAW_EMP_ATT = @"insert into T_RAW_EMP_ATT values(
                                    '{0}',
                                    '{1}',
                                    '{2}',
                                    '{3}',
                                    '{4}',
                                    '{5}',
                                    '{6}',
                                    '{7}')";


        public const string BP03_GET_BATCHRUNTIME = @"select 
                                    SCH_TIME,
                                    MODE 
                                    from 
                                    M_BATCH_SCH 
                                    where 
                                    BATCH_CODE='BP03';";
        public const string BP03_GET_MAXDAY = @"select 
                                    VALUE
                                    from 
                                    T_GLOBAL_VAR 
                                    where 
                                    GVAR = 'MAXDAY';";
        public const string BP03_GET_T_RAW_EMP_ATT = @"select 
                                    EMP_ID,
                                    ATT_DATE,
                                    EMP_NAME,
                                    EMP_DEPT,
                                    EMP_COSTCENTER,
                                    SHIFT_GRP,
                                    DAY,
                                    STATUS
                                    from 
                                    T_RAW_EMP_ATT 
                                    where 
                                    ATT_DATE = '{0}';";
        public const string BP03_INS_T_BATCH_LOG = @"insert into T_BATCH_LOG values(
                                    '{0}',
                                    'BP03',
                                    GETDATE(),
                                    '{1}',
                                    '',
                                    '3',
                                    '{0}');";
        public const string BP03_INS_DATA_ATT_DAY = @"exec SP_INS_ATT_DAY 
                                    '{0}',                                   
                                    '{1}',
                                    '{2}',
                                    '{3}',
                                    '{4}',
                                    '{5}',
                                    '{6}',                                    
                                    '{7}';";
        public const string BP03_INS_TEMP_CC = @"exec SP_TEMP_CC 
                                    '{0}';";
        public const string BP03_INS_DATA_ATT_COSTCENTER = @"exec SP_INS_ATT_CC 
                                    '{0}';";
        public const string BP03_INS_TEMP_DEPT = @"exec SP_TEMP_DEPT ;";
        public const string BP03_INS_TEMP_DIV = @"exec SP_TEMP_DIV ;";
        public const string BP03_INS_DATA_ATT_DEPT_DIV = @"exec SP_INS_ATT_DEPT_DIV 
                                    '{0}';";
        public const string BP03_INS_DATA_ATT_DD_MTH = @"exec SP_INS_ATT_DD_MTH 
                                    '{0}';";
        public const string BP03_UPD_T_BATCH_LOG = @"update T_BATCH_LOG set
                                    STATUS = '{0}'
                                    where 
                                    BP_ID = '{1}';";
        public const string BP03_DEL_DATA_ATT_DAY = @"delete from DATA_ATT_DAY                                   
                                    where 
                                    DATEDIFF(day,DATE ,DATEADD(yyyy,-2,GETDATE())) > 0 or 
                                    DATEDIFF(day,DATEADD(dd,-{0},GETDATE()),DATE) >= 0;";
        public const string BP03_DEL_DATA_ATT_COSTCENTER = @"delete from DATA_ATT_COSTCENTER                                   
                                    where 
                                    DATEDIFF(day,ATT_DATE ,DATEADD(yyyy,-2,GETDATE())) > 0 or 
                                    DATEDIFF(day,DATEADD(dd,-{0},GETDATE()),ATT_DATE) >= 0;";
        public const string BP03_DEL_DATA_ATT_DEPT_DIV = @"delete from DATA_ATT_DEPT_DIV                                   
                                    where 
                                    DATEDIFF(day,ATT_DATE ,DATEADD(yyyy,-2,GETDATE())) > 0 or 
                                    DATEDIFF(day,DATEADD(dd,-{0},GETDATE()),ATT_DATE) >= 0;";
        public const string BP03_DEL_DATA_ATT_DD_MTH = @"delete from DATA_ATT_DD_MTH                                   
                                    where 
                                    ATT_MONTH = CONVERT(char(6), GETDATE(),112) or
                                    ATT_MONTH = CONVERT(char(6), DATEADD(MM,-1,GETDATE()),112);";



        /// <summary>
        /// get RELATION information from RESOURCESMAINT by code
        /// </summary>
        public const string QRY_M_RELATION_BY_CODE = " select RESOURCE_ID as REL_CODE, UPPER(DESC_BM) as REL_DESC_BM, UPPER(DESC_EN) as REL_DESC_EN from RESOURCESMAINT where CATEGORY='RELATIONSP' and IS_ACTIVE='1' and RESOURCE_ID='{0}';";


        #endregion



        #region Method
        /// <summary>
        /// get Information
        /// </summary>
        /// <param name="sqlStr">sql string</param>
        /// <returns></returns>
        public static DataTable getData(string sqlStr)
        {
            DataTable dt = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    DataSet ds = new DataSet();
                    SqlCommand cmd = new SqlCommand(sqlStr, connection);
                    SqlDataAdapter dp = new SqlDataAdapter(cmd);
                    dp.Fill(ds);
                    dt = ds.Tables[0];
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
        /// update content by sql text        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        public static int updateDataBySqlText(string sqlStr)
        {
            int result = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(sqlStr, connection);
                    result = cmd.ExecuteNonQuery();
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

    }
}
