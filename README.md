# YDownload

Aplicación de escritorio para Windows, en C# / .NET, que descarga vídeos de YouTube: el vídeo
completo como MP4, o sólo la pista de audio como MP3 con etiquetas ID3 (título, canal, año, URL
de origen y carátula). El mismo ejecutable funciona también sin interfaz si se le pasan
argumentos.

El repositorio tiene dos proyectos:

- `src/YDownload.Core`: la lógica (resolver el vídeo, descargar, convertir o unir, etiquetar), sin
  dependencias de interfaz.
- `src/YDownload.WinForms`: la aplicación, que genera `YDownload.exe`.

## Requisitos

- Windows.
- [SDK de .NET 10](https://dotnet.microsoft.com/download).
- [ffmpeg](https://ffmpeg.org/), en el `PATH` o junto al ejecutable: `winget install Gyan.FFmpeg`.

## Compilar

```
dotnet publish src/YDownload.WinForms -c Release -o publish
```

Deja `YDownload.exe` en `publish/`. Para probar sin publicar, `dotnet run --project src/YDownload.WinForms`.

## Con interfaz

Sin argumentos se abre la ventana. Se pega la URL, se elige entre `Descargar vídeo completo` y
`Descargar solo audio`, la carpeta y, para el audio, el bitrate, y `Descargar`. La barra
muestra la fase en curso (descarga, conversión o unión, con porcentaje) y `Cancelar` interrumpe
en cualquier punto sin dejar restos. Al terminar, `Abrir carpeta` selecciona el fichero en el
Explorador.

El tipo de descarga, la carpeta y el bitrate se recuerdan entre sesiones en
`%APPDATA%\YDownload\settings.json`. El tipo se guarda en cuanto se cambia, aunque no se llegue a
descargar nada.

## Sin interfaz

Con argumentos no se abre ninguna ventana: descarga, escribe el progreso en la consola y
termina con un código de salida. No usa lo elegido en la ventana: si no se indica `--mode`,
descarga el audio.

```
YDownload.exe <url o id de vídeo> [opciones]
```

| Opción | Descripción |
|---|---|
| `-m, --mode <audio\|video>` | `audio` guarda sólo el audio como MP3; `video`, el vídeo completo como MP4. Por defecto, `audio`. |
| `-o, --out <carpeta>` | Carpeta de salida. Por defecto, la actual. Se crea si no existe. |
| `-b, --bitrate <kbps>` | Bitrate del MP3, entre 32 y 320. Por defecto, 192. Sólo se usa en modo `audio`. |
| `--ffmpeg <ruta>` | Ejecutable de ffmpeg, si no está en el `PATH`. |
| `-h, --help` | Muestra la ayuda. |

Códigos de salida: `0` si todo ha ido bien, `1` si la descarga falla o se cancela con
`Ctrl+C`, `2` si los argumentos no son válidos.

Al ser un ejecutable de ventana, Windows no hace que la consola interactiva espere a que
termine: el prompt vuelve enseguida y la salida aparece después. En ficheros `.bat`/`.cmd` sí
se espera. Para esperar también desde el prompt:

```
start /wait YDownload.exe https://www.youtube.com/watch?v=XXXXXXXXXXX -o "%USERPROFILE%\Music" -b 256
start /wait YDownload.exe https://www.youtube.com/watch?v=XXXXXXXXXXX -m video -o "%USERPROFILE%\Videos"
```

y en PowerShell:

```
Start-Process YDownload.exe -ArgumentList 'https://www.youtube.com/watch?v=XXXXXXXXXXX' -NoNewWindow -Wait
```

## Cómo funciona

Primero se resuelven los metadatos del vídeo y el manifiesto de streams con
[YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode). Después depende del modo.

Sólo audio:

1. Se descarga a un fichero temporal la pista de sólo audio de mayor bitrate (normalmente
   WebM/Opus).
2. ffmpeg la transcodifica a MP3 con `libmp3lame` a bitrate constante.
3. Se escriben las etiquetas ID3v2.3 con [TagLibSharp](https://github.com/mono/taglib-sharp),
   con la miniatura del vídeo como carátula.

Vídeo completo:

1. YouTube sólo sirve vídeo y audio juntos en baja resolución, así que se descargan por
   separado la pista de vídeo de mayor calidad (a igual calidad, H.264 antes que VP9 o AV1,
   porque se reproduce en cualquier sitio) y la de audio AAC.
2. ffmpeg las une en un MP4 sin recodificar, así que es rápido y no pierde calidad, y le pone
   título, canal, año y URL de origen como metadatos.

En los dos casos los temporales se borran, también si algo ha fallado por el camino.

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
- La carátula del MP3 es la miniatura del vídeo. Si no se puede descargar, el MP3 se guarda
  igualmente sin ella y se avisa.
- En vídeos con resolución superior a 1080p la pista suele ser VP9 o AV1. El MP4 las admite,
  pero en Windows puede hacer falta instalar las extensiones de vídeo VP9 o AV1 de la Microsoft
  Store para verlo con el reproductor del sistema.

## Aviso

Descargar contenido de YouTube va contra sus Condiciones del Servicio, y según qué material
puede vulnerar además los derechos de autor. Publico el código como ejercicio propio; el uso
que se le dé y sus consecuencias son responsabilidad de quien lo ejecute.

## Licencia

[MIT](LICENSE).
