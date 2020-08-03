using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MGSurvey.Infrastructure.Database
{ 
    public class DbInterceptor : DbCommandInterceptor
    {
       public override InterceptionResult<DbDataReader> ReaderExecuting(
                                                        DbCommand command,
                                                        CommandEventData eventData,
                                                        InterceptionResult<DbDataReader> result)
        {
            //log and optimize sql query
            var sql = command.CommandText;
           // StringBuilder sqlCommand = new StringBuilder(command.CommandText);
            return base.ReaderExecuting(command, eventData, result);
        }
        public override Task<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
                                                                DbCommand command, 
                                                                CommandEventData eventData, 
                                                                InterceptionResult<DbDataReader> result,
                                                                CancellationToken cancellationToken = default)
        {
            //log and optimize sql query
            var sql = command.CommandText;
            //  StringBuilder sqlCommand = new StringBuilder(command.CommandText);  
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }
    }
}
