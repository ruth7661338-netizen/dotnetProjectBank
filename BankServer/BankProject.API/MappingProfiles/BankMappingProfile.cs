using AutoMapper;
using BankProject.Core.Dtos.Responses;
using BankProject.Core.Models;

namespace BankProject.API.MappingProfiles;

public class BankMappingProfile : Profile
{
    public BankMappingProfile()
    {
        CreateMap<Account, AccountSummaryDto>();

        CreateMap<Account, AccountResponseDto>()
            .ForMember(dest => dest.CustomerName,
                opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FullName : null))
            .ForMember(dest => dest.Tags,
                opt => opt.MapFrom(src => src.Tags.Select(t => t.Name)));

        CreateMap<Customer, CustomerResponseDto>();

        CreateMap<Transaction, TransactionResponseDto>();
    }
}
