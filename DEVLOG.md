# Historia de desarrollo - The Immortal Merchant

Este archivo conserva la historia global del proyecto. Los README de cada version documentan el resultado tecnico; este diario registra tambien las decisiones, pruebas y cambios de direccion que llevaron hasta alli.

## Vision del juego

**The Immortal Merchant** combina dungeon crawling, combate de accion y gestion de una tienda. El jugador entra a dungeons interconectadas, enfrenta encuentros con distinto riesgo, recoge mercancia y decide cuando regresar. En la tienda vende lo conseguido y reinvierte el oro en mejoras permanentes para futuras expediciones.

El desarrollo sigue una regla deliberada: primero validar sistemas y sensaciones con formas y nombres provisionales; despues producir contenido definitivo, sprites, animaciones, objetos, monstruos y balance profundo.

## 11 de junio de 2026 - Nace el proyecto

- Se creo el proyecto Unity 2D con URP.
- Se preparo Git, `.gitignore`, paquetes y configuracion inicial.
- Se establecieron las ramas `main`, `develop` y ramas de prototipo.
- Commit inicial: `21b8fad`.

## 12 de junio de 2026 - Del proyecto vacio al ciclo jugable

### Primera estructura

- Se organizo `Assets/_Project` por arte, audio, escenas, prefabs, datos y scripts.
- Se crearon las bases de movimiento, ataque, enemigos, vida, inventario, tienda, interfaz y estado global.
- Se decidio automatizar buena parte del armado mediante builders de Unity Editor.

### Primer prototipo jugable

- El Player pudo moverse, hacer dash y atacar.
- Un enemigo perseguidor recibia dano y soltaba un objeto.
- El objeto podia recogerse, mostrarse en inventario y venderse.
- El oro podia gastarse para aumentar el dano.
- Este recorrido fue la primera prueba completa de la fantasia central del juego.

### Tienda y dungeon separadas

- El prototipo paso de una escena aislada a un ciclo entre `ShopScene` y `DungeonScene`.
- El estado del jugador se mantuvo entre escenas.
- Se valido entrar, combatir, volver con botin, vender y regresar fortalecido.

### Combate legible

- El ataque comenzo a respetar la direccion de movimiento.
- Se agregaron destellos de ataque y dano.
- El Player obtuvo invulnerabilidad temporal para evitar dano continuo por contacto.
- Se incorporaron knockback y barras de vida.
- Se corrigio una barra del Player que actualizaba el numero pero no su representacion visual.

### Encuentros y salida

- La salida de la dungeon quedo bloqueada hasta derrotar a todos los enemigos.
- Se agrego un contador de enemigos restantes.
- El portal cambio de estado visual al habilitarse.
- Los enemigos volvieron a aparecer correctamente al comenzar otra expedicion.

### Primeras familias de enemigos y objetos

- Se crearon enemigos normal, rapido y resistente.
- Cada uno recibio vida, velocidad, dano, escala y color diferentes.
- Se crearon objetos Common, Uncommon y Rare con precios distintos.
- Las probabilidades de drop se ajustaron mediante varias expediciones de prueba: primero eran demasiado frecuentes y luego se equilibraron.

### La dungeon se convierte en recorrido

- Se agrego una segunda sala conectada por corredor.
- Las puertas comenzaron a cerrarse durante encuentros.
- La camara siguio al Player entre espacios.
- Aparecieron un cofre y una curacion provisional.
- Se valido conservar el botin tanto al completar la dungeon como al usar una salida anticipada.

### Eleccion de rutas

- La dungeon recibio una bifurcacion excluyente.
- La ruta superior ofrecio combate y mayor riesgo.
- La ruta inferior ofrecio curacion, cofre y salida segura.
- Elegir una ruta cerro la otra, introduciendo una decision real dentro de cada expedicion.

### Datos, economia y persistencia

- Enemigos, drops y mejoras migraron a `ScriptableObject`.
- Se agregaron mejoras de dano, vida y velocidad con costos crecientes.
- Se implemento guardado local de oro y progreso.
- Durante pruebas se descubrio que el inventario no persistia al cerrar el juego; se corrigio y se valido tambien al regresar primero a la tienda.
- Se agregaron herramientas de depuracion y una opcion para mostrar la ruta del archivo de guardado.

## 13 de junio de 2026 - Cierre de 0.1 e inicio de 0.2

### Cierre de Prototype 0.1

- Se mejoro la lectura visual de puertas, cofres y portales.
- La rama `feature/prototype-0.1` quedo cerrada en `cd8e0a6`.
- El ciclo de tienda, expedicion, rutas, botin, venta, mejoras y guardado quedo funcional.

### Prototype 0.2: profundidad de combate

- Los enemigos cuerpo a cuerpo recibieron ataques anticipados mediante indicadores visibles.
- El combo del Player paso a tener tres golpes.
- Los primeros dos golpes pudieron cancelarse con dash.
- El tercer golpe se convirtio en una accion comprometida y mas fuerte.
- Se agrego un enemigo a distancia cian.
- El enemigo a distancia mantiene separacion, muestra una linea de apuntado y dispara proyectiles esquivables.
- La ruta de combate mantuvo tres enemigos, reemplazando uno normal por el nuevo enemigo a distancia.
- Commit del bloque: `79b468b`.

## 14 de junio de 2026 - Sensacion de impacto y enemigos especializados

### Feedback de combate

- Se agrego hit stop al conectar ataques.
- La camara recibio sacudidas breves.
- El tercer golpe produce un impacto mayor.
- Los enemigos encogen, giran y desaparecen al morir.
- Se evito aplicar dano duplicado cuando un enemigo tiene varios colliders.
- Los golpes del Player recibieron un pequeno avance hacia la direccion atacada.
- Los enemigos melee comenzaron a embestir despues de su anticipacion.
- Se impidio que enemigos muertos completaran ataques o proyectiles pendientes.
- Commit del bloque: `af5d142`.

### Ataque doble del enemigo rapido

- El enemigo rapido ahora ejecuta dos embestidas consecutivas.
- El segundo golpe tiene una anticipacion amarilla mas corta.
- Al terminar la secuencia, el enemigo retrocede para crear distancia.
- La invulnerabilidad de `0.75 s` evita que ambos golpes castiguen injustamente si el primero conecta; el segundo funciona como presion de posicionamiento.
- El comportamiento fue probado correctamente en Unity.
- Estado: validado y pendiente de commit manual.

## Estado actual

- Version activa: `prototype-0.2`.
- Rama activa: `feature/prototype-0.2`.
- El arte, nombres y balance siguen siendo provisionales.
- El foco actual es demostrar que la arquitectura soporta enemigos con mecanicas realmente distintas.
- Proximos candidatos: golpe de area del tanque, retirada y patrones del enemigo a distancia, y nuevas respuestas defensivas del Player.

## Forma de trabajo acordada

Cada bloque sigue este orden:

1. Elegir una mejora concreta.
2. Implementarla y compilar sin errores ni advertencias.
3. Probarla manualmente en Unity.
4. Corregir cualquier problema encontrado.
5. Confirmar la prueba.
6. Entregar los comandos para que el desarrollador haga commit y push manualmente.
7. Actualizar este diario y el documento de la version cuando corresponda.
