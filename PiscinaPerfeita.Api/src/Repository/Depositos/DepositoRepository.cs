using Microsoft.EntityFrameworkCore;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Models;


namespace PiscinaPerfeita.Api.Repository.Depositos
{
    public class DepositoRepository : IDepositoRepository
    {
        private readonly Data.PiscinaPerfeitaContext _context;

        public DepositoRepository(Data.PiscinaPerfeitaContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<DepositoResponseDto>> Show()
        {
            var query = _context.Depositos.Select(d => new DepositoResponseDto
            {
                Id = d.Id,
                Nome = d.Nome,
                Observacao = d.Observacao,
            });

            return await query.ToListAsync();

        }

        public async Task<DepositoResponseDto?> GetById(Guid id)
        {
            var query = await _context
                .Depositos.Where(e => e.Id == id)
                .Select(a => new DepositoResponseDto
                {
                    Id = a.Id,
                    Nome = a.Nome,
                    Observacao = a.Observacao,
                })
                .FirstOrDefaultAsync();

            return query ?? null;
        }

        public async Task Create(Deposito data)
        {
            _context.Depositos.Add(data);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Guid id, Deposito deposito)
        {
            var depositoGetById = await _context.Depositos.FindAsync(id);
            if (depositoGetById == null)
                throw new KeyNotFoundException($"Deposito com ID {id} não encontrada.");

            depositoGetById.Id = id;
            depositoGetById.Nome = deposito.Nome;
            depositoGetById.Observacao = deposito.Observacao;

            await _context.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var deposito = await _context.Depositos.FirstOrDefaultAsync(a => a.Id == id);
            if (deposito == null)
            {
                throw new KeyNotFoundException($"Análise com ID {id} não encontrada.");
            }

            _context.Remove(deposito);
            await _context.SaveChangesAsync();
        }
    }
}
