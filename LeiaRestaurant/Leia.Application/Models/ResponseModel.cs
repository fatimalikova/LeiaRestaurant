using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Application.Models
{
    public class ResponseModel<T>
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new();
        public T? Data { get; set; }
        public int StatusCode { get; set; }
    }
}
