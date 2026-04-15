using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SFA.DAS.ApprenticeCommitments.Web.Helpers;
using SFA.DAS.ApprenticeCommitments.Web.Models;
using SFA.DAS.ApprenticeCommitments.Web.Services;
using SFA.DAS.ApprenticeCommitments.Web.Services.OuterApi;
using SFA.DAS.ApprenticePortal.Authentication;
using SFA.DAS.ApprenticePortal.SharedUi.Filters;
using SFA.DAS.ApprenticePortal.SharedUi.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Web.Pages
{
    [HideNavigationBar]    
    public class CheckYourDetails : PageModel
    {
        private readonly ApprenticeApi _apprentices;
        private readonly IOuterApiClient _outerApiClient;        
        private readonly NavigationUrlHelper _urlHelper;
        private readonly CommitmentsService _commitmentsService;

        public CheckYourDetails(ApprenticeApi _apprentices, IOuterApiClient outerApiClient, NavigationUrlHelper urlHelper, CommitmentsService commitmentsService)
        {
            this._apprentices = _apprentices;
            _outerApiClient = outerApiClient;            
            _urlHelper = urlHelper;
            _commitmentsService = commitmentsService;
        }

        [BindProperty]
        public string FirstName { get; set; } = string.Empty;
        [BindProperty]
        public string LastName { get; set; } = string.Empty;
        [BindProperty]
        public DateInputModel DateOfBirth { get; set; } = new();

        public async Task<ActionResult> OnGet(
            [FromServices] AuthenticatedUser user)
        {
            var apprentice = await _apprentices.TryGetApprentice(user.ApprenticeId);
            if (apprentice == null) return Redirect(_urlHelper.Generate(NavigationSection.ApprenticeAccounts, $"Account"));

            FirstName = apprentice.FirstName;
            LastName = apprentice.LastName;

            if (apprentice.DateOfBirth.HasValue)
            {
                DateOfBirth = new DateInputModel
                {
                    Day = apprentice.DateOfBirth?.Day,
                    Month = apprentice.DateOfBirth?.Month,
                    Year = apprentice.DateOfBirth?.Year
                };
            }
            
            return Page();
        }

        public async Task<ActionResult> OnPostAsync([FromServices] AuthenticatedUser user)
        {
            if (!ModelState.IsValid) return Page();

            if (!DateOfBirth.Day.HasValue ||
                !DateOfBirth.Month.HasValue ||
                !DateOfBirth.Year.HasValue)
            {
                ModelState.AddModelError(nameof(DateOfBirth), "Enter a valid date of birth");
                return Page();
            }            

            var firstName = FirstName.Trim();
            var lastName = LastName.Trim();
            var dateOfBirth = new DateTime(
                DateOfBirth.Year.Value,
                DateOfBirth.Month.Value,
                DateOfBirth.Day.Value);

            try
            {
                // fetch registrations and apprentice
                var registrations = await _outerApiClient.GetRegistrationByAccountDetails(firstName, lastName, dateOfBirth.ToIsoDate());                

                if (registrations == null || registrations.Count == 0)
                    return RedirectToPage("AccountNotFound");

                // Update Apprentice Account if needed for firstname, lastname and dateOfBirth
                await _commitmentsService.EnsureApprenticeHasBasicFields(user.ApprenticeId, firstName, lastName, dateOfBirth);

                var apprentice = await _outerApiClient.GetApprentice(user.ApprenticeId);
                await AuthenticationEvents.UserAccountCreated(HttpContext, apprentice);

                // Will return to Uln Page
                if (registrations.Count >= 2)
                {                    
                    var state = new CheckUlnState
                    {
                        ApprenticeshipIds = registrations.Select(x => x.CommitmentsApprenticeshipId).ToList(),
                        RegistrationIds = registrations.Select(x => x.RegistrationId).ToList()
                    };

                    TempData["CheckUlnState"] = JsonSerializer.Serialize(state);

                    return RedirectToPage("CheckUln");
                }

                var registration = registrations.SingleOrDefault();
                if (registration == null)
                    return RedirectToPage("AccountNotFound");

                var model = await _commitmentsService.GenerateConfirmationModel(user.ApprenticeId, registration.RegistrationId, registration.CommitmentsApprenticeshipId);
                TempData["ConfirmationModel"] = JsonSerializer.Serialize(model);
                return RedirectToPage("ConfirmYourApprenticeship");

            } catch
            {
                return RedirectToPage("AccountNotFound");
            }                 
        }     
    }

    public class DateInputModel
    {
        public int? Day { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
    }
}
