namespace PiscinaPerfeita.Api.Models;

public partial class Produto
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string UnidadeMedida { get; set; } = string.Empty;

    public virtual ICollection<Estoque> Estoques { get; set; } = new List<Estoque>();

    public virtual ICollection<MovimentacaoEstoque> MovimentacoesEstoques { get; set; } = new List<MovimentacaoEstoque>();
}
