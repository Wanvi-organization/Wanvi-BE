using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanvi.ModelViews.RequestModelViews
{
    public class CancelRequestFromAdminModel
    {
        public string Id { get; set; }
        public long OrderCode { get; set; } = 0;
        public string? Reason { get; set; }

    }
}
