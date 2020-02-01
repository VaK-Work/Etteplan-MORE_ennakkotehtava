using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EtteplanMORE.ServiceManual.ApplicationCore.Entities;

namespace EtteplanMORE.ServiceManual.ApplicationCore.Interfaces
{
    public interface IFactoryDeviceService
    {
        Task<IEnumerable<FactoryDevice>> GetAll();

        Task<IEnumerable<FactoryDevice>> GetAllKohde(string kohde);

        Task<int> InsertNew(FactoryDevice fd);

        Task<string[]> Delete(int id);

        Task<string[]> Update(int id, FactoryDevice fd);

        Task<FactoryDevice> Get(int id);
    }
}