using System;
using Game.Domain;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly LinkGenerator linkGenerator;

        public UsersController(
            IUserRepository userRepository,
            IMapper mapper,
            LinkGenerator linkGenerator)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.linkGenerator = linkGenerator;
        }

        [HttpGet("{userId}", Name = nameof(GetUserById))]
        [HttpHead("{userId}")]
        [Produces("application/json", "application/xml")]
        public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
        {
            var userFromRepository = userRepository.FindById(userId);
            if (userFromRepository == null)
                return NotFound();

            var userDto = mapper.Map<UserDto>(userFromRepository);
            return Ok(userDto);
        }

        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json", "application/xml")]
        public IActionResult CreateUser([FromBody] CreaterUser user)
        {
            if (user == null)
                return BadRequest();

            if ((user.Login != "") && 
                (user.Login != null) &&
                (!user.Login.All(char.IsLetterOrDigit)))
            {
                ModelState.AddModelError(nameof(CreaterUser.Login),
                    "Login should contain only letters or digits.");
            }

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            var createdUserEntity = userRepository
                .Insert(mapper.Map<UserEntity>(user));
            var userId = createdUserEntity.Id;

            return CreatedAtRoute(
                nameof(GetUserById),
                new { userId = userId },
                userId);
        }

        [HttpPut("{userId}")]
        [Consumes("application/json")]
        [Produces("application/json", "application/xml")]
        public IActionResult UpdateUser([FromRoute] Guid userId, [FromBody] UpdaterUser user)
        {
            if (user == null)
                return BadRequest();
            
            if (userId == Guid.Empty)
                return BadRequest();

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            var newUserEntity = new UserEntity(userId);
            mapper.Map(user, newUserEntity);
            userRepository.UpdateOrInsert(newUserEntity, out bool isInserted);

            if (isInserted)
            {
                return CreatedAtRoute(
                    nameof(GetUserById),
                    new { userId = newUserEntity.Id },
                    newUserEntity.Id);
            }
            return NoContent();
        }

        [HttpPatch("{userId}")]
        [Consumes("application/json")]
        [Produces("application/json", "application/xml")]
        public IActionResult PartiallyUpdateUser([FromRoute] Guid userId,
            [FromBody] JsonPatchDocument<UpdaterUser> changes) {
            if (userId == Guid.Empty)
                return NotFound();

            if (changes == null)
                return BadRequest();

            var userFromRepository = userRepository.FindById(userId);             
            if (userFromRepository == null)
                return NotFound();

            var user = mapper.Map<UpdaterUser>(userFromRepository);
            changes.ApplyTo(user, ModelState);
            TryValidateModel(user);
            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            mapper.Map(user, userFromRepository);
            userRepository.Update(userFromRepository);

            return NoContent();       
        }

        [HttpDelete("{userId}")]
        [Consumes("application/json")]
        [Produces("application/json", "application/xml")]
        public IActionResult DeleteUser([FromRoute] Guid userId) 
        {
            if (userId == Guid.Empty)
                return NotFound(); //BadRequest();
            
            var userFromRepository = userRepository.FindById(userId);             
            if (userFromRepository == null)
                return NotFound();

            userRepository.Delete(userId);
                return NoContent();
        }

        [HttpGet]
        [Consumes("application/json")]
        [Produces("application/json", "application/xml")]
        public ActionResult<IEnumerable<UserDto>> GetUsers(int pageNumber = 1, int pageSize = 10)
        {
            pageNumber = pageNumber < 1? 1 : pageNumber;
            pageSize = pageSize < 1? 1 : pageSize > 20? 20 : pageSize;

            var pageList = userRepository.GetPage(pageNumber, pageSize);
            var users = mapper.Map<IEnumerable<UserDto>>(pageList);

            var paginationHeader = new
            {
                previousPageLink = pageList.HasPrevious ? CreateUsersInf(pageList.CurrentPage - 1, pageList.PageSize) : null,
                nextPageLink = pageList.HasNext ? CreateUsersInf(pageList.CurrentPage + 1, pageList.PageSize) : null,
                totalCount = pageList.TotalCount,
                pageSize = pageList.PageSize,
                currentPage = pageList.CurrentPage,
                totalPages = pageList.TotalPages
            };
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationHeader));

            return Ok(users);
        }
        private string CreateUsersInf(int pageNumber, int pageSize)
        {
            return linkGenerator.GetUriByRouteValues(HttpContext, nameof(GetUsers), new {pageNumber, pageSize});
        }
    }
}