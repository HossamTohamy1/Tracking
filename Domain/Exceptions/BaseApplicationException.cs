using System;
using System.Collections.Generic;
using System.Text;

using Domain.Enums;

namespace Domain.Exceptions
{
    public abstract class BaseApplicationException : Exception
    {
        public string Module { get; protected set; }
        public AppErrorCode ErrorCode { get; protected set; }
        public int HttpStatusCode { get; protected set; }

        protected BaseApplicationException(
            string message,
            string module,
            AppErrorCode errorCode,
            int httpStatusCode)
            : base(message)
        {
            Module = module;
            ErrorCode = errorCode;
            HttpStatusCode = httpStatusCode;
        }

        protected BaseApplicationException(
            string message,
            Exception innerException,
            string module,
            AppErrorCode errorCode,
            int httpStatusCode)
            : base(message, innerException)
        {
            Module = module;
            ErrorCode = errorCode;
            HttpStatusCode = httpStatusCode;
        }
    }
}