# Changes I made

1. Made it so you can put the UsmToolkit folder on PATH and it will work anywhere, instead of needing to be in the UsmToolkit folder to use the tool.
2. Changed how the extracted .usm files are handled. Before, if a .usm file has multiple audio extracted, only the newest one will stay. Now, all the files will stay and get converted.
3. Made it so the original .usm file is also deleted after conversion.
4. Removed the need of vgmstream as FFmpeg can handle .adx audio files again and updated the ffmpeg download link in `deps.json`.
5. Removed Video and Audio parameters in `config.json`, as it didn't really work for me and made my output worse. The original config will be in `og-config.json`.



# UsmToolkit - Original Description

Tool to convert USM video files into user-friendly formats.

## Getting started

Download the latest version and run `UsmToolkit get-dependencies`. This will download ffmpeg and vgmstream from the URLs provided in `deps.json`. These are neccessary for this tool to operate!

After that, it's as easy as it can get.

### Extracting
```
UsmToolkit extract <file/folder>
```

### Converting
```
UsmToolkit convert <file/folder>
```

For more informations run `UsmToolkit extract -h` and `UsmToolkit convert -h`.

## Custom conversion parameter

You should find `config.json` in the folder of the executable. With it, you can completly customize how the extracted file is processed by ffmpeg.
The default configuration ships as follows:

* Video: Will be copied
* Audio: Re-encoded as AC3 at 640kb/s. If the file has 6 channels, they will be merged into stereo
    * Left channel: CH1, CH3, CH5 50% volume, CH6
    * Right channel: CH2, CH4, CH5 50% volume, CH6
* Output is a MP4 file

You can change these settings to your likings, it's standard ffmpeg syntax.

## License

UsmToolkit follows the MIT License. It uses code from [VGMToolbox](https://sourceforge.net/projects/vgmtoolbox/).