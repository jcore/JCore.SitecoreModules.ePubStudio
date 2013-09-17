using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.SharedModules.ePub
{
    public class RenderItemHelper
    {
        public static HtmlNode CreateHtmlNode(string nodeName, HtmlNode parentNode)
        {
            if (parentNode.OwnerDocument != null)
                return (HtmlNode)parentNode.OwnerDocument.CreateElement(nodeName);
            else
                return (HtmlNode)null;
        }

        public static HtmlNode CreateHtmlNode(string nodeName, HtmlDocument htmlDocument)
        {
            return (HtmlNode)htmlDocument.CreateElement(nodeName);
        }

        public static void CreateHtmlAttribute(string attributeName, string value, HtmlNode node)
        {
            if (node.OwnerDocument == null)
                return;
            HtmlAttribute attribute = node.OwnerDocument.CreateAttribute(attributeName);
            attribute.Value = value;
            if (node.Attributes == null)
                return;
            node.Attributes.Append(attribute);
        }

        public static void CreateFileFromStream(Stream stream, string destination)
        {
            using (BufferedStream bufferedStream = new BufferedStream(stream))
            {
                using (FileStream fileStream = File.OpenWrite(destination))
                {
                    byte[] buffer = new byte[8192];
                    int count;
                    while ((count = bufferedStream.Read(buffer, 0, buffer.Length)) > 0)
                        fileStream.Write(buffer, 0, count);
                }
            }
        }

        public static bool CacheFileExists(string fileCachePath, MediaItem mediaItem)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(fileCachePath);
                return fileInfo.Exists && mediaItem.Size == fileInfo.Length;
            }
            catch (IOException ex)
            {
                Log.Error("CacheImageExists failed for path " + fileCachePath, (Exception)ex, typeof(RenderItemHelper));
                return false;
            }
        }

        public static void OutputToFile(string resultFilePath, Epub.Document ePubDocument)
        {
            try
            {
                ePubDocument.Generate(resultFilePath);
            }
            catch (Exception ex)
            {
                Log.Error("OutputToFile: " + resultFilePath, ex, typeof(RenderItemHelper));
            }
        }

        public static string EnsureFolderPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            char ch = path.IndexOf('\\') >= 0 ? '\\' : '/';
            if ((int)path[path.Length - 1] == (int)ch)
                return path;
            else
                return path + (object)ch;
        }
    }
}
