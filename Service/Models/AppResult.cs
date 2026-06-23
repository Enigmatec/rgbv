using System.Collections.Generic;
using System.Linq;

namespace Service.Models
{
    public class AppResult<T>
    {
        public AppResult(string errorMessage)
        {
            AddError(errorMessage);
        }
        public AppResult()
        {

        }
        public T Data { get; set; }

        public bool HasError => Errors.Any();

        public int StatusCode { get; set; }

        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public void AddError(string error)
        {
            Errors.Add(error);
        }
    }
}