
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGSurvey.Odata.Api
{
    public class ModelBinderOptions
    {
        public bool ValidateModelSchema { get; set; }
        public bool ThrowErrorOnEmptyModelSchema { get; set; }
        public Dictionary<string, Func<JObject, object>> CustomModelBinders = new Dictionary<string, Func<JObject, object>>();
    }
}
