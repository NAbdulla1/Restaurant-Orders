﻿namespace Restaurant_Orders.Models.DTOs
{
    public class PagedData<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public long Total { get; set; }
        public bool HasPreviousPage {  get; set; }
        public bool HasNextPage { get; set; }
        public IAsyncEnumerable<T> PageData { get; set;}
    }
}
