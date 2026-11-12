namespace RepairService.Core.Abstractions;

/// <summary>
/// Сповіщувач клієнта про події з квитанцією.
/// </summary>
public interface INotifier
{
    /// <summary>Надсилає сповіщення.</summary>
    /// <param name="message">Текст сповіщення.</param>
    void Notify(string message);
}
