using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.Items;
using Sitecore.Pipelines;

namespace Sitecore.SharedModules.ePub.Pipelines.PrintEngine
{
    /// <summary>
    /// 
    /// </summary>
    public class PrintPipelineArgs : PipelineArgs
    {
        /// <summary>
        /// The print options
        /// </summary>
        private readonly PrintOptions printOptions;
        private string epubResultFile;

        /// <summary>
        /// Gets or sets the print job id.
        /// </summary>
        /// <value>
        /// The print job id.
        /// </value>
        public string PrintJobId { get; set; }

        /// <summary>
        /// Gets the print options.
        /// </summary>
        /// <value>
        /// The print options.
        /// </value>
        public PrintOptions PrintOptions
        {
            get
            {
                return this.printOptions;
            }
        }

        /// <summary>
        /// Gets the result file.
        /// </summary>
        /// <value>
        /// The result file.
        /// </value>
        public string ResultFile
        {
            get
            {
                return Path.Combine(this.printOptions.ResultFolder, this.printOptions.ResultFileName);
            }
        }

        /// <summary>
        /// Gets or sets the epub result file.
        /// </summary>
        /// <value>
        /// The epub result file.
        /// </value>
        public string EpubResultFile
        {
            get
            {
                if (epubResultFile == null)
                {
                    epubResultFile = ResultFile;
                }
                return epubResultFile;
            }
            set
            {
                epubResultFile = value;
            }
        }        

        public bool RenderPartial { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrintPipelineArgs"/> class.
        /// </summary>
        /// <param name="printItem">The print item.</param>
        /// <param name="printOptions">The print options.</param>
        public PrintPipelineArgs(Item printItem, PrintOptions printOptions)
        {
            this.ProcessorItem = (ProcessorItem)printItem;
            this.printOptions = printOptions;
        }
    }
}
