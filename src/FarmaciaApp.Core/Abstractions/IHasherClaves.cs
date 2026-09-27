/**
 * @file IHasherClaves.cs
 * @brief Contrato de derivación y verificación de contraseñas.
 * @author Santiago Caicedo
 */
namespace FarmaciaApp.Core.Abstractions
{
    /**
     * @brief Genera y verifica hashes de contraseñas con sal; nunca se guarda la contraseña.
     */
    public interface IHasherClaves
    {
        /**
         * @brief Genera el hash de una contraseña nueva.
         * @param clave Contraseña en texto plano
         * @return Hash y sal en Base64
         */
        (string Hash, string Sal) Crear(string clave);

        /**
         * @brief Comprueba una contraseña contra su hash guardado.
         * @param clave Contraseña escrita por el usuario
         * @param hash Hash guardado (Base64)
         * @param sal Sal guardada (Base64)
         * @return true si la contraseña es correcta
         */
        bool Verificar(string clave, string hash, string sal);
    }
}
