using System;

namespace Diplom.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public string SenderEmail { get; set; }
        public string RecipientEmail { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
    }
}
