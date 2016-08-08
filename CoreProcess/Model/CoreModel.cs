using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreProcess.Model
{
    public class CoreModel
    {
        public string ModuleName { get; set; }
        public bool IsToRun { get; set; }
        public DateTime LastProcessedDate { get; set; }
    }
}
