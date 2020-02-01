using System;

namespace EtteplanMORE.ServiceManual.ApplicationCore.Entities
{
    public class FactoryDeviceDto
    {

        public int Id { get; set; }
        public string Kohde { get; set; }
        public DateTime Kirjausaika { get; set; }
        public string Kuvaus { get; set; }
        public int Kriittisyys { get; set; }
        public bool? Tila { get; set; }
    }
}