﻿using Domain.Models.Catalogues;
using Domain.Models.Tables;

namespace Application.DTOs.Tables
{
    public class TableEventDto
    {
        public Guid? Id { get; set; }
        public Event? Event { get; set; }
        public Guid? EventId { get; set; }
        public EventTable? Table { get; set; }
        public Guid? TableId { get; set; }
    }
}
