using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using HtmlAgilityPack;
using Sitecore;
using Sitecore.Data.Items;

namespace Sitecore.SharedModules.ePub.Publishing.Renderers
{
    /// <summary>
    /// Base class for all ePub renderers
    /// </summary>
    public abstract class EpubItemRendererBase
    {
        public virtual string Tag { get; set; }
        public bool ShowId { get; set; }
        public Item RenderingItem { get; set; }
        public string DataSource { get; set; }
        public string ItemSelector { get; set; }
        public bool RenderDeep { get; set; }
        public string OutputFormat { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="EpubItemRendererBase"/> class.
        /// </summary>
        protected EpubItemRendererBase()
        {
            this.DataSource = string.Empty;           
        }

        /// <summary>
        /// Renders the specified print context.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="output">The output.</param>
        public void Render(PrintContext printContext, object output, Epub.NavPoint parentEndPoint)
        {            
            if (this.RenderingItem == null)
                this.RenderingItem = printContext.StartItem;            
            this.BeginRender(printContext);
            this.RenderContent(printContext, output, parentEndPoint);
        }

        /// <summary>
        /// Gets the data item.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <returns></returns>
        protected virtual Item GetDataItem(PrintContext printContext)
        {
            Item obj = (Item)null;
            string dataSource = this.DataSource;
            if (dataSource.Length > 0)
                obj = !MainUtil.IsFullPath(dataSource) ? this.RenderingItem.Axes.GetItem(dataSource) : printContext.Database.GetItem(dataSource);
            return obj;
        }

        /// <summary>
        /// Begins the render.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        protected virtual void BeginRender(PrintContext printContext)
        {
        }

        /// <summary>
        /// Renders the children.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="parentContainer">The parent container.</param>
        protected virtual void RenderChildren(PrintContext printContext, object parentContainer, Epub.NavPoint parentEndPoint)
        {
            this.RenderChildren(printContext, parentContainer, (IEnumerable<Item>)this.RenderingItem.Children, parentEndPoint);
        }

        /// <summary>
        /// Renders the children.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="parentContainer">The parent container.</param>
        /// <param name="children">The children.</param>
        protected virtual void RenderChildren(PrintContext printContext, object parentContainer, IEnumerable<Item> children, Epub.NavPoint parentEndPoint)
        {
            if (children == null || this.RenderingItem == null || !this.RenderDeep)
                return;
            int idx = printContext.CurrentItemAncestors.IndexOf(this.RenderingItem.ID);
            if (printContext.CurrentItemAncestors.Count > 0 && idx > 0)
            {
                Item renderingItem = Enumerable.FirstOrDefault<Item>(children, (Func<Item, bool>)(s => s.ID == printContext.CurrentItemAncestors[idx - 1]));
                this.RenderChild(printContext, parentContainer, renderingItem, parentEndPoint);
            }
            else
            {
                foreach (Item renderingItem in children)
                    this.RenderChild(printContext, parentContainer, renderingItem, parentEndPoint);
            }
        }

        /// <summary>
        /// Renders the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="output">The output.</param>
        protected abstract void RenderContent(PrintContext printContext, object output, Epub.NavPoint parentEndPoint);

        /// <summary>
        /// Renders the child.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="parentContainer">The parent container.</param>
        /// <param name="renderingItem">The rendering item.</param>
        protected void RenderChild(PrintContext printContext, object parentContainer, Item renderingItem, Epub.NavPoint parentEndPoint)
        {
            if (renderingItem == null)
                return;
            EpubItemRendererBase renderer = PrintFactory.GetRenderer(renderingItem);
            if (renderer == null)
                return;
            renderer.RenderDeep = this.RenderDeep;
            renderer.DataSource = this.DataSource;
            renderer.Render(printContext, parentContainer, parentEndPoint);
        }


        /// <summary>
        /// Reads Stream into byte[]
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        protected static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        } 
    }
}
