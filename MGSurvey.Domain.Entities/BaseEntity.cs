using System;
using System.Collections.Generic;

namespace MGSurvey.Domain.Entities
{
    public class BaseEntity<Tkey> where Tkey : IEquatable<Tkey>
    {
        public Tkey Id { get; set; } 
        public string CreatedBy { get; set; } = "OdataAdminApi";
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now; 
        public IDictionary<string, object> EntityData { get; set; } = new Dictionary<string, object>();  
    }
}
