using Microsoft.OData.Edm;
using MGSurvey.Domain.Entities;
using Microsoft.AspNet.OData.Builder;

namespace MGSurvey.OData.Edm
{
    public static class MGSurveyODataEdmModelBuilder
    {
       public static IEdmModel BuildEdmModel()
        {
            var odataBuilder = new ODataConventionModelBuilder(); 
            odataBuilder.EntitySet<Form>("Form");
            odataBuilder.EntitySet<ValidationSchema>("ValidationSchema");
            odataBuilder.EntitySet<SurveyResponse>("SurveyResponse");


			//   odataBuilder.EntitySet<SurveySummary>("Surveys");

			// odataBuilder.EntitySet<SurveySummary>("SurveySummarys");

			var pullRequestsByProjectByContributor = odataBuilder.EntityType<Form>().Collection
			.Function("GetSurvey")
			.ReturnsCollectionFromEntitySet<Form>("Forms");

			var applicationTypeListing = odataBuilder.EntityType<Form>().Collection
			.Function("GetApplicationTypes")
			.ReturnsCollectionFromEntitySet<FormType>("FormTypes");
			
			
			//pullRequestsByProjectByContributor.Parameter<Microsoft.AspNet.OData.Query.ODataQueryOptions>("oquery").Required();

			//var pullRequestsByProjectByContributor = odataBuilder.EntityType<Form>().Collection
			//.Function("GetMamarSurvey")
			//.ReturnsCollectionFromEntitySet<Form>("Form");
			//pullRequestsByProjectByContributor.Parameter<string>("appName").Required();
			//pullRequestsByProjectByContributor.Parameter<string>("userName").Required();


			// var submissionRequests = odataBuilder.EntityType<SurveyResponse>().Collection
			//.Function("GetSubmissionList")
			//.ReturnsCollectionFromEntitySet<SurveyResponse>("SurveyResponse");
			// submissionRequests.Parameter<string>("appName").Required();
			// submissionRequests.Parameter<string>("companyName").Required();
			// submissionRequests.Parameter<string>("userName").Required();


			var surveySummaryReq = odataBuilder.EntityType<SurveyResponse>().Collection
			.Function("GetSurveySummary")
		   .ReturnsCollection<SurveyResponsDetail>();
			surveySummaryReq.Parameter<string>("surveyId").Required();
	
            return odataBuilder.GetEdmModel();
        }
    }
}
