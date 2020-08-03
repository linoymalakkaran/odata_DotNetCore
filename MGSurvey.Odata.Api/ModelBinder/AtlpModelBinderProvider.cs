
using System;
using MGSurvey.Domain.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
namespace MGSurvey.Odata.Api
{
    public class MGSurveyModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.Metadata.ModelType != null &&
            context.Metadata.ModelType.BaseType == typeof(BaseEntity<string>))
        {
            
            return new BinderTypeModelBinder(typeof(CustomOdataModelBinder));
        }

        return null;
    }
}
}