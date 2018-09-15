using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OauthIdentityApp.Models;
using OauthIdentityApp.Providers;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace OauthIdentityApp.Controllers
{
    [RoutePrefix("api/Account")]
    public class AccountController : BaseApiController
    {
        private AuthRepository _repo = null;

        public AccountController()
        {
            _repo = new AuthRepository();
        }

        [Route("users")]
        public IHttpActionResult GetUsers()
        {
            return Ok(AppUserManager.Users.ToList().Select(u => TheModelFactory.Create(u)));
        }

        [Route("user/{id:guid}", Name = "GetUserById")]
        public async Task<IHttpActionResult> GetUser(string id)
        {
            var user = await AppUserManager.FindByIdAsync(id);
            if (user != null)
            {
                return Ok(TheModelFactory.Create(user));
            }
            return NotFound();
        }

        [Route("user/{username}")]
        public async Task<IHttpActionResult> GetUserByName(string username)
        {
            var user = await AppUserManager.FindByNameAsync(username);
            if (user != null)
            {
                return Ok(TheModelFactory.Create(user));
            }
            return NotFound();
        }

        [Route("create")]
        public async Task<IHttpActionResult> CreateUser(AccountBindingModel createUserModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser()
            {
                UserName = createUserModel.Username,
                Email = createUserModel.Email,
                FirstName = createUserModel.FirstName,
                LastName = createUserModel.LastName,
                Level = 3,
                JoinDate = DateTime.Now.Date,
            };

            IdentityResult addUserResult = await AppUserManager.CreateAsync(user, createUserModel.Password);

            if (!addUserResult.Succeeded)
            {
                return GetErrorResult(addUserResult);
            }
            Uri locationHeader = new Uri(Url.Link("GetUserById", new { id = user.Id }));
            return Created(locationHeader, TheModelFactory.Create(user));
        }

        [AllowAnonymous]
        [Route("Register")]
        [Permission(Action = "Create", Function = "REGISTER")]
        public async Task<IHttpActionResult> Register(UserModel userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await _repo.RegisterUser(userModel);
            IHttpActionResult errorResult = GetErrorResult(result);
            if (errorResult != null)
            {
                return errorResult;
            }
            return Ok();
        }

        [AllowAnonymous]
        [Route("Login")]
        public async Task<IHttpActionResult> Login(UserModel userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityUser res = await _repo.FindUser(userModel.UserName, userModel.Password);
            return Ok(res);
        }
    }
}
