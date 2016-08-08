using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using CoreProcess.Model;
using CoreProcess;
using System.IO;
using NLog;

namespace BatchProcess
{
    public partial class CanonBatchProcess : ServiceBase
    {
        private System.Timers.Timer _timer;
        public List<string> logStr = new List<string>();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public CanonBatchProcess()
        {
            InitializeComponent();
        }

        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timer.Stop();
            try
            {
                cp.StartCore();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            finally
            {
                _timer.Start();
            }
        }

        CoreProcess.Core cp = new CoreProcess.Core();

        protected override void OnStart(string[] args)
        {
          
            logger.Log(LogLevel.Info, "Batch process started on" + DateTime.Now.ToString()); 
  
            //2. Create Log that Service has started
            cp.StartCore();

            //_timer = new System.Timers.Timer(60 * 60 * 1000);on Prod setup
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        protected override void OnStop()
        {
            // kill the timer.
            _timer.Stop();
            _timer.Dispose();
            //1. Create Log that Service has Ended
            cp.EndCore();

            logger.Log(LogLevel.Info, "Batch process stopped on" + DateTime.Now.ToString()); 
        }

      
    }
}
