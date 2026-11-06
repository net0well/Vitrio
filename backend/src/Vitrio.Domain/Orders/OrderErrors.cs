using Vitrio.Shared.Results;

namespace Vitrio.Domain.Orders;

public static class OrderErrors
{
    public static readonly Error NotFound = Error.NotFound("order.not_found", "Pedido não encontrado.");

    public static readonly Error MissingService = Error.Validation("order.missing_service", "Escolha o serviço.");

    public static readonly Error InvalidTitle = Error.Validation("order.invalid_title", "Resuma o que precisa com 5 a 80 caracteres.");

    public static readonly Error InvalidDescription =
        Error.Validation("order.invalid_description", "Descreva o que precisa com 20 a 500 caracteres.");

    public static readonly Error InvalidPreferredDay =
        Error.Validation("order.invalid_preferred_day", "Escolha um dia entre hoje e os próximos 30 dias.");

    public static readonly Error PeriodAlreadyOver =
        Error.Validation("order.period_already_over", "Esse período de hoje já acabou. Escolha outro período ou outro dia.");

    public static readonly Error TooManyPhotos = Error.Validation("order.too_many_photos", "Envie até 5 fotos.");

    public static readonly Error PhotoNotFound = Error.NotFound("order.photo_not_found", "Foto não encontrada.");

    public static readonly Error PhotoUploadNotFound =
        Error.Validation("order.photo_upload_not_found", "Uma das fotos não chegou. Envie de novo.");

    public static readonly Error PhotosRemoved =
        Error.Conflict("order.photos_removed", "As fotos deste pedido já foram removidas.");

    public static readonly Error PhoneRequired =
        Error.Unprocessable("order.phone_required", "Cadastre seu celular para o profissional conseguir falar com você.");

    public static readonly Error OwnProvider = Error.Conflict("order.own_provider", "Você não pode pedir orçamento para a sua própria vitrine.");

    public static readonly Error ProviderNotReceiving =
        Error.Conflict("order.provider_not_receiving", "Este profissional não está recebendo pedidos agora.");

    public static readonly Error ServiceNotFound =
        Error.Validation("order.service_not_found", "Esse serviço não está mais na vitrine. Escolha outro.");

    public static readonly Error TooManyOpenOrders =
        Error.Conflict("order.too_many_open", "Você já tem 10 pedidos em andamento. Conclua ou cancele algum para pedir outro.");

    public static readonly Error TooManyWaitingInCategory =
        Error.Conflict("order.too_many_waiting", "Você já tem 3 pedidos aguardando resposta nesta categoria. Espere uma resposta ou cancele algum.");

    public static readonly Error NoSuggestions =
        Error.Conflict("order.no_suggestions", "Só mostramos outros profissionais quando o pedido foi recusado ou ficou sem resposta.");

    public static readonly Error CannotQuote = Error.Conflict("order.cannot_quote", "Este pedido não aceita orçamento agora.");

    public static readonly Error ResponseWindowClosed =
        Error.Conflict("order.response_window_closed", "O prazo para responder este pedido acabou.");

    public static readonly Error InvalidAmount = Error.Validation("quote.invalid_amount", "Informe um valor entre R$ 1,00 e R$ 100.000,00.");

    public static readonly Error InvalidSchedule =
        Error.Validation("quote.invalid_schedule", "Escolha o início de 30 em 30 minutos e uma duração de 30 minutos a 8 horas.");

    public static readonly Error ScheduleCrossesMidnight =
        Error.Validation("quote.schedule_crosses_midnight", "O serviço precisa terminar até a meia-noite do mesmo dia.");

    public static readonly Error StartsTooSoon = Error.Validation("quote.starts_too_soon", "Escolha um horário a partir de 1 hora daqui.");

    public static readonly Error InvalidMaterialDetails =
        Error.Validation("quote.invalid_material_details", "Descreva o material com até 80 caracteres, só quando ele estiver incluso.");

    public static readonly Error InvalidMessage = Error.Validation("quote.invalid_message", "Escreva a mensagem com até 500 caracteres.");

    public static readonly Error CannotDecline = Error.Conflict("order.cannot_decline", "Este pedido não pode mais ser recusado.");

    public static readonly Error DeclineDetailsRequired =
        Error.Validation("order.decline_details_required", "Explique o motivo em até 200 caracteres.");

    public static readonly Error ContactInDetails = Error.Validation("order.contact_in_details", "Tire telefone, e-mail e links do texto.");

    public static readonly Error CannotAnswerQuote =
        Error.Conflict("quote.cannot_answer", "Este orçamento não pode mais ser aceito ou recusado.");

    public static readonly Error QuoteVersionMismatch =
        Error.Conflict("quote.version_mismatch", "O profissional mudou o orçamento. Confira o novo valor antes de aceitar.");

    public static readonly Error QuoteExpired = Error.Conflict("quote.expired", "Este orçamento venceu.");

    public static readonly Error CannotReschedule =
        Error.Conflict("order.cannot_reschedule", "Só dá para ajustar o horário de um serviço agendado que ainda não começou.");

    public static readonly Error RescheduleLimitReached =
        Error.Conflict("order.reschedule_limit", "O horário deste pedido já foi ajustado 2 vezes.");

    public static readonly Error CannotCancel = Error.Conflict("order.cannot_cancel", "Este pedido não pode ser cancelado agora.");

    public static readonly Error InvalidCancellationReason =
        Error.Validation("order.invalid_cancellation_reason", "Escolha um dos motivos da lista.");

    public static readonly Error CancellationDetailsRequired =
        Error.Validation("order.cancellation_details_required", "Conte o motivo em até 200 caracteres.");

    public static readonly Error CannotComplete = Error.Conflict("order.cannot_complete", "Este pedido não pode ser concluído agora.");

    public static readonly Error CompleteBeforeStart =
        Error.Conflict("order.complete_before_start", "Você poderá marcar como concluído a partir do horário agendado.");
}
