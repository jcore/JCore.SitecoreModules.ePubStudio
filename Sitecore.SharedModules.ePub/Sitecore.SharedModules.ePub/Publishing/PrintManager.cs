using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Globalization;
using Sitecore.Pipelines;
using Sitecore.SharedModules.ePub.Pipelines.PrintEngine;

namespace Sitecore.SharedModules.ePub.Publishing
{
    /// <summary>
    /// 
    /// </summary>
    public class PrintManager
    {
        private readonly Database database;
        private readonly Language language;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrintManager"/> class.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="language">The language.</param>
        public PrintManager(string database, string language)
        {
            this.database = Factory.GetDatabase(database);
            this.language = LanguageManager.GetLanguage(language, this.database);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrintManager"/> class.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="language">The language.</param>
        public PrintManager(Database database, Language language)
        {
            this.database = database;
            this.language = language;
        }

        /// <summary>
        /// Prints the specified item id.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        /// <param name="printOptions">The print options.</param>
        /// <returns></returns>
        public string Print(string itemId, PrintOptions printOptions)
        {
            using (new LanguageSwitcher(this.language))
            {
                Item printItem = this.database.GetItem(ID.Parse(itemId));
                if (printItem == null)
                    return (string)null;
                else
                    return this.RunPipeline("ePubPrint", new PrintPipelineArgs(printItem, printOptions));
            }
        }

        /// <summary>
        /// Runs the pipeline.
        /// </summary>
        /// <param name="pipelineName">Name of the pipeline.</param>
        /// <param name="pipelineArgs">The pipeline args.</param>
        /// <returns></returns>
        private string RunPipeline(string pipelineName, PrintPipelineArgs pipelineArgs)
        {
            CorePipeline.Run(pipelineName, (PipelineArgs)pipelineArgs);
            if (!string.IsNullOrEmpty(pipelineArgs.Message))
                return pipelineArgs.Message;
            else
                return pipelineArgs.EpubResultFile;
        }
    }
}
