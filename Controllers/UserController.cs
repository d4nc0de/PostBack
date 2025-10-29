using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Neo4jClient;

namespace HR.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IGraphClient _client;

        public UserController(IGraphClient client)
        {
            _client = client;
        }

        //Obtener todos los usuarios
        [HttpGet]
        public async Task<IActionResult> Get(){
            var users = await _client.Cypher
                                    .Match("(u:USUARIO)")
                                    .Return(u => u.As<USUARIO>())
                                    .ResultsAsync;

            return Ok(users);
        }

        //Obtener usuario por id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id){
            var user = (await _client.Cypher
                                    .Match("(u:USUARIO)")
                                    .Where((USUARIO u) => u.idu == id)
                                    .Return(u => u.As<USUARIO>())
                                    .ResultsAsync)
                        .FirstOrDefault();

            if (user == null) return NotFound();
            return Ok(user);
        }


        [HttpGet("Login")]
        public async Task<IActionResult> Login(string mail, string password)
        {
            var user = (await _client.Cypher
                                    .Match("(u:USUARIO)")
                                    .Where((USUARIO u) => u.mail == mail && u.password == password)
                                    .Return(u => u.As<USUARIO>())
                                    .ResultsAsync)
                        .FirstOrDefault();

            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] USUARIO user)
        {
            var exists = (await _client.Cypher
                                      .Match("(u:USUARIO)")
                                      .Where((USUARIO u) => u.idu == user.idu)
                                      .Return(u => u.As<USUARIO>())
                                      .ResultsAsync)
                         .Any();
            if (exists) return Conflict($"Usuario con idu={user.idu} ya existe.");

            await _client.Cypher
                            .Create("(u:USUARIO $user)")
                            .WithParam("user", user)
                            .ExecuteWithoutResultsAsync();

            return Ok();
        }

        //Crear un nuevo usuario
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]USUARIO user){
            var exists = (await _client.Cypher
                                      .Match("(u:USUARIO)")
                                      .Where((USUARIO u) => u.idu == user.idu)
                                      .Return(u => u.As<USUARIO>())
                                      .ResultsAsync)
                         .Any();
            if (exists) return Conflict($"Usuario con idu={user.idu} ya existe.");

            await _client.Cypher
                            .Create("(u:USUARIO $user)")
                            .WithParam("user", user)
                            .ExecuteWithoutResultsAsync();

            return Ok();
        }

        //Actualizar un usuario existente
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody]USUARIO user){
            var existing = (await _client.Cypher
                                        .Match("(u:USUARIO)")
                                        .Where((USUARIO u) => u.idu == id)
                                        .Return(u => u.As<USUARIO>())
                                        .ResultsAsync)
                           .Any();
            if (!existing) return NotFound();

            await _client.Cypher
                        .Match("(u:USUARIO)")
                        .Where((USUARIO u) => u.idu == id)
                        .Set("u = $user")
                        .WithParam("user", user)
                        .ExecuteWithoutResultsAsync();

            return Ok();
        }

        //Eliminar un usuario
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id){
            var existing = (await _client.Cypher
                                        .Match("(u:USUARIO)")
                                        .Where((USUARIO u) => u.idu == id)
                                        .Return(u => u.As<USUARIO>())
                                        .ResultsAsync)
                           .Any();
            if (!existing) return NotFound();

            await  _client.Cypher
                            .Match("(u:USUARIO)")
                            .Where((USUARIO u) => u.idu == id)
                            .Delete("u")
                            .ExecuteWithoutResultsAsync();

            return Ok();
        }
        
    }
}
