
using AutoMapper;
using System.Text.Json;
using MGSurvey.Domain.Entities;
using MGSurvey.Business.Models;
using System.Collections.Generic;

namespace MGSurvey.Data.Mapper
{
    public class DisctionaryValueResolver<TSource, TDestination> : IValueResolver<TSource, TDestination, IDictionary<string, object>>
        where TSource : BaseEntity<string>
        where TDestination : BaseModel<string>
    {
        public IDictionary<string, object> Resolve(TSource source, TDestination destination, IDictionary<string, object> destMember, ResolutionContext context)
        {
            return null;
           // return JsonSerializer.Deserialize<IDictionary<string, object>>(source.EntityData);
        }
    }
    public class JsonValueResolver<TSource, TDestination> : IValueResolver<TSource, TDestination, string>
        where TSource : BaseModel<string>
        where TDestination : BaseEntity<string>
    {
        public string Resolve(TSource source, TDestination destination, string destMember, ResolutionContext context)
        {
            return JsonSerializer.Serialize(source.EntityData);
        }
    }

}
