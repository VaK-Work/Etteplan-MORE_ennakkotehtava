using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EtteplanMORE.ServiceManual.ApplicationCore.Entities;
using EtteplanMORE.ServiceManual.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EtteplanMORE.ServiceManual.Web.Controllers
{
    [Route("api/[controller]")]
    public class FactoryDevicesController : Controller
    {
        private readonly IFactoryDeviceService _factoryDeviceService;

        public FactoryDevicesController(IFactoryDeviceService factoryDeviceService)
        {
            _factoryDeviceService = factoryDeviceService;
        }

        /// <summary>
        ///     Hakee kaikki kohteet kriittisyys- ja kirjauspäivämääräjärjestyksen mukaan
        ///     HTTP GET: api/factorydevices/
        ///     Parametrin kanssa haetaan kohteen perusteella:
        ///     HTTP GET: api/factorydevices?kohde=[hakuparametri]
        /// </summary>
        [HttpGet]
        public async Task<IEnumerable<FactoryDeviceDto>> Get(string kohde)       
        {
            //Ensin tarkistetaan onko kutsussa hakuparametria "kohde" tarkistamalla, onko se null tai tyhjä
            if (String.IsNullOrEmpty(kohde))
            {
                //Jos "kohde" on tyhjä, noudetaan kaikki tiedot normaalisti GetAll-haulla
                return (await _factoryDeviceService.GetAll())
                 .Select(fd =>
                     new FactoryDeviceDto
                     {
                         Id = fd.Id,
                         Kirjausaika = fd.Kirjausaika,
                         Kohde = fd.Kohde,
                         Kriittisyys = fd.Kriittisyys,
                         Kuvaus = fd.Kuvaus,
                         Tila = fd.Tila
                     }
                 );
            }
            else if (!String.IsNullOrEmpty(kohde))
            {
                //Jos "kohde"-parametrissa on tekstiä, haetaan sen avulla vain kohteet, jotka sisältävät sen GetAllKohde-haun avulla.
                return (await _factoryDeviceService.GetAllKohde(kohde))
              .Select(fd =>
                  new FactoryDeviceDto
                  {
                      Id = fd.Id,
                      Kirjausaika = fd.Kirjausaika,
                      Kohde = fd.Kohde,
                      Kriittisyys = fd.Kriittisyys,
                      Kuvaus = fd.Kuvaus,
                      Tila = fd.Tila
                  }
              );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///     HTTP GET: api/factorydevices/1
        ///     Haetaan rivi tietokannasta ID:n perusteella
        /// </summary>
        [HttpGet("{id}", Name = "GetByID")]
        public async Task<IActionResult> Get(int id)
        {
            //Suoritetaan haku id:n avulla, ja tallennetaan sen tulos fd-muuttujaan.
            var fd = await _factoryDeviceService.Get(id);
            //jos fd on null, palautetaan NotFound() vastaus.
            if (fd == null)
            {
                return NotFound();
            }

            //Jos hakutulos löytyy, palautetaan se
            return Ok(new FactoryDeviceDto
            {
                Id = fd.Id,
                Kohde = fd.Kohde,
                Kirjausaika = fd.Kirjausaika,
                Kuvaus = fd.Kuvaus,
                Kriittisyys = fd.Kriittisyys,
                Tila = fd.Tila
            });
        }

        /// <summary>
        ///     HTTP POST: api/factorydevices
        ///     Luodaan uusi huoltotehtävä
        ///     Tiedot tulee kutsujan bodysta
        /// </summary>
        [HttpPost]
        public IActionResult Create([FromBody]FactoryDevice huoltoteht)
        {
            //jos syötetty huoltotehtävä on tyhjä, poistutaan funktiosta
            if (huoltoteht == null)
            {
                return NoContent();
            }

            //luodaan FactoryDevice-tyyppinen muuttuja fd, ja lisätään siihen kutsun Bodystä tulleet tiedot. Jos tietoja puuttuu, palautetaan BadRequest()
            FactoryDevice fd = new FactoryDevice();

            if (!String.IsNullOrEmpty(huoltoteht.Kohde) && !String.IsNullOrEmpty(huoltoteht.Kuvaus) && (huoltoteht.Kriittisyys <= 3 && huoltoteht.Kriittisyys >= 1))
            {
                
                fd.Kohde = huoltoteht.Kohde;
                fd.Kuvaus = huoltoteht.Kuvaus;
                fd.Kriittisyys = huoltoteht.Kriittisyys;
                fd.Kirjausaika = DateTime.Today;
                fd.Tila = huoltoteht.Tila;

                //Lähetetään fd syöttöfunktiolle, ja otetaan vastaan saatu tulos, joka on luodun rivin uusi id
                //Palautetaan Id:llä tehty haku, jolla haetaan vasta luodun rivin tiedot.
                
                return CreatedAtRoute("GetByID", new { id = _factoryDeviceService.InsertNew(fd).Result}, fd);
                //return CreatedAtRoute("GetByID", new { Id = createdID }, huoltoteht);
            }
            else
            {
                return BadRequest();
            }
        }

        /// <summary>
        ///     HTTP DELETE: api/factorydevices/5
        ///     Poistetaan Id:llä oleva huoltotehtävä
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            //Lähetetään poistokutsu, ja sen vastaus otetaan vastaan string-taulukkoon. Taulukon ensimmäisessä osiossa [0] on tekstivastaus, ja toisessa osiossa [1] on boolean arvo onnistumisesta.
            string[] result = _factoryDeviceService.Delete(id).Result;

            //jos poisto onnistui, palautetaan Ok-vastaus viestin kera
            if(result[1] == "1")
            {
                return Ok((string)result[0]);
            }
            //Jos poisto epäonnistui, palautetaan NotFound-vastaus ja viesti
            else if(result[1] == "0")
            {
                return NotFound((string)result[0]);
            }
            //Jos ei ole kumpikaan, tulee BadRequest
            else
            {
                return BadRequest();
            }

            
        }

        /// <summary>
        ///     HTTP PUT: api/factorydevices/5
        ///        Kutsun kehossa tulee päivitettävät tiedot
        /// </summary>
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody]FactoryDevice fd)
        {
            //Jos päivitettävät tiedot ovat tyhjiä, palautetaan NoContent
            if (fd == null)
            {
                return NoContent();
            }

            //Lähetetään päivitettävät tiedot fd päivitysfunktiolla, ja otetaan vastaan string-taulukko, jonka ensimmäisessä osiossa [0] on tekstivastaus, ja toisessa osiossa [1] on boolean arvo onnistumisesta.
            string[] result = _factoryDeviceService.Update(id, fd).Result;

            //tarkistetaan onnistuminen, ja palautetaan viesti
            if (result[1] == "1")
            {
                return Ok((string)result[0]);
            }
            else if (result[1] == "0")
            {
                return NotFound((string)result[0]);
            }
            else
            {
                return BadRequest();
            }
        }
    }
}