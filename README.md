# PicChron

A command-line tool that organizes photos and videos into folders by date, with support for modern image formats like HEIC and video metadata extraction.

## Features

- **Smart Date Extraction**: Reads EXIF data from images and metadata from videos (MP4, MOV, etc.)
- **Modern Format Support**: Handles HEIC, HEIF, MP4, MOV, AVI, and more
- **Fallback Logic**: Extracts dates using regex pattern matching if metadata is unavailable
- **Safe Operations**: Move or copy files while preserving original metadata
- **Directory Organization**: Automatically organizes files into `YYYY/YYYY-MM-DD` folder structure
- **Recursive Scanning**: Option to scan current directory or recursively through subdirectories

## Quick Start

### Installation

1. Requires .NET 7.0 or later
2. Clone the repository and build:
   ```bash
   dotnet build
   dotnet publish -c Release
   ```

### Basic Usage

```bash
picchron -s <source_path> -d <destination_path> -t <scan_type> <optional_flags>
```

**Examples:**

Sort photos in current directory and copy to `./Sorted`:
```bash
picchron -s . -d ./Sorted -t Current -m Copy
```

Recursively organize all photos from a folder:
```bash
picchron -s D:\Photos -d D:\Photos\Organized -t All
```

**Note**: Move mode deletes original files after organizing - be careful!

### Command-Line Options

- `-s, --source` (required): Source directory path
- `-d, --destination` (optional): Destination directory (defaults to source)
- `-t, --scan-type` (required): `Current` (current folder only) or `All` (recursive)
- `-m, --mode` (optional): `Copy` (default) or `Move` (deletes originals)
- `--rewrite-timestamps` (flag): Update file access/write times to match extracted date

### Supported File Types

Images: `.jpg`, `.jpeg`, `.png`, `.gif`, `.tiff`, `.webp`, `.heic`, `.heif`  
Videos: `.mp4`, `.mov`, `.avi`, `.3gp`, `.mkv`, `.flv`, `.wmv`, `.m4v`

### Exit Codes

- `0`: Success
- `1`: Operation cancelled by user
- `2`: Error occurred (check console output for details)