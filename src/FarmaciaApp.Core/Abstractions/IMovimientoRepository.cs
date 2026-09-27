/**
 * @file IMovimientoRepository.cs
 * @brief Contrato del repositorio de movimientos del registro de auditoría.
 * @author Santiago Caicedo
 */
using FarmaciaApp.Core.Models;

namespace FarmaciaApp.Core.Abstractions
{
    /**
     * @brief Acceso a datos de movimientos del registro de auditoría.
     */
    public interface IMovimientoRepository
    {
        /**
         * @brief Registra un movimiento con la fecha actual del servidor.
         * @param usuId Usuario que hizo la acción, o null si no hay sesión
         * @param accion Acción realizada
         * @param detalle Detalle de la acción
         */
        void Insert(decimal? usuId, string accion, string detalle);

        /**
         * @brief Busca movimientos en un rango de fechas.
         * @param desde Fecha inicial
         * @param hastaExclusivo Fecha final (no incluida)
         * @param usuId Filtrar por usuario, o null para todos
         * @param texto Texto a buscar en la acción o el detalle, o null
         * @return Hasta 500 movimientos, del más reciente al más antiguo
         */
        IEnumerable<Movimiento> Buscar(DateTime desde, DateTime hastaExclusivo, decimal? usuId, string texto);
    }
}
