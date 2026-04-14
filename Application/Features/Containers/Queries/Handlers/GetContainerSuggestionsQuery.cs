using Application.DTOs.ContainerDtos;
using Application.ViewModel;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Containers.Queries.Handlers
{
    public class GetContainerSuggestionsQuery : IRequest<ResponseViewModel<List<ContainerSuggestionDto>>>
    {
        public Guid ImportRequestId { get; set; }
    }
}
