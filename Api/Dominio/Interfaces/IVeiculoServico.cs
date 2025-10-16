using MinimalApi.Dominio.Entidades;
using MinimalApi.DTOs;

namespace MinimalApi.Dominio.Interfaces;

public interface IVeiculoServico
{
    List<Veiculo> Todos(int pagina, string? nome = null, string? marca = null);
    Veiculo? BuscaPorId(int id);
    void Adicionar(Veiculo veiculo);
    void Atualizar(Veiculo veiculo);
    void Deletar(Veiculo veiculo);
}