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
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using MGSurvey.Business.Models;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Http;
using MGSurvey.Odata.Api.Validators;
using Microsoft.AspNetCore.Authorization;

namespace MGSurvey.Odata.Api.Controllers
{
	[Authorize()]
	[ApiController]
	[Route("[controller]")]
	public class FormController : BaseController<Entities.Form, Models.Form>
	{
		private readonly IMapper _mapper;
		private readonly IMGSurveyDbContext _mgSurveyDbContext;
		private readonly ISchemaValidatorService _schemaValidatorService;
		private readonly ILogger<FormController> _logger;
		public FormController(
									IMGSurveyDbContext mgSurveyDbContext,
									ISchemaValidatorService schemaValidatorService,
									IMapper mapper,
									ILogger<FormController> logger
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
			//	_mgSurveyDbContext.Forms.Where(PredicateBuilder.Equal<Entities.Form>("StartDate", customDate)).Where(t => t.Name == appName).ToList();

			//var pBuilder = PredicateBuilder<Entities.Form>.Builder;
			//var startDateBuilder = pBuilder.GreaterThan<DateTime>("StartDate", DateTime.Today);
			//var fieldCompared = pBuilder.GreaterThan(
			//	pBuilder.GetJsonFieldValue<DateTime>("EndDate"),
			//	pBuilder.GetJsonFieldValue<DateTime>("StartDate")
			//	);
			//var andBuilder= pBuilder.And(startDateBuilder, fieldCompared);
			//pBuilder.Or(andBuilder, startDateBuilder);
			if (!JWTTokenDeserializer.IsValidPCSSuperUser(this.HttpContext.Request.Headers["Authorization"].ToString()))
			{
				return null;
			}

			var response = _mgSurveyDbContext.Forms.Include(x => x.SurveyResponses).AsQueryable();
			return response;
		}


		[HttpGet("GetSurvey")]
		[EnableQuery()]
		public ActionResult GetSurvey()
		{
			try
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
					return new StatusCodeResult(StatusCodes.Status401Unauthorized); ;
				}

				var pformBuilder = PredicateBuilder<Entities.Form>.Builder;
				var startDateBuilder = pformBuilder.LessThanOrEqual<DateTime>("StartDate", DateTime.Today);
				var endDateBuilder = pformBuilder.GreaterThanOrEqual<DateTime>("EndDate", DateTime.Today);

				//var fieldCompared = pBuilder.GreaterThan(
				//	pBuilder.GetJsonFieldValue<DateTime>("EndDate"),
				//	pBuilder.GetJsonFieldValue<DateTime>("StartDate")
				//	);

				var andBuilder = pformBuilder.And(startDateBuilder, endDateBuilder);
				//pBuilder.Or(andBuilder, startDateBuilder);

				var pSurveyRBuilder = PredicateBuilder<Entities.SurveyResponse>.Builder;
				var userDataBuilder = pSurveyRBuilder.Equal("UserName", user);


				var response = _mgSurveyDbContext.Forms.Where(t => t.Type.ToLower() == appName.ToLower() && t.IsActive == true).Where(andBuilder).Include(s => s.SurveyResponses).OrderByDescending(t => t.CreatedDate).FirstOrDefault();

				if (response == null)
					return new StatusCodeResult(StatusCodes.Status204NoContent);

				var surveyR = _mgSurveyDbContext.SurveyResponses.Where(t => t.FormId == response.Id).Where(userDataBuilder).FirstOrDefault();

				if (surveyR == null)
					return Ok(response);
				else
					return new StatusCodeResult(StatusCodes.Status204NoContent);
			}
			catch (Exception e)
			{
				return new StatusCodeResult(StatusCodes.Status500InternalServerError);
			}
		}

		[HttpGet("GetApplicationTypes")]
		[EnableQuery()]
		public IQueryable GetApplicationTypes()
		{
			if (!JWTTokenDeserializer.IsValidPCSSuperUser(this.HttpContext.Request.Headers["Authorization"].ToString()))
			{
				return null;
			}
			return _mgSurveyDbContext.FormTypes.Where(t => t.IsActive == true).OrderBy(t => t.Name).AsQueryable();
		}

		//		[EnableQuery()]
		//		public dynamic GetMamarSurvey([FromODataUri] string appName, [FromODataUri] string userName)
		////		public dynamic GetMamarSurvey([FromODataUri] string formId)
		//		{

		//			//string customDate = DateTime.Today.Date.ToString("dd/MM/yyyy");
		//			//var response = _mgSurveyDbContext.Forms.Where(PredicateBuilder.Equal<Entities.Form>("StartDate", customDate)).Where(t => t.Name == appName).ToList();

		//			var response = _mgSurveyDbContext.Forms.Include(x=>x.SurveyResponses).Where(t => t.Type == appName).ToList();
		//			//var availableResponse = _mgSurveyDbContext.Forms.Where(t => t.Id == appName).FirstOrDefault();

		//			//var surveyResponse = availableResponse.SurveyResponses.Any(t => t.CreatedBy == userName);

		//			if (true)

		//			{
		//				return response;
		//			}
		//			return null;
		//		}

		//var validationErrors = _schemaValidatorService.Validate<Entities.Form>(entity, schema);
		//if (validationErrors != null && validationErrors.Count > 0)
		//{
		//	return BadRequest(validationErrors);
		//}
		// check if record with same name already exists
		//var dbEntity = await _mgSurveyDbContext.Forms.FirstOrDefaultAsync(e => e.Id.ToLower() == entity.Id.ToLower());
		//         if (dbEntity != null)
		//         {
		//             validationErrors = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary();
		//             validationErrors.AddModelError("Name", "Form with same name already exists");
		//             return BadRequest(validationErrors);
		//         }


		[HttpPost]
		public override async Task<ActionResult<Entities.Form>> Post(Entities.Form entity)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);


			if (!JWTTokenDeserializer.IsValidPCSSuperUser(this.HttpContext.Request.Headers["Authorization"].ToString()))
			{
				return new StatusCodeResult(StatusCodes.Status401Unauthorized); ;
			}

			//link form validation schema if any
			if (entity.ValidationSchema != null)
			{
				entity.ValidationSchema.Id = Guid.NewGuid().ToString();
				entity.ValidationSchema.FormId = entity.Id;
			}

			//validate entity
			//var schemaName = typeof(Entities.Form).Name;
			//var schema = _mgSurveyDbContext.ValidationSchemas.FirstOrDefault(s => s.Name == schemaName || s.Code == schemaName);

			_mgSurveyDbContext.Forms.Add(entity);
			await _mgSurveyDbContext.SaveChangesAsync();
			return Ok(entity);
		}

		[HttpPut]
		public override async Task<ActionResult<Entities.Form>> Put(Entities.Form entity)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);
			
			if (!JWTTokenDeserializer.IsValidPCSSuperUser(this.HttpContext.Request.Headers["Authorization"].ToString()))
			{
				return new StatusCodeResult(StatusCodes.Status401Unauthorized);
			}

			//validate entity
			//var schemaName = typeof(Entities.Form).Name;
			//var schema = _mgSurveyDbContext.ValidationSchemas.FirstOrDefault(s => s.Name == schemaName || s.Code == schemaName);
			//var validationErrors = _schemaValidatorService.Validate<Entities.Form>(entity, schema);
			//if (validationErrors != null && validationErrors.Count > 0)
			//{
			//	return BadRequest(validationErrors);
			//}
			// check if record with same name or code already exists
			var dbEntity = await _mgSurveyDbContext.Forms.FirstOrDefaultAsync(e => e.Id != entity.Id);
			if (dbEntity == null)
			{
				//validationErrors = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary();
				//validationErrors.AddModelError("Name", "Form with same name already exists");
				return BadRequest();
			}
			_mgSurveyDbContext.Forms.Update(entity);
			await _mgSurveyDbContext.SaveChangesAsync();
			return Ok(entity);
		}
	}
}
