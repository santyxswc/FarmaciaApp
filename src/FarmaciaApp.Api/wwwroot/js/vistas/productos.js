/**
 * @file productos.js
 * @brief Catálogo de productos: consulta para todos, mantenimiento para el administrador.
 * @author Santiago Caicedo
 */
import { api } from '../api.js';
import { aviso, confirmar, esperar, formulario, h, moneda, numero, pintar, tabla } from '../ui.js';

const CAMPOS = [
  { nombre: 'nombre', etiqueta: 'Nombre', requerido: true },
  { nombre: 'descripcion', etiqueta: 'Descripción', tipo: 'textarea' },
  { nombre: 'precio', etiqueta: 'Precio (IVA incluido)', tipo: 'number', requerido: true, min: 1, step: 1 },
  { nombre: 'stock', etiqueta: 'Stock', tipo: 'number', requerido: true, min: 0, step: 1 },
];

const aPeticion = (d) => ({ nombre: d.nombre, descripcion: d.descripcion || null, precio: Number(d.precio), stock: Number(d.stock) });

export async function vistaProductos(contenedor, { esAdmin }) {
  const resultado = h('div', { class: 'tarjeta' });
  const buscador = h('input', { type: 'search', placeholder: 'Buscar por nombre', 'aria-label': 'Buscar producto' });

  async function cargar() {
    try {
      const termino = buscador.value.trim();
      const productos = await api.get(`/api/productos${termino ? `?buscar=${encodeURIComponent(termino)}` : ''}`);
      pintar(resultado, tabla({
        vacio: 'No se encontraron productos.',
        filas: productos,
        columnas: [
          { titulo: 'Producto', valor: (p) => [p.nombre, p.stockBajo && h('span', { class: 'etiqueta-bajo' }, 'Stock bajo')] },
          { titulo: 'Descripción', valor: (p) => p.descripcion ?? '' },
          { titulo: 'Precio', numerica: true, valor: (p) => moneda(p.precio) },
          { titulo: 'Stock', numerica: true, valor: (p) => numero(p.stock) },
          esAdmin && { titulo: '', valor: (p) => h('div', { class: 'acciones' },
            h('button', { class: 'enlace', onclick: () => editar(p) }, 'Editar'),
            h('button', { class: 'enlace peligro', onclick: () => eliminar(p) }, 'Eliminar')) },
        ].filter(Boolean),
      }));
    } catch (error) {
      pintar(resultado, h('p', { class: 'error' }, error.message));
    }
  }

  async function crear() {
    const guardado = await formulario({
      titulo: 'Nuevo producto', campos: CAMPOS,
      guardar: (datos) => api.post('/api/productos', aPeticion(datos)),
    });
    if (guardado) { aviso('Producto creado.'); cargar(); }
  }

  async function editar(producto) {
    const guardado = await formulario({
      titulo: 'Editar producto', campos: CAMPOS, valores: producto,
      guardar: (datos) => api.put(`/api/productos/${producto.id}`, aPeticion(datos)),
    });
    if (guardado) { aviso('Producto actualizado.'); cargar(); }
  }

  async function eliminar(producto) {
    if (!await confirmar('Eliminar producto', `¿Eliminar "${producto.nombre}"? Esta acción no se puede deshacer.`, 'Eliminar')) return;
    try {
      await api.delete(`/api/productos/${producto.id}`);
      aviso('Producto eliminado.');
      cargar();
    } catch (error) {
      aviso(error.message, 'error');
    }
  }

  buscador.addEventListener('input', esperar(cargar));
  pintar(contenedor,
    h('div', { class: 'barra' }, h('h1', null, 'Productos'),
      h('div', { class: 'filtros' }, buscador, esAdmin && h('button', { class: 'principal', onclick: crear }, 'Nuevo producto'))),
    resultado);
  await cargar();
}
