using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public class BaseEnity<T>
    {
        public T Id { get; set; }

        [MaxLength(27)]
        public DateTime CreatedAt { get; set; }

        [MaxLength(27)]
        public DateTime? ModifiedAt { get; set; }

        public BaseEnity()
        {
            CreatedAt = DateTime.Now.ToUniversalTime().AddHours(1);
        }
    }
}