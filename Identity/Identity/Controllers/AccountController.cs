using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Utility = Identity.Models.Utility;

namespace Identity.Controllers
{
    [Produces("application/json")]
    [Route("api/Account")]
    public class AccountController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<ActionResult> Register([FromBody]RegisterViewModel model)
        {
            if (!ModelState.IsValid) return Json(model);

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedDate = DateTime.Now.ToUniversalTime(),
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded) return Json("Failed");

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var callbackUrl = Url.Action("VerifyEmail", "Account", new { userId = user.Id, code },
                Request.Scheme);
            var oMailModel = new MailModel
            {
                To = model.Email,
                Subject = "Confirm your account",
                Body = "Hi " + model.UserName +
                       "<br/><br/> Your registration process is almost complete, all you need to do is click on the confirmation link below to finish activation.<br/>Please confirm your account by clicking <a href=\"" + callbackUrl + ">here </a>.<br/><br/>Regards,<br/>Registration Team"
            };
            Utility.SendMail(oMailModel);
            await _userManager.AddToRoleAsync(user, "User");
            ViewBag.Message = "Success";
            return Json("success");
        }

        [HttpPost]
        [Route("VerifyEmail")]
        [AllowAnonymous]
        public async Task<ActionResult> VerifyEmail(string id, string token)
        {
            if (id == null || token == null)
            {
                return Json("Enter all value");
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                throw new ApplicationException($"Unable to load user with ID '{id}'.");

            var emailConfirmationResult = await _userManager.ConfirmEmailAsync(user, token);
            return Json(!emailConfirmationResult.Succeeded ? "Error" : "Success");
        }

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login([FromBody]LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid) return Json(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return Ok(new
                {
                    authenticated = false,
                    message = "You are unautorized to access this site."
                });
            }
            if (!user.EmailConfirmed)
            {
                return Ok(new
                {
                    authenticated = false,
                    message = "User is registered and can login after activation."
                });
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

            var roles = await _userManager.GetRolesAsync(user);
            var token = TokenAuthOptions.GenerateToken(user.UserName);
            if (result.Succeeded)
            {
                return Ok(new
                {
                    authenticated = true,
                    token = token,
                    user = user,
                    message = "Welcome to the Identity"
                });
            }
            if (result.IsLockedOut)
            {
                return BadRequest("User account locked out.");
            }
            return BadRequest("Something wen't wrong.");
        }

        

        [HttpPost]
        [Route("ForgotPassword")]
        [AllowAnonymous]
        public ActionResult ForgotPassword([FromBody]ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return Json(model);

            var user = _userManager.FindByEmailAsync(model.Email).Result;
            if (user == null || !(_userManager.IsEmailConfirmedAsync(user).Result))
            {
                return NotFound();
            }

            var code = _userManager.GeneratePasswordResetTokenAsync(user);

            var oMailModel = new MailModel
            {
                To = model.Email,
                Subject = "Forgot Password",
                Body =
                    "<table><tr>Hi  " + user.Email +
                    ",</tr><tr><br/><br/>Please reset your password by clicking <a href = 'https://" + Request.Scheme + "/Account/ResetPassword?userid=" + user.Id + "&code=" + code + "'> here</a>.<br/><br/><br/>Registration Team</table>"
            };
            Utility.SendMail(oMailModel);
            return Json("success");
        }

        [HttpPost]
        [Route("ResetPassword")]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword([FromBody]ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(model);

            var user = _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return NotFound();

            var result = await _userManager.ResetPasswordAsync(user.Result, model.Code, model.Password);

            return Json(result.Succeeded ? "Success" : "Something wen't wrong");
        }

        [HttpPost]
        [Route("ChangePassword")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult> ChangePassword([FromBody]ChangePasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return NotFound();

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                return Json(changePasswordResult);
            }
            return Json("Your password has been changed.");
        }

        [HttpPost]
        [Route("logout")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Json(new Response(HttpStatusCode.OK)
            {
                Message = "You have been successfully logged out"
            });
        }
    }
}