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
    public class CoverPageRenderer : ChapterRenderer
    {
        /// <summary>
        /// Renders the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="output">The output.</param>
        protected override void RenderContent(PrintContext printContext, object output, Epub.NavPoint parentEndPoint)
        {
            var xhtmlDocument = this.GenerateHtmlDocument(printContext);
            this.RenderChildren(printContext, xhtmlDocument, parentEndPoint);
            if (DoesChapterHaveContent(xhtmlDocument))
            {
                var ePubDocument = (Epub.Document)output;
                this.ProcessImages(ePubDocument, xhtmlDocument, printContext);
                this.ProcessStyleSheets(xhtmlDocument, printContext);
                this.PopulateOutput(printContext, ePubDocument, xhtmlDocument);
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
        private void PopulateOutput(PrintContext printContext, Epub.Document ePubDocument, HtmlDocument xhtmlDocument)
        {            
            var ePubChapterFile = "cover";
            var result = ePubDocument.AddXhtmlData(this.RenderingItem["ePub Base Path"] + ePubChapterFile + ".html", xhtmlDocument.DocumentNode.OuterHtml, false);
            ePubDocument.AddMetaItem("cover", "cover.html");
            if (printContext.Images.Any())
            {
                ePubDocument.AddMetaItem("cover-image", printContext.Images.FirstOrDefault().Key);
            } 
        }

        #endregion        
    }
}
