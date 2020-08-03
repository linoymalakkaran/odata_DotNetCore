
using System;
using System.IO;
using System.Linq;
using MGSurvey.Utilities;
using Newtonsoft.Json;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MGSurvey.Odata.Api;

namespace MGSurvey.Odata.Api
{
    public class CustomOdataModelBinder : IModelBinder
    {
        private readonly ModelBinderOptions _modelBinderOptions;
        private ModelStateDictionary _modelState;
        private string _userName = "";
        private string _requestMethod;
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }
            _modelState = bindingContext.ModelState ?? new ModelStateDictionary();
            _userName = bindingContext.HttpContext.User?.Identity?.Name ?? "OdataAdminApi";
            _requestMethod = bindingContext.HttpContext.Request.Method;
            string rawBody = "";
            try
            {
                if (bindingContext.HttpContext.Request.Body.CanRead)
                {
                    using (StreamReader reader = new StreamReader(bindingContext.HttpContext.Request.Body, System.Text.Encoding.UTF8))
                    {
                        rawBody = await reader.ReadToEndAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _modelState.AddModelError("RequestBodyException", ex.Message);
                bindingContext.ModelState = _modelState;
                bindingContext.Result = ModelBindingResult.Failed();

            }
            if (!string.IsNullOrWhiteSpace(rawBody))
            {
                try
                {
                    //bind json to model
                    var jsonBody = JsonConvert.DeserializeObject<JObject>(rawBody);
                    var modelType = bindingContext.ModelType;
                    var modelProperties = modelType.GetProperties().ToList();
                    var model = Activator.CreateInstance(modelType);
                    Bind(modelType, jsonBody, model);
                    if (_modelState.IsValid)
                        bindingContext.Result = ModelBindingResult.Success(model);
                    else
                        bindingContext.Result = ModelBindingResult.Failed();
                }
                catch (Exception ex)
                {
                    _modelState.AddModelError("ModelBindingException", ex.Message);
                    bindingContext.ModelState = _modelState;
                    bindingContext.Result = ModelBindingResult.Failed();
                }
            }
        }

        private void Bind(Type modelType, JObject jObject, object model)
        {
            //base conditions
            if (modelType == null || jObject == null || model == null)
            {
                return;
            }

            var jsonProperties = jObject.Properties().ToList();
            var modelProperties = modelType.GetProperties().ToList();
            var dynamicProp = modelProperties.FirstOrDefault(p => IsDictionaryProperty(p));
            var entityData = new Dictionary<string, object>();
            //Bind model owned properties
            BindOwnedProperties(jsonProperties, modelProperties, model);
            //bind open properties 
            BindOpenProperties(modelProperties, jObject, model);
            //map navigational properties
            foreach (var mProp in modelProperties)
            {
                if (IsNavigationProperty(mProp))
                {
                    var jProp = jsonProperties.FirstOrDefault(p => p.Name.Equals(mProp.Name, StringComparison.OrdinalIgnoreCase));
                    BindNavigationProperty(mProp, jProp, model);
                }

            }
        }

        private void BindNavigationProperty(PropertyInfo mProp, JProperty jProp, object model)
        {
            try
            {
                //base conditions
                if (mProp == null || jProp == null || model == null)
                {
                    return;
                }

                if (jProp.Value != null &&
                    jProp.Value.Type != JTokenType.Null &&
                    jProp.Value.Type != JTokenType.None)
                {
                    if (!mProp.PropertyType.IsGenericType && jProp.Value.Type == JTokenType.Object)
                    {
                        var navPropModel = Activator.CreateInstance(mProp.PropertyType);
                        Bind(mProp.PropertyType, (JObject)jProp.Value, navPropModel);
                        mProp.SetValue(model, navPropModel);
                    }
                    else if (mProp.PropertyType.IsGenericType && jProp.Value.Type == JTokenType.Array)
                    {
                        object collection = GetCollectionInstance(mProp);
                        if (collection != null)
                        {
                            var navPropModelType = mProp.PropertyType.GetGenericArguments()[0];
                            MethodInfo addMethod = collection.GetType().GetMethod("Add", new Type[] { navPropModelType });
                            var jArray = (JArray)jProp.Value;
                            foreach (var jObj in jArray)
                            {
                                var navPropModel = Activator.CreateInstance(navPropModelType);
                                Bind(navPropModelType, (JObject)jObj, navPropModel);
                                if (addMethod != null)
                                {
                                    addMethod.Invoke(collection, new object[] { navPropModel });
                                }
                            }
                        }
                        mProp.SetValue(model, collection);
                    }
                }
                else
                {
                    if (!mProp.PropertyType.IsGenericType)
                    {
                        //defult to null
                        mProp.SetValue(model, null);
                    }
                    else
                    {
                        //collections should not be null 
                        mProp.SetValue(model, GetCollectionInstance(mProp));
                    }
                }
            }
            catch (Exception ex)
            {

                _modelState.AddModelError("BindNavigationPropertyException", $"PropertyName: {mProp.Name} Error: {ex.Message}");
            }
        }

        private void BindOwnedProperties(List<JProperty> jsonProperties, List<PropertyInfo> modelProperties, object model)
        {
            try
            {
                if (model == null ||
                    jsonProperties == null ||
                    jsonProperties.Count == 0 ||
                    modelProperties == null ||
                    modelProperties.Count == 0)
                {
                    return;
                }

                //bind non navigational root properties 
                foreach (var mProp in modelProperties)
                {
                    if (!IsDictionaryProperty(mProp) &&
                        !IsNavigationProperty(mProp))
                    {
                        if (!SetIfDefault(mProp, model))
                        {
                            var jProp = jsonProperties.FirstOrDefault(p => p.Name.Equals(mProp.Name, StringComparison.OrdinalIgnoreCase));
                            BindProperty(mProp, jProp?.Value, model);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                _modelState.AddModelError("BindOwnedPropertiesException", ex.Message);
            }
        }

        private void BindOpenProperties(List<PropertyInfo> modelProperties, JObject jObject, object model)
        {
            try
            {
                if (model == null ||
                    jObject == null ||
                    modelProperties == null ||
                    modelProperties.Count == 0)
                {
                    return;
                }
                var jsonProperties = jObject.Properties().ToList();
                var dynamicProp = modelProperties.FirstOrDefault(p => IsDictionaryProperty(p));
                var entityData = new Dictionary<string, object>();
                //bind open properties
                foreach (var jProp in jsonProperties)
                {
                    var mProp = GetProperty(jProp.Name, modelProperties);
                    if (mProp == null)
                    {
                        //fall back check
                        if (entityData == null)
                            entityData = new Dictionary<string, object>();

                        //add to dynamic dictionary 
                        object fieldValue = GetFieldValue(jProp.Value, jProp.Name);
                        entityData.Add(jProp.Name, fieldValue);

                    }
                    //bind dynamic property
                    else if (dynamicProp != null && mProp == dynamicProp)
                    {
                        if (jProp.Value != null &&
                            jProp.Value.Type != JTokenType.Null &&
                            jProp.Value.Type == JTokenType.Object)
                        {
                            var keyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, object>>(jProp.Value.ToString(),
                                                                                              new JsonConverter[] {
                                                                                           new MGSurveyJsonConverter()
                                                                                              });
                            //fall back check
                            if (entityData == null)
                                entityData = new Dictionary<string, object>();
                            if (keyValuePairs != null && keyValuePairs.Count > 0)
                                entityData = keyValuePairs.Concat(entityData).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);

                        }
                    }
                }
                //set dynamic property dictionary
                if (dynamicProp != null)
                    dynamicProp.SetValue(model, entityData);
            }
            catch (Exception ex)
            {
                _modelState.AddModelError("BindOpenPropertiesException", ex.Message);
            }
        }

        private void BindProperty(PropertyInfo property, JToken jValue, object model)
        {
            try
            {
                if (property == null || model == null)
                    return;

                object fieldValue = null;
                if (property.PropertyType == typeof(DateTime))
                    fieldValue = GetDateFieldValue(jValue, property.Name);
                else
                    fieldValue = GetFieldValue(jValue, property.Name);

                if (fieldValue == null)
                {
                    if (IsNullAble(property.PropertyType))
                        property.SetValue(model, null);

                    else
                        property.SetValue(model, default);
                }
                else
                {
                    property.SetValue(model, fieldValue);
                }
            }
            catch (Exception ex)
            {
                _modelState.AddModelError(property.Name, ex.Message);
            }

        }
        private object GetCollectionInstance(PropertyInfo mProp)
        {
            if (mProp == null)
                return null;

            var args = mProp.PropertyType.GetGenericArguments();
            if (args == null || args.Length == 0)
            {
                _modelState.AddModelError("CollectionTypeArgs", $"The property {mProp.Name} is not collection type property");
                return null;
            }
            var navPropModelType = args[0];
            if (mProp.PropertyType.IsAssignableFrom(typeof(HashSet<>).MakeGenericType(navPropModelType)))
            {
                return Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(navPropModelType));
            }
            else if (mProp.PropertyType.IsAssignableFrom(typeof(List<>).MakeGenericType(navPropModelType)))
            {
                return Activator.CreateInstance(typeof(List<>).MakeGenericType(navPropModelType));
            }
            else
            {
                _modelState.AddModelError("CollectionTypeInstance", $"The Property: {mProp.Name} with TypeName: {mProp.PropertyType.Name} is not supported collection type property");
                return null;
            }
        }
        private object GetFieldValue(JToken jValue, string propertyName)
        {
            object fieldValue = null;
            try
            {
                if (jValue == null || jValue.Type == JTokenType.Null)
                    return fieldValue;

                if (jValue.Type == JTokenType.Date || jValue.Type == JTokenType.TimeSpan)
                {
                    fieldValue = GetDateFieldValue(jValue, propertyName);
                    return fieldValue;
                }
                else if (jValue.Type == JTokenType.Boolean)
                    fieldValue = Convert.ChangeType(jValue.ToString().ToLowerInvariant(), typeof(bool), System.Globalization.CultureInfo.InvariantCulture);
                else if (jValue.Type == JTokenType.Array
                    || jValue.Type == JTokenType.Object
                    || jValue.Type == JTokenType.Bytes)
                {

                    fieldValue = jValue;
                }
                else if (jValue.Type == JTokenType.Float)
                    fieldValue = Convert.ChangeType(jValue.ToString().ToLowerInvariant(), typeof(float), System.Globalization.CultureInfo.InvariantCulture);
                else if (jValue.Type == JTokenType.Integer)
                    fieldValue = Convert.ChangeType(jValue.ToString().ToLowerInvariant(), typeof(int), System.Globalization.CultureInfo.InvariantCulture);
                else
                    fieldValue = jValue.ToString();

                return fieldValue;
            }
            catch (Exception ex)
            {
                _modelState.AddModelError("JsonValueParsingException", $"PropertName: {propertyName} JsonValue: {jValue.ToString()} ExceptionMessage:  {ex.Message}");
                return fieldValue;
            }

        }

        private DateTime? GetDateFieldValue(JToken jValue, string propertyName)
        {
            DateTime dateValue;
            if (jValue == null || jValue.Type == JTokenType.Null || jValue.Type == JTokenType.None)
            {
                return null;
            }
            if (DateTime.TryParse(jValue.ToString(), System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out dateValue))
                return dateValue;

            else if (DateTime.TryParseExact(jValue.ToString(), "MM/dd/yyyy", System.Globalization.CultureInfo.CurrentCulture,
               System.Globalization.DateTimeStyles.None, out dateValue))
                return dateValue;

            else if (DateTime.TryParseExact(jValue.ToString(), "MM-dd-yyyy", System.Globalization.CultureInfo.CurrentCulture,
             System.Globalization.DateTimeStyles.None, out dateValue))
                return dateValue;

            else if (DateTime.TryParse(jValue.ToString(), System.Globalization.CultureInfo.CurrentCulture,
               System.Globalization.DateTimeStyles.None, out dateValue))
                return dateValue;

            else
                _modelState.AddModelError(propertyName, $"Unable to convert json to datatime value,Fallback Format: 'MM/dd/yyyy', JsonValue: {jValue.ToString()}");

            return null;
        }
        private bool SetIfDefault(PropertyInfo mProp, object model)
        {
            if (IsPostRequest())
            {
                if (IsIdProperty(mProp.Name))
                {
                    mProp.SetValue(model, Guid.NewGuid().ToString());
                    return true;
                }
                else if (IsCreatedDate(mProp.Name))
                {
                    mProp.SetValue(model, DateTime.Now);
                    return true;
                }

                else if (IsCreatedBy(mProp.Name))
                {
                    mProp.SetValue(model, _userName);
                    return true;
                }
                else if (IsIsActive(mProp.Name))
                {
                    mProp.SetValue(model, true);
                    return true;
                }
            }
            else if (IsPutOrPatchRequest())
            {
                if (IsUpdatedBy(mProp.Name))
                {
                    mProp.SetValue(model, _userName);
                    return true;
                }
                else if (IsUpdatedDate(mProp.Name))
                {
                    mProp.SetValue(model, DateTime.Now);
                    return true;
                }
            }
            return false;
        }
        private PropertyInfo GetProperty(string pName, List<PropertyInfo> properties)
        {
            if (properties == null)
                return null;

            return properties.FirstOrDefault(p => p.Name.Equals(pName, StringComparison.OrdinalIgnoreCase));
        }
        private bool IsNavigationProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                return false;

            if (propertyInfo.PropertyType.IsGenericType &&
                !IsDictionaryProperty(propertyInfo))
            {
                Type[] args = propertyInfo.PropertyType.GetGenericArguments();
                //need to improve this logic
                if (args.Length == 1 && args[0].IsClass && args[0] != typeof(string))
                {
                    return true;
                }
                return false;
            }

            else if (!propertyInfo.PropertyType.IsGenericType &&
                 propertyInfo.PropertyType.IsClass &&
                 propertyInfo.PropertyType != typeof(string))
            {
                return true;
            }
            return false;
        }
        private bool IsDictionaryProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                return false;

            return propertyInfo.PropertyType.IsAssignableFrom(typeof(IDictionary<string, object>));
        }
        private bool IsNullAble(Type type)
        {
            if (Nullable.GetUnderlyingType(type) != null)
            {
                return true;
            }
            return false;
        }
        private bool IsIdProperty(string propertyName)
        {
            return string.Equals("Id", propertyName, StringComparison.OrdinalIgnoreCase);
        }
        private bool IsCreatedBy(string propertyName)
        {
            return string.Equals("CreatedBy", propertyName, StringComparison.OrdinalIgnoreCase);
        }
        private bool IsUpdatedBy(string propertyName)
        {
            return string.Equals("UpdatedBy", propertyName, StringComparison.OrdinalIgnoreCase);
        }
        private bool IsCreatedDate(string propertyName)
        {
            return string.Equals("CreatedDate", propertyName, StringComparison.OrdinalIgnoreCase);
        }
        private bool IsIsActive(string propertyName)
        {
            return string.Equals("IsActive", propertyName, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsUpdatedDate(string propertyName)
        {
            return string.Equals("UpdatedDate", propertyName, StringComparison.OrdinalIgnoreCase);
        }
        private bool IsPostRequest()
        {
            return string.Equals("Post", _requestMethod, StringComparison.OrdinalIgnoreCase);
        }
        private bool IsPutOrPatchRequest()
        {
            return string.Equals("Put", _requestMethod, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals("Patch", _requestMethod, StringComparison.OrdinalIgnoreCase);
        }
    }
}
