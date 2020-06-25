using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoMapper;
using Game.Domain;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        // Чтобы ASP.NET положил что-то в userRepository требуется конфигурация
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            this.mapper = mapper;
            this.userRepository = userRepository;
        }

        [HttpGet("{userId}", Name = nameof(GetUserById))]
        [HttpHead("{userId}")]
        [Produces("application/json", "application/xml")]
        [SwaggerResponse(200, "OK", typeof(UserDto))]
        [SwaggerResponse(404, "Пользователь не найден")]
        public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
        {
            var userFromRepository = userRepository.FindById(userId);
            
            if (userFromRepository == null)
                return NotFound();

            var userDto = mapper.Map<UserDto>(userFromRepository);
            return Ok(userDto);
        }

        [HttpPost]
        [Produces("application/json", "application/xml")]
        public IActionResult CreateUser([FromBody] CreaterUser user)
        {
            if (user == null)
                return BadRequest();

            if (!string.IsNullOrEmpty(user.Login) &&
                !user.Login.All(char.IsLetterOrDigit))
            {
                ModelState.AddModelError(nameof(CreaterUser.Login),
                    "Логин должен быть из цифр или букв!");
            }

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            var UserNew = mapper.Map<UserEntity>(user);
            var CreateUserNew = userRepository.Insert(UserNew);

            return CreatedAtRoute(
                nameof(GetUserById),
                new { userId = CreateUserNew.Id },
                CreateUserNew.Id);
        }

        [HttpPost]
        [Produces("application/json", "application/xml")]
        public IActionResult UpdateUser([FromRoute, Required] Guid userId, [FromBody] CreaterUser user)
        {

            if (user == null)
                return BadRequest();

            if (!user.Login.All(c => char.IsLetter(c) || char.IsDigit(c)))
                ModelState.AddModelError(nameof(CreaterUser),
                    "Логин должен быть из цифр или букв!");

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            var userFromRepo = userRepository.FindById(userId);
            if (userFromRepo == null)
            {
                var userEntity = mapper.Map<UserEntity>(new UserEntity(userId));
                userRepository.UpdateOrInsert(userEntity, out bool isInserted);
                
                return CreatedAtRoute(
                    nameof(GetUserById),
                    new { userId = userEntity.Id },
                    userEntity.Id);
            }

            mapper.Map(user, userFromRepo);
            userRepository.Update(userFromRepo);
            return NoContent();
        }
    }
}