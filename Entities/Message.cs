﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diplom.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string RecipientEmail { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }

    }
}
