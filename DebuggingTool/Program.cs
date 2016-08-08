using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace DebuggingTool
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {

            logger.Log(LogLevel.Info, "Debug Program Started");
            try
            {
                CoreProcess.Core cp = new CoreProcess.Core();
                cp.StartCore();
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }
    }
}
