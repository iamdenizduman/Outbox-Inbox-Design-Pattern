﻿using Microsoft.EntityFrameworkCore;
using Order.API.Models.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Order.API.Models.Contexts
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Entities.Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderOutbox> OrderOutboxes { get; set; }
    }
}
