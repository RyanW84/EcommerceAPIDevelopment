﻿namespace ECommerceApp.RyanW84.Data.Models;

public class Sale
{
    public int SaleId { get; set; }
    public required DateTime SaleDate { get; set; }
    public required decimal TotalAmount { get; set; }
    public required string CustomerName { get; set; } = null!;
    public required string CustomerEmail { get; set; } = null!;
    public required string CustomerAddress { get; set; } = null!;

    // sale has many items (each item references a product)
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();


    }
