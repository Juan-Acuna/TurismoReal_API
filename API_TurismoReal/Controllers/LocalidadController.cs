﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using API_TurismoReal.Conexiones;
using API_TurismoReal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_TurismoReal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocalidadController : ControllerBase
    {
        OracleCommandManager cmd = new OracleCommandManager(ConexionOracle.Conexion);

        [Authorize(Roles = "1,2,3,4")]
        [HttpGet("asignado/{username}")]
        public async Task<IActionResult> Asignado([FromRoute]String username)
        {
            if (!ConexionOracle.Activa)
            {
                ConexionOracle.Open();
                if (!ConexionOracle.Activa)
                {
                    return StatusCode(504, ConexionOracle.NoConResponse);
                }
            }
            var l = await cmd.Find<LocalidadUsuario>("Username", username.ToLower());
            if (l.Count > 0)
            {
                var u = await cmd.Get<Usuario>(l[0].Username);
                var p = await cmd.Get<Persona>(u.Rut);
                return Ok(new PersonaUsuario { Usuario = u, Persona = p });
            }
            return BadRequest();
        }
        [Authorize(Roles = "1")]
        [HttpPost("asignar")]
        public async Task<IActionResult> Asignar([FromBody]LocalidadUsuario lu)
        {
            if (!ConexionOracle.Activa)
            {
                ConexionOracle.Open();
                if (!ConexionOracle.Activa)
                {
                    return StatusCode(504, ConexionOracle.NoConResponse);
                }
            }
            lu.Username = lu.Username.ToLower();
            if (await cmd.Insert(lu,false))
            {
                return Ok();
            }
            return BadRequest();
        }
        [Authorize(Roles = "1,2")]
        [HttpDelete("desasignar/{username}")]
        public async Task<IActionResult> Desasignar([FromRoute]String username)
        {
            if (!ConexionOracle.Activa)
            {
                ConexionOracle.Open();
                if (!ConexionOracle.Activa)
                {
                    return StatusCode(504, ConexionOracle.NoConResponse);
                }
            }
            var lu = (await cmd.Find<LocalidadUsuario>("username", username.ToLower()))[0];
            if (await cmd.Delete(lu))
            {
                return Ok();
            }
            return BadRequest();
        }

        [ProducesResponseType(typeof(List<Localidad>), 200)]
        [ProducesResponseType(typeof(MensajeError), 400)]
        [ProducesResponseType(typeof(MensajeError), 500)]
        [ProducesResponseType(typeof(MensajeError), 504)]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (!ConexionOracle.Activa)
            {
                ConexionOracle.Open();
                if (!ConexionOracle.Activa)
                {
                    return StatusCode(504, ConexionOracle.NoConResponse);
                }
            }
            try
            {
                List<Localidad> l = await cmd.GetAll<Localidad>();
                if (l.Count > 0)
                {
                    return Ok(l);
                }
                return BadRequest(MensajeError.Nuevo("No se encontraron localidades"));
            }
            catch(Exception e)
            {
                return StatusCode(500, MensajeError.Nuevo(e.Message));
            }
        }
        [ProducesResponseType(typeof(Localidad), 200)]
        [ProducesResponseType(typeof(MensajeError), 400)]
        [ProducesResponseType(typeof(MensajeError), 500)]
        [ProducesResponseType(typeof(MensajeError), 504)]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            if (!ConexionOracle.Activa)
            {
                ConexionOracle.Open();
                if (!ConexionOracle.Activa)
                {
                    return StatusCode(504, ConexionOracle.NoConResponse);
                }
            }
            try
            {
                Localidad l = await cmd.Get<Localidad>(id);
                if (l != null)
                {
                    return Ok(l);
                }
                return BadRequest(MensajeError.Nuevo("No se encontró la localidad"));
            }
            catch(Exception e)
            {
                return StatusCode(500, MensajeError.Nuevo(e.Message));
            }
        }
        [Authorize(Roles = "1")]
        [ProducesResponseType(typeof(Localidad), 200)]
        [ProducesResponseType(typeof(MensajeError), 400)]
        [ProducesResponseType(typeof(MensajeError), 500)]
        [ProducesResponseType(typeof(MensajeError), 504)]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Localidad l)
        {
            if (!ConexionOracle.Activa)
            {
                ConexionOracle.Open();
                if (!ConexionOracle.Activa)
                {
                    return StatusCode(504, ConexionOracle.NoConResponse);
                }
            }
            try
            {
                String n = "";
                foreach (var c in l.Nombre.Split(' '))
                {
                    n += " " + Tools.Capitalize(c);
                }
                n = n.TrimStart();
                l.Nombre = n;
                if (await cmd.Insert(l))
                {
                    if (!Directory.Exists(Secret.RUTA_RAIZ + "img\\" + Tools.ToUrlCompatible(l.Nombre) + "\\"))
                    {
                        Directory.CreateDirectory(Secret.RUTA_RAIZ + "img\\" + Tools.ToUrlCompatible(l.Nombre));
                    }
                    return Ok(await cmd.Find<Localidad>("Nombre",l.Nombre));
                }
                return BadRequest();
            }catch(Exception e)
            {
                return StatusCode(500, MensajeError.Nuevo(e.Message));
            }
        }
        [Authorize(Roles = "1")]
        [ProducesResponseType(typeof(Localidad), 200)]
        [ProducesResponseType(typeof(MensajeError), 400)]
        [ProducesResponseType(typeof(MensajeError), 500)]
        [ProducesResponseType(typeof(MensajeError), 504)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch([FromRoute]int id,[FromBody]dynamic data)
        {
            if (!ConexionOracle.Activa)
            {
                ConexionOracle.Open();
                if (!ConexionOracle.Activa)
                {
                    return StatusCode(504, ConexionOracle.NoConResponse);
                }
            }
            try
            {
                var l = await cmd.Get<Localidad>(id);
                var old = l.Nombre;
                if (data.Nombre != null)
                {
                    String n = "";
                    foreach (var c in data.Nombre.Split(' '))
                    {
                        n += " " + Tools.Capitalize(c);
                    }
                    n = n.TrimStart();
                    l.Nombre = n;
                }
                if (await cmd.Update(l))
                {
                    if (!Directory.Exists(Secret.RUTA_RAIZ + "img\\" + Tools.ToUrlCompatible(l.Nombre) + "\\"))
                    {
                        Directory.Move(Secret.RUTA_RAIZ + "img\\" + Tools.ToUrlCompatible(old), Secret.RUTA_RAIZ + "img\\" + Tools.ToUrlCompatible(l.Nombre));
                    }
                    return Ok(await cmd.Get<Localidad>(id));
                }
                return BadRequest(MensajeError.Nuevo("No se pudo actualizar."));
            }catch(Exception e)
            {
                return StatusCode(500, MensajeError.Nuevo(e.Message));
            }
        }
        [Authorize(Roles = "1")]
        [ProducesResponseType(typeof(MensajeError), 400)]
        [ProducesResponseType(typeof(MensajeError), 500)]
        [ProducesResponseType(typeof(MensajeError), 504)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromBody]int id)
        {
            if (!ConexionOracle.Activa)
            {
                ConexionOracle.Open();
                if (!ConexionOracle.Activa)
                {
                    return StatusCode(504, ConexionOracle.NoConResponse);
                }
            }
            try
            {
                Localidad l = await cmd.Get<Localidad>(id);
                if (await cmd.Delete(l))
                {
                    return Ok();
                }
                return BadRequest(MensajeError.Nuevo("No se pudo eliminar."));
            }
            catch(Exception e)
            {
                return StatusCode(500, MensajeError.Nuevo(e.Message));
            }
        }
    }
}