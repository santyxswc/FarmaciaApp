/**
 * @file venta.js
 * @brief Registro de una venta con varios productos.
 * @author Santiago Caicedo
 */
import { api } from '../api.js';
import { aviso, h, moneda, numero, pintar, tabla } from '../ui.js';
import { mostrarFactura } from './facturas.js';

const IVA = 1.19;

export async function vistaVenta(contenedor, { esAdmin }) {
  let datos;
  try {
    const [clientes, vendedores, productos, metodos] = await Promise.all([
      api.get('/api/ventas/clientes'),
      esAdmin ? api.get('/api/ventas/vendedores') : [],
      api.get('/api/ventas/productos'),
      api.get('/api/ventas/metodos-pago'),
    ]);
    datos = { clientes, vendedores, productos, metodos };
  } catch (error) {
    pintar(contenedor, h('h1', null, 'Nueva venta'), h('p', { class: 'error' }, error.message));
    return;
  }

  const lineas = new Map();
  const opciones = (lista) => lista.map((o) => h('option', { value: o.id }, o.nombre));

  const cliente = h('select', { required: true, 'aria-label': 'Cliente' }, h('option', { value: '' }, 'Selecciona un cliente'), opciones(datos.clientes));
  const vendedor = esAdmin
    ? h('select', { required: true, 'aria-label': 'Vendedor' }, h('option', { value: '' }, 'Selecciona un vendedor'), opciones(datos.vendedores))
    : null;
  const pago = h('select', { 'aria-label': 'Método de pago' }, datos.metodos.map((m) => h('option', { value: m }, m)));
  const producto = h('select', { 'aria-label': 'Producto' });
  const cantidad = h('input', { type: 'number', min: 1, step: 1, value: 1, 'aria-label': 'Cantidad' });
  const detalle = h('div');
  const registrar = h('button', { class: 'principal', disabled: true }, 'Registrar venta');

  function llenarProductos() {
    pintar(producto, datos.productos.map((p) =>
      h('option', { value: p.id, disabled: p.stock <= 0 },
        `${p.nombre} · ${moneda(p.precioFinal)}${p.descuento > 0 ? ` (-${numero(p.descuento)} %)` : ''} · stock ${p.stock}`)));
  }

  function agregar() {
    const elegido = datos.productos.find((p) => p.id === Number(producto.value));
    const unidades = Number(cantidad.value);
    if (!elegido || !Number.isInteger(unidades) || unidades <= 0) {
      aviso('La cantidad debe ser un número entero mayor a cero.', 'error');
      return;
    }
    const total = (lineas.get(elegido.id)?.cantidad ?? 0) + unidades;
    if (total > elegido.stock) {
      aviso(`Solo hay ${elegido.stock} unidades de ${elegido.nombre}.`, 'error');
      return;
    }
    lineas.set(elegido.id, { producto: elegido, cantidad: total });
    cantidad.value = 1;
    pintarDetalle();
  }

  function pintarDetalle() {
    const items = [...lineas.values()];
    const total = items.reduce((suma, l) => suma + l.producto.precioFinal * l.cantidad, 0);
    registrar.disabled = items.length === 0;
    pintar(detalle,
      tabla({
        vacio: 'Agrega al menos un producto.',
        filas: items,
        columnas: [
          { titulo: 'Producto', valor: (l) => l.producto.nombre },
          { titulo: 'Cant.', numerica: true, valor: (l) => numero(l.cantidad) },
          { titulo: 'Precio', numerica: true, valor: (l) => moneda(l.producto.precioFinal) },
          { titulo: 'Subtotal', numerica: true, valor: (l) => moneda(l.producto.precioFinal * l.cantidad) },
          { titulo: '', valor: (l) => h('button', { class: 'enlace peligro', onclick: () => { lineas.delete(l.producto.id); pintarDetalle(); } }, 'Quitar') },
        ],
      }),
      h('div', { class: 'totales' },
        h('span', { class: 'suave' }, 'Precios con IVA incluido (19 %)'),
        h('span', null, `Subtotal ${moneda(total / IVA)}`),
        h('span', null, `IVA ${moneda(total - total / IVA)}`),
        h('span', { class: 'total' }, `Total ${moneda(total)}`)));
  }

  async function enviar(evento) {
    evento.preventDefault();
    registrar.disabled = true;
    try {
      const factura = await api.post('/api/ventas', {
        clienteId: Number(cliente.value),
        vendedorId: vendedor ? Number(vendedor.value) : null,
        metodoPago: pago.value,
        items: [...lineas.values()].map((l) => ({ productoId: l.producto.id, cantidad: l.cantidad })),
      });
      aviso(`Venta registrada: factura N° ${factura.numero}.`);
      lineas.clear();
      datos.productos = await api.get('/api/ventas/productos');
      llenarProductos();
      pintarDetalle();
      mostrarFactura(factura.numero);
    } catch (error) {
      aviso(error.message, 'error');
      registrar.disabled = lineas.size === 0;
    }
  }

  llenarProductos();
  pintarDetalle();
  pintar(contenedor,
    h('h1', null, 'Nueva venta'),
    h('form', { class: 'tarjeta', style: { display: 'grid', gap: '1rem' }, onsubmit: enviar },
      h('div', { class: 'cuadricula' },
        h('label', null, 'Cliente', cliente),
        vendedor && h('label', null, 'Vendedor', vendedor),
        !esAdmin && h('p', { class: 'suave' }, 'La venta saldrá a tu nombre.'),
        h('label', null, 'Método de pago', pago)),
      h('div', { class: 'filtros', style: { display: 'flex', flexWrap: 'wrap', gap: '.75rem', alignItems: 'end' } },
        h('label', { style: { flex: '1 1 280px' } }, 'Producto', producto),
        h('label', null, 'Cantidad', cantidad),
        h('button', { type: 'button', onclick: agregar }, 'Agregar')),
      detalle,
      h('div', { class: 'acciones' }, registrar)));
}
