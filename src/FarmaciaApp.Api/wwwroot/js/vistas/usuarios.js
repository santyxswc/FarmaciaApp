/**
 * @file usuarios.js
 * @brief Cuentas de usuario (administrador) y cambio de la propia contraseña.
 * @author Santiago Caicedo
 */
import { api } from '../api.js';
import { aviso, confirmar, fechaHora, formulario, h, pintar, tabla } from '../ui.js';

const ROLES = ['Empleado', 'Administrador'];

const CAMPOS_CLAVE_NUEVA = [
  { nombre: 'nueva', etiqueta: 'Contraseña nueva', tipo: 'password', requerido: true, autocompletar: 'new-password' },
  { nombre: 'confirmacion', etiqueta: 'Confirmar contraseña', tipo: 'password', requerido: true, autocompletar: 'new-password' },
];

/** Diálogo para que cualquier usuario cambie su propia contraseña. */
export async function cambiarMiClave() {
  const guardado = await formulario({
    titulo: 'Cambiar mi contraseña',
    campos: [{ nombre: 'actual', etiqueta: 'Contraseña actual', tipo: 'password', requerido: true, autocompletar: 'current-password' }, ...CAMPOS_CLAVE_NUEVA],
    guardar: (d) => api.put('/api/auth/clave', { actual: d.actual, nueva: d.nueva, confirmacion: d.confirmacion }),
  });
  if (guardado) aviso('Contraseña actualizada.');
}

export async function vistaUsuarios(contenedor, { usuario }) {
  const resultado = h('div', { class: 'tarjeta' });
  let personas = [];

  async function cargar() {
    try {
      const cuentas = await api.get('/api/usuarios');
      pintar(resultado, tabla({
        vacio: 'No hay cuentas.',
        filas: cuentas,
        columnas: [
          { titulo: 'Usuario', valor: (u) => u.login },
          { titulo: 'Nombre', valor: (u) => u.nombre },
          { titulo: 'Rol', valor: (u) => u.rol },
          { titulo: 'Estado', valor: (u) => h('span', { class: u.activo ? 'insignia ok' : 'insignia' }, u.activo ? 'Activa' : 'Desactivada') },
          { titulo: 'Último ingreso', valor: (u) => (u.ultimoIngreso ? fechaHora(u.ultimoIngreso) : 'Nunca') },
          { titulo: '', valor: (u) => h('div', { class: 'acciones' },
            h('button', { class: 'enlace', onclick: () => restablecer(u) }, 'Restablecer clave'),
            u.id !== usuario.id && h('button', { class: u.activo ? 'enlace peligro' : 'enlace', onclick: () => cambiarEstado(u) }, u.activo ? 'Desactivar' : 'Activar')) },
        ],
      }));
    } catch (error) {
      pintar(resultado, h('p', { class: 'error' }, error.message));
    }
  }

  async function crear() {
    const guardado = await formulario({
      titulo: 'Nueva cuenta',
      campos: [
        { nombre: 'login', etiqueta: 'Usuario', requerido: true, autocompletar: 'off' },
        { nombre: 'rol', etiqueta: 'Rol', tipo: 'select', opciones: ROLES.map((r) => ({ valor: r, texto: r })) },
        { nombre: 'personaId', etiqueta: 'Persona (obligatoria para empleados)', tipo: 'select',
          opciones: [{ valor: '', texto: 'Sin persona' }, ...personas.map((p) => ({ valor: p.id, texto: `${p.nombre} ${p.apellido}` }))] },
        { nombre: 'clave', etiqueta: 'Contraseña', tipo: 'password', requerido: true, autocompletar: 'new-password' },
        { nombre: 'confirmacion', etiqueta: 'Confirmar contraseña', tipo: 'password', requerido: true, autocompletar: 'new-password' },
      ],
      guardar: (d) => api.post('/api/usuarios', {
        login: d.login, rol: d.rol, clave: d.clave, confirmacion: d.confirmacion,
        personaId: d.personaId === '' ? null : Number(d.personaId),
      }),
    });
    if (guardado) { aviso('Cuenta creada.'); cargar(); }
  }

  async function restablecer(cuenta) {
    const guardado = await formulario({
      titulo: `Restablecer la contraseña de ${cuenta.login}`, campos: CAMPOS_CLAVE_NUEVA, textoGuardar: 'Restablecer',
      guardar: (d) => api.put(`/api/usuarios/${cuenta.id}/clave`, { nueva: d.nueva, confirmacion: d.confirmacion }),
    });
    if (guardado) aviso('Contraseña restablecida.');
  }

  async function cambiarEstado(cuenta) {
    const accion = cuenta.activo ? 'Desactivar' : 'Activar';
    if (!await confirmar(`${accion} cuenta`, `¿${accion.toLowerCase()} la cuenta de ${cuenta.login}?`, accion)) return;
    try {
      await api.put(`/api/usuarios/${cuenta.id}/estado`, { activo: !cuenta.activo });
      aviso(cuenta.activo ? 'Cuenta desactivada.' : 'Cuenta activada.');
      cargar();
    } catch (error) {
      aviso(error.message, 'error');
    }
  }

  pintar(contenedor,
    h('div', { class: 'barra' }, h('h1', null, 'Usuarios'),
      h('div', { class: 'filtros' }, h('button', { class: 'principal', onclick: crear }, 'Nueva cuenta'))),
    resultado);
  try {
    personas = await api.get('/api/personas');
  } catch (error) {
    aviso(error.message, 'error');
  }
  await cargar();
}
