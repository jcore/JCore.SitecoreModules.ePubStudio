using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.SharedModules.ePub.Publishing.Renderers
{
    /// <summary>
    /// Renderer for static content
    /// </summary>
    public class ContentRenderer : EpubItemRendererBase
    {
        private Item contentItem;
        private string contentFieldName;

        /// <summary>
        /// Static Text Field Name
        /// </summary>
        private const string StaticTextFieldName = "Content";

        /// <summary>
        /// Gets or sets the name of the CSS class.
        /// </summary>
        /// <value>
        /// The name of the CSS class.
        /// </value>
        public virtual string CssClass { get; set; }

        /// <summary>
        /// Gets or sets the CSS inline style.
        /// </summary>
        /// <value>
        /// The CSS inline style.
        /// </value>
        public virtual string CssInlineStyle { get; set; }


        /// <summary>
        /// Gets or sets the content item.
        /// </summary>
        /// <value>
        /// The internal data.
        /// </value>
        protected virtual Item ContentItem
        {
            get
            {
                return this.contentItem ?? (this.contentItem = this.RenderingItem);
            }

            set
            {
                this.contentItem = value;
            }
        }

        /// <summary>
        /// Renders the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="output">The output.</param>
        protected override void RenderContent(PrintContext printContext, object output, Epub.NavPoint parentEndPoint)
        {
            if (string.IsNullOrEmpty(this.Tag))
            {
                this.RenderChildren(printContext, output, parentEndPoint);
            }
            else
            {                
                if (output is HtmlDocument)
                {
                    var elem = (HtmlDocument)output;
                    HtmlNode element = RenderItemHelper.CreateHtmlNode(this.Tag, elem);
                    element.InnerHtml = this.ParseContent(printContext);
                    this.RenderStyling(element);
                    elem.DocumentNode.SelectSingleNode("/html/body").AppendChild(element);
                    this.RenderChildren(printContext, element, parentEndPoint);
                }
                else if (output is HtmlNode)
                {
                    var elem = (HtmlNode)output;
                    HtmlNode element = RenderItemHelper.CreateHtmlNode(this.Tag, elem);
                    element.InnerHtml = this.ParseContent(printContext);
                    this.RenderStyling(element);
                    elem.AppendChild(element);
                    this.RenderChildren(printContext, element, parentEndPoint);
                }
            }
        }

        /// <summary>
        /// Begins the render.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        protected override void BeginRender(PrintContext printContext)
        {
            var dataItem = this.GetDataItem(printContext);
            string query = this.RenderingItem["Item Selector"];
            if (string.IsNullOrEmpty(query))
            {
                return;
            }
            var obj2 = dataItem.Axes.SelectSingleItem(query);
            if (obj2 == null)
                return;
            this.DataSource = obj2.ID.ToString();
        }

        /// <summary>
        /// Parses the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <returns></returns>
        protected virtual string ParseContent(PrintContext printContext)
        {
            Assert.IsNotNull(this.ContentItem, "Content item is null");
            Assert.IsNotNullOrEmpty(this.ContentFieldName, "Missing content field");

            Field contentField = this.ContentItem.Fields[this.ContentFieldName];
            if (contentField == null)
            {
                return string.Empty;
            }

            CustomField field = FieldTypeManager.GetField(contentField);
            if (field == null)
            {
                return contentField.Value;
            }
            if (field is DateField)
            {
                return ((DateField)field).DateTime.ToString("MMMM dd, yyyy");
            }

            return field.Value;
        }

        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        /// <returns>The field name</returns>
        protected virtual string GetFieldName()
        {
            return StaticTextFieldName;
        }

        /// <summary>
        /// Gets or sets the name of the content field.
        /// </summary>
        /// <value>
        /// The name of the content field.
        /// </value>
        public string ContentFieldName
        {
            get
            {
                if (string.IsNullOrEmpty(this.contentFieldName))
                {
                    this.contentFieldName = this.GetFieldName();
                }

                return this.contentFieldName;
            }

            set
            {
                this.contentFieldName = value;
            }
        }


        /// <summary>
        /// Renders the styling.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual void RenderStyling(HtmlNode element)
        {
            if (element == null)
            {
                return;
            }
            if (!string.IsNullOrEmpty(this.CssClass))
            {
                element.Attributes.Add("class", this.CssClass);
            }
            if (!string.IsNullOrEmpty(this.CssInlineStyle))
            {
                element.Attributes.Add("style", this.CssInlineStyle);
            }
            if (!string.IsNullOrEmpty(this.RenderingItem["Css Classes"]))
            {
                var cssClassItems = ((MultilistField)this.RenderingItem.Fields["Css Classes"]).GetItems();

                var classes = cssClassItems.Where(c => !string.IsNullOrEmpty(c["Class Name"])).Select(c => c["Class Name"]).ToArray();
                if (classes.Any())
                {
                    element.Attributes.Add("class", string.Join(" ", classes));
                }

                var styles = cssClassItems.Where(c => !string.IsNullOrEmpty(c["Inline Css"])).Select(c => c["Inline Css"]).ToArray();
                if (styles.Any())
                {
                    element.Attributes.Add("style", string.Join("; ", styles));
                }
            }
        }        

    }
}
