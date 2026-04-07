using Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Exceptions
{
    public class BusinessLogicException : BaseApplicationException
    {
        public BusinessLogicException(
            string message,
            string module,
            AppErrorCode errorCode = AppErrorCode.BusinessLogicError)
            : base(message, module, errorCode, StatusCodes.Status400BadRequest)
        {
        }

        public BusinessLogicException(
            string message,
            Exception innerException,
            string module,
            AppErrorCode errorCode = AppErrorCode.BusinessLogicError)
            : base(message, innerException, module, errorCode, StatusCodes.Status400BadRequest)
        {
        }
    }
}
