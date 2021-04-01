using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SFA.DAS.ApprenticeCommitments.Web.Pages.IdentityHashing;
using SFA.DAS.ApprenticeCommitments.Web.Services;
using SFA.DAS.ApprenticeCommitments.Web.Services.OuterApi;

namespace SFA.DAS.ApprenticeCommitments.Web.Pages.Apprenticeships
{
    public class HowYourApprenticeshipWillBeDeliveredModel : PageModel, IHasBackLink
    {
        private readonly IOuterApiClient _client;
        private readonly AuthenticatedUser _authenticatedUser;
        [BindProperty(SupportsGet = true)]
        public HashedId ApprenticeshipId { get; set; }
        [BindProperty]
        public bool? ConfirmedHowApprenticeshipDelivered { get; set; }
        public string Backlink => $"/apprenticeships/{ApprenticeshipId.Hashed}";

        public HowYourApprenticeshipWillBeDeliveredModel(IOuterApiClient client, AuthenticatedUser authenticatedUser)
        {
            _client = client;
            _authenticatedUser = authenticatedUser;
        }

        public async Task OnGet()
        {
            var apprenticeship = await _client
                .GetApprenticeship(_authenticatedUser.RegistrationId, ApprenticeshipId.Id);
            ConfirmedHowApprenticeshipDelivered = apprenticeship.HowApprenticeshipDeliveredCorrect;
        }

        public async Task<IActionResult> OnPost()
        {
            if (ConfirmedHowApprenticeshipDelivered == null)
            {
                ModelState.AddModelError(nameof(ConfirmedHowApprenticeshipDelivered), "Select an answer");
                return new PageResult();
            }

            await _client.ConfirmHowApprenticeshipDelivered(
                _authenticatedUser.RegistrationId, ApprenticeshipId.Id,
                new HowApprenticeshipDeliveredConfirmationRequest(ConfirmedHowApprenticeshipDelivered.Value));

            var nextPage = ConfirmedHowApprenticeshipDelivered.Value ? "Confirm" : "CannotConfirm";

            return new RedirectToPageResult(nextPage, new { ApprenticeshipId });
        }
    }
}
