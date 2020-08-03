using MGSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MGSurvey.Business.Models
{
    public class FormType
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string SecretKey { get; set; }
    }
}
