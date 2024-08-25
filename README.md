# MCAsset2Font

Converts Minecraft font assets to OpenType (OTF), with both formats implemented from scratch.

Hopefully rewritten in Rust some day.

## Requirements

- C# 10.0
- .NET 6.0
- NuGet SixLabors.ImageSharp
- Extracted Minecraft jar or resource pack

## Usage

Starting the program displays a prompt accepting these commands:

`set verbose [true | false]` \
Toggles the amount of information displayed during conversion. \
Default: `false`

`set assets` \
Allows specifying the assets directory that contains the font files *after* pressing enter.

`set namespace` \
Allows specifying the namespace within the assets folder that contains the font files *after* pressing enter. \
Default: `minecraft`

`set out` \
Allows specifying the output directory for the OpenType Font files *after* pressing enter. 

`set name <key>` \
Allows setting the name string corresponding to `key` *after* pressing enter. \
See more below.

`set created` \
Allows setting the creation timestamp *after* pressing enter. \
Default: System time during conversion

`set modified` \
Allows setting the modification timestamp *after* pressing enter. \
Default: System time during conversion

`run` \
Starts the conversion with the provided parameters. Requires assets and output path to be specified.

During conversion, the program prompts the *family name* corresponding to each font. For example, the user might be shown `default.json` and name it `Seven`. The program will then generate the files `Seven-Regular.otf`, `Seven-Bold.otf`, `Seven-Italic.otf` and `Seven-Bold Italic.otf`; then move on to the next font.

### Name strings

In user-defined and default name strings alike, `<family>` is replaced with the user-specified family name corresponding to each font; and `<subfamily>` is replaced with `Regular`, `Bold`, `Italic` and `Bold Italic` for the four generated font variants respectively.

| ID | Key                     | Default                                    |
|----|-------------------------|--------------------------------------------|
|  0 | `copyright`             |                                            |
|  1 | `family`                | `<family>`                                 |
|  2 | `subfamily`             | `<subfamily>`                              |
|  3 | `identifier`            | `<family> <subfamily> [modification year]` |
|  4 | `full`                  | `<family> <subfamily>`                     |
|  5 | `version`               | `Version 1.0`                              |
|  6 | `postscript`            | `<family>-<subfamily>`                     |
|  7 | `trademark`             |                                            |
|  8 | `manufacturer`          |                                            |
|  9 | `designer`              |                                            |
| 10 | `description`           |                                            |
| 11 | `vendor-url`            |                                            |
| 12 | `designer-url`          |                                            |
| 13 | `license`               |                                            |
| 14 | `license-url`           |                                            |
| 16 | `typographic-family`    |                                            |
| 17 | `typographic-subfamily` |                                            |
| 18 | `compatible-full`       |                                            |
| 19 | `sample-text`           | `Minecrafty`                               |
| 20 | `postscript-findfont`   |                                            |
| 21 | `wws-family`            |                                            |
| 22 | `wws-subfamily`         |                                            |
| 23 | `light-palette`         |                                            |
| 24 | `dark-palette`          |                                            |
| 25 | `postscript-prefix`     |                                            |

See more at the [OTF specification](<https://learn.microsoft.com/en-us/typography/opentype/spec/name#name-ids>).