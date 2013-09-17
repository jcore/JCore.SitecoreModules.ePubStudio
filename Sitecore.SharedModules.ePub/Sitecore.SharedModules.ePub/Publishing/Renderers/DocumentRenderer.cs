using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.Fields;

namespace Sitecore.SharedModules.ePub.Publishing.Renderers
{
    public class DocumentRenderer : EpubItemRendererBase
    {
        protected override void RenderContent(PrintContext printContext, object output, Epub.NavPoint parentEndPoint)
        {
            if (output == null)
            {
                return;
            }
            this.RenderChildren(printContext, output, parentEndPoint);           
        }
    }

}
