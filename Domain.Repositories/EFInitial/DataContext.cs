﻿using Domain.Models.Catalogues;
using Domain.Models.Tables;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Domain.Repositories.EFInitial
{
    public class DataContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public DataContext() : base()
        {
        }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer("Server=localhost,1433;Database=TicketMarket;User ID = SA;Password=Te11#x34&4*&vKXs;TrustServerCertificate=True");
        }

        public DbSet<EventCategory> EventCategory { get; set; }
        public DbSet<EventTable> EventTable { get; set; }
        public DbSet<EventType> EventType { get; set; }
        public DbSet<TicketType> TicketType { get; set; }
        public DbSet<TicketDiscount> TicketDiscount { get; set; }
        public DbSet<Event> Event { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<Ticket> Ticket { get; set; }

        public DbSet<TicketOrder> TicketOrder { get; set; }
        public DbSet<TableEvent> TableEvent { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventCategory>()
                .HasMany(ec => ec.Events)
                .WithOne(e => e.Category)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventTable>()
                .HasMany(et => et.TableEvents)
                .WithOne(te => te.Table)
                .HasForeignKey(te => te.TableId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventType>()
                .HasMany(et => et.Events)
                .WithOne(e => e.Type)
                .HasForeignKey(e => e.TypeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TicketType>()
                .HasMany(tt => tt.Tickets)
                .WithOne(t => t.Type)
                .HasForeignKey(t => t.TypeId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<TicketDiscount>()
                .HasMany(td => td.Tickets)
                .WithOne(t => t.Discount)
                .HasForeignKey(t => t.DiscountId).IsRequired(false)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.User)
                .WithMany(u => u.Events)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Event>()
                .HasMany(e => e.Tickets)
                .WithOne(t => t.Event)
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Event>()
                .HasMany(e => e.TableEvents)
                .WithOne(t => t.Event)
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<TicketOrder>()
                .HasOne(to => to.Ticket)
                .WithMany(t => t.TicketOrders)
                .HasForeignKey(to => to.TicketId).IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);
            
            modelBuilder.Entity<TicketOrder>()
                .HasOne(to => to.Order)
                .WithMany(o => o.TicketOrders)
                .HasForeignKey(to => to.OrderId).IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<TicketDiscount>()
                .HasOne(td => td.User)
                .WithMany(u => u.TicketDiscounts)
                .HasForeignKey(td => td.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
