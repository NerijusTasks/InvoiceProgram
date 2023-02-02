using Newtonsoft.Json.Linq;

namespace InvoiceProgram.Models
{
    public class ResponseModel
    {
        public string? Status { get; set; }

        public JToken? Data { get; set; }
    }
}
