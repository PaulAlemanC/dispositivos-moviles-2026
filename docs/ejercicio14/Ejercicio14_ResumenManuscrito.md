# Ejercicio 14: Resumen para escribir a mano

Guía para redactar con tus propias palabras. No hace falta copiarla. Con una o dos hojas basta.

## useCallback

- Guarda una **función** para que no se cree distinta en cada render.
- Se vuelve a crear solo si cambian sus dependencias.
- Sirve cuando la función se pasa a un componente con `memo` o es dependencia de otro hook.
- No hace la función más rápida. Si nadie compara la referencia, no se necesita.
- **Caso médico:** `abrirExpediente(id)` en una lista de pacientes con `FlatList`. Sin `useCallback`, cada fila se re-renderiza al escribir en el buscador.

## useMemo

- Guarda el **resultado** de un cálculo.
- Se recalcula solo si cambian las dependencias.
- Diferencia clave: `useCallback` guarda la función; `useMemo` guarda lo que devuelve.
- No usarlo para cálculos sencillos.
- **Caso médico:** filtrar y ordenar las citas del día de un médico con muchas citas, solo cuando cambian `citas` o la fecha.

## createContext

- Crea un "canal" para compartir datos sin pasar props por cada nivel.
- Tres partes: `createContext`, `Provider` con `value`, y `useContext` para leerlo.
- Cuando cambia el `value`, se re-renderizan los componentes que lo consumen.
- Sirve para datos globales: sesión, tema, idioma. No para todo el estado de la app.
- **Caso médico:** que cualquier pantalla sepa si el usuario es paciente o médico sin pasar el usuario pantalla por pantalla.

## Cleanup function

- Es la función que regresa un `useEffect`.
- Se ejecuta antes de repetir el efecto y al desmontar el componente.
- Sirve para quitar listeners, cancelar timers y abortar peticiones.
- Sin ella: fugas de memoria y errores al actualizar componentes que ya no existen.
- **Caso médico:** escuchar `AppState` para recargar las citas al volver a la app, y quitar el listener al salir de la pantalla.

## AuthContext y su funcionamiento

- Es un contexto que guarda la sesión: usuario, token, estado y funciones para entrar y salir.
- `AuthProvider` envuelve la app y da el valor; `useAuth` lo consume.
- `iniciarSesion` llama a `POST /api/auth/login`; si responde bien, guarda token y usuario.
- La contraseña nunca se guarda en el contexto.
- Usa los cuatro conceptos:
  - `createContext`: crea el contexto.
  - `useCallback`: mantiene estables `iniciarSesion` y `cerrarSesion`.
  - `useMemo`: el `value` solo cambia cuando cambia la sesión.
  - Cleanup: cancela el timer de expiración, la restauración y el login pendiente.
- **Caso médico:** el paciente inicia sesión, ve su nombre en todas las pantallas, sus peticiones llevan el token, y al vencer el JWT (60 min) vuelve al login.

## Qué decir sobre el alcance

- El código es un **ejemplo demostrativo**; todavía no hay app React Native en el repositorio.
- El backend de login sí existe (Ejercicio 13).
- Para un token real falta usar almacenamiento seguro (por ejemplo `expo-secure-store`), no `AsyncStorage`.
