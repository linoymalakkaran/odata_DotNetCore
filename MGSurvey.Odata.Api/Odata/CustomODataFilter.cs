using System;
using System.Reflection;
using System.Linq.Expressions;
using Microsoft.OData.UriParser;
using MGSurvey.Infrastructure.Database;
using Microsoft.AspNet.OData.Query.Expressions;
using System.Linq;

namespace MGSurvey.Odata.Api
{
    public class CustomODataFilter : FilterBinder
    {
        public CustomODataFilter(IServiceProvider requestContainer)
            : base(requestContainer)
        {
        }

        public override Expression BindDynamicPropertyAccessQueryNode(SingleValueOpenPropertyAccessNode openNode)
        {
            string fieldPath = openNode.Name;
            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            fieldPath = fieldPath.Replace("__", ".");

            //add JSON_VALUE support to open types   
            PropertyInfo prop = GetDynamicPropertyContainer(openNode);
            //get parameter expression 
            var source = Bind(openNode.Source);
            var propertyExp = Expression.Property(source, prop);
            //call JSON_VALUE function on this open property with user selected key
            var jsonEXP = Expression.Call(
                 typeof(DbJsonValueExtensions).GetMethod(
                     nameof(DbJsonValueExtensions.JSON_VALUE),
                     new Type[] { typeof(string), typeof(string) }),
                  Expression.Convert(propertyExp, typeof(string)),
                  Expression.Constant(fieldPath));
            return jsonEXP;

        }
        public override Expression BindConvertNode(ConvertNode convertNode)
        {

            if (convertNode.Source is SingleValueOpenPropertyAccessNode)
            {

                if (convertNode.TypeReference.Definition.TypeKind == Microsoft.OData.Edm.EdmTypeKind.Primitive)
                {
                    var defType = convertNode.TypeReference.Definition.GetType();
                    var primitiveKindProp = defType.GetProperties().FirstOrDefault(p => p.Name == "PrimitiveKind");
                    var pKind = (Microsoft.OData.Edm.EdmPrimitiveTypeKind)primitiveKindProp.GetValue(convertNode.TypeReference.Definition);
                    Expression source = Bind(convertNode.Source);
                    var ctype = GetClrType(pKind);
                    if (ctype != typeof(string))
                    {
                        return Expression.Convert(
                            Expression.Convert(source, typeof(object)),
                            ctype);
                    }

                }

            }
            return base.BindConvertNode(convertNode);
        }

        private Type GetClrType(Microsoft.OData.Edm.EdmPrimitiveTypeKind typeKind)
        {
            switch (typeKind)
            {
                case Microsoft.OData.Edm.EdmPrimitiveTypeKind.Int32:
                    return typeof(int);
                case Microsoft.OData.Edm.EdmPrimitiveTypeKind.Int64:
                    return typeof(Int64);
                case Microsoft.OData.Edm.EdmPrimitiveTypeKind.Single:
                    return typeof(float);
                case Microsoft.OData.Edm.EdmPrimitiveTypeKind.Double:
                    return typeof(double);
                case Microsoft.OData.Edm.EdmPrimitiveTypeKind.Decimal:
                    return typeof(decimal);
                case Microsoft.OData.Edm.EdmPrimitiveTypeKind.Byte:
                    return typeof(byte);
                case Microsoft.OData.Edm.EdmPrimitiveTypeKind.DateTimeOffset:
                    return typeof(DateTimeOffset);
                case Microsoft.OData.Edm.EdmPrimitiveTypeKind.Duration:
                    return typeof(TimeSpan);
                case Microsoft.OData.Edm.EdmPrimitiveTypeKind.TimeOfDay:
                    return typeof(Microsoft.OData.Edm.TimeOfDay);
                case Microsoft.OData.Edm.EdmPrimitiveTypeKind.Geography:
                    return typeof(Microsoft.Spatial.Geography);
                case Microsoft.OData.Edm.EdmPrimitiveTypeKind.None:
                    return typeof(string);
                case Microsoft.OData.Edm.EdmPrimitiveTypeKind.Binary:
                    return typeof(byte[]);
                case Microsoft.OData.Edm.EdmPrimitiveTypeKind.Boolean:
                    return typeof(bool);
                case Microsoft.OData.Edm.EdmPrimitiveTypeKind.Date:
                    return typeof(DateTime);
                case Microsoft.OData.Edm.EdmPrimitiveTypeKind.Guid:
                    return typeof(string);
                case Microsoft.OData.Edm.EdmPrimitiveTypeKind.Int16:
                    return typeof(Int16);
                case Microsoft.OData.Edm.EdmPrimitiveTypeKind.SByte:
                    return typeof(sbyte);
                case Microsoft.OData.Edm.EdmPrimitiveTypeKind.String:
                    return typeof(string);
                default:
                    return typeof(string);


            }
        }
        private DateTime? GetDateFieldValue(string value)
        {
            DateTime dateValue;

            if (DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out dateValue))
                return dateValue;

            else if (DateTime.TryParseExact(value, "MM/dd/yyyy", System.Globalization.CultureInfo.CurrentCulture,
               System.Globalization.DateTimeStyles.None, out dateValue))
                return dateValue;

            else if (DateTime.TryParseExact(value, "MM-dd-yyyy", System.Globalization.CultureInfo.CurrentCulture,
             System.Globalization.DateTimeStyles.None, out dateValue))
                return dateValue;

            else if (DateTime.TryParse(value, System.Globalization.CultureInfo.CurrentCulture,
               System.Globalization.DateTimeStyles.None, out dateValue))
                return dateValue;

            return null;
        }
    }
}