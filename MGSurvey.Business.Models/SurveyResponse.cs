using System;
using System.Collections.Generic;
using System.Text;

namespace MGSurvey.Business.Models
{
	public class SurveyResponse : BaseModel<string>
	{
		public string FormId { get; set; }
		public virtual Form Form { get; set; }
	}

	//public class SurveySummary
	//{
	//	public string SurveyName { get; set; }
	//	public string AppName { get; set; }
	//	public int TotalRespones { get; set; }
	//	public string Status { get; set; }
	//}
	public class RequestResponse
	{
		public RequestResponse(Object obj, bool IsValid, string Message)
		{
			this.obj = obj;
			this.IsValid = IsValid;
			this.Message = Message;
		}
		public Object obj { get; set; }
		public bool IsValid { get; set; }
		public string Message { get; set; }
	}

}
