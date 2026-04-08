using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums.Enums_Models
{
    public enum CustomsStatus
    {
        PendingDocuments = 0,    
        DocumentsSubmitted = 1,  
        UnderReview = 2,         
        RequiresAction = 3,      
        Approved = 4,            
        Released = 5,            
        Rejected = 6             
    }
}
