using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCS.DSS.ContentEnhancer.Models
{
    public class Subscriptions
    {
        public Guid CustomerId { get; set; }
        public Guid SubscriptionId { get; set; }
        public Guid TouchPointId { get; set; }
        public bool Subscribe { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public Guid LastModifiedBy { get; set; }
    }
}
