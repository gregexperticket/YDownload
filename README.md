# YDownload

Utilidad personal de consola, en C# / .NET, que descarga la pista de audio de un vídeo de
YouTube y la convierte a MP3.

Es un proyecto de andar por casa: un único fichero, sin tests y con la URL escrita a mano en
el código. Está publicado tal cual, más como archivo de algo que me hice en su día que como
herramienta lista para usar por terceros.

## Cómo funciona

1. Resuelve los metadatos del vídeo con [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode).
2. Del manifiesto de streams se queda con el de **sólo audio** de mayor bitrate.
3. Lo descarga a `c:/YDownloads/` con el título del vídeo saneado como nombre de fichero.
4. Lo transcodifica a MP3 con [NAudio](https://github.com/naudio/NAudio) sobre Media Foundation.
5. Borra el fichero intermedio.

## Requisitos

- **Windows.** La conversión usa Media Foundation, que no existe fuera de Windows.
  En ediciones N/KN hace falta el Media Feature Pack.
- **SDK de .NET 6.** Fijado en `global.json` con `rollForward: latestMinor`, así que un SDK
  más moderno por sí solo no vale.

## Uso

No hay argumentos de línea de comandos: la URL y la carpeta de salida están codificadas al
principio de `Program.cs`.

```
git clone https://github.com/gregexperticket/YDownload.git
cd YDownload
# editar videoUrl y outputPath en Program.cs
dotnet run
```

## Limitaciones conocidas

Sin maquillaje, para que quede constancia:

- **La carpeta de salida no se crea sola.** Si `c:/YDownloads/` no existe, la descarga falla
  con `DirectoryNotFoundException`.
- **La conversión es frágil.** Se coge el audio de mayor bitrate sin filtrar contenedor, y en
  YouTube ése suele ser WebM/Opus, que Media Foundation no sabe decodificar. Funciona con
  M4A/AAC y revienta con el resto.
- **Deja basura si falla.** El borrado del fichero intermedio no está en un `finally`, así que
  un error en la conversión deja el `.webm`/`.m4a` huérfano en disco.
- **El MP3 sale sin metadatos ID3**: ni título, ni artista, ni carátula.
- **Sin progreso ni cancelación.** Durante la descarga y la conversión la consola parece
  colgada.
- **Errores opacos**: se captura `Exception` y sólo se imprime el mensaje, sin tipo ni traza.
- **Dependencia frágil por naturaleza.** YoutubeExplode se apoya en el funcionamiento interno
  de la web de YouTube y se rompe cada pocas semanas. La versión fijada aquí (6.5.3, de marzo
  de 2025) es antigua y lo más probable es que ya no resuelva los streams.
- **.NET 6 está fuera de soporte** desde noviembre de 2024.

## Aviso

Descargar contenido de YouTube va contra sus Condiciones del Servicio, y según qué material
puede vulnerar además los derechos de autor. Publico el código como ejercicio propio; el uso
que se le dé y sus consecuencias son responsabilidad de quien lo ejecute.

## Licencia

[MIT](LICENSE).
