# The Immortal Merchant - Prototype 0.1

## Resumen

La version `0.1` construyo y valido el primer ciclo jugable completo de **The Immortal Merchant**:

1. Entrar desde la tienda a una dungeon.
2. Explorar salas y elegir una ruta.
3. Combatir enemigos.
4. Recoger objetos con distintas rarezas y valores.
5. Regresar a la tienda conservando el botin.
6. Vender los objetos.
7. Comprar mejoras permanentes.
8. Guardar el progreso localmente y comenzar una nueva expedicion.

Esta version usa arte provisional. Su objetivo fue demostrar que los sistemas centrales funcionan juntos antes de producir sprites, nombres, objetos y balance definitivos.

## Tecnologia

- Unity `6000.4.11f1`.
- Proyecto 2D con Universal Render Pipeline.
- Input System para teclado y mouse.
- TextMesh Pro para la interfaz.
- Escenas y recursos provisionales generables desde herramientas de Editor.

## Controles del prototipo

- Movimiento: `WASD`.
- Ataque: `J` o clic izquierdo.
- Dash: `Espacio`.
- Interaccion con tienda: botones de la interfaz.

## Sistemas implementados

### Ciclo tienda y dungeon

- Escenas independientes `ShopScene` y `DungeonScene`.
- Transicion entre ambas escenas.
- Conservacion de oro, inventario y mejoras durante la sesion.
- Portales de salida bloqueados hasta cumplir las condiciones de cada ruta.
- Regreso anticipado disponible en determinados recorridos.

### Movimiento y combate

- Movimiento del Player en ocho direcciones.
- Dash con velocidad, duracion y enfriamiento configurables.
- Ataque orientado hacia la ultima direccion de movimiento.
- Vida y muerte del Player.
- Periodo de invulnerabilidad de `0.75 s` despues de recibir dano.
- Knockback para Player y enemigos.
- Destello visual al atacar, recibir dano y golpear enemigos.
- Barras de vida para Player y enemigos.

### Enemigos provisionales

- Enemigo normal: estadisticas equilibradas.
- Enemigo rapido: menos vida y mayor velocidad.
- Enemigo resistente: mas vida, dano y tamano.
- Reaparicion de enemigos al comenzar una nueva expedicion.
- Encuentros asociados a salas y puertas.
- Contador de enemigos restantes.

### Dungeon y rutas

- Dungeon formada por varias salas conectadas.
- Puertas cerradas durante encuentros activos.
- Camara que sigue al Player entre salas.
- Primera sala de combate con salida anticipada.
- Eleccion excluyente entre ruta de combate y ruta segura.
- Ruta de combate con tres enemigos y mejor oportunidad de botin.
- Ruta segura con curacion, cofre y portal propio.
- Cofre provisional con recompensa adicional.
- Pickup de curacion de `25 HP`.

### Objetos, rareza y economia

- Inventario con nombre, rareza, cantidad y precio.
- Venta conjunta de todo el inventario.
- Objetos provisionales:
  - `Prototype Scrap`: Common, valor base de 20 de oro.
  - `Prototype Swift Shard`: Uncommon, valor base de 30 de oro.
  - `Prototype Heavy Core`: Rare, valor base de 50 de oro.
- Tablas de drop configurables.
- Probabilidades de drop ajustadas durante pruebas de varias expediciones.
- Recompensas diferentes por enemigo y por cofre.

### Mejoras del comerciante

- Mejora de dano.
- Mejora de vida maxima.
- Mejora de velocidad de movimiento.
- Costos crecientes por nivel comprado.
- Datos de mejoras separados mediante `ScriptableObject`.

### Persistencia

- Guardado local en JSON.
- Persistencia de oro y mejoras.
- Persistencia del inventario corregida y validada al volver a la tienda y reiniciar el juego.
- Herramienta para mostrar la ruta del archivo de guardado.
- Opciones provisionales de depuracion para facilitar pruebas de economia y encuentros.

### Arquitectura de datos

- `ItemData` para definir objetos.
- `DropTableData` para probabilidades y recompensas.
- `EnemyData` para estadisticas de enemigos.
- `UpgradeData` para mejoras de tienda.
- Prefabs separados para Player, enemigos y pickups.
- Builder de Editor para reconstruir las escenas y recursos del prototipo.

## Herramienta de reconstruccion

El prototipo puede regenerarse desde:

`Tools > The Immortal Merchant > Build Shop + Dungeon Scenes`

La opcion `Rebuild` actualiza escenas, prefabs y datos provisionales definidos por el builder.

## Pruebas completadas

- Movimiento, dash y ataque en cuatro direcciones.
- Dano, invulnerabilidad, knockback y barras de vida.
- Apertura de puertas al derrotar enemigos.
- Bloqueo y activacion visual de portales.
- Reaparicion de encuentros entre expediciones.
- Recorrido completo por las dos rutas.
- Curacion y apertura de cofres.
- Recoleccion, conservacion y venta de botin.
- Compra de mejoras y aumento progresivo de precios.
- Persistencia de oro, estadisticas e inventario despues de reiniciar.
- Reconstruccion de escenas con `0 errores` y `0 advertencias` en los bloques validados.

## Historial de la version

| Commit | Cambio principal |
| --- | --- |
| `d626c90` | Estructura del prototipo y scripts base. |
| `634c50c` | Primer ciclo jugable de la version 0.1. |
| `ce696a3` | Tienda y dungeon persistentes. |
| `31f13aa` | Direccion de ataque y feedback de combate. |
| `fe2b81e` | Knockback y barras de vida. |
| `1be3fb4` | Estado de limpieza y portal de salida. |
| `d7335bb` | Variedad provisional de enemigos y botin. |
| `98339fd` | Dungeon de varias salas. |
| `7bbe5d2` | Rutas ramificadas. |
| `879d69a` | Riesgo, recompensa y economia de rutas. |
| `3d5bddc` | Enemigos y drops configurados por datos. |
| `3b06326` | Mejoras del Player configuradas por datos. |
| `0e42b16` | Guardado persistente local. |
| `7f5abbe` | Herramientas de depuracion. |
| `cd8e0a6` | Mejoras de legibilidad visual del prototipo. |

## Resultado de la 0.1

La version demostro que la idea principal es viable: combatir para obtener mercancia, decidir cuanto riesgo asumir, regresar a la tienda y convertir el botin en crecimiento permanente. La siguiente version se concentra en mejorar la profundidad y sensacion del combate sin abandonar todavia el arte provisional.
