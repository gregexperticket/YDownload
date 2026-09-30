# YDownload

Herramienta en C# / .NET que descarga la pista de audio de un vídeo de YouTube y la guarda
como MP3 con etiquetas ID3: título, canal, año, URL de origen y carátula. Tiene una interfaz
gráfica y otra de consola, las dos sobre la misma librería.

El repositorio tiene estos proyectos:

- `src/YDownload.Core`: la lógica (resolver el vídeo, descargar, convertir, etiquetar), sin
  dependencias de interfaz.
- `src/YDownload.WinForms`: la aplicación de escritorio para Windows.
- `src/YDownload.Cli`: la aplicación de consola.

## Requisitos

- [SDK de .NET 10](https://dotnet.microsoft.com/download).
- [ffmpeg](https://ffmpeg.org/), en el `PATH` o junto al ejecutable (la consola admite además
  `--ffmpeg`). En Windows: `winget install Gyan.FFmpeg`.

La librería y la consola funcionan en Windows, Linux y macOS; la interfaz gráfica sólo en
Windows.

## Interfaz gráfica

```
dotnet run --project src/YDownload.WinForms
```

Se pega la URL, se elige carpeta y bitrate, y `Descargar`. La barra muestra la fase en curso
(descarga y conversión con porcentaje) y `Cancelar` interrumpe en cualquier punto sin dejar
restos. Al terminar, `Abrir carpeta` selecciona el MP3 en el Explorador.

La carpeta y el bitrate se recuerdan entre sesiones en `%APPDATA%\YDownload\settings.json`.

## Consola

```
dotnet run --project src/YDownload.Cli -- <url o id de vídeo> [opciones]
```

| Opción | Descripción |
|---|---|
| `-o, --out <carpeta>` | Carpeta de salida. Por defecto, la actual. Se crea si no existe. |
| `-b, --bitrate <kbps>` | Bitrate del MP3, entre 32 y 320. Por defecto, 192. |
| `--ffmpeg <ruta>` | Ejecutable de ffmpeg, si no está en el `PATH`. |
| `-h, --help` | Muestra la ayuda. |

Ejemplo:

```
dotnet run --project src/YDownload.Cli -- https://www.youtube.com/watch?v=XXXXXXXXXXX -o "%USERPROFILE%\Music" -b 256
```

`Ctrl+C` cancela y limpia lo que hubiera a medias.

Para tener ejecutables sueltos:

```
dotnet publish src/YDownload.WinForms -c Release -o publish/winforms
dotnet publish src/YDownload.Cli -c Release -o publish/cli
```

## Cómo funciona

1. Se resuelven los metadatos del vídeo y el manifiesto de streams con
   [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode).
2. Se descarga a un fichero temporal la pista de sólo audio de mayor bitrate (normalmente
   WebM/Opus).
3. ffmpeg la transcodifica a MP3 con `libmp3lame` a bitrate constante.
4. Se escriben las etiquetas ID3v2.3 con [TagLibSharp](https://github.com/mono/taglib-sharp),
   con la miniatura del vídeo como carátula.
5. Se borra el temporal, también si algo ha fallado por el camino.

El fichero se llama como el título del vídeo, saneado para que sea un nombre válido en
Windows. Si ya existe uno con ese nombre se añade un sufijo `(2)`, `(3)`… en lugar de
sobrescribirlo.

## Pruebas

```
dotnet test
```

Las pruebas que necesitan ffmpeg se omiten si no está instalado.

## Limitaciones

- YoutubeExplode se apoya en el funcionamiento interno de la web de YouTube, que cambia con
  frecuencia. Cuando eso ocurre la resolución de streams falla hasta que sale una versión nueva
  del paquete; lo habitual es que baste con actualizarlo.
- Sólo vídeos sueltos; no hay soporte para listas de reproducción.
- La carátula es la miniatura del vídeo. Si no se puede descargar, el MP3 se guarda igualmente
  sin ella y se avisa.

## Aviso

Descargar contenido de YouTube va contra sus Condiciones del Servicio, y según qué material
puede vulnerar además los derechos de autor. Publico el código como ejercicio propio; el uso
que se le dé y sus consecuencias son responsabilidad de quien lo ejecute.

## Licencia

[MIT](LICENSE).
