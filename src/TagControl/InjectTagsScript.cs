using Sitecore;
using Sitecore.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

namespace TagControl
{
    public class InjectTagsScript
    {
        public void Process(PipelineArgs args)
        {
            if (!Context.ClientPage.IsEvent)
            {
                HttpContext current = HttpContext.Current;
                if (current != null)
                {
                    Page handler = current.Handler as Page;
                    if (handler != null)
                    {
                      
                        handler.Header.Controls.Add(new LiteralControl("<script type='text/javascript' language='javascript' src='/sitecore/shell/controls/tag field/js/jquery-ui.min.js'></script>"));
                        handler.Header.Controls.Add(new LiteralControl("<script type='text/javascript' language='javascript' src='/sitecore/shell/controls/tag field/js/taghelper.js'></script>"));
                        handler.Header.Controls.Add(new LiteralControl("<script type='text/javascript' language='javascript' src='/sitecore/shell/controls/tag field/js/tag-it.js'></script>"));
                        
                    }
                }
            }
        }
    }
}
