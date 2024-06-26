﻿using Microsoft.EntityFrameworkCore;
using PassIn.Communication.Requests;
using PassIn.Communication.Responses;
using PassIn.Exceptions;
using PassIn.Infrastructure;
using System.Net.Mail;

namespace PassIn.Application.UseCases.Events.RegisterAttendee;
public class RegisterAttendeeOnEventUseCase
{
    private readonly PassInDbContext _dbContext;

    public RegisterAttendeeOnEventUseCase()
    {
        _dbContext = new PassInDbContext();
    }

    public ResponseRegisteredJson Execute(Guid eventId, RequestRegisterEventJson request)
    {
        Validate(eventId, request);

        var entity = new Infrastructure.Entities.Attendee
        {
            Name = request.Name,
            Email = request.Email,
            Event_Id = eventId,
            Created_At = DateTime.UtcNow,
        };

        _dbContext.Attendees.Add(entity);
        _dbContext.SaveChanges();

        return new ResponseRegisteredJson
        {
            Id = entity.Id,
        };
    }

    private void Validate(Guid eventId, RequestRegisterEventJson request)
    {
        var eventEntity = _dbContext.Events.Find(eventId);
        if (eventEntity is null)
        {
            throw new NotFoundException("Não existe evento com esse id.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ErrorOnValidationException("Esse nome é invalido.");
        }

        var emailIsValid = EmailIsValid(request.Email);
        if (emailIsValid == false) 
        {
            throw new ErrorOnValidationException("Esse e-mail é invalido.");
        }

        var attendeeAlreadyRegistered = _dbContext
            .Attendees
            .Any(attendee => attendee.Email.Equals(request.Email) && attendee.Event_Id == eventId);

        if (attendeeAlreadyRegistered)
        {
            throw new ConflictException("Você não pode se registrar duas vezes");
        }

        var attendeesForEvent = _dbContext.Attendees.Count(attendee => attendee.Id == eventId);
        if (attendeesForEvent > eventEntity.Maximum_Attendees)
        {
            throw new ErrorOnValidationException("Esse evento não tem mais vag a.");
        }
    }

    private bool EmailIsValid(string email)
    {
        try
        {
            new MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
