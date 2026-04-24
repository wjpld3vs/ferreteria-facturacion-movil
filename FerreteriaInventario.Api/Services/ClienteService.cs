using FerreteriaInventario.Api.Data;
using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Helpers;
using FerreteriaInventario.Api.Interfaces;
using FerreteriaInventario.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaInventario.Api.Services;

public class ClienteService : IClienteService
{
    private readonly AppDbContext _context;

    public ClienteService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClienteResponseDto>> GetAllAsync()
    {
        var clientes = await _context.Clientes
            .OrderBy(x => x.Nombre)
            .ToListAsync();

        return clientes.Select(x => x.ToDto()).ToList();
    }

    public async Task<ClienteResponseDto> GetByIdAsync(int id)
    {
        var cliente = await GetClienteAsync(id);
        return cliente.ToDto();
    }

    public async Task<ClienteResponseDto> CreateAsync(ClienteCreateDto request)
    {
        if (await _context.Clientes.AnyAsync(x => x.Documento.ToLower() == request.Documento.ToLower()))
        {
            throw new ApiException("El documento del cliente ya existe.");
        }

        var cliente = new Cliente
        {
            Nombre = request.Nombre.Trim(),
            Documento = request.Documento.Trim(),
            Telefono = request.Telefono?.Trim(),
            Email = request.Email?.Trim(),
            Direccion = request.Direccion?.Trim(),
            Activo = request.Activo
        };

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente.ToDto();
    }

    public async Task<ClienteResponseDto> UpdateAsync(int id, ClienteUpdateDto request)
    {
        var cliente = await GetClienteAsync(id);

        if (await _context.Clientes.AnyAsync(x =>
                x.Id != id &&
                x.Documento.ToLower() == request.Documento.ToLower()))
        {
            throw new ApiException("El documento del cliente ya existe.");
        }

        cliente.Nombre = request.Nombre.Trim();
        cliente.Documento = request.Documento.Trim();
        cliente.Telefono = request.Telefono?.Trim();
        cliente.Email = request.Email?.Trim();
        cliente.Direccion = request.Direccion?.Trim();
        cliente.Activo = request.Activo;

        await _context.SaveChangesAsync();
        return cliente.ToDto();
    }

    public async Task<ClienteResponseDto> SetActiveAsync(int id, bool activo)
    {
        var cliente = await GetClienteAsync(id);
        cliente.Activo = activo;
        await _context.SaveChangesAsync();
        return cliente.ToDto();
    }

    private async Task<Cliente> GetClienteAsync(int id)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(x => x.Id == id);
        return cliente ?? throw new ApiException("Cliente no encontrado.", StatusCodes.Status404NotFound);
    }
}
