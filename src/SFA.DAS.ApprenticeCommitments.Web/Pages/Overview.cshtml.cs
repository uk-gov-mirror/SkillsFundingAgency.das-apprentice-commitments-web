using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SFA.DAS.ApprenticeCommitments.Web.Services.OuterApi;
using SFA.DAS.HashingService;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Web.Pages
{
    [Authorize]
    public class OverviewModel : PageModel
    {
        private readonly IOuterApiClient _client;
        private readonly IHashingService _hashing;

        public OverviewModel(IOuterApiClient client, IHashingService hashing)
        {
            _client = client;
            _hashing = hashing;
        }

        [BindProperty(SupportsGet = true)]
        public string? ApprenticeshipId { get; set; }

        public async Task<IActionResult> OnGet([FromServices] AuthenticatedUser user)
        {
            return ApprenticeshipId == null
                ? await RedirectToLatestApprenticeship(user)
                : Page();
        }

        private async Task<IActionResult> RedirectToLatestApprenticeship(AuthenticatedUser user)
        {
            var apprenticeship = await _client.GetCurrentApprenticeship(user.RegistrationId);
            var hashedId = _hashing.HashValue(apprenticeship.Id);
            return Redirect($"/Overview/{hashedId}");
        }
    }
}