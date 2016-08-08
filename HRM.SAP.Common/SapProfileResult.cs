using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Canon.HRM.SAP.Common
{
    public class SapProfileResult
    {
        /// <summary>
        /// E_IMAGE
        /// </summary>
        public string E_IMAGE { get; set; }

        /// <summary>
        /// T_HEADER
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public DataTable T_HEADER { get; set; }

        /// <summary>
        /// T_WORKDETAIL
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public DataTable T_WORKDETAIL { get; set; }

        /// <summary>
        /// T_PERSONALINFO
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public DataTable T_PERSONALINFO { get; set; }

        /// <summary>
        /// T_SPOUSEDEPENDENT
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public DataTable T_SPOUSEDEPENDENT { get; set; }

        /// <summary>
        /// T_INSURANCE
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public DataTable T_INSURANCE { get; set; }

        /// <summary>
        /// T_EDUCATION
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public DataTable T_EDUCATION { get; set; }

        /// <summary>
        /// T_EMERGENCYCONT
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public DataTable T_EMERGENCYCONT { get; set; }

        /// <summary>
        /// T_DEPARTMENT
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public DataTable T_DEPARTMENT { get; set; }

        /// <summary>
        /// T_COSTCENTER
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public DataTable T_COSTCENTER { get; set; }

        /// <summary>
        /// T_DEPTCOST               
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public DataTable T_DEPTCOST { get; set; }

        /// <summary>
        /// ErrorMessage
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// T_LEAVESUMMARY
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public DataTable T_LEAVESUMMARY { get; set; }

        /// <summary>
        /// T_LEAVEHISTORY
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public DataTable T_LEAVEHISTORY { get; set; }

        /// <summary>
        /// T_PREVBALANCE
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public DataTable T_PREVBALANCE { get; set; }

        /// <summary>
        /// T_ACCUMHISTORY
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public DataTable T_ACCUMHISTORY { get; set; }

        /// <summary>
        /// T_ORGCHART
        /// </summary>
        public DataTable T_ORGCHART { get; set; }

        /// <summary>
        /// T_BOSSONLY
        /// </summary>
        public DataTable T_BOSSONLY { get; set; }

        /// <summary>
        /// GT_WS
        /// </summary>
        public DataTable GT_WS { get; set; }


    }
}
