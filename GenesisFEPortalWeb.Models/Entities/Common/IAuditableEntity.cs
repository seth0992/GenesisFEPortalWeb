using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenesisFEPortalWeb.Models.Entities.Common
{
    public interface IAuditableEntity
    {
        DateTime CreatedAt { get; set; }       
        DateTime? UpdatedAt { get; set; }
    }
}
