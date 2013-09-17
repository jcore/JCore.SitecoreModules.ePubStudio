using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Sitecore.SharedModules.ePub;
using Sitecore.SharedModules.ePub.Web.ActionResults;

namespace Sitecore.SharedModules.ePub.Web.Controllers
{
    public class EpubController : Controller
    {
        //
        // GET: /Epub/

        /// <summary>
        /// Prints the epub.
        /// </summary>
        /// <returns></returns>
        public JsonpResult PrintEpub(string projectId = null)
        {
            return new JsonpResult(EpubManager.PublishProject(projectId));
        }

    }
}
