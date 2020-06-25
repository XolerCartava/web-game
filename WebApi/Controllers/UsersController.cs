using System;
using Game.Domain;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WebApi.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;

        public UsersController(
            IUserRepository userRepository,
            IMapper mapper)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
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
                return BadRequest();
            
            var userFromRepository = userRepository.FindById(userId);             
            if (userFromRepository == null)
                return NotFound();

            userRepository.Delete(userId);
            return NoContent();
        }
    }
}