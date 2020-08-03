using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MGSurvey.Infrastructure.Database
{
    public class PredicateBuilder<T>
    {
        private string DynamicFieldName = "EntityData";
        private ParameterExpression ParamExp = Expression.Parameter(typeof(T), "x");
        private static PredicateBuilder<T> _builder = null;
        private PredicateBuilder()
        {

        }

        public static PredicateBuilder<T> Builder
        {
            get
            {
                if (_builder == null)
                {
                    _builder = new PredicateBuilder<T>();
                }
                return _builder;
            }
        }
        #region "StartsWith"
        public Expression<Func<T, bool>> StartsWith(string fieldPath, string fieldValue)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, DynamicFieldName);
            var jsonEXP = Expression.Call(
               Expression.Call(
                                typeof(DbJsonValueExtensions).GetMethod(
                                 nameof(DbJsonValueExtensions.JSON_VALUE),
                                 new Type[] { typeof(string), typeof(string) }),
                                Expression.Convert(memberExpr, typeof(string)),
                                Expression.Constant(fieldPath)),
                typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) }),
                    Expression.Constant(fieldValue)
                );


            return Expression.Lambda<Func<T, bool>>(jsonEXP, ParamExp);
        }
        public Expression<Func<T, bool>> StartsWith(string dynamicFieldName, string fieldPath, string fieldValue)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, dynamicFieldName);
            var jsonEXP = Expression.Call(
                Expression.Call(
                                 typeof(DbJsonValueExtensions).GetMethod(
                                  nameof(DbJsonValueExtensions.JSON_VALUE),
                                  new Type[] { typeof(string), typeof(string) }),
                                 Expression.Convert(memberExpr, typeof(string)),
                                 Expression.Constant(fieldPath)),
                 typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) }),
                     Expression.Constant(fieldValue)
                 );


            return Expression.Lambda<Func<T, bool>>(jsonEXP, ParamExp);
        }

        #endregion

        #region "EndsWith"
        public Expression<Func<T, bool>> EndsWith(string fieldPath, string fieldValue)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, DynamicFieldName);
            var jsonEXP = Expression.Call(
                Expression.Call(
                                 typeof(DbJsonValueExtensions).GetMethod(
                                  nameof(DbJsonValueExtensions.JSON_VALUE),
                                  new Type[] { typeof(string), typeof(string) }),
                                 Expression.Convert(memberExpr, typeof(string)),
                                 Expression.Constant(fieldPath)),
                 typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) }),
                     Expression.Constant(fieldValue)
                 );


            return Expression.Lambda<Func<T, bool>>(jsonEXP, ParamExp);
        }
        public Expression<Func<T, bool>> EndsWith(string dynamicFieldName, string fieldPath, string fieldValue)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, dynamicFieldName);
            var jsonEXP = Expression.Call(
                Expression.Call(
                                 typeof(DbJsonValueExtensions).GetMethod(
                                  nameof(DbJsonValueExtensions.JSON_VALUE),
                                  new Type[] { typeof(string), typeof(string) }),
                                 Expression.Convert(memberExpr, typeof(string)),
                                 Expression.Constant(fieldPath)),
                 typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) }),
                     Expression.Constant(fieldValue)
                 );


            return Expression.Lambda<Func<T, bool>>(jsonEXP, ParamExp);
        }

        #endregion

        #region "Contains"
        public Expression<Func<T, bool>> Contains(string fieldPath, string fieldValue)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, DynamicFieldName);
            var jsonEXP = Expression.Call(
                Expression.Call(
                                 typeof(DbJsonValueExtensions).GetMethod(
                                  nameof(DbJsonValueExtensions.JSON_VALUE),
                                  new Type[] { typeof(string), typeof(string) }),
                                 Expression.Convert(memberExpr, typeof(string)),
                                 Expression.Constant(fieldPath)),
                 typeof(string).GetMethod("Contains", new Type[] { typeof(string) }),
                     Expression.Constant(fieldValue)
                 );


            return Expression.Lambda<Func<T, bool>>(jsonEXP, ParamExp);
        }
        public Expression<Func<T, bool>> Contains(string dynamicFieldName, string fieldPath, string fieldValue)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, dynamicFieldName);
            var jsonEXP = Expression.Call(
                Expression.Call(
                                 typeof(DbJsonValueExtensions).GetMethod(
                                  nameof(DbJsonValueExtensions.JSON_VALUE),
                                  new Type[] { typeof(string), typeof(string) }),
                                 Expression.Convert(memberExpr, typeof(string)),
                                 Expression.Constant(fieldPath)),
                 typeof(string).GetMethod("Contains", new Type[] { typeof(string) }),
                     Expression.Constant(fieldValue)
                 );


            return Expression.Lambda<Func<T, bool>>(jsonEXP, ParamExp);
        }

        #endregion

        #region "Equals"
        public Expression<Func<T, bool>> Equal<TField>(string fieldPath, TField fieldValue)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, DynamicFieldName);
            var jsonEXP = Expression.Equal(
                Expression.Convert(
                    Expression.Convert(
                       Expression.Call(
                               typeof(DbJsonValueExtensions).GetMethod(
                                nameof(DbJsonValueExtensions.JSON_VALUE),
                                new Type[] { typeof(string), typeof(string) }),
                               Expression.Convert(memberExpr, typeof(string)),
                               Expression.Constant(fieldPath)),
                       typeof(object)),
                 typeof(TField)),
               Expression.Constant(fieldValue));

            //var jsonEXP = Expression.Equal(
            //         Expression.Convert(
            //             Expression.Call(changeTypeMethod,
            //                 Expression.Call(
            //                            typeof(DbJsonValueExtensions).GetMethod(
            //                             nameof(DbJsonValueExtensions.JSON_VALUE),
            //                             new Type[] { typeof(string), typeof(string) }),
            //                            Expression.Convert(memberExpr, typeof(string)),
            //                            Expression.Constant(fieldPath)),
            //                Expression.Constant(Type.GetTypeCode(typeof(TField)))),
            //        typeof(TField)),
            //    Expression.Constant(fieldValue));

            //var jsonEXP = Expression.Equal(

            //       Expression.Call(
            //               typeof(DbJsonValueExtensions).GetMethod(
            //                nameof(DbJsonValueExtensions.JSON_VALUE),
            //                new Type[] { typeof(string), typeof(string) }),
            //               Expression.Convert(memberExpr, typeof(string)),
            //               Expression.Constant(fieldPath)),
            //   Expression.Constant(fieldValue));



            return Expression.Lambda<Func<T, bool>>(jsonEXP, ParamExp);
        }
        public Expression<Func<T, bool>> Equal<TField>(string dynamicFieldName, string fieldPath, TField fieldValue)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, dynamicFieldName);
            var jsonEXP = Expression.Equal(
              Expression.Convert(
                  Expression.Convert(
                     Expression.Call(
                             typeof(DbJsonValueExtensions).GetMethod(
                              nameof(DbJsonValueExtensions.JSON_VALUE),
                              new Type[] { typeof(string), typeof(string) }),
                             Expression.Convert(memberExpr, typeof(string)),
                             Expression.Constant(fieldPath)),
                     typeof(object)),
               typeof(TField)),
             Expression.Constant(fieldValue));


            return Expression.Lambda<Func<T, bool>>(jsonEXP, ParamExp);
        }
        public Expression<Func<T, bool>> Equal(string fieldPath, string fieldValue)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, DynamicFieldName);
            var jsonEXP = Expression.Equal(
                      Expression.Call(
                              typeof(DbJsonValueExtensions).GetMethod(
                               nameof(DbJsonValueExtensions.JSON_VALUE),
                               new Type[] { typeof(string), typeof(string) }),
                              Expression.Convert(memberExpr, typeof(string)),
                              Expression.Constant(fieldPath)),
                        Expression.Constant(fieldValue));
            return Expression.Lambda<Func<T, bool>>(jsonEXP, ParamExp);
        }
        public Expression<Func<T, bool>> Equal(string dynamicFieldName, string fieldPath, string fieldValue)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, dynamicFieldName);
            var jsonEXP = Expression.Equal(
                       Expression.Call(
                               typeof(DbJsonValueExtensions).GetMethod(
                                nameof(DbJsonValueExtensions.JSON_VALUE),
                                new Type[] { typeof(string), typeof(string) }),
                               Expression.Convert(memberExpr, typeof(string)),
                               Expression.Constant(fieldPath)),
                         Expression.Constant(fieldValue));


            return Expression.Lambda<Func<T, bool>>(jsonEXP, ParamExp);
        }

        #endregion

        #region "NotEqual"
        public Expression<Func<T, bool>> NotEqual<TField>(string fieldPath, TField fieldValue)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, DynamicFieldName);
            var jsonEXP = Expression.NotEqual(
                Expression.Convert(
                    Expression.Convert(
                       Expression.Call(
                               typeof(DbJsonValueExtensions).GetMethod(
                                nameof(DbJsonValueExtensions.JSON_VALUE),
                                new Type[] { typeof(string), typeof(string) }),
                               Expression.Convert(memberExpr, typeof(string)),
                               Expression.Constant(fieldPath)),
                       typeof(object)),
                 typeof(TField)),
               Expression.Constant(fieldValue));

            return Expression.Lambda<Func<T, bool>>(jsonEXP, ParamExp);
        }
        public Expression<Func<T, bool>> NotEqual<TField>(string dynamicFieldName, string fieldPath, TField fieldValue)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, dynamicFieldName);
            var jsonEXP = Expression.NotEqual(
              Expression.Convert(
                  Expression.Convert(
                     Expression.Call(
                             typeof(DbJsonValueExtensions).GetMethod(
                              nameof(DbJsonValueExtensions.JSON_VALUE),
                              new Type[] { typeof(string), typeof(string) }),
                             Expression.Convert(memberExpr, typeof(string)),
                             Expression.Constant(fieldPath)),
                     typeof(object)),
               typeof(TField)),
             Expression.Constant(fieldValue));


            return Expression.Lambda<Func<T, bool>>(jsonEXP, ParamExp);
        }
        public Expression<Func<T, bool>> NotEqual(string fieldPath, string fieldValue)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, DynamicFieldName);
            var jsonEXP = Expression.NotEqual(
                      Expression.Call(
                              typeof(DbJsonValueExtensions).GetMethod(
                               nameof(DbJsonValueExtensions.JSON_VALUE),
                               new Type[] { typeof(string), typeof(string) }),
                              Expression.Convert(memberExpr, typeof(string)),
                              Expression.Constant(fieldPath)),
                        Expression.Constant(fieldValue));
            return Expression.Lambda<Func<T, bool>>(jsonEXP, ParamExp);
        }
        public Expression<Func<T, bool>> NotEqual(string dynamicFieldName, string fieldPath, string fieldValue)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, dynamicFieldName);
            var jsonEXP = Expression.NotEqual(
                       Expression.Call(
                               typeof(DbJsonValueExtensions).GetMethod(
                                nameof(DbJsonValueExtensions.JSON_VALUE),
                                new Type[] { typeof(string), typeof(string) }),
                               Expression.Convert(memberExpr, typeof(string)),
                               Expression.Constant(fieldPath)),
                         Expression.Constant(fieldValue));


            return Expression.Lambda<Func<T, bool>>(jsonEXP, ParamExp);
        }

        #endregion

        #region "Greater"
        public Expression<Func<T, bool>> GreaterThan<TField>(string fieldPath, TField fieldValue)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, DynamicFieldName);
            var jsonEXP = Expression.GreaterThan(
                Expression.Convert(
                    Expression.Convert(
                       Expression.Call(
                               typeof(DbJsonValueExtensions).GetMethod(
                                nameof(DbJsonValueExtensions.JSON_VALUE),
                                new Type[] { typeof(string), typeof(string) }),
                               Expression.Convert(memberExpr, typeof(string)),
                               Expression.Constant(fieldPath)),
                       typeof(object)),
                 typeof(TField)),
               Expression.Constant(fieldValue));

            return Expression.Lambda<Func<T, bool>>(jsonEXP, ParamExp);
        }
        public Expression<Func<T, bool>> GreaterThan<TField>(string dynamicFieldName, string fieldPath, TField fieldValue)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, dynamicFieldName);
            var jsonEXP = Expression.GreaterThan(
              Expression.Convert(
                  Expression.Convert(
                     Expression.Call(
                             typeof(DbJsonValueExtensions).GetMethod(
                              nameof(DbJsonValueExtensions.JSON_VALUE),
                              new Type[] { typeof(string), typeof(string) }),
                             Expression.Convert(memberExpr, typeof(string)),
                             Expression.Constant(fieldPath)),
                     typeof(object)),
               typeof(TField)),
             Expression.Constant(fieldValue));


            return Expression.Lambda<Func<T, bool>>(jsonEXP, ParamExp);
        }
        public Expression<Func<T, bool>> GreaterThanOrEqual<TField>(string fieldPath, TField fieldValue)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, DynamicFieldName);
            var jsonEXP = Expression.GreaterThanOrEqual(
                Expression.Convert(
                    Expression.Convert(
                       Expression.Call(
                               typeof(DbJsonValueExtensions).GetMethod(
                                nameof(DbJsonValueExtensions.JSON_VALUE),
                                new Type[] { typeof(string), typeof(string) }),
                               Expression.Convert(memberExpr, typeof(string)),
                               Expression.Constant(fieldPath)),
                       typeof(object)),
                 typeof(TField)),
               Expression.Constant(fieldValue));

            return Expression.Lambda<Func<T, bool>>(jsonEXP, ParamExp);
        }
        public Expression<Func<T, bool>> GreaterThanOrEqual<TField>(string dynamicFieldName, string fieldPath, TField fieldValue)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, dynamicFieldName);
            var jsonEXP = Expression.GreaterThanOrEqual(
              Expression.Convert(
                  Expression.Convert(
                     Expression.Call(
                             typeof(DbJsonValueExtensions).GetMethod(
                              nameof(DbJsonValueExtensions.JSON_VALUE),
                              new Type[] { typeof(string), typeof(string) }),
                             Expression.Convert(memberExpr, typeof(string)),
                             Expression.Constant(fieldPath)),
                     typeof(object)),
               typeof(TField)),
             Expression.Constant(fieldValue));


            return Expression.Lambda<Func<T, bool>>(jsonEXP, ParamExp);
        }

        #endregion

        #region "Less"
        public Expression<Func<T, bool>> LessThan<TField>(string fieldPath, TField fieldValue)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, DynamicFieldName);
            var jsonEXP = Expression.LessThan(
                Expression.Convert(
                    Expression.Convert(
                       Expression.Call(
                               typeof(DbJsonValueExtensions).GetMethod(
                                nameof(DbJsonValueExtensions.JSON_VALUE),
                                new Type[] { typeof(string), typeof(string) }),
                               Expression.Convert(memberExpr, typeof(string)),
                               Expression.Constant(fieldPath)),
                       typeof(object)),
                 typeof(TField)),
               Expression.Constant(fieldValue));

            return Expression.Lambda<Func<T, bool>>(jsonEXP, ParamExp);
        }
        public Expression<Func<T, bool>> LessThan<TField>(string dynamicFieldName, string fieldPath, TField fieldValue)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, dynamicFieldName);
            var jsonEXP = Expression.LessThan(
              Expression.Convert(
                  Expression.Convert(
                     Expression.Call(
                             typeof(DbJsonValueExtensions).GetMethod(
                              nameof(DbJsonValueExtensions.JSON_VALUE),
                              new Type[] { typeof(string), typeof(string) }),
                             Expression.Convert(memberExpr, typeof(string)),
                             Expression.Constant(fieldPath)),
                     typeof(object)),
               typeof(TField)),
             Expression.Constant(fieldValue));


            return Expression.Lambda<Func<T, bool>>(jsonEXP, ParamExp);
        }
        public Expression<Func<T, bool>> LessThanOrEqual<TField>(string fieldPath, TField fieldValue)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, DynamicFieldName);
            var jsonEXP = Expression.LessThanOrEqual(
                Expression.Convert(
                    Expression.Convert(
                       Expression.Call(
                               typeof(DbJsonValueExtensions).GetMethod(
                                nameof(DbJsonValueExtensions.JSON_VALUE),
                                new Type[] { typeof(string), typeof(string) }),
                               Expression.Convert(memberExpr, typeof(string)),
                               Expression.Constant(fieldPath)),
                       typeof(object)),
                 typeof(TField)),
               Expression.Constant(fieldValue));

            return Expression.Lambda<Func<T, bool>>(jsonEXP, ParamExp);
        }
        public Expression<Func<T, bool>> LessThanOrEqual<TField>(string dynamicFieldName, string fieldPath, TField fieldValue)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, dynamicFieldName);
            var jsonEXP = Expression.LessThanOrEqual(
              Expression.Convert(
                  Expression.Convert(
                     Expression.Call(
                             typeof(DbJsonValueExtensions).GetMethod(
                              nameof(DbJsonValueExtensions.JSON_VALUE),
                              new Type[] { typeof(string), typeof(string) }),
                             Expression.Convert(memberExpr, typeof(string)),
                             Expression.Constant(fieldPath)),
                     typeof(object)),
               typeof(TField)),
             Expression.Constant(fieldValue));


            return Expression.Lambda<Func<T, bool>>(jsonEXP, ParamExp);
        }

        #endregion

        #region "GetJsonFieldValue"
        public Expression<Func<T, string>> GetJsonFieldValue(string fieldPath)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, DynamicFieldName);
            var jsonEXP = Expression.Call(
                               typeof(DbJsonValueExtensions).GetMethod(
                                nameof(DbJsonValueExtensions.JSON_VALUE),
                                new Type[] { typeof(string), typeof(string) }),
                               Expression.Convert(memberExpr, typeof(string)),
                               Expression.Constant(fieldPath));


            return Expression.Lambda<Func<T, string>>(jsonEXP, ParamExp);
        }
        public Expression<Func<T, string>> GetJsonFieldValue(string dynamicFieldName, string fieldPath)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, dynamicFieldName);
            var jsonEXP = Expression.Call(
                               typeof(DbJsonValueExtensions).GetMethod(
                                nameof(DbJsonValueExtensions.JSON_VALUE),
                                new Type[] { typeof(string), typeof(string) }),
                               Expression.Convert(memberExpr, typeof(string)),
                               Expression.Constant(fieldPath));


            return Expression.Lambda<Func<T, string>>(jsonEXP, ParamExp);
        }
        public Expression<Func<T, TField>> GetJsonFieldValue<TField>(string fieldPath)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, DynamicFieldName);
            var jsonEXP = Expression.Convert(
                    Expression.Convert(
                       Expression.Call(
                               typeof(DbJsonValueExtensions).GetMethod(
                                nameof(DbJsonValueExtensions.JSON_VALUE),
                                new Type[] { typeof(string), typeof(string) }),
                               Expression.Convert(memberExpr, typeof(string)),
                               Expression.Constant(fieldPath)),
                       typeof(object)),
                 typeof(TField));


            return Expression.Lambda<Func<T, TField>>(jsonEXP, ParamExp);
        }
        public Expression<Func<T, TField>> GetJsonFieldValue<TField>(string dynamicFieldName, string fieldPath)
        {
            if (string.IsNullOrEmpty(fieldPath))
                throw new ArgumentNullException(nameof(fieldPath));

            if (!fieldPath.StartsWith("$."))
                fieldPath = $"$.{fieldPath}";

            var memberExpr = Expression.Property(ParamExp, dynamicFieldName);
            var jsonEXP = Expression.Convert(
                    Expression.Convert(
                       Expression.Call(
                               typeof(DbJsonValueExtensions).GetMethod(
                                nameof(DbJsonValueExtensions.JSON_VALUE),
                                new Type[] { typeof(string), typeof(string) }),
                               Expression.Convert(memberExpr, typeof(string)),
                               Expression.Constant(fieldPath)),
                       typeof(object)),
                 typeof(TField));


            return Expression.Lambda<Func<T, TField>>(jsonEXP, ParamExp);
        }
        #endregion


        public Expression<Func<T, bool>> Or<TField>(Expression<Func<T, TField>> left, Expression<Func<T, TField>> right)
        {
            var exp = Expression.Or(
                left.Body,
                right.Body);
            return Expression.Lambda<Func<T, bool>>(exp, ParamExp);
        }
        public Expression<Func<T, bool>> And<TField>(Expression<Func<T, TField>> left, Expression<Func<T, TField>> right)
        {
            var exp = Expression.AndAlso(
                left.Body,
                right.Body);
            return Expression.Lambda<Func<T, bool>>(exp, ParamExp);
        }
        public Expression<Func<T, bool>> Equal<TField>(Expression<Func<T, TField>> left, Expression<Func<T, TField>> right)
        {
            var exp = Expression.Equal(
                left.Body,
                right.Body);
            return Expression.Lambda<Func<T, bool>>(exp, ParamExp);
        }
        public Expression<Func<T, bool>> NotEqual<TField>(Expression<Func<T, TField>> left, Expression<Func<T, TField>> right)
        {
            var exp = Expression.NotEqual(
                left.Body,
                right.Body);
            return Expression.Lambda<Func<T, bool>>(exp, ParamExp);
        }
        public Expression<Func<T, bool>> GreaterThan<TField>(Expression<Func<T, TField>> left, Expression<Func<T, TField>> right)
        {
            var exp = Expression.GreaterThan(
                left.Body,
                right.Body);
            return Expression.Lambda<Func<T, bool>>(exp, ParamExp);
        }
        public Expression<Func<T, bool>> GreaterThanOrEqual<TField>(Expression<Func<T, TField>> left, Expression<Func<T, TField>> right)
        {
            var exp = Expression.GreaterThanOrEqual(
                left.Body,
                right.Body);
            return Expression.Lambda<Func<T, bool>>(exp, ParamExp);
        }
        public Expression<Func<T, bool>> LessThan<TField>(Expression<Func<T, TField>> left, Expression<Func<T, TField>> right)
        {
            var exp = Expression.LessThan(
                left.Body,
                right.Body);
            return Expression.Lambda<Func<T, bool>>(exp, ParamExp);
        }
        public Expression<Func<T, bool>> LessThanOrEqual<TField>(Expression<Func<T, TField>> left, Expression<Func<T, TField>> right)
        {
            var exp = Expression.LessThanOrEqual(
                left.Body,
                right.Body);
            return Expression.Lambda<Func<T, bool>>(exp, ParamExp);
        }
    }
}
