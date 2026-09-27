/**
 * @file IAuditoria.cs
 * @brief Contrato del registro de movimientos.
 * @author Santiago Caicedo
 */
namespace FarmaciaApp.Core.Abstractions
{
    /**
     * @brief Registra lo que hace el usuario del turno. Los servicios la llaman tras cada operación exitosa.
     */
    public interface IAuditoria
    {
        /**
         * @brief Registra un movimiento a nombre del usuario de la sesión.
         * @param accion Acción realizada
         * @param detalle Detalle opcional
         */
        void Registrar(string accion, string detalle = null);
    }
}
