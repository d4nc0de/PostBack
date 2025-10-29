using HR.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HR.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ComentarioController : ControllerBase
    {
        private readonly IGraphClient _client;

        public ComentarioController(IGraphClient client)
        {
            _client = client;
        }

        // 🔹 Obtener los comentarios
        [HttpGet("ObtenerPosts")]
        public async Task<IActionResult> Get()
        {
            var comentarios = await _client.Cypher
                                            .Match("(c:COMENTARIO)")
                                            .Return(c => c.As<Comentario>())
                                            .ResultsAsync;

            return Ok(comentarios);
        }

        // 🔹 Obtener los comentarios de un Post
        [HttpGet("post/{idp}")]
        public async Task<IActionResult> GetByPost(int idp)
        {
            var comentarios = await _client.Cypher
                                            .Match("(c:COMENTARIO)")
                                            .Where((Comentario c) => c.idp == idp)
                                            .Return(c => c.As<Comentario>())
                                            .ResultsAsync;

            return Ok(comentarios);
        }

        [HttpGet("Comentario/{idp}")]
        public async Task<IActionResult> GetComentsAutByPost(int idp)
        {
            var comentarios = await _client.Cypher
                                            .Match("(c:COMENTARIO)")
                                            .Where((Comentario c) => c.idp == idp && c.iduAutorizador != null)
                                            .Return(c => c.As<Comentario>())
                                            .ResultsAsync;

            return Ok(comentarios);
        }

        // 🔹 Obtener comentario 
        [HttpGet("post/{idp}/COMENTARIO/{consec}")]
        public async Task<IActionResult> GetByPostAndConsec(int idp, int consec)
        {
            var comentario = await _client.Cypher
                                          .Match("(c:COMENTARIO)")
                                          .Where((Comentario c) => c.idp == idp && c.consec == consec)
                                          .Return(c => c.As<Comentario>())
                                          .ResultsAsync;

            return Ok(comentario.LastOrDefault());
        }

        // 🔹 Crear comentario
        [HttpPost("Crear")]
        public async Task<IActionResult> Create([FromBody] Comentario comentario)
        {
            await _client.Cypher
                         .Create("(c:COMENTARIO $COMENTARIO)")
                         .WithParam("COMENTARIO", comentario)
                         .ExecuteWithoutResultsAsync();

            return Ok("Comentario creado correctamente.");
        }

        // 🔹 Actualizar comentario 
        [HttpPut("post/{idp}/COMENTARIO/{consec}")]
        public async Task<IActionResult> Update(int idp, int consec, [FromBody] Comentario comentario)
        {
            await _client.Cypher
                         .Match("(c:COMENTARIO)")
                         .Where((Comentario c) => c.idp == idp && c.consec == consec)
                         .Set("c = COMENTARIO")
                         .WithParam("COMENTARIO", comentario)
                         .ExecuteWithoutResultsAsync();

            return Ok("Comentario actualizado correctamente.");
        }

        // 🔹 Eliminar comentario
        [HttpDelete("post/{idp}/COMENTARIO/{consec}")]
        public async Task<IActionResult> Delete(int idp, int consec)
        {
            await _client.Cypher
                         .Match("(c:COMENTARIO)")
                         .Where((Comentario c) => c.idp == idp && c.consec == consec)
                         .Delete("c")
                         .ExecuteWithoutResultsAsync();

            return Ok("Comentario eliminado correctamente.");
        }
    }
}
