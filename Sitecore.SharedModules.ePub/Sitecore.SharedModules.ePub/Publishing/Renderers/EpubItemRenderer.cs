using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using HtmlAgilityPack;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;

namespace Sitecore.SharedModules.ePub.Publishing.Renderers
{
    /// <summary>
    /// 
    /// </summary>
    public class EpubItemRenderer : EpubItemRendererBase
    {
        /// <summary>
        /// Gets or sets the print context.
        /// </summary>
        /// <value>
        /// The print context.
        /// </value>
        public PrintContext PrintContext { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EpubItemRenderer"/> class.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        public EpubItemRenderer(PrintContext printContext)
        {
            this.PrintContext = printContext;
        }

        /// <summary>
        /// Renders the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="output">The output.</param>
        protected override void RenderContent(PrintContext printContext, object output, Epub.NavPoint parentEndPoint)
        {
            if (TemplateManager.IsTemplate(this.RenderingItem))
            {
                Item standardValues = ((TemplateItem)this.RenderingItem).StandardValues;
                if (standardValues == null)
                    return;
                if (printContext.StartItem == this.RenderingItem)
                    printContext.StartItem = standardValues;
                this.RenderChild(printContext, output, standardValues, parentEndPoint);
            }
            else
                this.RenderChild(printContext, output, this.RenderingItem, parentEndPoint);
        }

        /// <summary>
        /// Renders the epub document.
        /// </summary>
        /// <returns></returns>
        public Epub.Document RenderEpubDocument(Epub.NavPoint parentEndPoint)
        {
            Epub.Document output = new Epub.Document();
            this.Render(this.PrintContext, output, parentEndPoint);
            return output;
        }

    }
}
