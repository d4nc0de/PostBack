using HR.Models;
using HR.Models;
using HR.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neo4jClient;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace HR.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        private readonly IGraphClient _client;

        public PostController(IGraphClient client)
        {
            _client = client;
        }

        [HttpGet("view")]
        public async Task<IActionResult> GetViewList()
        {
            // Trae todos los posts
            var posts = await _client.Cypher
                .Match("(p:POST)")
                .Return(p => p.As<POST>())
                .ResultsAsync;

            // Por cada post, buscar user y comentarios (simple y claro).
            // Para alta escala, conviene optimizar con patrones/colecciones en un solo Cypher.
            var list = new List<PostViewDto>();

            foreach (var post in posts)
            {
                var user = (await _client.Cypher
                    .Match("(u:USUARIO {idu:$idu})")
                    .WithParam("idu", post.idu)
                                    .Return(u => u.As<USUARIO>())
                    .ResultsAsync).FirstOrDefault();

                var comentarios = await _client.Cypher
                    .Match("(c:COMENTARIO {idp:$idp})")
                    .WithParam("idp", post.idp)
                    .Return(c => c.As<COMENTARIO>())
                    .OrderBy("c.consec ASC")
                    .ResultsAsync;

                list.Add(new PostViewDto
                {
                    idp = post.idp,
                    title = post.tittle,
                    description = post.contenido,
                    category = post.Category,
                    CreatedBy = user == null ? null : new UserViewDto
                    {
                        id = user.idu,
                        name = user.nombre,
                        mail = user.mail
                    },
                    comments = comentarios?.Select(c => new CommentViewDto
                    {
                        consec = c.consec,
                        content = c.contenidoCom,
                        createdAt = c.fechorCom,
                        likeNotLike = c.likeNotLike,
                        authorAt = c.fechorAut
                    }).ToList() ?? new List<CommentViewDto>()
                });
            }

            return Ok(list);
        }

        // 🔹 Obtener los comentarios
        [HttpGet("ObtenerPosts")]
        public async Task<IActionResult> Get()
        {
            var Posts = await _client.Cypher
                                            .Match("(c:POST)")
                                            .Return(c => c.As<POST>())
                                    .ResultsAsync;

            return Ok(Posts);
        }

        // 🔹 Obtener los comentarios de un Post
        [HttpGet("post/{idp:int}")]
        public async Task<IActionResult> GetByid(int idp)
        {
            // 1️⃣ Buscar el post principal
            var post = (await _client.Cypher
                .Match("(p:POST {idp:$idp})")
                .WithParam("idp", idp)
                .Return(p => p.As<POST>())
                .ResultsAsync)
                .FirstOrDefault();

            if (post is null)
                return NotFound($"No existe el post con idp={idp}");

            // 2️⃣ Buscar el usuario creador
            var user = (await _client.Cypher
                .Match("(u:USUARIO {idu:$idu})")
                .WithParam("idu", post.idu)
                                    .Return(u => u.As<USUARIO>())
                                    .ResultsAsync)
                        .FirstOrDefault();

            // 3️⃣ Buscar comentarios asociados
            var comentarios = await _client.Cypher
                .Match("(c:COMENTARIO {idp:$idp})")
                .WithParam("idp", idp)
                .Return(c => c.As<COMENTARIO>())
                .OrderBy("c.consec ASC")
                .ResultsAsync;

            // 4️⃣ Mapear al nuevo DTO (idp en lugar de id)
            var dto = new
            {
                idp = post.idp,
                title = post.tittle,
                description = post.contenido,
                category = post.Category,
                CreatedBy = user == null
                    ? null
                    : new
                    {
                        id = user.idu,
                        name = user.nombre,
                        mail = user.mail
                    },
                comments = comentarios?.Select(c => new CommentViewDto
                {
                    consec = c.consec,
                    content = c.contenidoCom,
                    createdAt = c.fechorCom,
                    likeNotLike = c.likeNotLike,
                    authorAt = c.fechorAut
                }).ToList() ?? new List<CommentViewDto>()
            };

            return Ok(dto);
        }

        //Crear un nuevo usuario
        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] USUARIO user)
        //{
        //    var exists = (await _client.Cypher
        //                              .Match("(u:USUARIO)")
        //                              .Where((USUARIO u) => u.idu == user.idu)
        //                              .Return(u => u.As<USUARIO>())
        //                              .ResultsAsync)
        //                 .Any();
        //    if (exists) return Conflict($"Usuario con idu={user.idu} ya existe.");
        //}

            // 🔹 Obtener comentario 
            //[HttpGet("post/{idp}/COMENTARIO/{consec}")]
            //public async Task<IActionResult> GetByPostAndConsec(int idp, int consec)
            //{
            //    var comentario = await _client.Cypher
            //                                  .Match("(c:COMENTARIO)")
            //                                  .Where((Comentario c) => c.idp == idp && c.consec == consec)
            //                                  .Return(c => c.As<Comentario>())
            //                                  .ResultsAsync;

            //    return Ok(comentario.LastOrDefault());
            //}

            // 🔹 Crear comentario
            [HttpPost]
            public async Task<IActionResult> Create([FromBody] POST post)
            {
                await _client.Cypher
                             .Create("(c:POST $POST)")
                             .WithParam("POST", post)
                                .ExecuteWithoutResultsAsync();

                return Ok("Post creado correctamente.");
            }

            // 🔹 Actualizar comentario 
            [HttpPut("post/{idp}")]
            public async Task<IActionResult> Update(int idp, [FromBody] POST post)
            {
                await _client.Cypher
                             .Match("(c:POST)")
                             .Where((POST c) => c.idp == idp)
                             .Set("c = POST")
                             .WithParam("POST", post)
                            .ExecuteWithoutResultsAsync();

                return Ok("Post actualizado correctamente.");
            }

            // 🔹 Eliminar comentario
            [HttpDelete("post/{idp}")]
            public async Task<IActionResult> Delete(int idp)
            {
                await _client.Cypher
                             .Match("(c:POST)")
                             .Where((POST c) => c.idp == idp)
                             .Delete("POST")
                                .ExecuteWithoutResultsAsync();

                return Ok("Post eliminado correctamente.");
            }

        }
    }
