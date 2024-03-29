﻿using Domain.Models.Tables;
using Domain.Repositories.DTOs;

namespace Domain.Repositories.Repos.Interfaces.Tables;

public interface ITicketRepository : IRepository<Ticket>
{
    public Task<bool> HasUserAccessToTheTicketAsync(Guid ticketId, Guid userId);
    public Task<EventTicketsAmountDto> GetEventsTicketAmountAsync(Guid eventId);
    public Task<Ticket> GetOneTypeIncludedAsync(Guid ticketId);
    public Task<Ticket> GetOneDetailedAsync(Guid ticketId);
    public Task<List<Ticket>> GetAvailableTicketListAsync(Guid eventId);
    public Task<List<Ticket>> GetDetailedListAsync(Guid eventId);
    public Task<Ticket?> GetOneToBuyDetailedAsync(Guid eventId, Guid typeId);
    public Task<int> GetCreatedEventsTicketAmount(Guid eventId);
}