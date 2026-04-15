using Microsoft.AspNetCore.Mvc;
using SFA.DAS.ApprenticeCommitments.Web.Models;
using SFA.DAS.ApprenticeCommitments.Web.Pages;
using SFA.DAS.ApprenticeCommitments.Web.Services.OuterApi;
using System;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace SFA.DAS.ApprenticeCommitments.Web.Services
{
    public class CommitmentsService
    {
        private readonly IOuterApiClient _outerApiClient;

        public CommitmentsService(IOuterApiClient outerApiClient)
        {
            _outerApiClient = outerApiClient;
        }

        public async Task<ConfirmYourApprenticeshipViewModel> GenerateConfirmationModel(Guid apprenticeId, Guid registrationId, long commitmentsApprenticeshipId)
        {
            // Create apprenticeship and prepare view model
            var apprentice = await _outerApiClient.GetApprentice(apprenticeId);
            var claimedRevision = await ClaimApprenticeship(apprenticeId, registrationId);
            var claimedRegistration = await _outerApiClient.GetRegistrationById(registrationId);
            var commitmentsApprenticeship = await _outerApiClient.GetCommitmentsApprenticeshipById(commitmentsApprenticeshipId);

            if (claimedRevision == null || claimedRegistration == null || commitmentsApprenticeship == null)
            {
                return new ConfirmYourApprenticeshipViewModel();
            }

            return new ConfirmYourApprenticeshipViewModel
            {
                ApprenticeId = apprenticeId,
                ApprenticeshipId = claimedRegistration.ApprenticeshipId,
                RevisionId = claimedRevision.RevisionId,
                CommitmentsApprenticeshipId = commitmentsApprenticeship.Id,
                Uln = long.Parse(commitmentsApprenticeship!.Uln!),
                FullName = $"{apprentice.FirstName} {apprentice.LastName}",
                EmployerName = claimedRevision.EmployerName,
                TrainingProviderName = claimedRevision.TrainingProviderName,
                TrainingProviderId = commitmentsApprenticeship?.ProviderId ?? 0,
                CourseName = claimedRevision.CourseName,
                Level = claimedRevision.CourseLevel,
                Type = claimedRevision.ApprenticeshipType.HasValue ? claimedRevision.ApprenticeshipType.Value.ToString() : string.Empty,
                StartDate = commitmentsApprenticeship?.StartDate?.ToString("MMMM yyyy"),
                EndDate = commitmentsApprenticeship?.EndDate.ToString("MMMM yyyy")
            };            
        }

        public async Task<Apprenticeship> ClaimApprenticeship(Guid apprenticeId, Guid registrationId)
        {
            await _outerApiClient.ClaimApprenticeship(
                               new ApprenticeshipAssociation
                               {
                                   ApprenticeId = apprenticeId,
                                   RegistrationId = registrationId.ToString()
                               });

            var registration = await _outerApiClient.GetRegistrationById(registrationId);
            var revision = await _outerApiClient.GetApprenticeship(apprenticeId, registration.ApprenticeshipId);

            return revision;
        }
    }
}
