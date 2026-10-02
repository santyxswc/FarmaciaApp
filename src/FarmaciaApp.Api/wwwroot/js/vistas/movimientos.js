/**
 * @file movimientos.js
 * @brief Registro de movimientos de los usuarios (administrador).
 * @author Santiago Caicedo
 */
import { api } from '../api.js';
import { aIso, fechaHora, h, haceDias, pintar, tabla } from '../ui.js';

export async function vistaMovimientos(contenedor) {
  const desde = h('input', { type: 'date', value: aIso(haceDias(6)), 'aria-label': 'Desde' });
  const hasta = h('input', { type: 'date', value: aIso(new Date()), 'aria-label': 'Hasta' });
  const texto = h('input', { type: 'search', placeholder: 'Acción o detalle', 'aria-label': 'Buscar movimiento' });
  const resultado = h('div', { class: 'tarjeta' });

  async function cargar() {
    try {
      const consulta = new URLSearchParams({ desde: desde.value, hasta: hasta.value });
      if (texto.value.trim()) consulta.set('buscar', texto.value.trim());
      const movimientos = await api.get(`/api/movimientos?${consulta}`);
      pintar(resultado, tabla({
        vacio: 'No hay movimientos en el periodo.',
        filas: movimientos,
        columnas: [
          { titulo: 'Fecha', valor: (m) => fechaHora(m.fecha) },
          { titulo: 'Usuario', valor: (m) => m.usuario },
          { titulo: 'Rol', valor: (m) => m.rol },
          { titulo: 'Acción', valor: (m) => m.accion },
          { titulo: 'Detalle', valor: (m) => m.detalle ?? '' },
        ],
      }), h('p', { class: 'suave' }, 'Se muestran hasta 500 movimientos.'));
    } catch (error) {
      pintar(resultado, h('p', { class: 'error' }, error.message));
    }
  }

  pintar(contenedor,
    h('div', { class: 'barra' }, h('h1', null, 'Movimientos'),
      h('form', { class: 'filtros', onsubmit: (e) => { e.preventDefault(); cargar(); } },
        h('label', null, 'Desde', desde), h('label', null, 'Hasta', hasta), texto,
        h('button', { class: 'principal', type: 'submit' }, 'Buscar'))),
    resultado);
  await cargar();
}
