using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.IO;
using Sitecore.Resources.Media;

namespace Sitecore.SharedModules.ePub.Publishing.Renderers
{
    /// <summary>
    /// Renderer for an image tag
    /// </summary>
    public class ImageRenderer : HtmlTagRenderer
    {
        private const string _tag = "img";
        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public override string Tag
        {
            get
            {
                return _tag;
            }
            set
            {
                base.Tag = value;
            }
        }
        /// <summary>
        /// Renders the content.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        /// <param name="output">The output.</param>
        protected override void RenderContent(PrintContext printContext, object output, Epub.NavPoint parentEndPoint)
        {
            MediaItem mediaItem = null;
            string str1 = this.RenderingItem["MediaLibrary Reference"];
            bool flag = string.IsNullOrEmpty(str1);
            
            if (!flag)
            {
                mediaItem = (MediaItem)((ImageField)this.RenderingItem.Fields["MediaLibrary Reference"]).MediaItem;
            }
            else
            {
                string index = this.RenderingItem["Item Field"];
                flag = flag & string.IsNullOrEmpty(index);
                Item dataItem = this.GetDataItem(printContext);
                if (dataItem != null)
                {
                    Field field = null;
                    if (!string.IsNullOrEmpty(index))
                    {
                        field = dataItem.Fields[index];
                    }
                    else if (!string.IsNullOrEmpty(this.RenderingItem["Item Field"]))
                    {
                        field = dataItem.Fields[this.RenderingItem["Item Field"]];
                    }
                    if (field != null)
                    {
                        if (field.Type == "Image")
                        {
                            mediaItem = (MediaItem)((ImageField)field).MediaItem;
                        }
                    }
                }
            }
            if (mediaItem != null && !flag)
            {
                var imageName = string.Concat(mediaItem.ID.Guid, ".", mediaItem.Extension);
                var imageEpubPath = !string.IsNullOrEmpty(this.RenderingItem["ePub Base Path"]) ? string.Concat(this.RenderingItem["ePub Base Path"], imageName) : string.Concat("images/", imageName);
                printContext.Images.Add(imageEpubPath, MediaManager.Effects.TransformImageStream(mediaItem.GetMediaStream(), GetTransformationOptions(mediaItem), ImageFormat.Jpeg));
                var imageSrc = !string.IsNullOrEmpty(this.RenderingItem["ePub Base Path"]) ? imageEpubPath : string.Concat("../", imageEpubPath);
                if (output is HtmlDocument)
                {
                    var elem = (HtmlDocument)output;
                    HtmlNode element = RenderItemHelper.CreateHtmlNode(this.Tag, elem);
                    element.Attributes.Add("src", imageSrc);
                    element.Attributes.Add("alt", mediaItem.Alt);
                    this.RenderStyling(element);
                    elem.DocumentNode.SelectSingleNode("/html/body").AppendChild(element);
                    this.RenderChildren(printContext, element, parentEndPoint);
                }
                else if (output is HtmlNode)
                {
                    var elem = (HtmlNode)output;
                    HtmlNode element = RenderItemHelper.CreateHtmlNode(this.Tag, elem);
                    element.Attributes.Add("src", imageSrc);
                    element.Attributes.Add("alt", mediaItem.Alt);
                    this.RenderStyling(element);
                    elem.AppendChild(element);
                    this.RenderChildren(printContext, element, parentEndPoint);
                }                     
            }
            this.RenderChildren(printContext, output, parentEndPoint);
        }

        /// <summary>
        /// Gets the transformation options.
        /// </summary>
        /// <returns></returns>
        private TransformationOptions GetTransformationOptions(MediaItem mediaItem)
        {
            var options = new TransformationOptions();
            if (mediaItem == null)
            {
                return options;
            }

            var mediaWidth = Int32.Parse(mediaItem.InnerItem["Width"]);
            var mediaHeight = Int32.Parse(mediaItem.InnerItem["Height"]);

            var size = new Size();

            var width = 0;
            if (!string.IsNullOrEmpty(this.RenderingItem["Width"]) && Int32.TryParse(this.RenderingItem["Width"], out width))
            {
                size.Width = width;
                size.Height = width * mediaHeight / mediaWidth;
            }
            
            var height = 0;
            if (string.IsNullOrEmpty(this.RenderingItem["Width"]) && !string.IsNullOrEmpty(this.RenderingItem["Height"]) && Int32.TryParse(this.RenderingItem["Height"], out height))
            {
                size.Height = height;
                size.Width = height * mediaWidth / mediaHeight;
            }            

            if (size.Width > 0 && size.Height > 0)
            {
                options.Size = size;
            }
            
            options.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            options.IgnoreAspectRatio = false;
            options.MaxSize = new Size(500, 500);
            options.Quality = 100;
            return options;
        }
    }
}
