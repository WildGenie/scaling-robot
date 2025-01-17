﻿using Librarian.Domain.Enums;

namespace Librarian.Domain.Entities
{
    /*
     Kütüphanedeki kitaplarımı temsil edecek entity tipi. 
     */
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Authors { get; set; }
        public string Publisher { get; set; }
        public Status Status { get; set; }
        public Language Language { get; set; }
        public byte Row { get; set; }
        public byte Column { get; set; } 
    }
}
