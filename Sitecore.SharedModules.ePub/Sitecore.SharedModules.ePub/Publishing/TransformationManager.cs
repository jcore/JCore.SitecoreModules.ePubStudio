using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Sitecore.SharedModules.ePub.Publishing
{
    /// <summary>
    /// Class responsible for html transformations done for ePub documents
    /// </summary>
    public static class TransformationManager
    {
        /// <summary>
        /// Transforms rich text.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="transformationItem">The transformation item.</param>
        /// <returns>HtmlDocument with transformed html</returns>
        public static string TransformHtml(string html, Item transformationItem)
        {
            html = ReplaceInvalidCharacters(html, transformationItem);
            foreach (Sitecore.Data.Items.Item elementItem in transformationItem.Children)
            {
                var pattern = new StringBuilder("//");
                if (!string.IsNullOrEmpty(elementItem["Original Tag"]))
                {
                    pattern.Append(elementItem["Original Tag"]);
                }

                if (!string.IsNullOrEmpty(elementItem["Original Attribute Name"]))
                {
                    var attributeValue = elementItem["Original Attribute Name"];
                    if (string.IsNullOrEmpty(elementItem["Original Attribute Name"]))
                    {
                        attributeValue = "*";
                    }
                    pattern.AppendFormat("[@{0}='{1}']", elementItem["Original Attribute Name"], attributeValue);
                }
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                ProcessHtmlDocument(htmlDocument, elementItem, pattern);
                html = htmlDocument.DocumentNode.InnerHtml;
            }
            return html;
        }

        /// <summary>
        /// Processes the HTML html.
        /// </summary>
        /// <param name="html">The html.</param>
        /// <param name="elementItem">The element item.</param>
        /// <param name="pattern">The pattern.</param>
        private static void ProcessHtmlDocument(HtmlDocument document, Sitecore.Data.Items.Item elementItem, StringBuilder pattern)
        {
            var htmlElements = document.DocumentNode.SelectNodes(pattern.ToString());
            if (htmlElements != null && htmlElements.Any())
            {
                foreach (var element in htmlElements)
                {
                    if (!elementItem["Original Tag"].Equals(elementItem["Transformed Tag"]))
                    {
                        element.Name = elementItem["Transformed Tag"];
                    }

                    if (!string.IsNullOrEmpty(elementItem["Original Attribute Name"]))
                    {
                        element.Attributes.Remove(elementItem["Original Attribute Name"]);
                    }

                    foreach (var attributeItem in elementItem.Axes.GetDescendants())
                    {
                        if (!string.IsNullOrEmpty(attributeItem["Transformed Attribute Name"]) && !string.IsNullOrEmpty(attributeItem["Transformed Attribute Value"]))
                        {
                            ReferenceField transformedValueField = attributeItem.Fields["Transformed Attribute Value"];
                            if (transformedValueField != null && transformedValueField.TargetItem != null)
                            {
                                if (attributeItem["Transformed Attribute Name"].IndexOf("class", 0, StringComparison.InvariantCulture) > -1)
                                {
                                    element.Attributes.Add(attributeItem["Transformed Attribute Name"], transformedValueField.TargetItem["Class Name"]);
                                }
                                else if (attributeItem["Transformed Attribute Name"].IndexOf("style", 0, StringComparison.InvariantCulture) > -1)
                                {
                                    element.Attributes.Add(attributeItem["Transformed Attribute Name"], transformedValueField.TargetItem["Class Name"]);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes the invalid characters.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        internal static string ReplaceInvalidCharacters(string value, Item transformationItem)
        {
            var characters = transformationItem["Characters"].Split('|');
            var codes = transformationItem["Codes"].Split('|');

            for (var i = 0; i < characters.Length; i++ )
            {
                var character = characters[i];
                if (!string.IsNullOrEmpty(character) && codes.Length > i && !string.IsNullOrEmpty(codes[i]))
                {
                    value = value.Replace(character, codes[i]);
                }
            }
            return value;
        }
    }
}
