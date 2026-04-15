using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SFA.DAS.ApprenticeCommitments.Web.Services.OuterApi;
using SFA.DAS.ApprenticePortal.Authentication;
using SFA.DAS.ApprenticePortal.SharedUi.Menu;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeCommitments.Web.Pages.Apprenticeships
{
    [HideNavigationBar]
    [AllowAnonymous]
    public class TermsModel : PageModel
    {
        private readonly IOuterApiClient _outerApiClient;        

        [BindProperty]
        [Required(ErrorMessage = "You must accept the terms and conditions")]
        public bool TermsOfUseAccepted { get; set; }

        public TermsModel(IOuterApiClient outerApiClient)
        {
            _outerApiClient = outerApiClient;            
        }

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }                

        public async Task<IActionResult> OnPost([FromServices] AuthenticatedUser user)
        {           
                var patchDoc = new JsonPatchDocument<Apprentice>();
                patchDoc.Replace(a => a.TermsOfUseAccepted, true);

                await _outerApiClient.UpdateApprentice(user.ApprenticeId, patchDoc);
                await AuthenticationEvents.TermsOfUseAccepted(HttpContext);           

                return RedirectToPage("Index");
        }               
    }
    
}