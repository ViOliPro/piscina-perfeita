using PiscinaPerfeita.Api.Dtos.Request;
using PiscinaPerfeita.Api.Dtos.Response;
using PiscinaPerfeita.Api.Helpers.Authenticated;
using PiscinaPerfeita.Api.Models;
using PiscinaPerfeita.Api.Repository.Usuarios;

using PiscinaPerfeita.Api.Repository.Depositos;
namespace PiscinaPerfeita.Api.Service.Depositos
{
    public class DepositoService : IDepositoService
    {
        private readonly IDepositoRepository _depositoRepository;
        private readonly IAuthenticatedUser _user;
        private readonly IUsuarioRepository _userRepository;


        public DepositoService(
            IDepositoRepository depositoRepository,
            IAuthenticatedUser user,
            IUsuarioRepository userRepository

        )
        {
            _depositoRepository =
                depositoRepository ?? throw new ArgumentNullException(nameof(depositoRepository));
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _userRepository =
                userRepository ?? throw new ArgumentNullException(nameof(userRepository));

        }
        // Implementação dos métodos do serviço
        // Metodo Show: Retorna uma lista de todos os estoques, incluindo as informações relacionadas de piscina e produto.
        public async Task<List<DepositoResponseDto>> Show()
        {
            return await _depositoRepository.Show();
        }

        // Metodo GetById: Retorna um estoque específico com base no ID, incluindo as informações relacionadas de piscina e produto.
        public async Task<DepositoResponseDto> GetById(Guid id)
        {
            var data = await _depositoRepository.GetById(id);

            return data == null
                ? throw new KeyNotFoundException(
                    $"Não possivel localizar uma analise com o id {id} informado"
                )
                : data;
        }

        // Metodo Create: Cria um novo Analise com base nos dados fornecidos, incluindo as informações relacionadas de piscina e produto.
        public async Task<DepositoResponseDto> Create(DepositoRequestDto dto)
        {
            var userDb = await _userRepository.GetById(_user.GetUserId());
            if (userDb == null)
                throw new KeyNotFoundException(
                    "Problemas ao registrar, usuario ID analise não localizado"
                );


            var newDeposito = new Deposito
            {
                Nome = dto.Nome,
                Observacao = dto.Observacao,
            };

            await _depositoRepository.Create(newDeposito);

            return new DepositoResponseDto
            {
                Id = newDeposito.Id,
                Nome = newDeposito.Nome,
                Observacao = newDeposito.Observacao,
            };
        }

        // Metodo Update: Atualiza um estoque existente com base no ID e nos dados fornecidos, incluindo as informações relacionadas de piscina e produto.
        public async Task<DepositoResponseDto> Update(Guid id, DepositoRequestDto dto)
        {
            var depositoDb = await _depositoRepository.GetById(id);
            if (depositoDb == null)
            {
                throw new KeyNotFoundException($"Analise com id {id} não encontrado.");
            }

            var depositoUpdated = new Deposito
            {
                Id = id,
                Nome = dto.Nome,
                Observacao = dto.Observacao,
            };

            await _depositoRepository.Update(id, depositoUpdated);

            return new DepositoResponseDto
            {
                Id = id,
                Nome = depositoUpdated.Nome,
                Observacao = depositoUpdated.Observacao,
            };
        }

        // Metodo Delete: Exclui um estoque existente com base no ID.
        public async Task Delete(Guid id)
        {
            var analisesDb = await _depositoRepository.GetById(id);
            if (analisesDb == null)
            {
                throw new KeyNotFoundException($"Estoque com id {id} não encontrado.");
            }

            await _depositoRepository.Delete(id);
        }
    }
}
