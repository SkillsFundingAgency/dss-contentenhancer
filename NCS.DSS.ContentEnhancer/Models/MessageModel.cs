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

        //Delete digital identity fields (set from dss-digitalidentity)
        public Guid? IdentityStoreId { get; set; }

        //Update Email (set from dss-contacts)
        public string CurrentEmail { get; set; }
        public string NewEmail { get; set; }

        //generic digital identity fields (set from either dss-identity, or dss-contacts)
        public bool? IsDigitalAccount { get; set; }
        public bool? CreateDigitalIdentity { get; set; }
        public bool? DeleteDigitalIdentity { get; set; }
        public bool? ChangeEmailAddress { get; set; }
        public bool? UpdateDigitalIdentity { get; set; }
    }
}
