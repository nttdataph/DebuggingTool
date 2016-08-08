using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Net.Mail;

namespace Canon.HRM.Common
{
    public static class MailCommon
    {
        /// <summary>
        /// mail send
        /// </summary>
        /// <param name="subjectContxt">Subject content</param>
        /// <param name="bodyContxt">Body content</param>
        /// <param name="toMailAdrss">To mail Addresses</param>
        /// <param name="ccMailAdrss">CC mail Addresses</param>
        /// <param name="bccMailAdrss">BCC mail Adresses</param>
        /// <param name="fromMailAdrs">From mail Adresses</param>
        public static void SendMail(string subjectContxt, string bodyContxt, List<string> toMailAdrss, List<string> ccMailAdrss, List<string> bccMailAdrss, object fromMailAdrs = null)
        {
            try
            {
                string FromAdrs = string.Empty;
                if (fromMailAdrs != null)
                {
                    FromAdrs = fromMailAdrs.ToString();
                }
                else
                {
                    FromAdrs = SPContext.Current.Site.WebApplication.OutboundMailSenderAddress;
                }
                MailMessage message = new MailMessage
                {
                    Subject = subjectContxt,
                    Body = bodyContxt,
                    From = new MailAddress(FromAdrs)
                };
                if (toMailAdrss != null && toMailAdrss.Count > 0)
                {
                    for (int i = 0; i <= toMailAdrss.Count - 1; i++)
                    {
                        if (!String.IsNullOrEmpty(toMailAdrss[i]))
                        {
                            message.To.Add(toMailAdrss[i]);
                        }
                    }
                }
                if (ccMailAdrss != null && ccMailAdrss.Count > 0)
                {
                    for (int i = 0; i <= ccMailAdrss.Count - 1; i++)
                    {
                        if (!String.IsNullOrEmpty(ccMailAdrss[i]))
                        {
                            message.CC.Add(ccMailAdrss[i]);
                        }
                    }
                }
                if (bccMailAdrss != null && bccMailAdrss.Count > 0)
                {
                    for (int i = 0; i <= bccMailAdrss.Count - 1; i++)
                    {
                        if (!String.IsNullOrEmpty(bccMailAdrss[i]))
                        {
                            message.Bcc.Add(bccMailAdrss[i]);
                        }
                    }
                }
                if (SPContext.Current.Site.WebApplication.OutboundMailServiceInstance == null)
                {
                    return;

                }

                new SmtpClient(SPContext.Current.Site.WebApplication.OutboundMailServiceInstance.Server.Address).Send(message);
                message.Dispose();
            }
            catch (Exception)
            {
                return;
            }
        }
 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="body">mail body</param>
        /// <param name="PIC"></param>
        /// <param name="RequestId"></param>
        /// <param name="RequestorName"></param>
        /// <param name="RequestType">Requests from either myLeave or myProfile </param>
        /// <param name="StageName">Taken from current stage of workflow</param>
        /// <param name="ProfileType">Type request made from myProfile screen application</param>
        /// <param name="TaskUrl">Hyperlink to the task in Detailed Task Description</param>
        /// <returns>mail replace body</returns>
        public static string getEProfileBody(string body, string PIC, string RequestId, string RequestorName, string RequestType, string StageName, string ProfileType, string TaskUrl)
        {
            string tempBody = body;
            tempBody = tempBody.Replace("<PIC>", PIC).Replace("<RequestId>", RequestId)
                .Replace("<RequestorName>", RequestorName).Replace("<RequestType>", RequestType)
                .Replace("<StageName>", StageName).Replace("<ProfileType>", ProfileType)
                .Replace("<TaskUrl>", TaskUrl);
            return tempBody;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="LeaveType"></param>
        /// <param name="LeaveStart"></param>
        /// <param name="LeaveEnd"></param>
        /// <param name="NoOfDays"></param>
        /// <param name="LeaveReason"></param>
        /// <param name="TaskUrl"></param>
        public static string getELeaveBody(string body, string LeaveType, string LeaveStart, string LeaveEnd, string NoOfDays, string LeaveReason, string TaskUrl)
        {
            string tempBody = body;
            tempBody = tempBody.Replace("<LeaveType>", LeaveType).Replace("<LeaveStart>", LeaveStart)
                .Replace("<LeaveStart>", LeaveStart).Replace("<LeaveEnd>", LeaveEnd)
                .Replace("<NoOfDays>", NoOfDays).Replace("<LeaveReason>", LeaveReason)
                .Replace("<TaskUrl>", TaskUrl);
            return tempBody;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="body">mail body</param>
        /// <param name="PIC"></param>
        /// <param name="RequestId"></param>
        /// <param name="RequestorName"></param>
        /// <param name="StageName">Taken from current stage of workflow</param>
        /// <param name="RequestType">Requests from either myLeave or myProfile </param>
        /// <param name="ProfileType">Type request made from myProfile screen application</param>
        /// <param name="TaskUrl">Hyperlink to the task in Detailed Task Description</param>
        /// <param name="LeaveType">Leave type applied during leave application</param>
        /// <param name="LeaveStart">Leave Start Date</param>
        /// <param name="LeaveEnd">Leave End Date</param>
        /// <param name="NoOfDays">No of days applied for leave.</param>
        /// <param name="LeaveReason">Leave reason entered during leave application</param>
        /// <returns></returns>
        public static string getBody(string body, string PIC, string RequestId, string RequestorName, string StageName, string RequestType, string ProfileType,
            string TaskUrl, string LeaveType = null, string LeaveStart = null, string LeaveEnd = null, string NoOfDays = null, string LeaveReason = null)
        {
            string tempBody = body;
            tempBody = tempBody.Replace("<PIC>", PIC).Replace("<RequestId>", RequestId)
                .Replace("<RequestorName>", RequestorName).Replace("<RequestType>", RequestType)
                .Replace("<StageName>", StageName).Replace("<ProfileType>", ProfileType)
                .Replace("<LeaveType>", LeaveType).Replace("<LeaveStart>", LeaveStart)
                .Replace("<LeaveStart>", LeaveStart).Replace("<LeaveEnd>", LeaveEnd)
                .Replace("<NoOfDays>", NoOfDays).Replace("<LeaveReason>", LeaveReason)
                .Replace("<TaskUrl>", TaskUrl);
            return tempBody;
        }
    }
}
