using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Sitecore.IO;
using Weil.SC.ePub.Mvc.Models;

namespace Weil.SC.ePub.Mvc
{
    public class EpubResult : ActionResult
    {
        public string FileLocation { get; set; }
        public EpubResult(string fileLocation)
        {
            FileLocation = fileLocation;
        }
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            HttpResponseBase response = context.HttpContext.Response;
            //response.ContentType = "application/octet-stream";

            if (!string.IsNullOrEmpty(FileLocation) && File.Exists(FileLocation))
            {
                var file = new FileInfo(FileLocation);
                response.ContentType = "application/epub+zip";
                response.AppendHeader("content-disposition", string.Format("attachment; filename={0}", file.Name));
                response.TransmitFile(file.FullName);
                response.Flush();
                response.End();
            }
        }
    }
}
