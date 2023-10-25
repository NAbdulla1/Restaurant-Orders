using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Restaurant_Orders.Models.DTOs
{
    public class IndexingDTO
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, int.MaxValue)]
        public int? PageSize { get; set; }
    }
}
