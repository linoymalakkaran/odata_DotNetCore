using System.Collections.Generic;

namespace MGSurvey.Domain.Entities
{
    public class Form : BaseEntity<string>
    {
		public Form()
		{
            SurveyResponses = new HashSet<SurveyResponse>();
		}
        public string Name { get; set; }
        public string Type { get; set; } 
        public virtual ValidationSchema ValidationSchema { get; set; }
        public virtual ICollection<SurveyResponse> SurveyResponses { get; set; }
    }

    public enum FormTypes
    {
        Service,
        Configuration
    }
}
