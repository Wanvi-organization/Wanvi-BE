using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanvi.Core.Constants
{
    public class Enum
    {
        public enum BookingStatus
        {
            Pending,
            Confirmed,
            Completed,
            Cancelled,
            Refunded
        }

        public enum PaymentMethod
        {
            EWallet,
            ByCash
        }

        public enum PaymentStatus
        {
            Unpaid,
            Deposited,
            Paid,
            Refunded
        }

        public enum DayOfWeek
        {
            Monday,
            Tuesday,
            Wednesday,
            Thursday,
            Friday,
            Saturday,
            Sunday
        }

        public enum MediaType
        {
            Image,
            Video
        }
    }
}
