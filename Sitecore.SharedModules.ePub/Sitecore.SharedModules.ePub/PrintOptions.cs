using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.SharedModules.ePub.Configuration;

namespace Sitecore.SharedModules.ePub
{
    public class PrintOptions
    {
        private string rootCacheFolder = string.Empty;
        private string resultFolder = string.Empty;
        private string resultFileName = string.Empty;
        private string projectCacheFolder;
        private string resultFileExtension = ".epub";
        private Item transformationItem = null;

        /// <summary>
        /// Gets or sets a value indicating whether [override cache].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [override cache]; otherwise, <c>false</c>.
        /// </value>
        public bool OverrideCache { get; set; }

        /// <summary>
        /// Gets or sets the root cache folder.
        /// </summary>
        /// <value>
        /// The root cache folder.
        /// </value>
        public string RootCacheFolder
        {
            get
            {
                if (string.IsNullOrEmpty(this.rootCacheFolder))
                    this.rootCacheFolder = ConfigHandler.CommonSettings.PublishingCachePath;
                return this.rootCacheFolder;
            }
            set
            {
                Assert.ArgumentCondition(string.IsNullOrEmpty(value) || Path.IsPathRooted(value), "RootCacheFolder", string.Format("PrintOptions.RootCacheFolder '{0}' should be an absolute folder path.", (object)value));
                this.rootCacheFolder = value;
            }
        }

        /// <summary>
        /// Gets or sets the project cache folder.
        /// </summary>
        /// <value>
        /// The project cache folder.
        /// </value>
        public string ProjectCacheFolder
        {
            get
            {
                return this.projectCacheFolder;
            }
            set
            {
                Assert.ArgumentCondition(!Path.IsPathRooted(value), "ProjectCacheFolder", string.Format("PrintOptions.ProjectCacheFolder '{0}' should be a relative folder path.", (object)value));
                this.projectCacheFolder = value;
            }
        }

        /// <summary>
        /// Gets the cache folder.
        /// </summary>
        /// <value>
        /// The cache folder.
        /// </value>
        public string CacheFolder
        {
            get
            {
                return Path.Combine(this.RootCacheFolder, this.ProjectCacheFolder ?? string.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the result folder.
        /// </summary>
        /// <value>
        /// The result folder.
        /// </value>
        public string ResultFolder
        {
            get
            {
                if (string.IsNullOrEmpty(this.resultFolder))
                    this.resultFolder = this.CacheFolder;
                return this.resultFolder;
            }
            set
            {
                this.resultFolder = value;
            }
        }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public SafeDictionary<string, object> Parameters { get; set; }

        /// <summary>
        /// Gets or sets the name of the result file.
        /// </summary>
        /// <value>
        /// The name of the result file.
        /// </value>
        public string ResultFileName
        {
            get
            {
                if (!string.IsNullOrEmpty(this.resultFileName) && !this.resultFileName.Contains("."))
                    return this.resultFileName + this.resultFileExtension;
                else
                    return this.resultFileName;
            }
            set
            {
                Assert.IsNotNullOrEmpty(value, "Result file name cannot be null.");
                if (FileUtil.IsFullyQualified(value))
                {
                    this.resultFileName = FileUtil.GetFileName(value);
                    this.ResultFolder = FileUtil.GetParentPath(value);
                }
                else
                    this.resultFileName = value;
            }
        }

        /// <summary>
        /// Gets or sets the transformation item.
        /// </summary>
        /// <value>
        /// The transformation item.
        /// </value>
        public Item TransformationItem { get; set; }        

        /// <summary>
        /// Initializes a new instance of the <see cref="PrintOptions"/> class.
        /// </summary>
        public PrintOptions()
        {
            this.Parameters = new SafeDictionary<string, object>();
        }

        ///// <summary>
        ///// Gets the default formatting item.
        ///// </summary>
        ///// <param name="database">The database.</param>
        ///// <returns></returns>
        //public Item GetDefaultFormattingItem(Database database)
        //{
        //    if (this.defaultFormatting == null || this.defaultFormatting.Database.Name != database.Name)
        //    {
        //        string path = database.GetItem(ConfigHandler.EpubPrintEngineSettings.DefaultSettings)["Default transformation"];
        //        if (!string.IsNullOrEmpty(path))
        //            this.defaultFormatting = database.GetItem(path);
        //    }
        //    return this.defaultFormatting;
        //}

        public object ResultExtension { get { return this.resultFileExtension;  } }
    }
}
