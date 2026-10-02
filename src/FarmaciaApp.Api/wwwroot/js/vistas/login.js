/**
 * @file login.js
 * @brief Pantalla de inicio de sesión.
 * @author Santiago Caicedo
 */
import { api, sesion } from '../api.js';
import { h, pintar } from '../ui.js';

/** Muestra el formulario; al autenticarse guarda la sesión y llama a alEntrar. */
export function vistaLogin(raiz, alEntrar) {
  const login = h('input', { name: 'login', autocomplete: 'username', required: true, autofocus: true });
  const clave = h('input', { name: 'clave', type: 'password', autocomplete: 'current-password', required: true });
  const error = h('p', { class: 'error', role: 'alert' });
  const boton = h('button', { class: 'principal', type: 'submit' }, 'Iniciar sesión');

  const formulario = h('form', { class: 'tarjeta' },
    h('div', { class: 'marca' }, h('img', { src: 'favicon.svg', alt: '' }), 'FarmaciaApp'),
    h('label', null, 'Usuario', login),
    h('label', null, 'Contraseña', clave),
    error, boton,
    h('p', { class: 'ayuda' }, 'API REST con JWT · ', h('a', { href: '/scalar' }, 'Documentación')));

  formulario.addEventListener('submit', async (evento) => {
    evento.preventDefault();
    error.textContent = '';
    boton.disabled = true;
    try {
      const respuesta = await api.post('/api/auth/login', { login: login.value, clave: clave.value });
      sesion.guardar(respuesta);
      alEntrar(respuesta.usuario);
    } catch (fallo) {
      error.textContent = fallo.message;
      clave.value = '';
      boton.disabled = false;
    }
  });

  pintar(raiz, h('div', { class: 'pantalla-login' }, formulario));
}
