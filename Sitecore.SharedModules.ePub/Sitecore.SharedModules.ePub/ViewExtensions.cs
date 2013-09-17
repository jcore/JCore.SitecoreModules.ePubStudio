using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;

namespace Sitecore.SharedModules.ePub
{
    public static class ViewExtensions
    {
        /// <summary>
        /// Renders to string.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="viewName">Name of the view.</param>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public static string RenderToString(this ControllerContext context, string viewName, string masterName, object model)
        {
            return RenderToStringMethod(context, viewName, masterName, model);
        }
        /// <summary>
        /// render PartialView and return string
        /// </summary>
        /// <param name="context"></param>
        /// <param name="partialViewName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string RenderPartialToString(this ControllerContext context, string partialViewName, object model)
        {
            return RenderPartialToStringMethod(context, partialViewName, model);
        }

        /// <summary>
        /// render PartialView and return string
        /// </summary>
        /// <param name="context"></param>
        /// <param name="partialViewName"></param>
        /// <param name="viewData"></param>
        /// <param name="tempData"></param>
        /// <returns></returns>
        public static string RenderPartialToString(ControllerContext context, string partialViewName, ViewDataDictionary viewData, TempDataDictionary tempData)
        {
            return RenderPartialToStringMethod(context, partialViewName, viewData, tempData);
        }

        /// <summary>
        /// Renders the partial to string method.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="partialViewName">Partial name of the view.</param>
        /// <param name="viewData">The view data.</param>
        /// <param name="tempData">The temp data.</param>
        /// <returns></returns>
        public static string RenderPartialToStringMethod(ControllerContext context, string partialViewName, ViewDataDictionary viewData, TempDataDictionary tempData)
        {
            ViewEngineResult result = ViewEngines.Engines.FindPartialView(context, partialViewName);

            if (result.View != null)
            {
                StringBuilder sb = new StringBuilder();
                using (StringWriter sw = new StringWriter(sb))
                {
                    using (HtmlTextWriter output = new HtmlTextWriter(sw))
                    {
                        ViewContext viewContext = new ViewContext(context, result.View, viewData, tempData, output);
                        result.View.Render(viewContext, output);
                    }
                }

                return sb.ToString();
            }
            return String.Empty;
        }

        /// <summary>
        /// Renders to string method.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="viewName">Name of the view.</param>
        /// <param name="viewData">The view data.</param>
        /// <param name="tempData">The temp data.</param>
        /// <returns></returns>
        public static string RenderToStringMethod(ControllerContext context, string viewName, string masterName, ViewDataDictionary viewData, TempDataDictionary tempData)
        {
            ViewEngineResult result = ViewEngines.Engines.FindView(context, viewName, masterName);

            if (result.View != null)
            {
                StringBuilder sb = new StringBuilder();
                using (StringWriter sw = new StringWriter(sb))
                {
                    using (HtmlTextWriter output = new HtmlTextWriter(sw))
                    {
                        ViewContext viewContext = new ViewContext(context, result.View, viewData, tempData, output);
                        result.View.Render(viewContext, output);
                    }
                }

                return sb.ToString();
            }
            return String.Empty;
        }

        /// <summary>
        /// Renders the partial to string method.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="partialViewName">Partial name of the view.</param>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public static string RenderPartialToStringMethod(ControllerContext context, string partialViewName, object model)
        {
            ViewDataDictionary viewData = new ViewDataDictionary(model);
            TempDataDictionary tempData = new TempDataDictionary();
            return RenderPartialToStringMethod(context, partialViewName, viewData, tempData);
        }

        /// <summary>
        /// Renders to string method.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="viewName">Name of the view.</param>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public static string RenderToStringMethod(ControllerContext context, string viewName, string masterName, object model)
        {
            ViewDataDictionary viewData = new ViewDataDictionary(model);
            TempDataDictionary tempData = new TempDataDictionary();
            return RenderToStringMethod(context, viewName, masterName, viewData, tempData);
        }
    }
}
