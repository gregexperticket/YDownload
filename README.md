# YDownload

Herramienta de consola en C# / .NET que descarga la pista de audio de un vídeo de YouTube y
la guarda como MP3 con etiquetas ID3: título, canal, año, URL de origen y carátula.

El repositorio tiene dos proyectos:

- `src/YDownload.Core`: la lógica (resolver el vídeo, descargar, convertir, etiquetar), sin
  nada de consola, para poder reutilizarla desde otras interfaces.
- `src/YDownload.Cli`: la aplicación de consola. Sólo interpreta los argumentos y pinta el
  progreso.

## Requisitos

- [SDK de .NET 10](https://dotnet.microsoft.com/download).
- [ffmpeg](https://ffmpeg.org/), en el `PATH`, junto al ejecutable, o indicado con `--ffmpeg`.
  En Windows: `winget install Gyan.FFmpeg`.

Funciona en Windows, Linux y macOS.

## Uso

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

El fichero se llama como el título del vídeo, saneado para que sea un nombre válido en
Windows. Si ya existe uno con ese nombre se añade un sufijo `(2)`, `(3)`… en lugar de
sobrescribirlo.

Para tener un ejecutable suelto:

```
dotnet publish src/YDownload.Cli -c Release -o publish
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

`Ctrl+C` cancela en cualquier punto y limpia lo que hubiera a medias.

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
