/**
 * Ejercicio 14 - Componente demostrativo que consume useAuth.
 * Uso: <AuthProvider apiUrl="http://10.0.2.2:5080"><LoginDemo /></AuthProvider>
 */
import React, { useState } from 'react';
import { ActivityIndicator, Button, Text, TextInput, View } from 'react-native';
import { AuthProvider, useAuth } from './AuthContext';

function Pantalla() {
  const { estado, usuario, error, iniciarSesion, cerrarSesion } = useAuth();
  const [correo, setCorreo] = useState('');
  const [password, setPassword] = useState('');

  if (estado === 'cargando') return <ActivityIndicator />;

  if (estado === 'autenticado' && usuario) {
    return (
      <View>
        <Text>Sesión iniciada como {usuario.nombre}</Text>
        <Text>Rol: {usuario.rol}</Text>
        <Button title="Cerrar sesión" onPress={cerrarSesion} />
      </View>
    );
  }

  const entrar = async () => {
    const ok = await iniciarSesion(correo.trim(), password);
    if (ok) setPassword(''); // la contraseña solo vive en este estado local mientras se escribe
  };

  return (
    <View>
      <TextInput
        placeholder="Correo"
        autoCapitalize="none"
        keyboardType="email-address"
        value={correo}
        onChangeText={setCorreo}
      />
      <TextInput placeholder="Contraseña" secureTextEntry value={password} onChangeText={setPassword} />
      {error ? <Text>{error}</Text> : null}
      <Button title="Iniciar sesión" onPress={entrar} />
    </View>
  );
}

export default function LoginDemo({ apiUrl }: { apiUrl: string }) {
  return (
    <AuthProvider apiUrl={apiUrl}>
      <Pantalla />
    </AuthProvider>
  );
}
