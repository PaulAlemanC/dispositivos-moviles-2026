/**
 * Ejercicio 14 - Ejemplo demostrativo de AuthContext para React Native.
 *
 * Este archivo NO está integrado a una app: el repositorio todavía no tiene
 * proyecto React Native. Está pensado para copiarse a uno cuando exista.
 *
 * Contrato del backend (ya existe, ver AuthDtos.cs):
 *   POST /api/auth/login  { correo, password }
 *   -> { token, tokenType, expiresIn (segundos), usuario: { idUsuario, nombre, correo, rol } }
 */
import React, {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from 'react';

export interface Usuario {
  idUsuario: number;
  nombre: string;
  correo: string;
  rol: string;
}

/** Lo que se guarda de la sesión. Nunca incluye la contraseña. */
export interface Sesion {
  token: string;
  /** Momento de expiración en milisegundos (Date.now() + expiresIn * 1000). */
  expiraEn: number;
  usuario: Usuario;
}

/**
 * Almacenamiento de sesión inyectable. Se deja como interfaz para no añadir
 * dependencias. En una app real se implementaría con expo-secure-store o
 * react-native-keychain, porque el token es un dato sensible.
 */
export interface AlmacenSesion {
  cargar(): Promise<Sesion | null>;
  guardar(sesion: Sesion): Promise<void>;
  borrar(): Promise<void>;
}

type Estado = 'cargando' | 'autenticado' | 'anonimo';

export interface AuthContextValue {
  estado: Estado;
  usuario: Usuario | null;
  token: string | null;
  error: string | null;
  iniciarSesion: (correo: string, password: string) => Promise<boolean>;
  cerrarSesion: () => Promise<void>;
}

// Concepto 1: createContext. El valor por defecto es null para obligar a usar el Provider.
const AuthContext = createContext<AuthContextValue | null>(null);

interface AuthProviderProps {
  /** Ejemplo: 'http://10.0.2.2:5080' en el emulador de Android. */
  apiUrl: string;
  /** Opcional. Sin almacén, la sesión vive solo en memoria. */
  almacen?: AlmacenSesion;
  children: ReactNode;
}

export function AuthProvider({ apiUrl, almacen, children }: AuthProviderProps) {
  const [sesion, setSesion] = useState<Sesion | null>(null);
  const [cargando, setCargando] = useState(Boolean(almacen));
  const [error, setError] = useState<string | null>(null);
  const loginAbort = useRef<AbortController | null>(null);

  // Concepto 4a: cleanup para restaurar la sesión guardada.
  // Si el componente se desmonta antes de que termine la promesa, se ignora el resultado.
  useEffect(() => {
    if (!almacen) return;
    let cancelado = false;

    almacen
      .cargar()
      .then((guardada) => {
        if (cancelado) return;
        if (guardada && guardada.expiraEn > Date.now()) {
          setSesion(guardada);
        } else if (guardada) {
          void almacen.borrar();
        }
      })
      .catch(() => {
        /* Si falla la lectura, se continúa como anónimo. */
      })
      .finally(() => {
        if (!cancelado) setCargando(false);
      });

    return () => {
      cancelado = true;
    };
  }, [almacen]);

  // Concepto 4b: cleanup del temporizador. Cierra la sesión cuando vence el token
  // y limpia el timeout si la sesión cambia o el componente se desmonta.
  useEffect(() => {
    if (!sesion) return;
    const restante = sesion.expiraEn - Date.now();
    const id = setTimeout(() => {
      setSesion(null);
      void almacen?.borrar();
    }, Math.max(restante, 0));

    return () => clearTimeout(id);
  }, [sesion, almacen]);

  // Cancela un login en curso si el Provider se desmonta.
  useEffect(() => () => loginAbort.current?.abort(), []);

  // Concepto 2: useCallback. Las funciones conservan su identidad entre renders
  // mientras apiUrl y almacen no cambien, así el useMemo de abajo puede reutilizar el valor.
  const iniciarSesion = useCallback(
    async (correo: string, password: string): Promise<boolean> => {
      loginAbort.current?.abort();
      const controlador = new AbortController();
      loginAbort.current = controlador;
      setError(null);

      try {
        const respuesta = await fetch(`${apiUrl}/api/auth/login`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ correo, password }),
          signal: controlador.signal,
        });

        if (respuesta.status === 401) {
          setError('Correo o contraseña incorrectos.');
          return false;
        }
        if (!respuesta.ok) {
          setError('No se pudo iniciar sesión. Intenta más tarde.');
          return false;
        }

        const datos = (await respuesta.json()) as {
          token: string;
          expiresIn: number;
          usuario: Usuario;
        };
        // La contraseña no se guarda en ningún lado; solo se usó en la petición.
        const nueva: Sesion = {
          token: datos.token,
          expiraEn: Date.now() + datos.expiresIn * 1000,
          usuario: datos.usuario,
        };
        setSesion(nueva);
        await almacen?.guardar(nueva);
        return true;
      } catch (e) {
        if ((e as Error).name !== 'AbortError') {
          setError('Error de red. Revisa tu conexión.');
        }
        return false;
      }
    },
    [apiUrl, almacen],
  );

  const cerrarSesion = useCallback(async () => {
    loginAbort.current?.abort();
    setSesion(null);
    setError(null);
    await almacen?.borrar();
  }, [almacen]);

  // Concepto 3: useMemo. Sin esto, el objeto cambiaría en cada render del Provider
  // y todos los consumidores se volverían a renderizar aunque nada haya cambiado.
  const valor = useMemo<AuthContextValue>(
    () => ({
      estado: cargando ? 'cargando' : sesion ? 'autenticado' : 'anonimo',
      usuario: sesion?.usuario ?? null,
      token: sesion?.token ?? null,
      error,
      iniciarSesion,
      cerrarSesion,
    }),
    [cargando, sesion, error, iniciarSesion, cerrarSesion],
  );

  return <AuthContext.Provider value={valor}>{children}</AuthContext.Provider>;
}

/** Hook personalizado para consumir el contexto con una validación clara. */
export function useAuth(): AuthContextValue {
  const contexto = useContext(AuthContext);
  if (contexto === null) {
    throw new Error('useAuth debe usarse dentro de un <AuthProvider>.');
  }
  return contexto;
}
