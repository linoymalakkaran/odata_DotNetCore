using System.Collections.Generic;

namespace MGSurvey.Business.Models
{
    public class Form : BaseModel<string>
    { 
        public string Name { get; set; }
        public string Type { get; set; } = "Service"; 
        public virtual ValidationSchema  Schema { get; set; }
        public virtual ICollection<SurveyResponse> SurveyResponses { get; set; }
    }
}
