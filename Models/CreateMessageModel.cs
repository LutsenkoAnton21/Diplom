using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diplom.Models
{
    public class CreateMessageModel
    {
        public string RecipientEmail { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
    }
}
