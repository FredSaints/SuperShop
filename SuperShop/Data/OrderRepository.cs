﻿using Microsoft.EntityFrameworkCore;
using SuperShop.Data.Entities;
using SuperShop.Helpers;
using SuperShop.Models;
using System;
using System.Linq;
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

        public async Task<bool> ConfirmOrderAsync(string username)
        {
            var user = await _userHelper.GetUserByEmailAsync(username);
            if (user is null)
            {
                return false;
            }

            var orderTmps = await _context.OrderDetailTemp
                .Include(o => o.Product)
                .Where(u => u.User == user)
                .ToListAsync();

            if (orderTmps is null ||orderTmps.Count == 0)
            {
                return false;
            }

            var details = orderTmps.Select(o => new OrderDetail()
            {
                Price = o.Price,
                Product = o.Product,
                Quantity = o.Quantity,

            }).ToList();

            var order = new Order()
            {
                OrderDate = DateTime.UtcNow,
                User = user,
                Items = details
            };

            await CreateAsync(order);
            _context.OrderDetailTemp.RemoveRange(orderTmps);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task DeleteDetailTempAsync(int id)
        {
            var orderDetailTemp = await _context.OrderDetailTemp.FindAsync(id);
            if (orderDetailTemp is null)
            {
                return;
            }

            _context.OrderDetailTemp.Remove(orderDetailTemp);
            await _context.SaveChangesAsync();
        }

        public async Task DeliveryOrder(DeliveryViewModel model)
        {
            var order = await _context.Orders.FindAsync(model.Id);
            if (order is null)
            {
                return;
            }

            order.DeliveryDate = model.DeliveryDate;
            _context.Orders.Update(order);
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
                    .Include(u => u.User)
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

        public async Task<Order> GetOrderAsync(int id)
        {
            return await _context.Orders.FindAsync(id);
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
