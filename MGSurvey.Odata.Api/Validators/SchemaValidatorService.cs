using MGSurvey.Domain.Entities;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MGSurvey.Infrastructure.Database;
using MGSurvey.Utilities;
using MGSurvey.Odata.Api.Validators;

namespace MGSurvey.Odata.Api
{
    public interface ISchemaValidatorService
    {
        ModelStateDictionary Validate<TEntity>(TEntity entity, ValidationSchema schema) where TEntity : class;
    }
    public class SchemaValidatorService : ISchemaValidatorService
    {
        private const string Dynamic_Field_Name = "EntityData";
        private readonly ModelStateDictionary _validationErrors;
        public SchemaValidatorService()
        {
            _validationErrors = new ModelStateDictionary();
        }
        public ModelStateDictionary Validate<TEntity>(TEntity entity, ValidationSchema schema) where TEntity : class
        {
            var entityType = typeof(TEntity);
            string entityName = entityType.Name;

            if (entity == null)
            {
                _validationErrors.AddModelError("Entity", "Entity object cann not be null");
                return _validationErrors;
            }

            //convert entity into json object
            var entityRawJson = JsonConvert.SerializeObject(entity, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None
            });
            var jsonEntity = JsonConvert.DeserializeObject<JObject>(entityRawJson, new JsonConverter[] {
                                                                                          new MGSurveyJsonConverter()
                                                                                       });
            var _jsonEntityProperties = jsonEntity.Properties();
            //validate schema
            if (schema == null)
            {
                //_validationErrors.AddModelError("Schema", "Provided schema for given entity can not be null");
                return _validationErrors;
            }

            //convert schema entity into json object
            var schemaRawJson = JsonConvert.SerializeObject(schema, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None
            });
            var jsonSchemaEntity = JsonConvert.DeserializeObject<JObject>(schemaRawJson, new JsonConverter[] {
                                                                                          new MGSurveyJsonConverter()
                                                                                       });

            if (jsonSchemaEntity[Dynamic_Field_Name] == null ||
                jsonSchemaEntity.SelectToken($"{Dynamic_Field_Name}.Schema") == null ||
                jsonSchemaEntity.SelectToken($"{Dynamic_Field_Name}.Schema.Fields") == null)
            {
                _validationErrors.AddModelError("SchemaFields", "Field definitions are not defined on provided schema");
                return _validationErrors;
            }

            var schemaFields = JsonConvert.DeserializeObject<List<Field>>(jsonSchemaEntity.SelectToken($"{Dynamic_Field_Name}.Schema.Fields").ToString());
            var rootFields = schemaFields.Where(f => f.Name != Dynamic_Field_Name &&
                                                        (string.IsNullOrWhiteSpace(f.Path) || f.Path == f.Name));
            //validate root properties 
            foreach (var field in rootFields)
            { 
                var _eProp = _jsonEntityProperties.FirstOrDefault(p => p.Name == field.Name);   
                if (_eProp == null &&  field.Required)
                {
                    _validationErrors.AddModelError(field.Name, $"The ({field.Name}) field is not defined at (Root) path on entity ({entityName})");
                    continue;
                }
                ValidateJsonProperty(_eProp, field);
            }

            //check dynamic field is defined on entity
            var dProp = _jsonEntityProperties.FirstOrDefault(p => p.Name == Dynamic_Field_Name);
            if (dProp == null && dProp.Value != null)
            {
                // dynamic field is optional
                return _validationErrors;
            }
            //convert dynamic propert to json object
            var dJsonObject = JObject.Parse(dProp.Value.ToString());
            var dJonProperties = dJsonObject.Properties();
            //validate open/dynamic fields 
            var openSchemaFields = schemaFields.Where(f => !string.IsNullOrWhiteSpace(f.Path)).ToList();
            foreach (var field in openSchemaFields)
            { 
                var eProp = dJonProperties.FirstOrDefault(p=> p.Name == field.Name);
                if (eProp == null && field.Required)
                {
                    _validationErrors.AddModelError(field.Name, $"The ({field.Name}) field is missing on path ({field.Path})");
                    continue;
                } 

                ValidateJsonProperty(eProp, field);
            }
            return _validationErrors;

        }
        private void AddError(string key, string message)
        {
            _validationErrors.AddModelError(key, message);
        } 
        private void ValidateJsonProperty(JProperty jProperty, Field field)
        {
            if(jProperty == null)
            {
                return;
            }
            if (field.Required && jProperty.Value == null)
            {
                _validationErrors.AddModelError(field.Name, $"{field.Name} is required");
                return;
            }

            if (jProperty.Value.Type == JTokenType.Array ||
                jProperty.Value.Type == JTokenType.Object)
            {                 
                return;
            }

            var pValue =  jProperty.Value.ToString();
            if (!string.IsNullOrWhiteSpace(field.ValidationRegx) &&
                !string.IsNullOrWhiteSpace(pValue))
            {
                Match match = Regex.Match(pValue, field.ValidationRegx, RegexOptions.IgnoreCase);
                if (!match.Success)
                {
                    _validationErrors.AddModelError(field.Name, $"The ({field.Name}) field is not complaint with Regx ({field.ValidationRegx})");
                    return;
                }
            }
            if (!string.IsNullOrWhiteSpace(field.ClrType) && 
                !string.IsNullOrWhiteSpace(pValue))
            {
                try
                {
                    var pType = GetType(field.ClrType);
                    if (pType == typeof(string) && 
                        field.MaxLength.HasValue &&
                        pValue.Length > field.MaxLength.Value)
                    {
                        _validationErrors.AddModelError(field.Name, $"The allowed max length for field ({field.Name}) is ({field.MaxLength.Value})");
                    }
                    else
                    { 
                        Convert.ChangeType(pValue, pType, System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
                catch
                {
                    _validationErrors.AddModelError(field.Name, $"The ({field.Name}) must be type of ({field.ClrType})");
                }
            }
        }
        private Type GetType(string typeName)
        {
            switch (typeName.ToLowerInvariant())
            {
                case "string":
                case "guid":
                case "json": 
                    return typeof(string);
                case "bool":
                case "boolean":
                    return typeof(bool);
                case "byte":
                    return typeof(byte); 
                case "int":
                    return typeof(int);
                case "long":
                case "int64":
                    return typeof(long);
                case "decimal":
                    return typeof(decimal);
                case "double":
                    return typeof(double);
                case "datetime":
                    return typeof(DateTime);
                case "timespan":
                    return typeof(TimeSpan);
                default:
                    return typeof(string);
            }
        } 
       
    }
}
