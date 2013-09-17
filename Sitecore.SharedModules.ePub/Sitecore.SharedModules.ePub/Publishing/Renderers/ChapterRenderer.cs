using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Sitecore;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Sitecore.SharedModules.ePub.Publishing.Renderers
{
    /// <summary>
    /// Renders Chapter item
    /// </summary>
    public class ChapterRenderer : EpubItemRendererBase
    {

        /// <summary>
        /// Renders the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="output">The output.</param>
        protected override void RenderContent(PrintContext printContext, object output, Epub.NavPoint parentEndPoint)
        {
            var xhtmlDocument = this.GenerateHtmlDocument(printContext);
            var ePubDocument = printContext.EpubDocument;
            string chapterTitle, ePubChapterPath;
            parentEndPoint = this.PopulateNavPoint(printContext, ePubDocument, parentEndPoint, out ePubChapterPath, out chapterTitle);

            this.RenderChildren(printContext, xhtmlDocument, parentEndPoint);
            if (DoesChapterHaveContent(xhtmlDocument))
            {                
                this.ProcessImages(ePubDocument, xhtmlDocument, printContext);
                this.ProcessStyleSheets(xhtmlDocument, printContext);
                printContext.PageNumber++;
                this.PopulateOutput(printContext, ePubDocument, xhtmlDocument, chapterTitle, ePubChapterPath);
            }
        }

        /// <summary>
        /// Begins the render.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        protected override void BeginRender(PrintContext printContext)
        {
            if (!string.IsNullOrEmpty(this.RenderingItem["Item Reference"]))
                this.DataSource = this.RenderingItem["Item Reference"];
            try
            {
                Item obj1 = printContext.Database.GetItem(this.DataSource);
                string query = this.RenderingItem["Item Selector"];
                if (string.IsNullOrEmpty(query))
                {
                    this.ItemSelector = query;
                    return;
                }
                var obj2 = obj1.Axes.SelectSingleItem(query);
                if (obj2 == null)
                    return;
                this.DataSource = obj2.ID.ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex, this);
            }
        }

        #region Private

        /// <summary>
        /// Does the chapter have content .
        /// </summary>
        /// <param name="xhtmlDocument">The XHTML html.</param>
        /// <returns></returns>
        private static bool DoesChapterHaveContent(HtmlDocument xhtmlDocument)
        {
            var test = xhtmlDocument.DocumentNode.SelectSingleNode("/html/body").ChildNodes.Where(n => !string.IsNullOrEmpty(n.InnerText));
            return xhtmlDocument.DocumentNode.SelectSingleNode("/html/body").ChildNodes.Any() && xhtmlDocument.DocumentNode.SelectSingleNode("/html/body").ChildNodes.Where(n => !string.IsNullOrEmpty(n.InnerText)).Any();
        }
        /// <summary>
        /// Populates the output.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="xhtmlDocument">The XHTML html.</param>
        private void PopulateOutput(PrintContext printContext, Epub.Document ePubDocument, HtmlDocument xhtmlDocument, string chapterTitle, string ePubChapterFile)
        {
            var result = ePubDocument.AddXhtmlData(this.RenderingItem["ePub Base Path"] + ePubChapterFile + ".html", xhtmlDocument.DocumentNode.OuterHtml);
        }

        /// <summary>
        /// Populates the nav point.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="ePubDocument">The decimal pub document.</param>
        /// <param name="xhtmlDocument">The XHTML document.</param>
        private Epub.NavPoint PopulateNavPoint(PrintContext printContext, Epub.Document ePubDocument, Epub.NavPoint parentEndPoint, out string ePubChapterFile, out string chapterTitle)
        {
            chapterTitle = string.Empty;
            ePubChapterFile = GetChapterTitle(printContext, out chapterTitle);
            if (parentEndPoint == null)
            {
                parentEndPoint = ePubDocument.AddNavPoint(chapterTitle, this.RenderingItem["ePub Base Path"] + ePubChapterFile + ".html", printContext.PageNumber);
            }
            else
            {
                parentEndPoint = parentEndPoint.AddNavPoint(chapterTitle, this.RenderingItem["ePub Base Path"] + ePubChapterFile + ".html", printContext.PageNumber);
            }
            return parentEndPoint;
        }

        /// <summary>
        /// Gets the chapter title.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="chapterTitle">The chapter title.</param>
        /// <returns></returns>
        private string GetChapterTitle(PrintContext printContext, out string chapterTitle)
        {
            chapterTitle = this.RenderingItem["Display Title"];
            var chapterTitleField = this.RenderingItem["Display Title Field"];

            var chapterFileName = string.Empty;

            if (string.IsNullOrEmpty(chapterTitle))
            {
                if (!string.IsNullOrEmpty(this.DataSource))
                {
                    var data = this.GetDataItem(printContext);
                    if (data != null)
                    {
                        chapterFileName = data.Name;
                        if (!string.IsNullOrEmpty(chapterTitleField))
                        {
                            chapterTitle = data[chapterTitleField];
                        }
                        else
                        {
                            chapterTitle = data.Name;
                        }
                    }
                }
                if (string.IsNullOrEmpty(chapterTitle))
                {
                    chapterTitle = this.RenderingItem.Name;
                    chapterFileName = chapterTitle;
                }
            }
            return String.Concat(chapterFileName, "_", printContext.PageNumber).Replace(" ", "_");
        }

        /// <summary>
        /// Generates the HTML html.
        /// </summary>
        /// <returns></returns>
        public virtual HtmlDocument GenerateHtmlDocument(PrintContext printContext)
        {
            var xhtmlDocument = new HtmlDocument();
            try
            {
                if (this.RenderingItem.Fields["Format"] != null && ((ReferenceField)this.RenderingItem.Fields["Format"]).TargetItem != null)
                {
                    this.OutputFormat = ((ReferenceField)this.RenderingItem.Fields["Format"]).TargetItem["Xhtml Format Code"];
                }

                if (!string.IsNullOrEmpty(this.OutputFormat))
                {
                    xhtmlDocument.LoadHtml(this.OutputFormat);
                }
                else
                {
                    xhtmlDocument.LoadHtml("<html><head></head><body></body></html>");
                }

                // make sure img tags are closed
                if (HtmlNode.ElementsFlags.ContainsKey("img"))
                {
                    HtmlNode.ElementsFlags["img"] = HtmlElementFlag.Closed;
                }
                else
                {
                    HtmlNode.ElementsFlags.Add("img", HtmlElementFlag.Closed);
                }

                // make sure link tags are closed
                if (HtmlNode.ElementsFlags.ContainsKey("link"))
                {
                    HtmlNode.ElementsFlags["link"] = HtmlElementFlag.Closed;
                }
                else
                {
                    HtmlNode.ElementsFlags.Add("link", HtmlElementFlag.Closed);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex, this);
            }
            return xhtmlDocument;
        }

        /// <summary>
        /// Processes the images.
        /// </summary>
        /// <param name="ePubDocument">The e pub html.</param>
        /// <param name="xhtmlDocument">The XHTML html.</param>
        public virtual void ProcessImages(Epub.Document ePubDocument, HtmlDocument xhtmlDocument, PrintContext printContext)
        {
            //check for inline images and add them to ePub Document if there are any
            foreach (var pair in printContext.Images)
            {
                ePubDocument.AddImageData(pair.Key, ReadFully(pair.Value));
            }
            printContext.Images.Clear();
        }

        /// <summary>
        /// Processes the style sheets.
        /// </summary>
        /// <param name="xhtmlDocument">The XHTML html.</param>
        public virtual void ProcessStyleSheets(HtmlDocument xhtmlDocument, PrintContext printContext)
        {
            //check for inline images and add them to ePub Document if there are any
            foreach (var pair in printContext.StyleSheets)
            {
                if (!string.IsNullOrEmpty(pair.Value))
                {
                    if (pair.Value.Contains(".css"))
                    {
                        HtmlNode styleSheet = RenderItemHelper.CreateHtmlNode("link", xhtmlDocument);
                        styleSheet.Attributes.Add("href", pair.Value);
                        styleSheet.Attributes.Add("rel", "stylesheet");
                        styleSheet.Attributes.Add("type", "text/css");
                       
                        xhtmlDocument.DocumentNode.SelectSingleNode("/html/head").AppendChild(styleSheet);
                    }                    
                }
            }
        }

        #endregion        
    }
}
