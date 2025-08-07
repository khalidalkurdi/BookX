using Models;
using MyProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.InterfacesRepository
{
    public interface ICompanyRepository : IRepository<Company>
    {
        void Update(Company Company);
    }
}
