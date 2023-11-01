using System.ComponentModel.DataAnnotations;

namespace RestaurantOrder.Core.DTOs
{
    public class IndexingDTO
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, int.MaxValue)]
        public int? PageSize { get; set; }

        public string? SearchBy { get; set; }

        public string? SortBy { get; set; }

        [RegularExpression("asc|desc", ErrorMessage = "Allowed values are 'asc' or 'desc'.")]
        public string? SortOrder { get; set; } = "asc";
    }
}
