using System.Text.Json;
using System.Text.Json.Serialization;
using Inventory.Repository;
using Inventory.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Inventory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly InventoryDbContext _context;

        public OrderController(InventoryDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<Orders>> AddOrder(OrderRequest orderRequest)
        {

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var order = new Orders(orderRequest.CustomerPhone, orderRequest.Note);
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                var orderId = order.OrdersId;
                var totAmount = 0;
                if (orderRequest.Items != null && orderRequest.Items.Count > 0)
                {
                    foreach (var items in orderRequest.Items)
                    {
                        var NetAmount = _context.Products.Where(x => x.Id == items.ProductId).Select(x => x.Amount).FirstOrDefault();
                        NetAmount = NetAmount * items.Quantity;
                        totAmount = (int)(totAmount + NetAmount);

                        OrderItem orderItem = new OrderItem();
                        orderItem.AddOrderItem(orderId, items.ProductId, items.Quantity, NetAmount, NetAmount);
                        _context.OrderItem.Add(orderItem);
                        await _context.SaveChangesAsync();

                    }
                    order.ActualTotalPrice = totAmount;
                    order.NetTotalPrice = totAmount;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Ok("Order Saved successfully.");
                }
                else
                {
                    throw new ArgumentException("No Items to Order");
                }
            }
            catch (ArgumentException ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }


        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Orders>> UpdateOrder(int id, UpdateOrderRequest orderRequest)
        {
            var existingOrder = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrdersId == id);

            if (existingOrder == null) return NotFound();

            existingOrder.UpdateOrder(orderRequest.CustomerPhone, orderRequest.Note);

            await _context.SaveChangesAsync();

            return BadRequest("Order Updated");
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDetailsResponse>> GetOrderById(int id)
        {           
            var orderDetails = _context.Orders
                    .Include(o => o.Items)
                        .ThenInclude(item => item.Product)
                    .Where(o => o.OrdersId == id)
                    .Select(order => new OrderDetailsResponse
                    {
                        OrderId = order.OrdersId,
                        CustomerPhone = order.CustomerPhone,
                        Note = order.Note,
                        CreatedOnDatetime = order.CreatedOnDatetime,
                        Status = order.Status,
                        ActualTotalPrice = order.ActualTotalPrice,
                        NetTotalPrice = order.NetTotalPrice,
                        Items = order.Items.Select(item => new OrderItemDetailsResponse
                        {
                            ProductId = item.Product.Id,
                            ProductName = item.Product.Name,
                            Quantity = item.Quantity,
                            NetAmount = item.NetAmount,
                            ActualAmount = item.ActualAmount
                        }).ToList()
                    })
                    .FirstOrDefault();

            if (orderDetails == null)
            {
                return NotFound();
            }

            return Ok(orderDetails);  

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            var orderItem = _context.OrderItem.Where(x => x.OrdersId == id).FirstOrDefault();

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == "processed")
            {
                return BadRequest("Can't delete Order already processed");
            }

            _context.OrderItem.Remove(orderItem);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return BadRequest("Order Deleted");
        }

        [HttpPost("{orderId}/process")]
        public IActionResult ProcessOrder(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.Items)
                .FirstOrDefault(o => o.OrdersId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == "processed")
            {
                return BadRequest("Order is already processed.");
            }


            if (order.Items != null && order.Items.Count > 0)
            {
                decimal baseDisc = 0;
                decimal totDisc = 0;
                decimal totNetAmount = 0;
                foreach (var item in order.Items)
                {
                    baseDisc = 0;
                    decimal currentProductAmount = 0;
                    var product = _context.Products.Find(item.ProductId);
                    if (product == null)
                    {
                        continue;
                    }

                    if (product.Category == "Grocery")
                    {
                        // Apply grocery discount logic
                        if (item.Quantity > 2)
                        {
                            totDisc = (totDisc + (product.Amount * 0.10m)); // 10% discount
                            currentProductAmount = (product.Amount - (product.Amount * 0.10m));
                            item.NetAmount = currentProductAmount * item.Quantity;
                            totNetAmount = totNetAmount + item.NetAmount;
                        }
                        else if (item.Quantity == 2)
                        {
                            totDisc = totDisc + (product.Amount * 0.05m); // 5% discount
                            currentProductAmount = product.Amount - (product.Amount * 0.05m);
                            item.NetAmount = currentProductAmount * item.Quantity;
                            totNetAmount = (totNetAmount + item.NetAmount);
                        }
                        else if (item.Quantity == 1)
                        {
                            if (product.BaseDiscountInPercentage > 0)
                            {
                                baseDisc = (product.BaseDiscountInPercentage / 100);
                                totDisc = totDisc + (product.Amount * baseDisc);
                                currentProductAmount = (product.Amount - (product.Amount * baseDisc));
                                item.NetAmount = currentProductAmount * item.Quantity;
                               
                            }
                            totNetAmount = (totNetAmount + item.NetAmount);
                        }
                    }
                    else if (product.Category == "Fashion")
                    {
                        // Apply fashion discount logic
                        var totalFashionItems = order.Items.Count(i => _context.Products.Find(i.ProductId)?.Category == "Fashion");
                        if (totalFashionItems >= 3)
                        {
                            totDisc = (totDisc + (product.Amount * 0.15m)); // 15% discount         
                            currentProductAmount = (product.Amount - (product.Amount * 0.15m));
                            item.NetAmount = currentProductAmount * item.Quantity;
                            totNetAmount = (totNetAmount + item.NetAmount);
                        }
                        else if (totalFashionItems < 3)
                        {
                            if (product.BaseDiscountInPercentage > 0)
                            {
                                baseDisc = (product.BaseDiscountInPercentage / 100);
                                totDisc = (totDisc + (product.Amount * baseDisc));
                                currentProductAmount = (product.Amount - (product.Amount * baseDisc));
                                item.NetAmount = currentProductAmount * item.Quantity;
                                
                            }
                            totNetAmount = (totNetAmount + item.NetAmount);
                        }


                    }
                }
                // Limit the total discount for an order to 500rs
                if (totDisc > 500m)
                {
                    totNetAmount = 0;
                    decimal eachDisc = 500m / order.Items.Count;
                    eachDisc = decimal.Round(eachDisc, 2);
                    foreach (var item in order.Items)
                    {
                        if ((item.ActualAmount - eachDisc) <= 0)
                        {
                            item.NetAmount = 0;
                        }
                        else
                        {
                            item.NetAmount = item.ActualAmount - eachDisc;
                        }

                        totNetAmount = (totNetAmount + item.NetAmount);
                    }
                }
                order.NetTotalPrice = totNetAmount;
            }

            order.Status = "processed";
            _context.SaveChanges();

            return Ok("Order processed successfully.");
        }

        [HttpPost("{orderId}/item")]
        public IActionResult AddItemToOrder(int orderId, List<OrderItemRequest> request)
        {
            var order = _context.Orders
                .Include(o => o.Items)
                .FirstOrDefault(o => o.OrdersId == orderId);

            if (order == null)
            {
                return NotFound();
            }
            if (order.Status == "processed")
            {
                return BadRequest("Order already processed");
            }
            
            if (request != null && request.Count > 0)
            {
                var totAmount = order.NetTotalPrice;
                foreach (var item in request)
                {
                    if (item.Quantity > 0)
                    {
                        var product = _context.Products.Find(item.ProductId);
                        if (product != null)
                        {
                            var NetAmount = _context.Products.Where(x => x.Id == item.ProductId).Select(x => x.Amount).FirstOrDefault();
                            NetAmount = NetAmount * item.Quantity;
                            totAmount = (int)(totAmount + NetAmount);
                            var existItem = _context.OrderItem.Where(x => x.OrdersId == orderId && x.ProductId == item.ProductId).FirstOrDefault();
                            if (existItem == null)
                            {
                                OrderItem orderItem = new OrderItem();
                                orderItem.AddOrderItem(orderId, item.ProductId, item.Quantity, NetAmount, NetAmount);
                                _context.OrderItem.Add(orderItem);
                                _context.SaveChangesAsync();
                            }
                            else
                            {
                                existItem.AddOrderItem(orderId, item.ProductId, item.Quantity, NetAmount, NetAmount);
                                _context.SaveChangesAsync();
                            }

                        }
                    }

                }
                order.ActualTotalPrice = totAmount;
                order.NetTotalPrice = totAmount;
                _context.SaveChangesAsync();
                return Ok("Item added to order successfully.");
            }
            else
            {
                return Ok("No item to update.");
            }



        }

        [HttpDelete("{orderId}/item/{itemId}")]
        public IActionResult DeleteOrderItem(int orderId, int itemId)
        {
            var order = _context.Orders
                 .Include(o => o.Items)
                 .FirstOrDefault(o => o.OrdersId == orderId);

            if (order == null)
            {
                return NotFound();
            }
            if (order.Status == "processed")
            {
                return BadRequest("Can't delete Order already processed");
            }
            if (order.Items.Count > 1)
            {
                var orderItem = _context.OrderItem.Where(x => x.OrdersId == orderId && x.ProductId == itemId).FirstOrDefault();
                if (orderItem != null)
                {
                    _context.OrderItem.Remove(orderItem);
                    _context.SaveChanges();

                    order.ActualTotalPrice = (decimal)_context.OrderItem.Where(x => x.OrdersId == orderId).Sum(x => (double)x.ActualAmount);
                    order.NetTotalPrice = (decimal)_context.OrderItem.Where(x => x.OrdersId == orderId).Sum(x => (double)x.NetAmount);
                    _context.SaveChangesAsync();
                }
            }
            else
            {
                return Ok("Can't Delete Order need atleast 1 Item");
            }

            return Ok("Order item deleted successfully.");
        }
    }
}

