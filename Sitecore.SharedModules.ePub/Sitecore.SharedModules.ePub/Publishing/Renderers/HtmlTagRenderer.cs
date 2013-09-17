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
    public class HtmlTagRenderer : ContentRenderer
    {        
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
                string dataSource = string.Empty;
                var dataItem = this.GetDataItem(printContext);
                if (dataItem != null)
                {
                    dataSource = dataItem.ID.ToString();
                }
                if (!string.IsNullOrEmpty(this.RenderingItem["Item Reference"]) && dataSource == null)
                {
                    dataSource = this.RenderingItem["Item Reference"];
                }
                if (!string.IsNullOrEmpty(dataSource))
                {
                    this.ContentItem = printContext.Database.GetItem(dataSource);

                    var xpath = this.RenderingItem["Item Selector"];
                    if (!string.IsNullOrEmpty(xpath))
                    {
                        Item selectorDataItem = this.ContentItem.Axes.SelectSingleItem(xpath);
                        if (selectorDataItem != null)
                        {
                            this.ContentItem = selectorDataItem;
                        }
                    }
                }
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
        /// Parses the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <returns></returns>
        protected override string ParseContent(PrintContext printContext)
        {
            Assert.IsNotNull(this.ContentItem, "Content item is null");
            if (string.IsNullOrEmpty(this.ContentFieldName))
            {
                return string.Empty;
            }
            Field contentField = this.ContentItem.Fields[this.ContentFieldName];
            if (contentField == null)
            {
                return string.Empty;
            }

            CustomField field = FieldTypeManager.GetField(contentField);
            if (field == null)
            {
                return TransformationManager.ReplaceInvalidCharacters(contentField.Value, printContext.Settings.TransformationItem);
            }
            if (field is DateField)
            {
                return ((DateField)field).DateTime.ToString("MMMM dd, yyyy");
            }
            else if (field is HtmlField)
            {
                return TransformationManager.TransformHtml(field.Value, printContext.Settings.TransformationItem);
            }
            return TransformationManager.ReplaceInvalidCharacters(field.Value, printContext.Settings.TransformationItem);
        }

        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        /// <returns>The field name</returns>
        protected override string GetFieldName()
        {
            return this.RenderingItem["Item Field"];
        }
    }
}
