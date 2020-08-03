using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGSurvey.Odata.Api.Validators
{
    public class Field
    {
        public string Label { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string ClrType { get; set; }
        public bool Required { get; set; }
        public int? MaxLength { get; set; }
        public string SchemaName { get; set; }
        public string ValidationRegx { get; set; }
    }
}
