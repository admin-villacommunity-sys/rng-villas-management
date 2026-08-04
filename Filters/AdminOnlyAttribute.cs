using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VillaCommunityManagement.Filters
{
    public class AdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("UserRole");

            if (role != "Admin")
            {
                context.Result = new RedirectToActionResult(
                    "AccessDenied",
                    "Login",
                    null
                );
            }

            base.OnActionExecuting(context);
        }
    }
}