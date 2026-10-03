/**
 * @file app.js
 * @brief Punto de entrada: sesión, menú y navegación por hash.
 * @author Santiago Caicedo
 */
import { sesion } from './api.js';
import { aviso, h, pintar } from './ui.js';
import { vistaClientes } from './vistas/clientes.js';
import { vistaFacturas } from './vistas/facturas.js';
import { vistaLogin } from './vistas/login.js';
import { vistaMovimientos } from './vistas/movimientos.js';
import { vistaPersonas } from './vistas/personas.js';
import { vistaProductos } from './vistas/productos.js';
import { vistaPromociones } from './vistas/promociones.js';
import { vistaReclamos } from './vistas/reclamos.js';
import { vistaReportes } from './vistas/reportes.js';
import { cambiarMiClave, vistaUsuarios } from './vistas/usuarios.js';
import { vistaVenta } from './vistas/venta.js';

const RUTAS = [
  { ruta: 'productos', titulo: 'Productos', vista: vistaProductos },
  { ruta: 'venta', titulo: 'Nueva venta', vista: vistaVenta },
  { ruta: 'facturas', titulo: 'Facturas', vista: vistaFacturas },
  { ruta: 'clientes', titulo: 'Clientes', vista: vistaClientes },
  { ruta: 'reclamos', titulo: 'Reclamos', vista: vistaReclamos },
  { ruta: 'promociones', titulo: 'Promociones', vista: vistaPromociones },
  { ruta: 'personas', titulo: 'Personas', vista: vistaPersonas },
  { ruta: 'reportes', titulo: 'Reportes', vista: vistaReportes, admin: true },
  { ruta: 'movimientos', titulo: 'Movimientos', vista: vistaMovimientos, admin: true },
  { ruta: 'usuarios', titulo: 'Usuarios', vista: vistaUsuarios, admin: true },
];

const raiz = document.getElementById('app');
let usuario = null;

function rutaActual() {
  const nombre = location.hash.replace(/^#\/?/, '');
  const disponibles = RUTAS.filter((r) => !r.admin || usuario.rol === 'Administrador');
  return disponibles.find((r) => r.ruta === nombre) ?? disponibles[0];
}

async function mostrarVista() {
  if (!usuario) return;
  const actual = rutaActual();
  document.querySelectorAll('.menu a').forEach((enlace) => {
    if (enlace.dataset.ruta === actual.ruta) enlace.setAttribute('aria-current', 'page');
    else enlace.removeAttribute('aria-current');
  });
  const principal = document.querySelector('main');
  await actual.vista(principal, { usuario, esAdmin: usuario.rol === 'Administrador' });
  document.title = `${actual.titulo} · FarmaciaApp`;
}

function mostrarLogin() {
  usuario = null;
  document.title = 'FarmaciaApp';
  vistaLogin(raiz, mostrarAplicacion);
}

function mostrarAplicacion(datos) {
  usuario = datos;
  const esAdmin = usuario.rol === 'Administrador';
  pintar(raiz,
    h('header', { class: 'encabezado' },
      h('div', { class: 'marca' }, h('img', { src: 'favicon.svg', alt: '' }), 'FarmaciaApp'),
      h('nav', { class: 'menu', 'aria-label': 'Secciones' },
        RUTAS.filter((r) => !r.admin || esAdmin).map((r) => h('a', { href: `#/${r.ruta}`, 'data-ruta': r.ruta }, r.titulo))),
      h('div', { class: 'sesion' },
        h('span', null, `${usuario.nombre} · ${usuario.rol}`),
        h('a', { href: '/scalar' }, 'API'),
        h('button', { onclick: cambiarMiClave }, 'Cambiar contraseña'),
        h('button', { onclick: cerrarSesion }, 'Cerrar sesión'))),
    h('main'));
  if (!location.hash) location.hash = '#/productos';
  mostrarVista();
}

function cerrarSesion() {
  sesion.borrar();
  location.hash = '';
  mostrarLogin();
}

window.addEventListener('hashchange', mostrarVista);
window.addEventListener('sesion-vencida', () => {
  aviso('Tu sesión venció. Inicia sesión de nuevo.', 'error');
  mostrarLogin();
});

const guardada = sesion.obtener();
if (guardada) mostrarAplicacion(guardada.usuario);
else mostrarLogin();
