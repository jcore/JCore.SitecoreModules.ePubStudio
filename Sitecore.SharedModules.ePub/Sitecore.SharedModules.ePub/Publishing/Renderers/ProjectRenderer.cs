using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;

namespace Sitecore.SharedModules.ePub.Publishing.Renderers
{
    /// <summary>
    /// Renderer responsible for processing epub project item
    /// </summary>
    public class ProjectRenderer : EpubItemRendererBase
    {
        /// <summary>
        /// Renders the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="output">The output.</param>
        protected override void RenderContent(PrintContext printContext, object output, Epub.NavPoint parentEndPoint)
        {
            if (output == null)
            {
                return;
            }
            var ePubDocument = (Epub.Document)output;
            ePubDocument.AddTitle(this.RenderingItem["Name"]);
            ePubDocument.AddDescription(this.RenderingItem["Description"]);
            ePubDocument.AddAuthor(this.RenderingItem["Author"]);

            ePubDocument.AddLanguage(this.RenderingItem["Language"]);
            ePubDocument.AddBookIdentifier(this.RenderingItem["Book Identifier"]);
            ePubDocument.AddRights(this.RenderingItem["Rights"]);

            ProcessFonts(printContext, ePubDocument);
            ProcessCssStyles(printContext, ePubDocument);

            printContext.EpubDocument = ePubDocument;

            this.RenderChildren(printContext, ePubDocument, parentEndPoint);
        }

        /// <summary>
        /// Processes the CSS styles.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="ePubDocument">The e pub html.</param>
        private void ProcessCssStyles(PrintContext printContext, Epub.Document ePubDocument)
        {
            //add style sheets - needs to be revisited. link tags are not being closed
            foreach (var styleId in ((MultilistField)this.RenderingItem.Fields["StyleSheets"]).TargetIDs)
            {
                var styleSheet = printContext.Database.GetItem(styleId);
                if (styleSheet != null)
                {
                    if (!string.IsNullOrEmpty(styleSheet["Custom Css"]))
                    {
                        var filePath = String.Concat("css/", styleSheet.Name, ".css");
                        ePubDocument.AddStylesheetData(filePath, styleSheet["Custom Css"]);
                        printContext.StyleSheets.Add(filePath, String.Concat("../", filePath));
                    }
                    if (!string.IsNullOrEmpty(styleSheet["MediaLibrary Reference"]))
                    {
                        var cssMediaItem = (MediaItem)((FileField)styleSheet.Fields["MediaLibrary Reference"]).MediaItem;
                        if (cssMediaItem != null)
                        {
                            var filePath = String.Concat("css/", cssMediaItem.Name, ".", cssMediaItem.Extension);
                            ePubDocument.AddData(filePath, ReadFully(cssMediaItem.GetMediaStream()), "text/css");
                            printContext.StyleSheets.Add(filePath, String.Concat("../", filePath));
                        }
                    }
                }
            }
            
        }

        /// <summary>
        /// Processes the fonts.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="ePubDocument">The e pub html.</param>
        private void ProcessFonts(PrintContext printContext, Epub.Document ePubDocument)
        {
            // add fonts
            foreach (var fontId in ((MultilistField)this.RenderingItem.Fields["Fonts"]).TargetIDs)
            {
                var font = printContext.Database.GetItem(fontId);
                if (font != null)
                {
                    if (font.Fields["MediaLibrary Reference"] != null)
                    {
                        var fontItem = (MediaItem)((FileField)font.Fields["MediaLibrary Reference"]).MediaItem;
                        ePubDocument.AddData(String.Concat(font["ePub Base Path"], fontItem.Name, ".", fontItem.Extension), ReadFully(fontItem.GetMediaStream()), "application/octet-stream");
                    }
                    else
                    {
                        ePubDocument.AddFile(font["ePub Base Path"] + font["File Name"], font["ePub Base Path"] + font["File Name"], "application/octet-stream");
                    }
                }
            }
        }

        /// <summary>
        /// Begins the render.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        protected override void BeginRender(PrintContext printContext)
        {
            if (!string.IsNullOrEmpty(this.RenderingItem["Output File Name"]))
            {
                printContext.Settings.ResultFileName = this.RenderingItem["Output File Name"];
            }

            if (!string.IsNullOrEmpty(this.RenderingItem["Item Reference"]))
                this.DataSource = this.RenderingItem["Item Reference"];

            printContext.PageNumber = 0;
            printContext.Images = new Dictionary<string, Stream>();
            printContext.StyleSheets = new Dictionary<string, string>();
        }
    }
}
