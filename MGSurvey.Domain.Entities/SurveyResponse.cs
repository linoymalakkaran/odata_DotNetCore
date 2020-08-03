using System;
using System.Collections.Generic;
using System.Text;

namespace MGSurvey.Domain.Entities
{
	public class SurveyResponse : BaseEntity<string>
	{
		public string FormId { get; set; }
		public virtual Form Form { get; set; }
	}


	public class SurveyResponsDetail
	{
		public string Id { get; set; }
		public string CompanyName { get; set; }
		public string UserName { get; set; }
		public string Question { get; set; }
		public string Answers { get; set; }
		public DateTime ResponseDate { get; set; }
		public string SurveyName { get; set; }
		public string SurveyStartDate { get; set; }
		public string SurveyEndDate { get; set; }

	}

}
