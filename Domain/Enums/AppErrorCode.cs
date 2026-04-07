using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums
{
    public enum AppErrorCode
    {
        // General
        ValidationError = 1000,
        BusinessLogicError = 1001,
        NotFound = 1002,
        UnauthorizedAccess = 1003,
        InternalServerError = 1004,

        // Auth
        InvalidCredentials = 2000,
        TokenExpired = 2001,
        AccountLocked = 2002,
    }
}
