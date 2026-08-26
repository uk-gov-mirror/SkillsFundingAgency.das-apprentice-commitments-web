using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeCommitments.Web.Services;
using SFA.DAS.ApprenticeCommitments.Web.Services.OuterApi;
using SFA.DAS.ApprenticePortal.Authentication;
using SFA.DAS.ApprenticePortal.SharedUi.Menu;
using SFA.DAS.Encoding;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Web.Pages.Apprenticeships
{
    //[RequiresIdentityConfirmed]
    public class ApprenticeshipIndexModel : PageModel
    {
        private readonly ApprenticeApi _client;
        private readonly IOuterApiClient _outerApiClient;
        private readonly ILogger<ApprenticeshipIndexModel> _logger;
        private readonly NavigationUrlHelper _urlHelper;
        private readonly CommitmentsService _commitmentsService;

        public ApprenticeshipIndexModel(ApprenticeApi client, IOuterApiClient outerApiClient, ILogger<ApprenticeshipIndexModel> logger, NavigationUrlHelper urlHelper, CommitmentsService commitmentsService)
        {
            _client = client;
            _outerApiClient = outerApiClient;
            _logger = logger;
            _urlHelper = urlHelper;
            _commitmentsService = commitmentsService;
        }

        public async Task<IActionResult> OnGet([FromServices] AuthenticatedUser user)
        {
            var apprentice = await _outerApiClient.GetApprentice(user.ApprenticeId);
            if (!apprentice.TermsOfUseAccepted)
            {
                _logger.LogInformation("User has not accepted terms of use, redirecting to terms of use page");
                return RedirectToPage("Terms");
            }

            return await RedirectToLatestApprenticeship(user);
        }

        private async Task<IActionResult> RedirectToLatestApprenticeship(AuthenticatedUser user)
        {
            using (_logger.BeginPropertyScope(("ApprenticeId", user.ApprenticeId)))
            {
                if (Request.Cookies.TryGetValue("RegistrationCode", out var registrationCode))
                {
                    _logger.LogInformation("RedirectToLatestApprenticeship - Found RegistrationCode {RegistrationCode}", registrationCode);
                    return RedirectToAction("Register", "Registration", registrationCode);
                }

                var email = user.Email?.Address;

                // Gets Revision
                var revision = await _client.TryGetApprenticeships(user.ApprenticeId);

                if (!string.IsNullOrWhiteSpace(email))
                {
                    try
                    {
                        if (revision == null || revision.Apprenticeships.Count == 0)
                        {
                            // First Time User - First Time Login
                            return await HandleRegistration(email, user.ApprenticeId);
                        }

                        if (revision != null && revision.Apprenticeships.Count > 0)
                        {
                            for (int i = 0; i < revision.Apprenticeships.Count; i++)
                            {
                                var apprenticeship = revision.Apprenticeships[i];

                                if (!apprenticeship.IsStopped && apprenticeship.ConfirmedOn == null && apprenticeship.PlannedEndDate >= DateTime.Now)
                                {
                                    return await HandleRegistration(email, user.ApprenticeId);
                                }

                                if (!apprenticeship.IsStopped && apprenticeship.ConfirmedOn != null)
                                {
                                    _logger.LogInformation("User has a confirmed apprenticeship, granting access | {RevisionId}", apprenticeship.RevisionId);
                                    return Redirect(_urlHelper.Generate(NavigationSection.Home, "Home"));
                                }

                                continue;
                            }

                            _logger.LogInformation("User has a no active Apprenticeships, Sending to check registration | {RevisionIds}",
                                string.Join(",", revision.Apprenticeships.Select(x => x.Id)));
                            return await HandleRegistration(email, user.ApprenticeId);
                        }

                        return RedirectToPage("AccountNotFound");

                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation("Email does not match any registration record | {ex}", ex);
                        return RedirectToPage("/CheckYourDetails");
                    }
                }

                return RedirectToPage("AccountNotFound");
            }
        }

        private async Task<IActionResult> HandleRegistration(string email, Guid apprenticeId)
        {
            try
            {
                var registrationByEmail = await _outerApiClient.GetRegistrationsByEmail(email);                              

                var firstName = registrationByEmail.FirstName;
                var lastName = registrationByEmail.LastName;
                if (firstName == null || lastName == null)
                {
                    _logger.LogInformation("Registration record does not contain first name and last name | {RegistrationId}", registrationByEmail.RegistrationId);
                    return RedirectToPage("/CheckYourDetails");
                }

                await _commitmentsService.EnsureApprenticeHasBasicFields(apprenticeId, firstName, lastName, registrationByEmail.DateOfBirth);

                var model = await _commitmentsService.GenerateConfirmationModel(apprenticeId, registrationByEmail.RegistrationId, registrationByEmail.CommitmentsApprenticeshipId);
                TempData["ConfirmationModel"] = JsonSerializer.Serialize(model);
                return RedirectToPage("/ConfirmYourApprenticeship");
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Email does not match any registration record | {ex}", ex);
                return RedirectToPage("/CheckYourDetails");
            }
        }
    }
}