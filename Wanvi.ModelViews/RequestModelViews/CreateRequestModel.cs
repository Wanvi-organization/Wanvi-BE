using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wanvi.Contract.Repositories.Entities;

namespace Wanvi.ModelViews.RequestModelViews
{
    public class CreateRequestModel
    {
        public string? Note { get; set; }
        public RequestType Type { get; set; }
        public int Balance { get; set; } = 0;

    }
}
