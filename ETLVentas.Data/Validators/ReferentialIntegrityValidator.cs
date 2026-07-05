using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ETLVentas.Data.Context;

namespace ETLVentas.Data.Validators
{
    public class ReferentialIntegrityValidator
    {
        private readonly AnalisisDeVentasContext _context;

        // Caches to avoid N+1 queries during ETL
        private readonly ConcurrentDictionary<string, int> _countryCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, int> _cityCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, int> _categoryCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, int> _customerCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, int> _productCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, int> _orderCache = new(StringComparer.OrdinalIgnoreCase);

        public ReferentialIntegrityValidator(AnalisisDeVentasContext context)
        {
            _context = context;
        }

        public async Task<int?> GetCountryIdAsync(string countryName)
        {
            if (_countryCache.TryGetValue(countryName, out var cachedId)) return cachedId;

            var country = await _context.Countries.AsNoTracking().FirstOrDefaultAsync(c => c.CountryName == countryName);
            if (country != null)
            {
                _countryCache[countryName] = country.CountryId;
                return country.CountryId;
            }
            return null;
        }

        public async Task<int?> GetCityIdAsync(string cityName, int countryId)
        {
            var key = $"{cityName}_{countryId}";
            if (_cityCache.TryGetValue(key, out var cachedId)) return cachedId;

            var city = await _context.Cities.AsNoTracking().FirstOrDefaultAsync(c => c.CityName == cityName && c.CountryId == countryId);
            if (city != null)
            {
                _cityCache[key] = city.CityId;
                return city.CityId;
            }
            return null;
        }

        public async Task<int?> GetCategoryIdAsync(string categoryName)
        {
            if (_categoryCache.TryGetValue(categoryName, out var cachedId)) return cachedId;

            var cat = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryName == categoryName);
            if (cat != null)
            {
                _categoryCache[categoryName] = cat.CategoryId;
                return cat.CategoryId;
            }
            return null;
        }

        public async Task<int?> GetCustomerIdAsync(string email)
        {
            if (_customerCache.TryGetValue(email, out var cachedId)) return cachedId;

            var cust = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Email == email);
            if (cust != null)
            {
                _customerCache[email] = cust.CustomerId;
                return cust.CustomerId;
            }
            return null;
        }

        public async Task<int?> GetProductIdAsync(string productName)
        {
            if (_productCache.TryGetValue(productName, out var cachedId)) return cachedId;

            var prod = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductName == productName);
            if (prod != null)
            {
                _productCache[productName] = prod.ProductId;
                return prod.ProductId;
            }
            return null;
        }
        
        public async Task<int?> GetOrderIdAsync(int customerId, DateTime orderDate)
        {
            var dDate = new DateOnly(orderDate.Year, orderDate.Month, orderDate.Day);
            var key = $"{customerId}_{dDate:yyyyMMdd}";
            
            if (_orderCache.TryGetValue(key, out var cachedId)) return cachedId;

            var order = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.CustomerId == customerId && o.OrderDate == dDate);
            if (order != null)
            {
                _orderCache[key] = order.OrderId;
                return order.OrderId;
            }
            return null;
        }
    }
}
