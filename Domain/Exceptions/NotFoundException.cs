using Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Exceptions
{
    namespace Domain.Exceptions
    {
        public class NotFoundException : BaseApplicationException
        {
            public NotFoundException(string message, string module = "General")
                : base(message, module, AppErrorCode.NotFound, StatusCodes.Status404NotFound)
            {
            }

            public NotFoundException(string message, Exception innerException, string module = "General")
                : base(message, innerException, module, AppErrorCode.NotFound, StatusCodes.Status404NotFound)
            {
            }
        }
    }
}
