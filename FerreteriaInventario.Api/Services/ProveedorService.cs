using FerreteriaInventario.Api.Data;
using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Helpers;
using FerreteriaInventario.Api.Interfaces;
using FerreteriaInventario.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaInventario.Api.Services;

public class ProveedorService : IProveedorService
{
    private readonly AppDbContext _context;

    public ProveedorService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProveedorResponseDto>> GetAllAsync()
    {
        var proveedores = await _context.Proveedores
            .OrderBy(x => x.Nombre)
            .ToListAsync();

        return proveedores.Select(x => x.ToDto()).ToList();
    }

    public async Task<ProveedorResponseDto> GetByIdAsync(int id)
    {
        var proveedor = await GetProveedorAsync(id);
        return proveedor.ToDto();
    }

    public async Task<ProveedorResponseDto> CreateAsync(ProveedorCreateDto request)
    {
        if (await _context.Proveedores.AnyAsync(x => x.DocumentoFiscal.ToLower() == request.DocumentoFiscal.ToLower()))
        {
            throw new ApiException("El documento fiscal del proveedor ya existe.");
        }

        var proveedor = new Proveedor
        {
            Nombre = request.Nombre.Trim(),
            DocumentoFiscal = request.DocumentoFiscal.Trim(),
            Telefono = request.Telefono?.Trim(),
            Email = request.Email?.Trim(),
            Direccion = request.Direccion?.Trim(),
            Activo = request.Activo
        };

        _context.Proveedores.Add(proveedor);
        await _context.SaveChangesAsync();
        return proveedor.ToDto();
    }

    public async Task<ProveedorResponseDto> UpdateAsync(int id, ProveedorUpdateDto request)
    {
        var proveedor = await GetProveedorAsync(id);

        if (await _context.Proveedores.AnyAsync(x =>
                x.Id != id &&
                x.DocumentoFiscal.ToLower() == request.DocumentoFiscal.ToLower()))
        {
            throw new ApiException("El documento fiscal del proveedor ya existe.");
        }

        proveedor.Nombre = request.Nombre.Trim();
        proveedor.DocumentoFiscal = request.DocumentoFiscal.Trim();
        proveedor.Telefono = request.Telefono?.Trim();
        proveedor.Email = request.Email?.Trim();
        proveedor.Direccion = request.Direccion?.Trim();
        proveedor.Activo = request.Activo;

        await _context.SaveChangesAsync();
        return proveedor.ToDto();
    }

    public async Task<ProveedorResponseDto> SetActiveAsync(int id, bool activo)
    {
        var proveedor = await GetProveedorAsync(id);
        proveedor.Activo = activo;
        await _context.SaveChangesAsync();
        return proveedor.ToDto();
    }

    private async Task<Proveedor> GetProveedorAsync(int id)
    {
        var proveedor = await _context.Proveedores.FirstOrDefaultAsync(x => x.Id == id);
        return proveedor ?? throw new ApiException("Proveedor no encontrado.", StatusCodes.Status404NotFound);
    }
}
