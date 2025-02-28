using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenesisFEPortalWeb.Models.Models.Exceptions
{
    public class ApiException : Exception
    {
        public string ErrorCode { get; }
        public int StatusCode { get; }

        public ApiException(string message, string errorCode = "API_ERROR", int statusCode = 500)
            : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }

        public ApiException(string message, Exception innerException, string errorCode = "API_ERROR", int statusCode = 500)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
        }
    }
}
