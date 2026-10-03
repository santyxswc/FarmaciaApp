/**
 * @file reclamos.js
 * @brief Reclamos de los clientes sobre sus facturas.
 * @author Santiago Caicedo
 */
import { api } from '../api.js';
import { aviso, confirmar, esperar, fechaHora, formulario, h, pintar, tabla } from '../ui.js';

export async function vistaReclamos(contenedor, { esAdmin }) {
  const resultado = h('div', { class: 'tarjeta' });
  const buscador = h('input', { type: 'search', placeholder: 'Cliente, estado o descripción', 'aria-label': 'Buscar reclamo' });
  let estados = [];

  const campos = (conFactura) => [
    conFactura && { nombre: 'facturaNumero', etiqueta: 'N° de factura', tipo: 'number', requerido: true, min: 1, step: 1 },
    { nombre: 'descripcion', etiqueta: 'Descripción', tipo: 'textarea', requerido: true },
    { nombre: 'estado', etiqueta: 'Estado', tipo: 'select', opciones: estados.map((e) => ({ valor: e, texto: e })) },
  ].filter(Boolean);

  async function cargar() {
    try {
      const termino = buscador.value.trim();
      const reclamos = await api.get(`/api/reclamos${termino ? `?buscar=${encodeURIComponent(termino)}` : ''}`);
      pintar(resultado, tabla({
        vacio: 'No se encontraron reclamos.',
        filas: reclamos,
        columnas: [
          { titulo: 'N°', valor: (r) => r.id },
          { titulo: 'Fecha', valor: (r) => fechaHora(r.fecha) },
          { titulo: 'Factura', valor: (r) => r.facturaNumero },
          { titulo: 'Cliente', valor: (r) => r.cliente },
          { titulo: 'Descripción', valor: (r) => r.descripcion },
          { titulo: 'Estado', valor: (r) => h('span', { class: 'insignia' }, r.estado) },
          { titulo: '', valor: (r) => h('div', { class: 'acciones' },
            h('button', { class: 'enlace', onclick: () => editar(r) }, 'Editar'),
            esAdmin && h('button', { class: 'enlace peligro', onclick: () => eliminar(r) }, 'Eliminar')) },
        ],
      }));
    } catch (error) {
      pintar(resultado, h('p', { class: 'error' }, error.message));
    }
  }

  async function crear() {
    const guardado = await formulario({
      titulo: 'Nuevo reclamo', campos: campos(true), valores: { estado: estados[0] },
      guardar: (d) => api.post('/api/reclamos', { facturaNumero: Number(d.facturaNumero), descripcion: d.descripcion, estado: d.estado }),
    });
    if (guardado) { aviso('Reclamo registrado.'); cargar(); }
  }

  async function editar(reclamo) {
    const guardado = await formulario({
      titulo: `Reclamo N° ${reclamo.id}`, campos: campos(false), valores: reclamo,
      guardar: (d) => api.put(`/api/reclamos/${reclamo.id}`, { facturaNumero: reclamo.facturaNumero, descripcion: d.descripcion, estado: d.estado }),
    });
    if (guardado) { aviso('Reclamo actualizado.'); cargar(); }
  }

  async function eliminar(reclamo) {
    if (!await confirmar('Eliminar reclamo', `¿Eliminar el reclamo N° ${reclamo.id}?`, 'Eliminar')) return;
    try {
      await api.delete(`/api/reclamos/${reclamo.id}`);
      aviso('Reclamo eliminado.');
      cargar();
    } catch (error) {
      aviso(error.message, 'error');
    }
  }

  buscador.addEventListener('input', esperar(cargar));
  pintar(contenedor,
    h('div', { class: 'barra' }, h('h1', null, 'Reclamos'),
      h('div', { class: 'filtros' }, buscador, h('button', { class: 'principal', onclick: crear }, 'Nuevo reclamo'))),
    resultado);
  try {
    estados = await api.get('/api/reclamos/estados');
  } catch (error) {
    aviso(error.message, 'error');
  }
  await cargar();
}
