namespace PiscinaPerfeita.Api.Data
{
    // Cada repositório chama SaveChangesAsync() de forma independente ao
    // final do próprio método (ver UsuarioRepository.Create,
    // UsuarioLocalRepository.Create, etc.) — o que é conveniente para
    // operações isoladas, mas cria uma janela real de inconsistência
    // quando um caso de uso do Service precisa gravar em mais de uma
    // tabela como uma única operação lógica. Exemplo: UsuarioService.Create
    // grava o Usuario e, na sequência, o vínculo UsuarioLocal — se a
    // segunda chamada falhar (violação de constraint, timeout, etc.), o
    // Usuario já foi commitado sozinho, sem nenhum vínculo com Local
    // nenhum (usuário órfão).
    //
    // IUnitOfWork existe para os Services orquestrarem esses casos:
    // ExecuteAsync abre uma transação real no banco, roda o delegate
    // (que internamente chama os repositórios normalmente, cada um com
    // seu SaveChangesAsync — todos compartilham o mesmo DbContext/conexão
    // dentro do escopo da requisição, então participam da mesma
    // transação) e só comita se tudo suceder. Qualquer exceção reverte
    // tudo e é relançada para o chamador tratar normalmente.
    //
    // Não usar para uma única chamada de repositório (não ganha nada) —
    // só quando 2+ chamadas de escrita precisam ser tudo-ou-nada.
    public interface IUnitOfWork
    {
        Task ExecuteAsync(Func<Task> operacao);
        Task<T> ExecuteAsync<T>(Func<Task<T>> operacao);
    }
}
