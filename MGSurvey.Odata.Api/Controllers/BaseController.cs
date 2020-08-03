using System;
using AutoMapper;
using System.Linq;
using System.Reflection;
using MGSurvey.Business.Models;
using MGSurvey.Domain.Entities;
using Microsoft.AspNet.OData;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using MGSurvey.Infrastructure.Database;
using Microsoft.AspNet.OData.Query;
using Microsoft.Extensions.Logging;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNet.OData.Routing;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MGSurvey.Odata.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BaseController<TEntity, TModel> : ODataController
        where TEntity : BaseEntity<string>
        where TModel : BaseModel<string>
    {
        private readonly IMapper _mapper;
        private readonly IMGSurveyDbContext _mgSurveyDbContext;
        private readonly ISchemaValidatorService _schemaValidatorService;
        private readonly ILogger<BaseController<TEntity, TModel>> _logger;
        public BaseController(
                                IMGSurveyDbContext mgSurveyDbContext,
                                ISchemaValidatorService schemaValidatorService,
                                IMapper mapper, 
                                ILogger<BaseController<TEntity, TModel>> logger)
        {
            _logger = logger;
            _mapper = mapper;
            _mgSurveyDbContext = mgSurveyDbContext;
            _schemaValidatorService = schemaValidatorService;
            UserName = User?.Identity?.Name ?? "Admin";
        }
        protected string UserName {get; private set; }
        protected IMGSurveyDbContext DbContext => this._mgSurveyDbContext;


        [HttpGet]
        [EnableQuery()]
        public virtual  IQueryable Get()
        {
            return  _mgSurveyDbContext.Context.Set<TEntity>().AsQueryable<TEntity>();
        }  

        [HttpPost]
        public virtual async Task<ActionResult<TEntity>> Post(TEntity entity)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState); 
            
            //validate entity
            var schemaName = typeof(TEntity).Name;
            var schema = _mgSurveyDbContext.ValidationSchemas.FirstOrDefault();
            var validationErrors = _schemaValidatorService.Validate<TEntity>(entity, schema);
            if(validationErrors != null && validationErrors.Count > 0)
            {
                return BadRequest(validationErrors);
            }

            _mgSurveyDbContext.Context.Set<TEntity>().Add(entity);
            await _mgSurveyDbContext.SaveChangesAsync();
            return Ok(entity);
        } 

        [HttpPut]
        public virtual async Task<ActionResult<TEntity>> Put(TEntity entity)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState); 
            //validate entity
            var schemaName = typeof(TEntity).Name;
            var schema = _mgSurveyDbContext.ValidationSchemas.FirstOrDefault();
            var validationErrors = _schemaValidatorService.Validate<TEntity>(entity, schema);
            if (validationErrors != null && validationErrors.Count > 0)
            {
                return BadRequest(validationErrors);
            }

            _mgSurveyDbContext.Context.Set<TEntity>().Update(entity);
            await _mgSurveyDbContext.SaveChangesAsync();
            return Ok(entity);
        }

        [HttpDelete]
        public virtual async Task<OkObjectResult> Delete([FromODataUri] string key)
        {
            var entity = await _mgSurveyDbContext.Context.Set<TEntity>().FindAsync(key);
            if (entity != null)
            {
                _mgSurveyDbContext.Context.Set<TEntity>().Remove(entity);
                await _mgSurveyDbContext.SaveChangesAsync(); 
                return Ok(new { Id = key, Status = "SUCCESS", Message = "" });
            }

            return Ok(new { Id = key, Status= "REJECTED", Message="Record not found"});
        }
    }
}
