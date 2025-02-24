using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFCWebApplication.Repositories;

namespace PFCWebApplication.Controllers
{

    [Authorize] //you are enforcing the authentication process before entering the controller
    public class FileController : Controller
    {

        BucketRepository _bucketRepository;
        FirestoreRepository _firestoreRepository;
        public FileController(BucketRepository bucketRepository, FirestoreRepository firestoreRepository)
        { 
            _firestoreRepository = firestoreRepository; 
            _bucketRepository = bucketRepository;  
        }


        public async Task<IActionResult> Index()
        {
            //getting the email address of the logged in user
            string emailOfUserLoggingIn = User.Claims.SingleOrDefault(x => x.Type.Contains("email")).Value;
            //calling our method to gives the list of files uploaded by the logged in user
            var listOfMyFiles = await _firestoreRepository.GetFilesOfUser(emailOfUserLoggingIn);
            //returing that list of files into the view
            return View(listOfMyFiles);
        }


        [HttpGet] //first method will be called when the user decides to upload something
                  //is to load the page where there is the file-upload control
                  //this is normally called first
        public async Task<IActionResult> Create()
        {
            return View(); //it returns and renders a View bearing the same action's name
        }

        [HttpPost] //second method will be called after the user selects the file, inputs the recipients
                    // and clicks Upload button
                    //this is called after the HttpGet
        public async Task<IActionResult> Create (IFormFile fileUploadedByUser, string recipients)
        {
            string[] recipientsList = recipients.Split(',');

            if (fileUploadedByUser != null && recipientsList.Length > 0)
            {
                //start uploading on the bucket
                //D22065E3-0A02-4D8C-AD3E-FCC0B8ACCEE6
                string uniqueFilename = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(fileUploadedByUser.FileName);

                MemoryStream msFile = new MemoryStream();
                fileUploadedByUser.CopyTo(msFile);
                msFile.Position = 0;
                await _bucketRepository.UploadFile(uniqueFilename, msFile);

                //assign permissions
                foreach (var recipient in recipientsList)
                {
                    await _bucketRepository.GrantAccess(uniqueFilename, recipient, "READER");
                }
                string emailOfUserLoggingIn = User.Claims.SingleOrDefault(x => x.Type.Contains("email")).Value;
                await _bucketRepository.GrantAccess(uniqueFilename, emailOfUserLoggingIn, "OWNER");


                //stores the file entry in the db
                await _firestoreRepository.AddFile(emailOfUserLoggingIn, uniqueFilename);

            }

            return View();
        }
    }
}
