using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;
using Sitecore.Web;
using Sitecore.SharedModules.ePub.Publishing;
using Sitecore.SharedModules.ePub.Publishing.Renderers;

namespace Sitecore.SharedModules.ePub.Pipelines.PrintEngine
{
    /// <summary>
    /// 
    /// </summary>
    public class RenderProject : IPrintProcessor
    {
        /// <summary>
        /// Processes the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
        public void Process(PrintPipelineArgs args)
        {
            if (args.PrintOptions == null)
            {
                args.AddMessage("PrintOptions cannot be null", PipelineMessageType.Error);
                args.AbortPipeline();
            }
            else 
            {
                
                List<ID> ancestorsList = new List<ID>();
                Item obj1 = args.ProcessorItem.InnerItem;
                if (!args.RenderPartial)
                {
                    Item obj2 = SitecoreHelper.LookupProject(args.ProcessorItem.InnerItem, out ancestorsList);
                    if (obj2 != null)
                        obj1 = obj2;
                }
                string sessionId = WebUtil.GetSessionID();
                string str = Context.User.Name + (!string.IsNullOrEmpty(sessionId) ? "_" + sessionId.Substring(0, 8) : string.Empty) + "\\";
                if (args.PrintOptions.ProjectCacheFolder == null)
                    args.PrintOptions.ProjectCacheFolder = str + Context.Language.Name + "\\" + obj1.ID.ToGuid().ToString() + "\\";
                try
                {
                    if (!Directory.Exists(args.PrintOptions.CacheFolder))
                        Directory.CreateDirectory(args.PrintOptions.CacheFolder);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message,ex, this);
                    args.AddMessage("Error creating the Cache folder: " + ex.Message, PipelineMessageType.Error);
                    args.AbortPipeline();
                    return;
                }

                SetTransformations(args);

                if (string.IsNullOrEmpty(args.PrintOptions.ResultFileName))
                    args.PrintOptions.ResultFileName = string.Concat(new object[4]
          {
            (object) args.ProcessorItem.ID.Guid,
            (object) "_",
            (object) DateTime.Now.Ticks,
            (object) args.PrintOptions.ResultExtension
          });
                EpubItemRenderer designItemRenderer = new EpubItemRenderer(new PrintContext(args.ProcessorItem.InnerItem, args.PrintOptions)
                {
                    CurrentItemAncestors = ancestorsList,
                    Database = args.ProcessorItem.Database
                });
                designItemRenderer.RenderDeep = true;
                designItemRenderer.RenderingItem = obj1;
                Epub.Document epubDocument = designItemRenderer.RenderEpubDocument(null);
                if (epubDocument == null)
                    return;
                if (!this.HasResultFileName(args))
                    args.EpubResultFile = Path.Combine(args.PrintOptions.CacheFolder, string.Concat(new object[4]
          {
            (object) obj1.ID.ToGuid(),
            (object) "_",
            (object) DateTime.Now.Ticks,
            (object) args.PrintOptions.ResultExtension
          }));
                RenderItemHelper.OutputToFile(args.EpubResultFile, epubDocument);
            }
        }

        /// <summary>
        /// Sets the transformations.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void SetTransformations(PrintPipelineArgs args)
        {
            if (args.PrintOptions.TransformationItem == null)
            {
                ReferenceField fld = args.ProcessorItem.InnerItem.Fields["Transformations"];
                if (fld != null && fld.TargetItem != null)
                {
                    args.PrintOptions.TransformationItem = fld.TargetItem;
                }
            }
        }

        /// <summary>
        /// Determines whether [has result file name] [the specified args].
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns>
        ///   <c>true</c> if [has result file name] [the specified args]; otherwise, <c>false</c>.
        /// </returns>
        private bool HasResultFileName(PrintPipelineArgs args)
        {
            if (string.IsNullOrEmpty(Path.GetFileName(args.EpubResultFile)))
                return false;
            try
            {
                return !string.IsNullOrEmpty(args.EpubResultFile) && !string.IsNullOrEmpty(Path.GetDirectoryName(args.EpubResultFile));
            }
            catch
            {
                return false;
            }
        }
    }
}
