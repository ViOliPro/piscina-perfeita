using Microsoft.EntityFrameworkCore;

namespace PiscinaPerfeita.Api.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PiscinaPerfeitaContext _context;

        public UnitOfWork(PiscinaPerfeitaContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task ExecuteAsync(Func<Task> operacao)
        {
            await ExecuteAsync(async () =>
            {
                await operacao();
                return true; // valor descartável — só pra reaproveitar o genérico abaixo
            });
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> operacao)
        {
            // O banco (Program.cs) está configurado com EnableRetryOnFailure,
            // então transações precisam necessariamente passar pela execution
            // strategy — abrir a transação "na mão" sem isso lança
            // InvalidOperationException em runtime ("The configured execution
            // strategy does not support user-initiated transactions").
            var estrategia = _context.Database.CreateExecutionStrategy();

            return await estrategia.ExecuteAsync(async () =>
            {
                await using var transacao = await _context.Database.BeginTransactionAsync();
                try
                {
                    var resultado = await operacao();
                    await transacao.CommitAsync();
                    return resultado;
                }
                catch
                {
                    await transacao.RollbackAsync();
                    throw;
                }
            });
        }
    }
}
