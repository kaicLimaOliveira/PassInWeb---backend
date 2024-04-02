
using PassIn.Communication.Requests;
using PassIn.Exceptions;
using PassIn.Infrastructure;

namespace PassIn.Application.UseCases.Events.Register;
public class RegisterEventUseCase
{
    public void Execute(RequestEventJson request)
    {
        Validate(request);

        var dbContext = new PassInDbContext();

        var eventity = new Infrastructure.Entities.Event
        {
            Title = request.Title,
            Details = request.Details,
            Maximum_Attendees = request.MaximumAttendees,
        };

        dbContext.Events.Add(eventity);
    }

    private void Validate(RequestEventJson request)
    {
        if (request.MaximumAttendees <= 0)
        {
            throw new PassInException("O máximo de participantes é invalido.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new PassInException("O título é invalido.");
        }

        if (string.IsNullOrWhiteSpace(request.Details))
        {
            throw new PassInException("Os detalhes são invalido.");
        }
    }
}
