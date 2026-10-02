/**
 * @file CuentaDesactivadaException.cs
 * @brief Error de inicio de sesión de una cuenta desactivada.
 * @author Santiago Caicedo
 */
using System;

namespace FarmaciaApp.Core.Services
{
    /**
     * @brief Se lanza al iniciar sesión con una cuenta desactivada.
     *
     * Es un InvalidOperationException para no cambiar el comportamiento de quienes ya lo capturan,
     * pero permite distinguirlo de un fallo de infraestructura.
     */
    public sealed class CuentaDesactivadaException : InvalidOperationException
    {
        /**
         * @brief Crea la excepción.
         * @param mensaje Mensaje para el usuario
         */
        public CuentaDesactivadaException(string mensaje) : base(mensaje) { }
    }
}
