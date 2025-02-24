using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore.V1;
using PFCWebApplication.Repositories;


namespace PFCWebApplication.Controllers
{
    public class AccountController : Controller
    {

        //Dependency Injection: centralize the intantiation of all the "services" (classes)
        //                      that you need to use in your controllers
        FirestoreRepository _firestoreRepository;
        public AccountController(FirestoreRepository firestoreRepository) { 
         _firestoreRepository = firestoreRepository;
        }



        [Authorize] //blocks any anonymous access to this method
        public async Task<IActionResult> Login()
        {
            //1. user will click on a link called Login with Google
            //2. it will try to access this action hence the url will be  Account/Login
            //3. The Authorize attribute will intercept the request, blocks anonymous access and
            //4. will redirect the user the Google Login page instead
            //5. Assuming, user will log in successfully, he is redirected back here

            //User built-in object will be populated automatically by the runtime when a user logs in successfully

            //SingleOrDefault is one of those methods that help me querying the list for a single item!
            string emailOfUserLoggingIn = User.Claims.SingleOrDefault(x => x.Type.Contains("email")).Value;
            string firstName = User.Claims.SingleOrDefault(x => x.Type.Contains("givenname")).Value;
            string lastName = User.Claims.SingleOrDefault(x => x.Type.Contains("surname")).Value;

            if (await(_firestoreRepository.DoesUserExist(emailOfUserLoggingIn)) == false) //querying the db 
            {
                //user doesn't exist
                await _firestoreRepository.AddUser(emailOfUserLoggingIn, firstName, lastName);
            }

            await _firestoreRepository.AddLoginLog(emailOfUserLoggingIn,
                Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString(),
                "user logged in");


            return View();
        }

        public IActionResult Logout()
        {
            //deletes a cookie holding you logged in
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home"); //Home/Index
        }
    }
}
