using Microsoft.AspNetCore.JsonPatch;
using RestEase;
using SFA.DAS.ApprenticeCommitments.Web.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Web.Services.OuterApi
{
    public interface IOuterApiClient
    {
        [Get("/registrations/{id}")]
        Task<VerifyRegistrationResponse> GetRegistration([Path] Guid id);

        [Get("/registration/registration-details/{id}")]
        Task<Registration> GetRegistrationById([Path] Guid id);

        [AllowAnyStatusCode]
        [Post("/registrations/{registrationCode}/firstseen")]
        Task RegistrationFirstSeenOn([Path] string registrationCode, [Body] RegistrationFirstSeenOnRequest request);

        [Get("registrations")]
        Task<List<Registration>> GetRegistrationByAccountDetails([Query] string firstName, [Query] string lastName, [Query] string dateOfBirth);

        [Get("/registrations/email")]
        Task<List<Registration>> GetRegistrationsByEmail([Query] string email);

        [Post("/registrations")]
        Task VerifyRegistration([Body] VerifyRegistrationRequest verification);

        [Get("/apprentices/{id}")]
        Task<Apprentice> GetApprentice([Path] Guid id);

        [Patch("/apprentices/{id}")]
        Task UpdateApprentice([Path] Guid id, [Body] JsonPatchDocument<Apprentice> patch);
        
        [Put("/apprentices")]
        Task<Apprentice> PutApprentice([Body] PutApprenticeAccount request);

        [Post("/apprenticeships")]
        Task ClaimApprenticeship([Body] ApprenticeshipAssociation association);

        [Get("/apprentices/{id}/apprenticeships")]
        Task<ApprenticeshipsWrapper> GetApprenticeships([Path] Guid id);

        [Get("/apprentices/{apprenticeid}/apprenticeships/{apprenticeshipid}")]
        Task<Apprenticeship> GetApprenticeship([Path] Guid apprenticeid, [Path] long? apprenticeshipid);

        [Get("/apprentices/{apprenticeid}/apprenticeships/{apprenticeshipid}/confirmed/latest")]
        Task<Apprenticeship> GetMyApprenticeship([Path] Guid apprenticeid, [Path] long apprenticeshipid);

        [Get("/apprentices/{apprenticeid}/apprenticeships/{apprenticeshipid}/revisions/{revisionId}")]
        Task<Apprenticeship> GetApprenticeshipRevision([Path] Guid apprenticeid, [Path] long apprenticeshipid, [Path] long revisionId);

        [Get("/commitments-apprenticeships/{apprenticeshipId}")]
        Task<CommitmentsApprenticeship> GetCommitmentsApprenticeshipById([Path] long apprenticeshipId);

        [Patch("/apprentices/{apprenticeid}/apprenticeships/{apprenticeshipid}/revisions/{revisionId}/confirmations")]
        Task ConfirmApprenticeship(
                 [Path] Guid apprenticeid, [Path] long apprenticeshipid, [Path] long revisionId,
                 [Body] ApprenticeshipConfirmationRequest confirmation);

        [Patch("/apprentices/{apprenticeId}/apprenticeships/{apprenticeshipId}")]
        Task UpdateApprenticeship(
            [Path] Guid apprenticeId, [Path] long apprenticeshipId,
            [Body] JsonPatchDocument<Apprenticeship> patch);

        [Patch("/apprentices/{apprenticeId}/apprenticeships/{apprenticeshipId}/revisions/{revisionId}")]
        Task UpdateRevision([Path] Guid apprenticeId, [Path] long apprenticeshipId, [Path] long revisionId, [Body] JsonPatchDocument<RevisionPatch> patch);
    }

    public static class OuterApiExtensions
    {
        public static async Task UpdateRevisionLastViewed(this IOuterApiClient client, Guid apprenticeId, long apprenticeship, long revisionId)
        {
            var patch = new JsonPatchDocument<RevisionPatch>().Replace(x => x.LastViewed, DateTime.UtcNow);
            await client.UpdateRevision(apprenticeId, apprenticeship, revisionId, patch);
        }
    }
}