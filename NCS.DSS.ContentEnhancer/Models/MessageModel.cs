using System;

namespace NCS.DSS.ContentEnhancer.Models
{
    public class MessageModel
    {
        public string TitleMessage { get; set; }
        public Guid? CustomerGuid { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string URL { get; set; }
        public bool IsNewCustomer { get; set; }
        public string TouchpointId { get; set; }
        public bool? DataCollections { get; set; }


        //Create Digital Identity Fields
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool? CreateDigitalIdentity { get; set; }
    }
}
