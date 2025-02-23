using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanvi.ModelViews.PaymentModelViews
{
    public class PayOSItem
    {
        [JsonProperty("name")]  // Đảm bảo key đúng chuẩn PayOS
        public string Name { get; set; } // 🔹 Thêm thuộc tính này
        public string TravelerName { get; set; }
        public string PhoneNumber { get; set; }
        public string? Email { get; set; }
        public int Age { get; set; }
        public string? IdentityCard { get; set; }
        public string? PassportNumber { get; set; }
    }
}
