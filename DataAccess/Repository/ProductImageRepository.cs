
using DataAccess.Db;
using DataAccess.InterfacesRepository;
using Models;
using MyProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductImageRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ProductImage productImage)
        {
            var productImagefromdb=_db.ProductImages.FirstOrDefault(p => p.Id == productImage.Id);
            productImagefromdb.ProductId=productImage.ProductId;
            productImagefromdb.ImageUrl=productImage.ImageUrl;
           
        }
    }
}
