using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Diagnostics;
using CoreProcess.Model;
using System.Threading;
using NLog;

namespace CoreProcess
{
    public class Core
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public void StartCore()
        {
           
            logger.Log(LogLevel.Info, "Core Start");

            var listModels = new List<CoreModel>();

            DataTable dt = getBatchStat();  // retrieve batch codes that is not running.
            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    var lstprocessedDate=dr["LAST_PROCESSED"].ToString();
                    var running = dr["IS_RUNNING"].ToString() == "0" ? false : true;
                    listModels.Add(new CoreModel { IsToRun = running, ModuleName = dr["BATCH_CODE"].ToString(),LastProcessedDate=Convert.ToDateTime(lstprocessedDate)});
                }
            }

            var threadsList = listModels; //ListCoreModel();
            logger.Log(LogLevel.Info, "TOTAL THREADS AT START= " + Process.GetCurrentProcess().Threads.Count);
            DateTime x = new DateTime(); 
            for (int i = 0; i < threadsList.Count; i++)
            {
                logger.Log(LogLevel.Info, "TOTAL THREADS = " + Process.GetCurrentProcess().Threads.Count + " --- Loop Count = " + i);
                if (!threadsList[i].IsToRun)
                {
                    x = threadsList[i].LastProcessedDate;

                    switch (threadsList[i].ModuleName)
                    {
                        case "BP00":
                            if ((DateTime.Now - x).TotalMinutes > 10)
                            {
                                DBHelper.updateDataBySqlText(DBHelper.UPDATE_ISNOTRUNNING + "'BP00'");
                                logger.Log(LogLevel.Info, "Updated BP00 isrunning to 1");
   
                                Threads.BP00 bp00 = new Threads.BP00();
                                bp00.CompletedEvent -= bp00_CompletedEvent;
                                bp00.CompletedEvent += bp00_CompletedEvent;
                                Thread _bp00 = new Thread(new ThreadStart(bp00.BP00Process));
                                _bp00.Start();
                            }
                            break;
                        case "BP01":
                            if ((DateTime.Now - x).TotalMinutes > 10)
                            {

                                DBHelper.updateDataBySqlText(DBHelper.UPDATE_ISNOTRUNNING + "'BP01'");
                                logger.Log(LogLevel.Info, "Updated BP01 isrunning to 1");

                                Threads.BP01 bp01 = new Threads.BP01();
                                bp01.CompletedEvent -= bp01_CompletedEvent;
                                bp01.CompletedEvent += bp01_CompletedEvent;
                                Thread _bp01 = new Thread(new ThreadStart(bp01.BP02Process));
                                _bp01.Start();
                           }
                            break;
                        case "BP03":
                            if ((DateTime.Now - x).TotalMinutes > 10)
                            {
                                DBHelper.updateDataBySqlText(DBHelper.UPDATE_ISNOTRUNNING + "'BP03'");
                                logger.Log(LogLevel.Info, "Updated BP03 isrunning to 1");

                                Threads.BP03 bp03 = new Threads.BP03();
                                bp03.CompletedEvent -= bp03_CompletedEvent;
                                bp03.CompletedEvent += bp03_CompletedEvent;
                                Thread _bp03 = new Thread(new ThreadStart(bp03.BP03Process));
                                _bp03.Start();
                            }
                            break;
                        case "BP04":
                            if ((DateTime.Now - x).TotalMinutes > 10)
                            {
                                DBHelper.updateDataBySqlText(DBHelper.UPDATE_ISNOTRUNNING + "'BP04'");
                                logger.Log(LogLevel.Info, "Updated BP04 isrunning to 1");

                                Threads.BP04 bp04 = new Threads.BP04();
                                bp04.CompletedEvent -= bp04_CompletedEvent;
                                bp04.CompletedEvent += bp04_CompletedEvent;
                                Thread _bp04 = new Thread(new ThreadStart(bp04.BP04Process));
                                _bp04.Start();
                            }
                            break;
                        case "BP05":
                            if ((DateTime.Now - x).TotalMinutes > 10)
                            {
                                DBHelper.updateDataBySqlText(DBHelper.UPDATE_ISNOTRUNNING + "'BP05'");
                                logger.Log(LogLevel.Info, "Updated BP05 isrunning to 1");

                                Threads.BP05 bp05 = new Threads.BP05();
                                bp05.CompletedEvent -= bp05_CompletedEvent;
                                bp05.CompletedEvent += bp05_CompletedEvent;
                                Thread _bp05 = new Thread(new ThreadStart(bp05.BP05Process));
                                _bp05.Start();
                            }
                            break;

                        case "BP07":
                            if ((DateTime.Now - x).TotalMinutes > 10)
                            {
                                DBHelper.updateDataBySqlText(DBHelper.UPDATE_ISNOTRUNNING + "'BP07'");
                                logger.Log(LogLevel.Info, "Updated BP07 isrunning to 1");

                                Threads.BP07 bp07 = new Threads.BP07();
                                bp07.CompletedEvent -= bp07_CompletedEvent;
                                bp07.CompletedEvent += bp07_CompletedEvent;
                                Thread _bp07 = new Thread(new ThreadStart(bp07.BP07Process));
                                _bp07.Start();
                            }
                            break;
                        case "BP08":
                            if ((DateTime.Now - x).TotalMinutes > 10)
                            {
                                DBHelper.updateDataBySqlText(DBHelper.UPDATE_ISNOTRUNNING + "'BP08'");
                                logger.Log(LogLevel.Info, "Updated BP08 isrunning to 1");

                                Threads.BP08 bp08 = new Threads.BP08();
                                bp08.CompletedEvent -= bp08_CompletedEvent;
                                bp08.CompletedEvent += bp08_CompletedEvent;
                                Thread _bp08 = new Thread(new ThreadStart(bp08.BP08Process));
                                _bp08.Start();
                            }
                            break;

                        default:
                            break;
                    }
                }
            }

            logger.Log(LogLevel.Info, "TOTAL THREADS AT End = " + Process.GetCurrentProcess().Threads.Count);
            logger.Log(LogLevel.Info, "Core End");

        }

        void bp00_CompletedEvent(object sender, EventArgs e)
        {
            //update table back to 0
            logger.Log(LogLevel.Info, "BP00 ended");
            DBHelper.updateDataBySqlText(DBHelper.UPDATE_ISRUNNING_ORIG + "'BP00'");
            logger.Log(LogLevel.Info, "BP00 updated isrunning to 0");
        }
        void bp01_CompletedEvent(object sender, EventArgs e)
        {
            //update table back to 0
            logger.Log(LogLevel.Info, "BP01 ended");
            DBHelper.updateDataBySqlText(DBHelper.UPDATE_ISRUNNING_ORIG + "'BP01'");
            logger.Log(LogLevel.Info, "BP01 updated isrunning to 0");
        }
        void bp03_CompletedEvent(object sender, EventArgs e)
        {
            //update table back to 0
            logger.Log(LogLevel.Info, "BP03 ended");
            DBHelper.updateDataBySqlText(DBHelper.UPDATE_ISRUNNING_ORIG + "'BP03'");
            logger.Log(LogLevel.Info, "BP03 updated isrunning to 0");
        }
        void bp04_CompletedEvent(object sender, EventArgs e)
        {
            //update table back to 0
            logger.Log(LogLevel.Info, "BP04 ended");
            DBHelper.updateDataBySqlText(DBHelper.UPDATE_ISRUNNING_ORIG + "'BP04'");
            logger.Log(LogLevel.Info, "BP04 updated isrunning to 0");
        }
        void bp05_CompletedEvent(object sender, EventArgs e)
        {
            //update table back to 0
            logger.Log(LogLevel.Info, "BP05 ended");
            DBHelper.updateDataBySqlText(DBHelper.UPDATE_ISRUNNING_ORIG + "'BP05'");
            logger.Log(LogLevel.Info, "BP05 updated isrunning to 0");
        }
        void bp07_CompletedEvent(object sender, EventArgs e)
        {
            //update table back to 0
            //logger.Log(LogLevel.Info, "BP07 ended");
            logger.Log(LogLevel.Info, "BP07 ended");
            DBHelper.updateDataBySqlText(DBHelper.UPDATE_ISRUNNING_ORIG + "'BP07'");
            logger.Log(LogLevel.Info, "BP07 updated isrunning to 0");
        }
        void bp08_CompletedEvent(object sender, EventArgs e)
        {
            //update table back to 0
            logger.Log(LogLevel.Info, "BP08 ended");
            DBHelper.updateDataBySqlText(DBHelper.UPDATE_ISRUNNING_ORIG + "'BP08'");
            logger.Log(LogLevel.Info, "BP08 updated isrunning to 0");
        }
        public void EndCore()
        {
            logger.Log(LogLevel.Info, "Core End");
        }

        public DataTable getBatchStat()
        {
            string reTime = string.Empty;
            DataTable dt = DBHelper.getData(DBHelper.GET_ISNOTRUNNING);
            return dt;
        }

        
    }
}
