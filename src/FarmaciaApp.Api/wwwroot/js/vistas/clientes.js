/**
 * @file clientes.js
 * @brief Clientes: alta y edición para todos, eliminación para el administrador.
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

export async function vistaClientes(contenedor, { esAdmin }) {
  const resultado = h('div', { class: 'tarjeta' });
  const buscador = h('input', { type: 'search', placeholder: 'Buscar por nombre o apellido', 'aria-label': 'Buscar cliente' });

  async function cargar() {
    try {
      const termino = buscador.value.trim();
      const clientes = await api.get(`/api/clientes${termino ? `?buscar=${encodeURIComponent(termino)}` : ''}`);
      pintar(resultado, tabla({
        vacio: 'No se encontraron clientes.',
        filas: clientes,
        columnas: [
          { titulo: 'Cliente', valor: (c) => `${c.nombre} ${c.apellido}` },
          { titulo: 'Teléfono', valor: (c) => c.telefono ?? '' },
          { titulo: 'Correo', valor: (c) => c.email ?? '' },
          { titulo: 'Dirección', valor: (c) => c.direccion ?? '' },
          { titulo: '', valor: (c) => h('div', { class: 'acciones' },
            h('button', { class: 'enlace', onclick: () => editar(c) }, 'Editar'),
            esAdmin && h('button', { class: 'enlace peligro', onclick: () => eliminar(c) }, 'Eliminar')) },
        ],
      }));
    } catch (error) {
      pintar(resultado, h('p', { class: 'error' }, error.message));
    }
  }

  async function crear() {
    const guardado = await formulario({
      titulo: 'Nuevo cliente', campos: CAMPOS,
      guardar: (datos) => api.post('/api/clientes', aPeticion(datos)),
    });
    if (guardado) { aviso('Cliente registrado.'); cargar(); }
  }

  async function editar(cliente) {
    const guardado = await formulario({
      titulo: 'Editar cliente', campos: CAMPOS, valores: cliente,
      guardar: (datos) => api.put(`/api/clientes/${cliente.id}`, aPeticion(datos)),
    });
    if (guardado) { aviso('Cliente actualizado.'); cargar(); }
  }

  async function eliminar(cliente) {
    if (!await confirmar('Eliminar cliente', `¿Eliminar a ${cliente.nombre} ${cliente.apellido}? Solo se puede si no tiene facturas.`, 'Eliminar')) return;
    try {
      await api.delete(`/api/clientes/${cliente.id}`);
      aviso('Cliente eliminado.');
      cargar();
    } catch (error) {
      aviso(error.message, 'error');
    }
  }

  buscador.addEventListener('input', esperar(cargar));
  pintar(contenedor,
    h('div', { class: 'barra' }, h('h1', null, 'Clientes'),
      h('div', { class: 'filtros' }, buscador, h('button', { class: 'principal', onclick: crear }, 'Nuevo cliente'))),
    resultado);
  await cargar();
}
