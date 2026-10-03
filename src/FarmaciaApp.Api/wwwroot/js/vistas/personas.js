/**
 * @file personas.js
 * @brief Personas con sus roles de cliente y vendedor.
 * @author Santiago Caicedo
 */
import { api } from '../api.js';
import { aviso, confirmar, esperar, formulario, h, pintar, tabla } from '../ui.js';

const CAMPOS = [
  { nombre: 'nombre', etiqueta: 'Nombre', requerido: true },
  { nombre: 'apellido', etiqueta: 'Apellido', requerido: true },
  { nombre: 'direccion', etiqueta: 'Dirección' },
  { nombre: 'telefono', etiqueta: 'Teléfono', tipo: 'tel' },
  { nombre: 'email', etiqueta: 'Correo electrónico', tipo: 'email' },
];

const aPeticion = (d) => ({
  nombre: d.nombre, apellido: d.apellido,
  direccion: d.direccion || null, telefono: d.telefono || null, email: d.email || null,
});

export async function vistaPersonas(contenedor, { esAdmin }) {
  const resultado = h('div', { class: 'tarjeta' });
  const buscador = h('input', { type: 'search', placeholder: 'Buscar por nombre', 'aria-label': 'Buscar persona' });

  async function cargar() {
    try {
      const termino = buscador.value.trim();
      const personas = await api.get(`/api/personas${termino ? `?buscar=${encodeURIComponent(termino)}` : ''}`);
      pintar(resultado, tabla({
        vacio: 'No se encontraron personas.',
        filas: personas,
        columnas: [
          { titulo: 'Persona', valor: (p) => `${p.nombre} ${p.apellido}` },
          { titulo: 'Teléfono', valor: (p) => p.telefono ?? '' },
          { titulo: 'Correo', valor: (p) => p.email ?? '' },
          { titulo: 'Roles', valor: (p) => [
            p.esCliente && h('span', { class: 'insignia' }, 'Cliente'),
            p.esVendedor && h('span', { class: 'insignia ok' }, 'Vendedor'),
          ] },
          { titulo: '', valor: (p) => h('div', { class: 'acciones' },
            h('button', { class: 'enlace', onclick: () => editar(p) }, 'Editar'),
            esAdmin && h('button', { class: 'enlace', onclick: () => cambiarVendedor(p) }, p.esVendedor ? 'Quitar vendedor' : 'Hacer vendedor'),
            esAdmin && h('button', { class: 'enlace peligro', onclick: () => eliminar(p) }, 'Eliminar')) },
        ],
      }));
    } catch (error) {
      pintar(resultado, h('p', { class: 'error' }, error.message));
    }
  }

  async function crear() {
    const guardado = await formulario({
      titulo: 'Nueva persona', campos: CAMPOS,
      guardar: (datos) => api.post('/api/personas', aPeticion(datos)),
    });
    if (guardado) { aviso('Persona registrada.'); cargar(); }
  }

  async function editar(persona) {
    const guardado = await formulario({
      titulo: 'Editar persona', campos: CAMPOS, valores: persona,
      guardar: (datos) => api.put(`/api/personas/${persona.id}`, aPeticion(datos)),
    });
    if (guardado) { aviso('Persona actualizada.'); cargar(); }
  }

  async function cambiarVendedor(persona) {
    try {
      await api.put(`/api/personas/${persona.id}/vendedor`, { esVendedor: !persona.esVendedor });
      aviso(persona.esVendedor ? 'Ya no es vendedor.' : 'Ahora es vendedor.');
      cargar();
    } catch (error) {
      aviso(error.message, 'error');
    }
  }

  async function eliminar(persona) {
    if (!await confirmar('Eliminar persona', `¿Eliminar a ${persona.nombre} ${persona.apellido}? Solo se puede si no tiene facturas ni cuenta.`, 'Eliminar')) return;
    try {
      await api.delete(`/api/personas/${persona.id}`);
      aviso('Persona eliminada.');
      cargar();
    } catch (error) {
      aviso(error.message, 'error');
    }
  }

  buscador.addEventListener('input', esperar(cargar));
  pintar(contenedor,
    h('div', { class: 'barra' }, h('h1', null, 'Personas'),
      h('div', { class: 'filtros' }, buscador, h('button', { class: 'principal', onclick: crear }, 'Nueva persona'))),
    resultado);
  await cargar();
}
