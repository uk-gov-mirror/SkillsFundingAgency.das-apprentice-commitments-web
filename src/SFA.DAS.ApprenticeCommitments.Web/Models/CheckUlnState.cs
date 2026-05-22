using System;
using System.Collections.Generic;

namespace SFA.DAS.ApprenticeCommitments.Web.Models
{
    public class CheckUlnState
    {
        public List<long> ApprenticeshipIds { get; set; } = new();
        public List<Guid> RegistrationIds { get; set; } = new();
    }
}
