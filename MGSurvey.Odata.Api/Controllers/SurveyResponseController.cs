using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MGSurvey.Infrastructure.Database;
using Microsoft.Extensions.Logging;
using Models = MGSurvey.Business.Models;
using Entities = MGSurvey.Domain.Entities;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using MGSurvey.Domain.Entities;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MGSurvey.Utilities;
using Microsoft.AspNetCore.Http;
using MGSurvey.Odata.Api.Validators;
using Microsoft.AspNetCore.Authorization;

namespace MGSurvey.Odata.Api.Controllers
{
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public class SurveyResponseController : BaseController<Entities.SurveyResponse, Models.SurveyResponse>
	{
		private readonly IMapper _mapper;
		private readonly IMGSurveyDbContext _mgSurveyDbContext;
		private readonly ISchemaValidatorService _schemaValidatorService;
		private readonly ILogger<SurveyResponseController> _logger;
		public SurveyResponseController(
									IMGSurveyDbContext mgSurveyDbContext,
									ISchemaValidatorService schemaValidatorService,
									IMapper mapper,
									ILogger<SurveyResponseController> logger
									) : base(mgSurveyDbContext, schemaValidatorService, mapper, logger)
		{
			_logger = logger;
			_mapper = mapper;
			_mgSurveyDbContext = mgSurveyDbContext;
			_schemaValidatorService = schemaValidatorService;
		}


		[HttpGet]
		[EnableQuery()]
		public override IQueryable Get()
		{
			if (!JWTTokenDeserializer.IsValidPCSSuperUser(this.HttpContext.Request.Headers["Authorization"].ToString()))
			{
				return null;
			}

			return _mgSurveyDbContext.SurveyResponses.Include(t => t.Form).AsQueryable();
		}

		//[HttpGet("GetSurveyReport")]
		//public dynamic GetSurveyReport(string surveyId)
		//{
		//	return _mgSurveyDbContext.SurveyResponsDetails.FromSqlRaw("EXEC [odata].[GetSurveyResponseDetail] {0}", surveyId).ToList();
		//}

		[HttpGet("GetSurveyReport")]
		public dynamic GetSurveyReport(string surveyId)
		{
			try
			{

				if (!JWTTokenDeserializer.IsValidPCSSuperUser(this.HttpContext.Request.Headers["Authorization"].ToString()))
				{
					return new StatusCodeResult(StatusCodes.Status401Unauthorized);
				}

				var surveyResponses = _mgSurveyDbContext.SurveyResponses.Include(x => x.Form.ValidationSchema).Where(t => t.FormId == surveyId).ToList();

				if (surveyResponses.Count == 0)
					return new StatusCodeResult(StatusCodes.Status204NoContent);

				var surveyTemplate = surveyResponses.FirstOrDefault().Form;

				//var validationScheme = _mgSurveyDbContext.ValidationSchemas.FirstOrDefault(t => t.FormId == surveyId);

				var validationScheme = surveyTemplate.ValidationSchema;

				var validationEntityData = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(validationScheme.EntityData),
																						  new JsonConverter[] {
																						   new MGSurveyJsonConverter() });


				var schema = validationEntityData["Schema"];

				var fields = JsonConvert.DeserializeObject<List<Field>>(JsonConvert.SerializeObject(schema["Fields"]));


				//	var sfields = JsonConvert.SerializeObject(actual.ToString());

				//var schema = JsonConvert.DeserializeObject<Field>(actual["Schema"]);


				//if (surveyTemplate == null)
				//	return new StatusCodeResult(StatusCodes.Status204NoContent);


				var templateEntityData = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(surveyTemplate.EntityData),
																						  new JsonConverter[] {
																						   new MGSurveyJsonConverter()
																						  });

				if (templateEntityData == null)
					return new StatusCodeResult(StatusCodes.Status204NoContent);


			//	List<Tuple<string, string>> questionsList = new List<Tuple<string, string>>();

				var surveyStartDate = templateEntityData["StartDate"];
				var surveyEndDate = templateEntityData["EndDate"];

				//var templateSchema = templateEntityData["Schema"];
				
				//var templateComponent = (JArray)templateSchema["components"];
				

				//foreach (var item in templateComponent)
				//{
				//	if (item["type"].ToString() == "panel")
				//	{
				//		foreach (var subitem in item["components"])
				//		{
				//			if (subitem["type"] == null || (subitem["type"].ToString() != "button"  && subitem["type"].ToString() != "submit"))
				//			{
				//				var question = subitem["label"];
				//				Tuple<string, string> tp = new Tuple<string, string>(subitem["key"].ToString(), subitem["label"].ToString());
				//				questionsList.Add(tp);
				//			}

				//		}
				//	}
				//	else
				//	{

				//		if (item["type"] == null || (item["type"].ToString() != "button" && item["type"].ToString() != "submit"))
				//		{
				//			var question = item["label"];
				//			Tuple<string, string> tp = new Tuple<string, string>(item["key"].ToString(), item["label"].ToString());
				//			questionsList.Add(tp);
				//		}
				//	}

				//}




				List<SurveyResponsDetail> reportData = new List<SurveyResponsDetail>();

				foreach (var response in surveyResponses)
				{
					var answers = response.EntityData["Data"];
					var ans = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(answers),
																						  new JsonConverter[] {
																						   new MGSurveyJsonConverter() });
					foreach (var qst in fields)
					{
						SurveyResponsDetail objDetail = new SurveyResponsDetail();
						objDetail.Answers = ans[qst.Name].ToString();
						objDetail.Question = qst.Label;
						objDetail.UserName = response.EntityData["UserName"].ToString();
						objDetail.CompanyName = response.EntityData["CompanyName"].ToString();
						objDetail.Id = response.Id;
						objDetail.ResponseDate = response.CreatedDate;
						objDetail.SurveyName = response.Form.Name;
						objDetail.SurveyStartDate = Convert.ToString(surveyStartDate);
						objDetail.SurveyEndDate = Convert.ToString(surveyEndDate);
						reportData.Add(objDetail);
					}

				}
				return reportData;
			}

			catch (Exception e)
			{
				return new StatusCodeResult(StatusCodes.Status500InternalServerError);
			}
		}


		//[EnableQuery()]
		//[HttpGet("GetSurveySummary")]
		//public ICollection<SurveyResponsDetail> GetSurveySummary(string surveyId)
		//{
		//	return _mgSurveyDbContext.SurveyResponsDetails.FromSqlRaw("EXEC [odata].[GetSurveyResponseDetail] {0}", surveyId).ToList();
		//}


		//[EnableQuery()]
		//public dynamic GetSubmissionList([FromODataUri] string appName, [FromODataUri] string companyName, [FromODataUri] string userName)
		//{

		//	//if(!String.IsNullOrEmpty(companyName) && !String.IsNullOrEmpty(userName))

		//	var response = _mgSurveyDbContext.SurveyResponses.Include(x => x.Form)
		//	.Where(t => t.Form.Type == ((appName != String.Empty || appName != null) ? appName : t.Form.Type)).ToList();

		//	//.Where(PredicateBuilder.Equal<Entities.SurveyResponse>("CompanyName", companyName))
		//	//.Where(PredicateBuilder.Equal<Entities.SurveyResponse>("UserName", userName)).OrderByDescending(t => t.CreatedDate).ToList();

		//	if (!String.IsNullOrEmpty(companyName) && !String.IsNullOrEmpty(userName))
		//	{
		//		response = _mgSurveyDbContext.SurveyResponses.Include(x => x.Form)
		//	   .Where(t => t.Form.Type == ((appName != String.Empty || appName != null) ? appName : t.Form.Type))
		//	   .Where(PredicateBuilder.Contains<Entities.SurveyResponse>("CompanyName", companyName))
		//	   .Where(PredicateBuilder.Contains<Entities.SurveyResponse>("UserName", userName)).OrderByDescending(t => t.CreatedDate).ToList();

		//	}
		//	else if (!String.IsNullOrEmpty(companyName) && String.IsNullOrEmpty(userName))
		//	{
		//		response = _mgSurveyDbContext.SurveyResponses.Include(x => x.Form)
		//   .Where(t => t.Form.Type == ((appName != String.Empty || appName != null) ? appName : t.Form.Type))
		//   .Where(PredicateBuilder.Contains<Entities.SurveyResponse>("CompanyName", companyName)).ToList();
		//	}
		//	else if (String.IsNullOrEmpty(companyName) && !String.IsNullOrEmpty(userName))
		//	{
		//		response = _mgSurveyDbContext.SurveyResponses.Include(x => x.Form)
		//	   .Where(t => t.Form.Type == ((appName != String.Empty || appName != null) ? appName : t.Form.Type))
		//	   .Where(PredicateBuilder.Contains<Entities.SurveyResponse>("UserName", userName)).OrderByDescending(t => t.CreatedDate).ToList();
		//	}


		//	//var response = _mgSurveyDbContext.SurveyResponses.Where(t => t.FormId == "1fa174aa-d077-4a98-8117-848a82950fdd").Include(x => x.Form).ToList();

		//	//foreach (var item in response)
		//	//{

		//	//}


		//	//var raw = response.FirstOrDefault().EntityData;

		//	//foreach (var item in raw)
		//	//{
		//	//	var fresult = response.Where(t => t.EntityData.ContainsKey(item.Key)).Select(t => t.EntityData[item.Key]).ToList();
		//	//	int value;
		//	//	int index = 0;
		//	//	foreach (var sub in fresult)
		//	//	{
		//	//		if (int.TryParse(sub.ToString(), out value))
		//	//		{

		//	//			index++;
		//	//		}
		//	//	}

		//	//}


		//	var result = response.Select(x => new
		//	{
		//		SurveyName = x.Form.Name,
		//		QA = x.EntityData,
		//		SubmittedDate = x.CreatedDate,
		//		CompanyName = x.EntityData["CompanyName"], // x.EntityData["CompanyName"],
		//		UserName = x.EntityData["UserName"] //x.EntityData["Username"]
		//	});

		//	return result;
		//}


		//[EnableQuery()]
		//public IQueryable GetSurveySummary([FromODataUri] string appName, [FromODataUri] string surveyName, [FromODataUri] string companyName, [FromODataUri] string userName)
		//{

		//	var response = _mgSurveyDbContext.SurveyResponses.Include(x => x.Form)
		//					.Where(t => t.Form.Type == (appName != String.Empty  ? appName : t.Form.Type) && t.Form.Name.Contains(surveyName != String.Empty ? surveyName : t.Form.Name)).ToList();

		//	if (!String.IsNullOrEmpty(companyName) && !String.IsNullOrEmpty(userName))
		//	{
		//		response = _mgSurveyDbContext.SurveyResponses.Include(x => x.Form)
		//				   .Where(t => t.Form.Type == (appName != String.Empty ? appName : t.Form.Type) && t.Form.Name.Contains(surveyName != String.Empty ? surveyName : t.Form.Name))
		//				   .Where(PredicateBuilder.Contains<Entities.SurveyResponse>("CompanyName", companyName))
		//				   .Where(PredicateBuilder.Contains<Entities.SurveyResponse>("UserName", userName)).OrderByDescending(t => t.CreatedDate).ToList();
		//	}
		//	else if (!String.IsNullOrEmpty(companyName) && String.IsNullOrEmpty(userName))
		//	{
		//		response = _mgSurveyDbContext.SurveyResponses.Include(x => x.Form)
		//				   .Where(t => t.Form.Type == (appName != String.Empty ? appName : t.Form.Type) && t.Form.Name.Contains(surveyName != String.Empty ? surveyName : t.Form.Name))
		//				   .Where(PredicateBuilder.Contains<Entities.SurveyResponse>("CompanyName", companyName)).ToList();
		//	}
		//	else if (String.IsNullOrEmpty(companyName) && !String.IsNullOrEmpty(userName))
		//	{
		//		response = _mgSurveyDbContext.SurveyResponses.Include(x => x.Form)
		//				   .Where(t => t.Form.Type == (appName != String.Empty ? appName : t.Form.Type) && t.Form.Name.Contains(surveyName != String.Empty ? surveyName : t.Form.Name))
		//				   .Where(PredicateBuilder.Contains<Entities.SurveyResponse>("UserName", userName)).OrderByDescending(t => t.CreatedDate).ToList();
		//	}

		////	return response;
		//	List<SurveySummary> result = new List<SurveySummary>();
		//	string status = String.Empty;

		//	if (!String.IsNullOrEmpty(companyName) || !string.IsNullOrEmpty(userName))
		//	{
		//		foreach (var item in response.Select(t => t.FormId).Distinct())
		//		{
		//			SurveySummary survey = new SurveySummary();

		//			//var startDate = Convert.ToDateTime(response.FirstOrDefault(t => t.FormId == item).Form.EntityData["StartDate"]);
		//			//var endDate = Convert.ToDateTime(response.FirstOrDefault(t => t.FormId == item).Form.EntityData["EndDate"]);
		//			//if (DateTime.Today >= startDate && DateTime.Today <= endDate)
		//			//{
		//			//	status = "Active";
		//			//}
		//			//else if (DateTime.Today < startDate)
		//			//{
		//			//	status = "Pending";
		//			//}
		//			//else if (DateTime.Today > endDate)
		//			//{
		//			//	status = "Expired";
		//			//}
		//			//else
		//			status = "Anonymous";
		//			survey.AppName = response.FirstOrDefault(t => t.FormId == item).Form.Type;
		//			survey.SurveyName = response.FirstOrDefault(t => t.FormId == item).Form.Name;
		//			survey.TotalRespones = response.Where(t => t.FormId == item).Count();
		//			survey.SurveyResponses = response.Where(t => t.FormId == item).ToList();
		//			survey.Status = status;
		//			result.Add(survey);
		//		}
		//	}
		//	else
		//	{
		//		var surveys = _mgSurveyDbContext.Forms.Where(t => t.Type == (appName != String.Empty ? appName : t.Type)).ToList();

		//		foreach (var item in surveys)
		//		{
		//			SurveySummary survey = new SurveySummary();

		//			survey.AppName = item.Type;
		//			survey.SurveyName = item.Name;
		//			survey.TotalRespones = response.Where(t => t.FormId == item.Id).Count();
		//			survey.SurveyResponses = response.Where(t => t.FormId == item.Id).ToList();
		//			//var startDate = Convert.ToDateTime(item.EntityData["StartDate"]);
		//			//var endDate = Convert.ToDateTime(item.EntityData["EndDate"]);
		//			//if (DateTime.Today >= startDate && DateTime.Today <= endDate)
		//			//{
		//			//	status = "Active";
		//			//}
		//			//else if (DateTime.Today < startDate)
		//			//{
		//			//	status = "Pending";
		//			//}
		//			//else if (DateTime.Today > endDate)
		//			//{
		//			//	status = "Expired";
		//			//}
		//			//else
		//			status = "Anonymous";
		//			survey.Status = status;
		//			result.Add(survey);
		//		}
		//	}

		//	return result.AsQueryable();
		//	//JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
		//	//{
		//	//	ReferenceLoopHandling = ReferenceLoopHandling.Ignore
		//	//};

		//	//return JsonConvert.SerializeObject(result, jsonSerializerSettings); 

		//}



		[HttpPost]
		public override async Task<ActionResult<Entities.SurveyResponse>> Post(Entities.SurveyResponse entity)
		{
			var appName = Convert.ToString(Request.Headers["AppName"]);
			var key = Convert.ToString(Request.Headers["AppKey"]);
			var user = Convert.ToString(Request.Headers["UserName"]);
			var type = _mgSurveyDbContext.FormTypes.FirstOrDefault(t => t.Name.ToLower() == appName.ToLower() && t.SecretKey.ToLower() == key.ToLower());
			if (type == null)
			{
				return BadRequest();
			}

			if (JWTTokenDeserializer.IsValidPCSSuperUser(this.HttpContext.Request.Headers["Authorization"].ToString()))
			{
				return new StatusCodeResult(StatusCodes.Status401Unauthorized);
			}


			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			_mgSurveyDbContext.SurveyResponses.Add(entity);
			await _mgSurveyDbContext.SaveChangesAsync();
			return Ok(entity);
			//
		}

		[HttpPut]
		public override async Task<ActionResult<Entities.SurveyResponse>> Put(Entities.SurveyResponse entity)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			
			if (JWTTokenDeserializer.IsValidPCSSuperUser(this.HttpContext.Request.Headers["Authorization"].ToString()))
			{
				return new StatusCodeResult(StatusCodes.Status401Unauthorized);
			}



			_mgSurveyDbContext.SurveyResponses.Update(entity);
			await _mgSurveyDbContext.SaveChangesAsync();
			return Ok(entity);
		}
	}
}
