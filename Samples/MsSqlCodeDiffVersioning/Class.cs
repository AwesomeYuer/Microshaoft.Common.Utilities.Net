using FluentValidation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluentValidationExample.Validators
{
    public class JTokenValidator : AbstractValidator<JToken>
    {
        public JTokenValidator()
        {
           // RuleFor(x => x.FirstName)
           // .NotEmpty()
           // .WithMessage("FirstName is mandatory.");

           // RuleFor(x => x.LastName)
           // .NotEmpty()
           // .WithMessage("LastName is mandatory.");

           // RuleFor(x => x.SIN)
           // .NotEmpty()
           // .WithMessage("SIN is mandatory.")
           // .Must((o, list, context) =>
           // {
           //     if (null != o.SIN)
           //     {
           //         context.MessageFormatter.AppendArgument("SIN", o.SIN);
           //         return Utilities.IsValidSIN(int.Parse(o.SIN));
           //     }
           //     return true;
           // })
           //.WithMessage("SIN ({SIN}) is not valid.");
        }
    }
}
