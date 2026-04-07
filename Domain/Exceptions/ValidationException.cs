using Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Exceptions
{
    public class ValidationException : BaseApplicationException
    {
        public Dictionary<string, string[]> ValidationErrors { get; }

        public ValidationException(
            string message,
            Dictionary<string, string[]> validationErrors,
            string module = "General")
            : base(message, module, AppErrorCode.ValidationError, StatusCodes.Status400BadRequest)
        {
            ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
        }

        public ValidationException(
            string message,
            Dictionary<string, string[]> validationErrors,
            Exception innerException,
            string module = "General")
            : base(message, innerException, module, AppErrorCode.ValidationError, StatusCodes.Status400BadRequest)
        {
            ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
        }
    }
}
