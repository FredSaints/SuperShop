using Microsoft.EntityFrameworkCore;
using SuperShop.Data.Entities;
using SuperShop.Helpers;
using SuperShop.Models;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SuperShop.Data
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public OrderRepository(DataContext context, IUserHelper userHelper) : base(context)
        {
            _context = context;
            _userHelper = userHelper;
        }

        public async Task AddItemToOrderAsync(AddItemViewModel model, string userName)
        {
            var user = await _userHelper.GetUserByEmailAsync(userName);
            if (user is null)
            {
                return;
            }

            var product = await _context.Products.FindAsync(model.ProductId);
            if (product is null)
            {
                return;
            }

            var orderDetailTemp = await _context.OrderDetailTemp
                .Where(odt => odt.User == user && odt.Product == product)
                .FirstOrDefaultAsync();

            if (orderDetailTemp is null)
            {
                orderDetailTemp = new OrderDetailTemp
                {
                    Price = product.Price,
                    Product = product,
                    Quantity = model.Quantity,
                    User = user
                };

                _context.OrderDetailTemp.Add(orderDetailTemp);
            }
            else
            {
                orderDetailTemp.Quantity += model.Quantity;
                _context.OrderDetailTemp.Update(orderDetailTemp);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IQueryable<OrderDetailTemp>> GetDetailsTempAsync(string userName)
        {
            var user = await _userHelper.GetUserByEmailAsync(userName);
            if (user is null)
            {
                return null;
            }

            return _context.OrderDetailTemp
                .Include(p => p.Product)
                .Where(u => u.User == user)
                .OrderBy(o => o.Product.Name);
        }

        public async Task<IQueryable<Order>> GetOrderAsync(string userName)
        {
            var user = await _userHelper.GetUserByEmailAsync(userName);
            if (user is null)
            {
                return null;
            }

            if (await _userHelper.IsUserInRoleAsync(user, "Admin"))
            {
                return _context.Orders
                    .Include(i => i.Items)
                    .ThenInclude(p => p.Product)
                    .OrderByDescending(o => o.OrderDate);
            }

            return _context.Orders
                .Include(i => i.Items)
                .ThenInclude(p => p.Product)
                .Where(u => u.User == user)
                .OrderByDescending(o => o.OrderDate);
        }

        public async Task ModifyOrderDetailTempQuantityAsync(int id, double quantity)
        {
            var orderDetailTemp = await _context.OrderDetailTemp.FindAsync(id);
            if (orderDetailTemp is null)
            {
                return;
            }
            orderDetailTemp.Quantity += quantity;
            if (orderDetailTemp.Quantity > 0)
            {
                _context.OrderDetailTemp.Update(orderDetailTemp);
                await _context.SaveChangesAsync();
            }
        }
    }
}
