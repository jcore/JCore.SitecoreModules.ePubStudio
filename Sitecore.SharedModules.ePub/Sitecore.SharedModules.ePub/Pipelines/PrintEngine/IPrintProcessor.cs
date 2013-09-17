using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.SharedModules.ePub.Pipelines.PrintEngine
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPrintProcessor
    {
        void Process(PrintPipelineArgs args);
    }
}
