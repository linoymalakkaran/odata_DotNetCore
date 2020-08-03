
using System;
using Microsoft.EntityFrameworkCore.Query;

namespace MGSurvey.Infrastructure.Database
{
    public static class DbJsonValueExtensions
    { 
        public static string JSON_VALUE(string column, [NotParameterized] string path)
        {
            throw new InvalidOperationException("JSON_VALUE method is for use with Entity Framework Core only and has no in-memory implementation.");
        }

    } 
}