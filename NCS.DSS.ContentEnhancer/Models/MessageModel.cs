using System;

namespace NCS.DSS.ContentEnhancer.Models
{
    public class MessageModel
    {
        public string TitleMessage { get; set; }
        public Guid? CustomerGuid { get; set; }
        public DateTime? LastmodifiedDate { get; set; }
        public string URL { get; set; }
    }
}
