
using DataAccess.Db;
using MyProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product product)
        {
            var productfromdb=_db.Products.FirstOrDefault(p => p.Id == product.Id);
            productfromdb.Id=product.Id;
            productfromdb.Title = product.Title;
            productfromdb.ISBN = product.ISBN;
            productfromdb.Description = product.Description;
            productfromdb.Author = product.Author;
            productfromdb.ListPrice = product.ListPrice;
            productfromdb.Price = product.Price;
            productfromdb.Price50 = product.Price50;
            productfromdb.Price100 = product.Price100;
            productfromdb.CategoryId = product.CategoryId;
            if(product.ImageUrl!=null) 
            {
                productfromdb.ImageUrl = product.ImageUrl;
            }
           
        }
    }
}
