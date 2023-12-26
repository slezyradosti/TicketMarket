﻿using Application.Core;
using Application.DTOs.Requests;
using Application.DTOs.Tables;
using Application.DTOs.Users.HTTP;
using AutoMapper;
using Domain.Repositories.DTOs;
using Domain.Repositories.Repos.Catalogues;
using Domain.Repositories.Repos.Interfaces.Catalogues;
using Domain.Repositories.Repos.Tables;
using Domain.Repositories.Repos.Interfaces.Tables;

namespace Application.Handlers.Tables.Ticket
{
    public class TicketHandler : ITicketHandler
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;
        private readonly ITicketOrderRepository _ticketOrderRepository;
        private readonly ITicketDiscountRepository _ticketDiscountRepository;
        private readonly ITicketTypeRepository _ticketTypeRepository;

        public TicketHandler(ITicketRepository ticketRepository, IMapper mapper, IUserAccessor userAccessor,
            ITicketOrderRepository ticketOrderRepository, ITicketDiscountRepository ticketDiscountRepository, 
            ITicketTypeRepository ticketTypeRepository)
        {
            _ticketRepository = ticketRepository;
            _mapper = mapper;
            _userAccessor = userAccessor;
            _ticketOrderRepository = ticketOrderRepository;
            _ticketDiscountRepository = ticketDiscountRepository;
            _ticketTypeRepository = ticketTypeRepository;
        }

        //public async Task<Result<List<TicketOrderDto>>> GetCustomersTicketListAsync()
        //{
        //    var userId = _userAccessor.GetUserId();

        //    var ticketOrders = await _ticketOrderRepository.GetCustomersTicketListSortedAsync(userId);

        //    List<TicketOrderDto> ticketOrdersDtos = new List<TicketOrderDto>();

        //    _mapper.Map(ticketOrders, ticketOrdersDtos);

        //    return Result<List<TicketOrderDto>>.Success(ticketOrdersDtos);
        //}

        public async Task<Result<TicketDto>> GetCustomersTicketAsync(Guid ticketId)
        {
            if (!await _ticketOrderRepository.HasUserAccessToTheTicketOrderAsync(ticketId,
                    _userAccessor.GetUserId()))
            {
                return Result<TicketDto>.Failure("You have no access to this data");
            }

            var ticket = await _ticketRepository.GetOneAsync(ticketId);

            TicketDto ticketDto = new TicketDto();

            _mapper.Map(ticket, ticketDto);

            return Result<TicketDto>.Success(ticketDto);
        }

        public async Task<Result<string>> CreateCustomersOneAsync(TicketDto ticketDto)
        {
            if (!await _ticketRepository.HasUserAccessToTheEventAsync(ticketDto.EventId,
                    _userAccessor.GetUserId()))
            {
                return Result<string>.Failure("You have no access to this action");
            }
            
            var ticket = new Domain.Models.Tables.Ticket();

            ticketDto.FinalPrice = await _ticketTypeRepository.GetPriceAsync(ticketDto.TypeId);
            _mapper.Map(ticketDto, ticket);

            var result = await _ticketRepository.AddAsync(ticket) > 0;

            if (!result) return Result<string>.Failure("Failed to create Ticket");

            return Result<string>.Success("Successfully");
        }

        public async Task<Result<string>> EditCustomersOneAsync(TicketDto ticketDto)
        {
            var ticket = await _ticketRepository.GetOneAsync(ticketDto.Id);

            if (ticket == null) return null;

            var userId = _userAccessor.GetUserId();

            if (!await _ticketOrderRepository.HasUserAccessToTheTicketOrderAsync(ticketDto.Id.Value, userId))
            {
                return Result<string>.Failure("You have no access to this data");
            }

            _mapper.Map(ticketDto, ticket);

            var result = await _ticketRepository.SaveAsync(ticket) > 0;

            if (!result) return Result<string>.Failure("Failed to update Ticket");

            return Result<string>.Success("Successfully");
        }

        public async Task<Result<string>> DeleteCustomersOneAsync(Guid ticketId)
        {
            var ticket = await _ticketRepository.GetOneAsync(ticketId);

            if (ticket == null) return null;

            if (!await _ticketOrderRepository.HasUserAccessToTheTicketOrderAsync(ticketId, _userAccessor.GetUserId()))
            {
                return Result<string>.Failure("You have no access to this data");
            }

            var result = await _ticketRepository.RemoveAsync(ticket) > 0;

            if (!result) return Result<string>.Failure("Failed to delete Ticket");

            return Result<string>.Success("Successfully");
        }

        public async Task<Result<string>> GenerateEventsTicketList(TicketDto ticketDto, int ticketAmount)
        {
            if (!await _ticketRepository.HasUserAccessToTheEventAsync(ticketDto.EventId,
                    _userAccessor.GetUserId()))
            {
                return Result<string>.Failure("You have no access to this action");
            }
            
            var tickets = new List<Domain.Models.Tables.Ticket>();
            var ticketDtos = new List<TicketDto>();

            for (int i = 0; i < ticketAmount; i++)
            {
                ticketDtos.Add(ticketDto);
            }
            
            _mapper.Map(ticketDtos, tickets);
            
            var result = await _ticketRepository.AddRangeAsync(tickets) > 0;

            if (!result) return Result<string>.Failure("Failed to create Tickets");

            return Result<string>.Success("Successfully");
        }
        
        public async Task<Result<EventTicketsAmountDto>> GetEventTicketsAmountAsync(Guid eventId)
        {
            var eventTicketsAmount = await _ticketRepository.GetEventsTicketAmountAsync(eventId);

            return Result<EventTicketsAmountDto>.Success(eventTicketsAmount);
        }
        
        public async Task<Result<string>> ApplyDiscountAsync(ApplyDiscountDto applyDiscountDto)
        {
            var ticketDisount = await _ticketDiscountRepository.GetDiscountByCodeAsync(applyDiscountDto.DiscountCode);
            
            if (ticketDisount == null) return Result<string>.Failure("Invalid code");
            
            var ticket = await _ticketRepository.GetOneDetailedAsync(applyDiscountDto.TicketId);

            if (ticket == null) return Result<string>.Failure("Invalid ticket");

            ticket.DiscountId = ticketDisount.Id;
            await CalculateTicketPrice(ticket);
            
            var result = await _ticketRepository.SaveAsync(ticket) > 0;

            if (!result) return Result<string>.Failure("Failed to apply discount");

            return Result<string>.Success("Successfully");
        }
        
        public async Task<Result<string>> RemoveDiscountAsync(Guid ticketId)
        {
            var ticket = await _ticketRepository.GetOneDetailedAsync(ticketId);

            if (ticket == null) return Result<string>.Failure("Invalid ticket");

            ticket.DiscountId = null;
            await CalculateTicketPrice(ticket);

            var result = await _ticketRepository.SaveAsync(ticket) > 0;

            if (!result) return Result<string>.Failure("Failed to remove discount");

            return Result<string>.Success("Successfully");
        }
        
        private async Task CalculateTicketPrice(Domain.Models.Tables.Ticket ticket)
        {
            double defaultPrice = ticket.Type.Price;
            
            if (ticket.DiscountId == null)
            {
                ticket.FinalPrice = defaultPrice;
                return;
            }
            
            int discount = await _ticketDiscountRepository.GetDiscountPercentageAsync(ticket.DiscountId.Value);
                
            double disocuntValue = (100 - discount) / 100.0;
            double finalPrice = defaultPrice * disocuntValue;

            ticket.FinalPrice = finalPrice;
        }
    }
}
