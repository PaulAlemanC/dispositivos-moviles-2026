# Ejercicio 14: Conceptos avanzados de React y AuthContext

Materia: Dispositivos Móviles. Proyecto: Sistema Centralizado de Atención Médica Universitaria.

## 1. Introducción

Este ejercicio investiga cuatro herramientas de React que también se usan en React Native: `useCallback`, `useMemo`, `createContext` y la función de limpieza (cleanup) de `useEffect`. Después se muestra un ejemplo de `AuthContext` que usa las cuatro.

Estado actual del repositorio, para que quede claro qué es real y qué es demostrativo:

- El repositorio solo contiene el backend ASP.NET Core 8 (`src/`) y sus pruebas (`tests/`). **No existe todavía una aplicación React Native**, así que no hay versión de React, dependencias ni navegación que respetar.
- El backend sí tiene autenticación (Ejercicio 13): `POST /api/auth/register` y `POST /api/auth/login`. El login recibe `correo` y `password` y regresa `token`, `tokenType`, `expiresIn` (segundos) y `usuario` (`idUsuario`, `nombre`, `correo`, `rol`). Los endpoints de pacientes y médicos exigen el token Bearer.
- Por eso el ejemplo vive en `docs/ejercicio14/ejemplo/` como código **demostrativo y autocontenido**. No está integrado a ninguna app ni se modificó el backend.

## 2. Investigación de los cuatro conceptos

### 2.1 useCallback

**Qué es.** Un hook que guarda una función entre renders y devuelve la misma referencia mientras sus dependencias no cambien.

**Para qué sirve.** En JavaScript, una función declarada dentro de un componente se crea de nuevo en cada render, así que `() => {}` nunca es igual a la anterior. Si esa función se pasa como prop a un componente envuelto en `memo`, o es dependencia de otro hook, esa referencia nueva provoca trabajo extra. `useCallback` mantiene la referencia estable.

**Cómo funciona.** `useCallback(fn, dependencias)`. React guarda `fn` y solo la reemplaza cuando alguna dependencia cambia (compara con `Object.is`).

**Cuándo conviene.** Cuando la función va a un componente con `memo`, cuando es dependencia de `useEffect`/`useMemo`, o cuando es parte del valor de un contexto.

**Ejemplo en React Native:**

```tsx
const abrirExpediente = useCallback((id: number) => {
  navigation.navigate('Expediente', { id });
}, [navigation]);

return <FlatList data={pacientes} renderItem={({ item }) => (
  <FilaPaciente paciente={item} onPress={abrirExpediente} />  // FilaPaciente usa memo
)} />;
```

**Errores comunes.**

- Envolver todas las funciones por costumbre. Si nada depende de la referencia, no aporta nada y solo agrega ruido.
- Olvidar dependencias y dejar un valor viejo atrapado dentro de la función.
- Esperar que haga más rápida la función: no lo hace, solo evita que cambie su identidad.

### 2.2 useMemo

**Qué es.** Un hook que guarda el **resultado** de un cálculo entre renders.

**Para qué sirve.** Evita repetir un cálculo costoso y mantiene estable la referencia de un objeto o arreglo que se calcula en el render.

**Cómo funciona.** `useMemo(() => calculo, dependencias)`. React ejecuta la función en el primer render y reutiliza el valor hasta que cambie una dependencia.

**Diferencia con useCallback.** `useCallback(fn, deps)` guarda la función misma; `useMemo(() => fn, deps)` guarda lo que la función devuelve. De hecho, `useCallback(fn, deps)` equivale a `useMemo(() => fn, deps)`. Una guarda "una acción", la otra guarda "un dato".

**Cuándo conviene.** Cálculos pesados (filtrar y ordenar listas grandes), objetos que se pasan a componentes con `memo`, y el `value` de un Provider.

**Ejemplo en React Native:**

```tsx
const citasDelDia = useMemo(
  () => citas.filter(c => c.fecha === hoy).sort((a, b) => a.hora.localeCompare(b.hora)),
  [citas, hoy],
);
```

**Errores comunes.**

- Usarlo con cálculos triviales (sumar dos números). El propio hook cuesta.
- Meter efectos secundarios dentro del cálculo. Debe ser una función pura.
- Tratarlo como garantía: React puede descartar el valor guardado en ciertos casos, por lo que la app debe funcionar bien aunque se recalcule.

### 2.3 createContext

**Qué es.** Una función que crea un contexto, un canal para compartir un valor con muchos componentes sin pasarlo por props en cada nivel.

**Para qué sirve.** Evita el "prop drilling". Datos como el usuario autenticado, el tema o el idioma los necesitan componentes en muchos lugares del árbol.

**Cómo funciona.** Tiene tres partes:

1. `createContext(valorPorDefecto)` crea el contexto.
2. `<Contexto.Provider value={...}>` envuelve una parte del árbol y define el valor.
3. `useContext(Contexto)` lee el valor desde cualquier descendiente.

Esto permite compartir información porque React busca hacia arriba en el árbol el Provider más cercano y entrega su valor. Cuando el `value` cambia (por referencia), todos los componentes que usan `useContext` de ese contexto se vuelven a renderizar. Por eso conviene que el `value` sea estable (`useMemo`).

**Cuándo conviene.** Para datos globales o semiglobales que cambian poco: sesión, tema, idioma.

**Ejemplo en React Native:**

```tsx
const TemaContext = createContext<'claro' | 'oscuro'>('claro');

function App() {
  return (
    <TemaContext.Provider value="oscuro">
      <Agenda />
    </TemaContext.Provider>
  );
}

function BotonCita() {
  const tema = useContext(TemaContext); // sin recibir props
  return <Button color={tema === 'oscuro' ? '#fff' : '#000'} title="Agendar" />;
}
```

**Errores comunes.**

- Poner todo el estado de la app en un solo contexto: cualquier cambio vuelve a renderizar a todos los consumidores.
- Pasar un objeto nuevo en `value` en cada render sin memorizarlo.
- Usar el valor por defecto como si fuera el real, olvidando poner el Provider.
- Usarlo para estado que solo necesitan dos componentes cercanos, donde bastan las props.

### 2.4 Función de limpieza (cleanup) en hooks

**Qué es.** La función que puede devolver el callback de `useEffect`.

**Para qué sirve.** Deshacer lo que el efecto empezó: cancelar timers, quitar listeners, cerrar conexiones, cancelar peticiones. Sin ella hay fugas de memoria y actualizaciones de estado en componentes ya desmontados.

**Cómo funciona.** React ejecuta la limpieza en dos momentos: antes de volver a correr el efecto cuando cambian sus dependencias, y cuando el componente se desmonta. Primero se limpia el efecto anterior y luego se ejecuta el nuevo.

**Cuándo conviene.** Siempre que el efecto cree algo que siga vivo después: `setTimeout`/`setInterval`, `addEventListener`, suscripciones, `fetch` con `AbortController`.

**Ejemplo en React Native:**

```tsx
useEffect(() => {
  const sub = AppState.addEventListener('change', estado => {
    if (estado === 'active') recargarCitas();
  });
  return () => sub.remove(); // limpieza
}, [recargarCitas]);
```

**Errores comunes.**

- Crear un listener o intervalo y no limpiarlo.
- Limpiar algo distinto a lo que se creó (otra referencia de función).
- Pensar que la limpieza solo corre al desmontar: también corre entre ejecuciones del efecto.
- En desarrollo con `StrictMode`, React monta, desmonta y vuelve a montar a propósito para detectar limpiezas faltantes. Si algo se duplica, falta la limpieza.

## 3. Explicación de AuthContext

`AuthContext` centraliza el estado de la sesión para que cualquier pantalla de la app pueda saber quién está autenticado sin recibirlo por props.

Cómo se usa cada concepto en el ejemplo:

| Concepto | Dónde se usa | Razón |
|---|---|---|
| `createContext` | `AuthContext` con valor por defecto `null` | Compartir sesión y acciones con toda la app |
| `useCallback` | `iniciarSesion`, `cerrarSesion` | Referencia estable; evitan que el `value` cambie en cada render |
| `useMemo` | `valor` del Provider | El objeto solo cambia si cambia la sesión, el error o las funciones |
| Cleanup | Restauración de sesión, temporizador de expiración y abort del login | Evitar fugas y estados sobre componentes desmontados |

Decisiones de diseño:

- **Contraseña.** Solo existe en el estado local del formulario y en el cuerpo de la petición. El contexto nunca la guarda y el formulario la borra tras iniciar sesión.
- **Backend.** `iniciarSesion` llama al `POST /api/auth/login` que ya existe, con la URL recibida por prop (`apiUrl`). No hay respuestas inventadas.
- **Persistencia.** Se define la interfaz `AlmacenSesion` y es opcional. No se agregó ninguna librería porque el proyecto no tiene app. Si no se pasa almacén, la sesión vive en memoria y se pierde al cerrar la app.
- **Limitación.** `AsyncStorage` guarda en texto plano y no es adecuado para un token. En una app real el almacén debería usar `expo-secure-store` o `react-native-keychain`. Ninguno está instalado ni probado aquí.
- **Expiración.** El backend no ofrece refresh token. Cuando el JWT vence (`expiresIn`, 60 minutos por defecto), la sesión se cierra localmente.
- **Roles.** Se expone `usuario.rol`, pero la autorización real siempre la decide el backend.

## 4. Código del ejemplo

Los archivos están en `docs/ejercicio14/ejemplo/`:

- [`AuthContext.tsx`](ejemplo/AuthContext.tsx): contexto, `AuthProvider` y `useAuth`.
- [`LoginDemo.tsx`](ejemplo/LoginDemo.tsx): pantalla sencilla que inicia sesión, muestra el usuario y cierra sesión.

Fragmento central (el archivo completo está en el enlace):

```tsx
const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ apiUrl, almacen, children }: AuthProviderProps) {
  const [sesion, setSesion] = useState<Sesion | null>(null);
  // ...

  useEffect(() => {
    if (!sesion) return;
    const restante = sesion.expiraEn - Date.now();
    const id = setTimeout(() => { setSesion(null); void almacen?.borrar(); }, Math.max(restante, 0));
    return () => clearTimeout(id);                       // cleanup
  }, [sesion, almacen]);

  const iniciarSesion = useCallback(async (correo, password) => { /* fetch al login */ }, [apiUrl, almacen]);
  const cerrarSesion = useCallback(async () => { /* limpia sesión */ }, [almacen]);

  const valor = useMemo(() => ({ /* estado, usuario, token, error, funciones */ }),
    [cargando, sesion, error, iniciarSesion, cerrarSesion]);

  return <AuthContext.Provider value={valor}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const contexto = useContext(AuthContext);
  if (contexto === null) throw new Error('useAuth debe usarse dentro de un <AuthProvider>.');
  return contexto;
}
```

## 5. Explicación del funcionamiento

1. **Arranque.** Si se pasa un `almacen`, el estado inicia en `cargando` y un `useEffect` lee la sesión guardada. Si no ha expirado, se restaura; si expiró, se borra. La variable `cancelado` y su cleanup evitan actualizar estado si el Provider se desmonta antes de que termine la lectura.
2. **Iniciar sesión.** `iniciarSesion` cancela un login previo, hace `fetch` al login con un `AbortController`, y según la respuesta: `401` guarda un error de credenciales, otro fallo guarda un mensaje genérico, y un `200` crea la sesión con `expiraEn = Date.now() + expiresIn * 1000`.
3. **Expiración.** Cuando hay sesión, un efecto programa un `setTimeout` que la cierra al vencer el token. Su cleanup lo cancela si la sesión cambia (por ejemplo, cierra sesión manualmente) o si se desmonta el Provider. Sin esa limpieza el temporizador seguiría vivo.
4. **Cerrar sesión.** `cerrarSesion` aborta peticiones pendientes, limpia el estado y borra el almacén.
5. **Consumo.** `useAuth` regresa el valor del contexto y lanza un error claro si se usa fuera del Provider.
6. **Render eficiente.** Como `iniciarSesion` y `cerrarSesion` son estables (`useCallback`) y el `value` se memoriza (`useMemo`), los consumidores solo se vuelven a renderizar cuando cambia algo relevante de la sesión.

Límites: el ejemplo no usa refresh token, no cifra el token (depende del almacén que se inyecte) y no maneja registro (`/api/auth/register`).

## 6. Casos de uso en el sistema médico universitario

- **Pantalla de inicio de sesión.** Paciente, médico o personal acceden con su correo institucional contra `POST /api/auth/login`.
- **Encabezado de la app.** Cualquier pantalla muestra el nombre del usuario con `useAuth()`, sin props.
- **Peticiones protegidas.** Las pantallas de pacientes y médicos toman `token` del contexto para enviar `Authorization: Bearer <token>`. El backend ya exige ese encabezado.
- **Navegación condicionada.** Mostrar el flujo de login si el estado es `anonimo` y el flujo principal si es `autenticado`.
- **Sesión vencida.** Si el JWT expira, la app regresa al login sin quedar con una sesión falsa.
- **Datos sensibles.** Cerrar sesión borra el token del dispositivo, importante porque se manejan datos de salud.

## 7. Conclusión

`useCallback` y `useMemo` son herramientas de optimización con propósitos distintos: una conserva funciones y la otra conserva resultados. `createContext` resuelve compartir datos globales como la sesión, y la cleanup function evita fugas con timers, listeners y peticiones. Combinados en un `AuthContext`, dan una forma ordenada de manejar la autenticación. Aun así, el código es solo un ejemplo: para usarlo hace falta crear la app React Native, elegir un almacenamiento seguro y probarlo contra el backend.

## 8. Fuentes consultadas

- React, `useCallback`: https://react.dev/reference/react/useCallback
- React, `useMemo`: https://react.dev/reference/react/useMemo
- React, `createContext`: https://react.dev/reference/react/createContext
- React, `useContext`: https://react.dev/reference/react/useContext
- React, `useEffect` (incluye cleanup): https://react.dev/reference/react/useEffect
- React, Sincronizar con efectos: https://react.dev/learn/synchronizing-with-effects
- React, Pasar datos con contexto: https://react.dev/learn/passing-data-deeply-with-context
- React Native, `AppState`: https://reactnative.dev/docs/appstate
- React Native, Seguridad (almacenamiento de datos sensibles): https://reactnative.dev/docs/security
- Expo, SecureStore: https://docs.expo.dev/versions/latest/sdk/securestore/
- MDN, `AbortController`: https://developer.mozilla.org/en-US/docs/Web/API/AbortController

> Nota: los enlaces corresponden a páginas conocidas de la documentación oficial, pero no se abrieron durante la elaboración de este documento. Conviene revisarlos antes de entregar.
