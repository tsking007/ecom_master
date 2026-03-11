namespace EcommerceApp.Application.Common.Interfaces;

public interface ICheckoutTransactionExecutor
{
    Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);
}