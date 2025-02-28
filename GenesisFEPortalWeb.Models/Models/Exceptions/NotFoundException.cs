using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenesisFEPortalWeb.Models.Models.Exceptions
{
    public class NotFoundException : ApiException
    {
        public NotFoundException(string message)
            : base(message, "NOT_FOUND", 404)
        {
        }

        public NotFoundException(string resource, object key)
            : base($"El recurso {resource} con id {key} no fue encontrado.", "NOT_FOUND", 404)
        {
        }
    }
}
